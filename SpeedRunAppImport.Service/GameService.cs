using System;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Client;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;
using System.Threading;
using SpeedRunCommon;
using System.IO;
using System.Net;

namespace SpeedRunAppImport.Service
{
    public class GameService : BaseService, IGameService
    {
        private readonly ISettingService _settingService = null;
        private readonly IUserService _userService = null;
        private readonly IGameRepository _gameRepo = null;
        private readonly IPlatformRepository _platformRepo = null;
        private readonly IUserRepository _userRepo = null;
        private readonly ISpeedRunRepository _speedRunRepo = null;
        private readonly ILogger _logger;

        public GameService(ISettingService settingService, IUserService userService, IGameRepository gameRepo, IPlatformRepository platformRepo, IUserRepository userRepo, ISpeedRunRepository speedRunRepo, ILogger logger)
        {
            _settingService = settingService;
            _userService = userService;
            _gameRepo = gameRepo;
            _platformRepo = platformRepo;
            _userRepo = userRepo;
            _speedRunRepo = speedRunRepo;
            _logger = logger;
        }

        public bool ProcessGames(DateTime lastImportDateUtc, bool isFullPull, bool isBulkReload)
        {
            bool result = true;

            try
            {
                _logger.Information("Started ProcessGames: {@LastImportDateUtc}, {@IsFullPull}, {@IsBulkReload}", lastImportDateUtc, isFullPull, isBulkReload);
                GamesOrdering? orderBy = isFullPull ? (GamesOrdering?)null : GamesOrdering.CreationDateDescending;
                var gameEmbeds = new GameEmbeds { EmbedCategories = true, EmbedLevels = true, EmbedModerators = true, EmbedPlatforms = false, EmbedVariables = true };
                var results = new List<Game>();
                var games = new List<Game>();
                var prevTotal = 0;

                do
                {
                    games = GetGamesWithRetry(MaxElementsPerPage, results.Count + prevTotal, gameEmbeds, orderBy);
                    results.AddRange(games);
                    _logger.Information("Pulled games: {@New}, total games: {@Total}", games.Count, results.Count + prevTotal);
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        prevTotal += results.Count;
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveGames(results, isBulkReload);
                        results.ClearMemory();
                    }
                }
                while (games.Count == MaxElementsPerPage && (isFullPull || games.Min(i => i.CreationDate ?? SqlMinDateTime) > lastImportDateUtc));
                //while (1 == 0);

                if (!isFullPull)
                {
                    results.RemoveAll(i => (i.CreationDate ?? SqlMinDateTime) <= lastImportDateUtc);
                }

                if (results.Any())
                {
                    SaveGames(results, isBulkReload);
                    var lastUpdateDate = results.Max(i => i.CreationDate) ?? DateTime.UtcNow;
                    _settingService.UpdateSetting("GameLastImportDate", lastUpdateDate);
                    results.ClearMemory();
                }

                if (!isBulkReload)
                {
                    ProcessChangedGames(gameEmbeds);
                }

                _logger.Information("Completed ProcessGames");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "ProcessGames");
            }

            return result;
        }

        public void ProcessChangedGames(GameEmbeds gameEmbeds, int retryCount = 0)
        {
            _logger.Information("Started ProcessChangedGames");

            var results = new List<Game>();
            var gameSpeedRunComViews = _gameRepo.GetGameSpeedRunComViews(i => i.IsChanged == true);

            if (gameSpeedRunComViews.Any())
            {
                var clientContainer = new ClientContainer();
                foreach (var gameSpeedRunComView in gameSpeedRunComViews)
                {
                    try
                    {
                        var game = clientContainer.Games.GetGame(gameSpeedRunComView.SpeedRunComID, gameEmbeds);
                        if (game != null)
                        {
                            results.Add(game);
                            _logger.Information("Pulled games: {@New}, total games: {@Total}", 1, results.Count);
                        }
                        Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                        _logger.Error(ex, "ProcessChangedGames");
                    }
                }
            }

            if (results.Any())
            {
                SaveGames(results, false);
                results.ClearMemory();
            }

            _logger.Information("Completed ProcessChangedGames");
        }

        public List<Game> GetGamesWithRetry(int elementsPerPage, int elementsOffset, GameEmbeds embeds, GamesOrdering? orderBy, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            List<Game> games = null;

            try
            {
                games = clientContainer.Games.GetGames(elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, embeds: embeds, orderBy: orderBy).ToList();
            }
            catch (Exception ex)
            {
                if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    retryCount++;
                    _logger.Information("Retrying pull games: {@New}, total games: {@Total}, retry: {@RetryCount}", elementsPerPage, elementsOffset, retryCount);
                    games = GetGamesWithRetry(elementsPerPage, elementsOffset, embeds, orderBy, retryCount);
                }
                else
                {
                    throw ex;
                }
            }

            return games;
        }

        public void SaveGames(IEnumerable<Game> games, bool isBulkReload)
        {
            _logger.Information("Started SaveGames: {@Count}, {@IsBulkReload}", games.Count(), isBulkReload);

            _gameRepo.RemoveObsoleteGameSpeedRunComIDs();

            games = games.GroupBy(g => new { g.ID })
                         .Select(i => i.First())
                         .ToList();

            games = games.OrderBy(i => i.CreationDate).ToList();
            var gameIDs = games.Select(i => i.ID).ToList();
            var gameSpeedRunComIDs = _gameRepo.GetGameSpeedRunComIDs();
            gameSpeedRunComIDs = gameSpeedRunComIDs.Join(gameIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var userIDs = games.SelectMany(i => i.ModeratorUsers.Select(i => i.ID)).Distinct().ToList();
            var userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs();
            userSpeedRunComIDs = userSpeedRunComIDs.Join(userIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var levelIDs = games.SelectMany(i => i.Levels.Select(i => i.ID)).Distinct().ToList();
            var levelSpeedRunComIDs = _gameRepo.GetLevelSpeedRunComIDs();
            levelSpeedRunComIDs = levelSpeedRunComIDs.Join(levelIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var categoryIDs = games.SelectMany(i => i.Categories.Select(i => i.ID)).Distinct().ToList();
            var categorySpeedRunComIDs = _gameRepo.GetCategorySpeedRunComIDs();
            categorySpeedRunComIDs = categorySpeedRunComIDs.Join(categoryIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var variableIDs = games.SelectMany(i => i.Variables.Select(i => i.ID)).Distinct().ToList();
            var variableSpeedRunComIDs = _gameRepo.GetVaraibleSpeedRunComIDs();
            variableSpeedRunComIDs = variableSpeedRunComIDs.Join(variableIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var variableValueIDs = games.SelectMany(i => i.Variables.SelectMany(g => g.Values.Select(h => h.ID))).Distinct().ToList();
            var variableValueSpeedRunComIDs = _gameRepo.GetVariableValueSpeedRunComIDs();
            variableValueSpeedRunComIDs = variableValueSpeedRunComIDs.Join(variableValueIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            var platformIDs = games.SelectMany(i => i.PlatformIDs).Distinct().ToList();
            var platformSpeedRunComIDs = _platformRepo.GetPlatformSpeedRunComIDs(i => platformIDs.Contains(i.SpeedRunComID)).ToList();

            var regionIDs = games.SelectMany(i => i.RegionIDs).Distinct().ToList();
            var regionSpeedRunComIDs = _gameRepo.GetRegionSpeedRunComIDs(i => regionIDs.Contains(i.SpeedRunComID)).ToList();

            var moderators = games.Where(i => i.ModeratorUsers != null)
                                  //.SelectMany(i => i.ModeratorUsers.Where(i => !userSpeedRunComIDs.Any(g => g.SpeedRunComID == i.ID)))
                                  .SelectMany(i => i.ModeratorUsers)
                                  .GroupBy(g => new { g.ID })
                                  .Select(i => i.First())
                                  .ToList();
            if (moderators.Any())
            {
                _userService.SaveUsers(moderators, isBulkReload, userSpeedRunComIDs);
                userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs().Where(i => userIDs.Contains(i.SpeedRunComID)).ToList();
            }

            var gameEntities = games.Select(i => new GameEntity()
            {
                ID = gameSpeedRunComIDs.Where(g => g.SpeedRunComID == i.ID).Select(g => g.GameID).FirstOrDefault(),
                SpeedRunComID = i.ID,
                Name = i.Name,
                YearOfRelease = i.YearOfRelease,
                IsRomHack = i.IsRomHack,
                Abbr = i.Abbreviation,
                CreatedDate = i.CreationDate
            }).ToList();
            var gameLinkEntities = games.Select(i => new GameLinkEntity()
            {
                GameSpeedRunComID = i.ID,
                SpeedRunComUrl = i.WebLink.ToString(),
                CoverImageUrl = i.Assets?.CoverLarge?.Uri.ToString()
            }).ToList();
            var levelEntities = games.SelectMany(i => i.Levels.Select(g => new LevelEntity
            {
                ID = levelSpeedRunComIDs.Where(h => h.SpeedRunComID == g.ID).Select(o => o.LevelID).FirstOrDefault(),
                SpeedRunComID = g.ID,
                Name = g.Name,
                GameSpeedRunComID = i.ID
            })).ToList();
            var levelRuleEntities = games.SelectMany(i => i.Levels.Select(g => new LevelRuleEntity
            {
                LevelSpeedRunComID = g.ID,
                Rules = g.Rules
            })).Where(i => !string.IsNullOrWhiteSpace(i.Rules))
            .ToList();
            var categoryEntities = games.SelectMany(i => i.Categories.Select(g => new CategoryEntity
            {
                ID = categorySpeedRunComIDs.Where(h => h.SpeedRunComID == g.ID).Select(o => o.CategoryID).FirstOrDefault(),
                SpeedRunComID = g.ID,
                Name = g.Name,
                GameSpeedRunComID = i.ID,
                CategoryTypeID = (int)g.Type,
                IsMiscellaneous = g.IsMiscellaneous,
                IsTimerAscending = g.IsTimerAscending
            })).ToList();
            var categoryRuleEntities = games.SelectMany(i => i.Categories.Select(g => new CategoryRuleEntity
            {
                CategorySpeedRunComID = g.ID,
                Rules = g.Rules
            })).Where(i => !string.IsNullOrWhiteSpace(i.Rules))
            .ToList();
            var variableEntities = games.SelectMany(i => i.Variables.Select(g => new VariableEntity
            {
                ID = variableSpeedRunComIDs.Where(h => h.SpeedRunComID == g.ID).Select(o => o.VariableID).FirstOrDefault(),
                SpeedRunComID = g.ID,
                Name = g.Name,
                VariableScopeTypeID = (int)g.Scope.Type,
                GameSpeedRunComID = i.ID,
                CategorySpeedRunComID = g.CategoryID,
                LevelSpeedRunComID = g.Scope.LevelID,
                IsSubCategory = g.IsSubCategory
            })).Where(i => (string.IsNullOrWhiteSpace(i.CategorySpeedRunComID) || categoryEntities.Any(g => g.GameSpeedRunComID == i.GameSpeedRunComID && g.SpeedRunComID == i.CategorySpeedRunComID))
                            && (string.IsNullOrWhiteSpace(i.LevelSpeedRunComID) || levelEntities.Any(g => g.GameSpeedRunComID == i.GameSpeedRunComID && g.SpeedRunComID == i.LevelSpeedRunComID)))
               .ToList();
            var variableValueEntities = games.SelectMany(i => i.Variables.SelectMany(g => g.Values.Select(h => new VariableValueEntity
            {
                ID = variableValueSpeedRunComIDs.Where(n => n.SpeedRunComID == h.ID).Select(o => o.VariableValueID).FirstOrDefault(),
                SpeedRunComID = h.ID,
                GameSpeedRunComID = g.GameID,
                VariableSpeedRunComID = h.VariableID,
                Value = h.Value,
                IsCustomValue = h.IsCustomValue
            }))).Where(i => variableEntities.Any(g => g.SpeedRunComID == i.VariableSpeedRunComID))
            .ToList();
            var gamePlatformEntities = games.SelectMany(i => i.PlatformIDs.Select(g => new GamePlatformEntity
            {
                GameSpeedRunComID = i.ID,
                PlatformID = platformSpeedRunComIDs.Where(h => h.SpeedRunComID == g).Select(o => o.PlatformID).FirstOrDefault(),
                PlatformSpeedRunComID = g
            }))
            .Where(i => i.PlatformID != 0)
            .ToList();
            var gameRegionEntities = games.SelectMany(i => i.RegionIDs.Select(g => new GameRegionEntity
            {
                GameSpeedRunComID = i.ID,
                RegionID = regionSpeedRunComIDs.Where(h => h.SpeedRunComID == g).Select(o => o.RegionID).FirstOrDefault()
            }))
            .Where(i => i.RegionID != 0)
            .ToList();
            var gameModeratorEntities = games.SelectMany(i => i.ModeratorUsers.Select(g => new GameModeratorEntity
            {
                GameSpeedRunComID = i.ID,
                UserID = userSpeedRunComIDs.Where(h => h.SpeedRunComID == g.ID).Select(o => o.UserID).FirstOrDefault(),
                UserSpeedRunComID = g.ID
            }))
            .Where(i => i.UserID != 0)
            .ToList();
            var gameRulesetEntities = games.Select(i => new GameRulesetEntity
            {
                GameSpeedRunComID = i.ID,
                ShowMilliseconds = i.Ruleset.ShowMilliseconds,
                RequiresVerification = i.Ruleset.RequiresVerification,
                RequiresVideo = i.Ruleset.RequiresVideo,
                DefaultTimingMethodID = (int)i.Ruleset.DefaultTimingMethod,
                EmulatorsAllowed = i.Ruleset.EmulatorsAllowed
            }).ToList();
            var gameTimingMethodEntities = games.SelectMany(i => i.Ruleset.TimingMethods.Select(g => new GameTimingMethodEntity
            {
                GameSpeedRunComID = i.ID,
                TimingMethodID = (int)g
            })).ToList();

            if (isBulkReload)
            {
                _gameRepo.InsertGames(gameEntities, gameLinkEntities, levelEntities, levelRuleEntities, categoryEntities, categoryRuleEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameRegionEntities, gameModeratorEntities, gameRulesetEntities, gameTimingMethodEntities);
            }
            else
            {
                var newGameEntities = gameEntities.Where(i => i.ID == 0).ToList();
                SetChangedGames(gameEntities, gameLinkEntities, categoryEntities, levelEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameModeratorEntities);
                var changedGameEntities = gameEntities.Where(i => i.IsChanged == true).ToList();
                var totalGames = gameEntities.Count();
                gameEntities = newGameEntities.Concat(changedGameEntities).ToList();
                var saveGameSpeedRunComIDs = gameEntities.Select(i => i.SpeedRunComID).ToList();

                _logger.Information("Found NewGames: {@New}, ChangedGames: {@Changed}, TotalGames: {@Total}", newGameEntities.Count(), changedGameEntities.Count(), totalGames);

                gameLinkEntities = gameLinkEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                levelEntities = levelEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                levelRuleEntities = levelRuleEntities.Where(i => levelEntities.Any(g => g.SpeedRunComID == i.LevelSpeedRunComID)).ToList();
                categoryEntities = categoryEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                categoryRuleEntities = categoryRuleEntities.Where(i => categoryEntities.Any(g => g.SpeedRunComID == i.CategorySpeedRunComID)).ToList();
                variableEntities = variableEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                variableValueEntities = variableValueEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                gamePlatformEntities = gamePlatformEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                gameModeratorEntities = gameModeratorEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                gameRulesetEntities = gameRulesetEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();
                gameTimingMethodEntities = gameTimingMethodEntities.Where(i => saveGameSpeedRunComIDs.Contains(i.GameSpeedRunComID)).ToList();

                _gameRepo.SaveGames(gameEntities, gameLinkEntities, levelEntities, levelRuleEntities, categoryEntities, categoryRuleEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameRegionEntities, gameModeratorEntities, gameRulesetEntities, gameTimingMethodEntities);
            }

            if (IsProcessGameCoverImages)
            {
                ProcessGameCoverImages(gameLinkEntities, gameEntities, isBulkReload);
            }

            _logger.Information("Completed SaveGames");
        }

        public void ProcessGameCoverImages(List<GameLinkEntity> gameLinks, List<GameEntity> games, bool isBulkReload)
        {
            gameLinks = gameLinks.Where(i => !string.IsNullOrWhiteSpace(i.CoverImageUrl)).ToList();
            var tempGameCoverPaths = GetGameCoverImages(gameLinks, games, isBulkReload);
            var gameCoverPaths = MoveGameCoverImages(tempGameCoverPaths);
            ClearTempFolder();
            SaveGameCoverImages(gameLinks, gameCoverPaths);
        }

        public Dictionary<int, string> GetGameCoverImages(List<GameLinkEntity> gameLinks, List<GameEntity> games, bool isBulkReload)
        {
            _logger.Information("Started GetGameCoverImages: {@Count}", gameLinks.Count);
            var tempGameCoverPaths = new Dictionary<int, string>();

            int count = 1;
            foreach (var gameLink in gameLinks)
            {
                var gameSpeedRunComID = games.Where(i => i.ID == gameLink.GameID).Select(i => i.SpeedRunComID).FirstOrDefault();
                var fileName = string.Format("GameCover_{0}.{1}", gameSpeedRunComID, ImageFileExt);
                var filePath = Path.Combine("/" + GameImageWebPath, fileName);
                if (!File.Exists(filePath))
                {
                    var tempFilePath = Path.Combine(TempImportPath, fileName);
                    try
                    {
                        using (WebClient _wc = new WebClient())
                        {
                            _wc.DownloadFile(new Uri(gameLink.CoverImageUrl), tempFilePath);
                        }
                        Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayShortMS));
                    }
                    catch (Exception ex)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                        _logger.Information(ex, "SetTempCoverImages");
                        tempFilePath = null;
                    }

                    if (!string.IsNullOrWhiteSpace(tempFilePath) && !tempGameCoverPaths.ContainsKey(gameLink.GameID))
                    {
                        tempGameCoverPaths.Add(gameLink.GameID, tempFilePath);
                    }
                }
                else if (isBulkReload)
                {
                    gameLink.CoverImagePath = filePath;
                }

                _logger.Information("Set gameImage {@Count} / {@Total}", count, gameLinks.Count);
                count++;
            }

            _logger.Information("Completed GetGameCoverImages");

            return tempGameCoverPaths;
        }

        public Dictionary<int, string> MoveGameCoverImages(Dictionary<int, string> tempGameCoverPaths)
        {
            var gameCoverPaths = new Dictionary<int, string>();

            foreach (var tempGameCoverPath in tempGameCoverPaths)
            {
                var fileName = Path.GetFileName(tempGameCoverPath.Value);
                var destFilePath = Path.Combine(BaseWebPath, GameImageWebPath, fileName);
                if (File.Exists(tempGameCoverPath.Value))
                {
                    File.Move(tempGameCoverPath.Value, destFilePath, true);
                    var gameCoverPath = Path.Combine("/" + GameImageWebPath, fileName);
                    gameCoverPaths.Add(tempGameCoverPath.Key, gameCoverPath);
                }
            }

            return gameCoverPaths;
        }

        public void ClearTempFolder()
        {
            var di = new DirectoryInfo(TempImportPath);
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        public void SaveGameCoverImages(List<GameLinkEntity> gameLinks, Dictionary<int, string> gameCoverPaths)
        {
            foreach (var gameLink in gameLinks)
            {
                if (gameCoverPaths.ContainsKey(gameLink.GameID))
                {
                    gameLink.CoverImagePath = gameCoverPaths[gameLink.GameID];
                }
            }

            gameLinks = gameLinks.Where(i => !string.IsNullOrWhiteSpace(i.CoverImagePath)).ToList();
            _gameRepo.UpdateGameCoverImages(gameLinks);
        }

        public void SetChangedGames(List<GameEntity> games, List<GameLinkEntity> gameLinks, IEnumerable<CategoryEntity> categories, IEnumerable<LevelEntity> levels, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameModeratorEntity> gameModerators)
        {
            _logger.Information("Started SetChangedGames: {@Count}", games.Count());

            var gameSpeedRunComViews = new List<GameSpeedRunComView>();

            var maxBatchCount = 500;
            var batchCount = 0;
            while (batchCount < games.Count())
            {
                var gameSpeedRunComIDsBatch = games.Skip(batchCount).Take(maxBatchCount).Select(i => i.SpeedRunComID).ToList();
                var gameSpeedRunComViewsBatch = _gameRepo.GetGameSpeedRunComViews(i => gameSpeedRunComIDsBatch.Contains(i.SpeedRunComID));
                gameSpeedRunComViews.AddRange(gameSpeedRunComViewsBatch);
                batchCount += maxBatchCount;
            }

            bool isChanged;
            bool isVariablesOrderChanged;
            foreach (var game in games)
            {
                isChanged = false;
                isVariablesOrderChanged = false;
                var gameSpeedRunComView = gameSpeedRunComViews.FirstOrDefault(i => i.SpeedRunComID == game.SpeedRunComID);
                var changeReasons = new List<string>();

                if (gameSpeedRunComView != null)
                {
                    var categoryIDs = categories.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).Select(h => h.SpeedRunComID).ToList();
                    var levelIDs = levels.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).Select(h => h.SpeedRunComID).ToList();
                    var variableIDs = variables.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).Select(h => h.SpeedRunComID).ToList();
                    var variableValueIDs = variableValues.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).Select(h => h.SpeedRunComID).ToList();
                    var platformIDs = gamePlatforms.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).Select(h => h.PlatformSpeedRunComID).ToList();
                    var moderatorUserIDs = gameModerators.Where(i => i.GameSpeedRunComID == game.SpeedRunComID).Select(h => h.UserSpeedRunComID).ToList();

                    if (gameSpeedRunComView.IsChanged.HasValue && gameSpeedRunComView.IsChanged.Value)
                    {
                        isChanged = true;
                        isVariablesOrderChanged = true;
                        GameIDsToUpdateSpeedRuns.Add(game.ID);
                        changeReasons.Add("Changed from DB");
                    }

                    if (!isChanged)
                    {
                        isChanged = (game.Name != gameSpeedRunComView.Name
                                     || game.IsRomHack != gameSpeedRunComView.IsRomHack
                                     || game.YearOfRelease != gameSpeedRunComView.YearOfRelease);

                        if (isChanged)
                        {
                            changeReasons.Add("Name, IsRomHack or YearOfRelease changed");
                        }
                    }

                    if (!isChanged)
                    {
                        var gameLink = gameLinks.FirstOrDefault(i => i.GameSpeedRunComID == gameSpeedRunComView.SpeedRunComID);
                        isChanged = gameLink?.CoverImageUrl != gameSpeedRunComView.CoverImageUrl;

                        if (isChanged)
                        {
                            changeReasons.Add("CoverImageUrl changed");
                        }
                    }

                    if (!isChanged)
                    {
                        isChanged = (categoryIDs.Except(gameSpeedRunComView.CategorySpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.CategorySpeedRunComIDArray.Except(categoryIDs).Any());

                        if (isChanged)
                        {
                            GameIDsToUpdateSpeedRuns.Add(game.ID);
                            changeReasons.Add("Categories changed");
                        }
                    }

                    if (!isChanged)
                    {
                        isChanged = (levelIDs.Except(gameSpeedRunComView.LevelSpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.LevelSpeedRunComIDArray.Except(levelIDs).Any());

                        if (isChanged)
                        {
                            GameIDsToUpdateSpeedRuns.Add(game.ID);
                            changeReasons.Add("Levels changed");
                        }
                    }

                    if (!isChanged)
                    {
                        isChanged = (variableIDs.Except(gameSpeedRunComView.VariableSpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.VariableSpeedRunComIDArray.Except(variableIDs).Any());

                        if (isChanged)
                        {
                            GameIDsToUpdateSpeedRuns.Add(game.ID);
                            changeReasons.Add("Variables changed");
                        }
                    }

                    if (!isChanged)
                    {
                        var variableIndex = 0;
                        foreach (var variableID in variableIDs)
                        {
                            isChanged = (variableID != gameSpeedRunComView.VariableSpeedRunComIDArray[variableIndex]);

                            if (isChanged)
                            {
                                isVariablesOrderChanged = true;
                                GameIDsToUpdateSpeedRuns.Add(game.ID);
                                changeReasons.Add("Variables order changed");
                                break;
                            }

                            variableIndex++;
                        }
                    }

                    if (!isChanged)
                    {
                        isChanged = (variableValueIDs.Except(gameSpeedRunComView.VariableValueSpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.VariableValueSpeedRunComIDArray.Except(variableValueIDs).Any());

                        if (isChanged)
                        {
                            GameIDsToUpdateSpeedRuns.Add(game.ID);
                            changeReasons.Add("VariableValues changed");
                        }
                    }

                    if (!isChanged)
                    {
                        isChanged = (platformIDs.Except(gameSpeedRunComView.PlatformSpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.PlatformSpeedRunComIDArray.Except(platformIDs).Any());

                        if (isChanged)
                        {
                            changeReasons.Add("Platforms changed");
                        }
                    }

                    if (!isChanged)
                    {
                        isChanged = (moderatorUserIDs.Except(gameSpeedRunComView.ModeratorSpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.ModeratorSpeedRunComIDArray.Except(moderatorUserIDs).Any());

                        if (isChanged)
                        {
                            changeReasons.Add("Moderators changed");
                        }
                    }

                    game.IsChanged = isChanged;
                    game.IsVariablesOrderChanged = isVariablesOrderChanged;

                    if (game.IsChanged.Value)
                    {
                        _logger.Information("GameID: {@GameID}, ChangeReason: {@ChangeReason}", game.ID, string.Join("; ", changeReasons));
                    }
                }
            }

            _logger.Information("Completed SetChangedGames");
        }
    }
}


