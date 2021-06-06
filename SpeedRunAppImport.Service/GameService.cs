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
                GamesOrdering orderBy = isFullPull ? GamesOrdering.CreationDate : GamesOrdering.CreationDateDescending;
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

                _logger.Information("Completed ProcessGames");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "ProcessGames");
            }

            return result;
        }

        public List<Game> GetGamesWithRetry(int elementsPerPage, int elementsOffset, GameEmbeds embeds, GamesOrdering orderBy, int retryCount = 0)
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

            games = games.OrderBy(i => i.CreationDate).ToList();
            var gameIDs = games.Select(i => i.ID).ToList();
            var gameSpeedRunComIDs = _gameRepo.GetGameSpeedRunComIDs();
            gameSpeedRunComIDs = gameSpeedRunComIDs.Join(gameIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();

            if (!isBulkReload)
            {
                var newGames = games.Where(i => !gameSpeedRunComIDs.Any(g => g.SpeedRunComID == i.ID)).ToList();
                var changedGames = GetChangedGames(games);
                games = newGames.Concat(changedGames);
                _logger.Information("Found NewGames: {@New}, ChangedGames: {@Changed}, TotalGames: {@Total}", newGames.Count(), changedGames.Count(), games.Count());
            }

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
                                  .SelectMany(i => i.ModeratorUsers.Where(i => !userSpeedRunComIDs.Any(g => g.SpeedRunComID == i.ID)))
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
                ID = levelSpeedRunComIDs.Where(g => g.SpeedRunComID == i.ID).Select(g => g.LevelID).FirstOrDefault(),
                SpeedRunComID = g.ID,
                Name = g.Name,
                GameSpeedRunComID = i.ID
            })).ToList();
            var levelRuleEntities = games.SelectMany(i => i.Levels.Select(g => new LevelRuleEntity
            {
                LevelSpeedRunComID = g.ID,
                Rules = g.Rules
            })).ToList();
            var categoryEntities = games.SelectMany(i => i.Categories.Select(g => new CategoryEntity
            {
                ID = categorySpeedRunComIDs.Where(g => g.SpeedRunComID == i.ID).Select(g => g.CategoryID).FirstOrDefault(),
                SpeedRunComID = g.ID,
                Name = g.Name,
                GameSpeedRunComID = i.ID,
                CategoryTypeID = (int)g.Type
            })).ToList();
            var categoryRuleEntities = games.SelectMany(i => i.Categories.Select(g => new CategoryRuleEntity
            {
                CategorySpeedRunComID = g.ID,
                Rules = g.Rules
            })).ToList();
            var variableEntities = games.SelectMany(i => i.Variables.Select(g => new VariableEntity
            {
                ID = variableSpeedRunComIDs.Where(g => g.SpeedRunComID == i.ID).Select(g => g.VariableID).FirstOrDefault(),
                SpeedRunComID = g.ID,
                Name = g.Name,
                VariableScopeTypeID = (int)g.Scope.Type,
                GameSpeedRunComID = i.ID,
                CategorySpeedRunComID = g.CategoryID,
                LevelSpeedRunComID = g.Scope.LevelID,
                IsSubCategory = g.IsSubCategory
            })).ToList();
            var variableValueEntities = games.SelectMany(i => i.Variables.SelectMany(g => g.Values.Select(h => new VariableValueEntity
            {
                ID = variableValueSpeedRunComIDs.Where(g => g.SpeedRunComID == i.ID).Select(g => g.VariableValueID).FirstOrDefault(),
                SpeedRunComID = h.ID,
                GameSpeedRunComID = g.GameID,
                VariableSpeedRunComID = h.VariableID,
                Value = h.Value,
                IsCustomValue = h.IsCustomValue
            }))).ToList();
            var gamePlatformEntities = games.SelectMany(i => i.PlatformIDs.Select(g => new GamePlatformEntity
            {
                GameSpeedRunComID = i.ID,
                PlatformID = platformSpeedRunComIDs.Where(h => h.SpeedRunComID == g).Select(h => h.PlatformID).FirstOrDefault()
            }))
            .Where(i => i.PlatformID != 0)
            .ToList();
            var gameRegionEntities = games.SelectMany(i => i.RegionIDs.Select(g => new GameRegionEntity
            {
                GameSpeedRunComID = i.ID,
                RegionID = regionSpeedRunComIDs.Where(h => h.SpeedRunComID == g).Select(h => h.RegionID).FirstOrDefault()
            }))
            .Where(i => i.RegionID != 0)
            .ToList();
            var gameModeratorEntities = games.SelectMany(i => i.ModeratorUsers.Select(g => new GameModeratorEntity
            {
                GameSpeedRunComID = i.ID,
                UserID = userSpeedRunComIDs.Where(h => h.SpeedRunComID == g.ID).Select(h => h.UserID).FirstOrDefault()
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
                _gameRepo.SaveGames(gameEntities, gameLinkEntities, levelEntities, levelRuleEntities, categoryEntities, categoryRuleEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameRegionEntities, gameModeratorEntities, gameRulesetEntities, gameTimingMethodEntities);
            }

            _logger.Information("Completed SaveGames");
        }

        public IEnumerable<Game> GetChangedGames(IEnumerable<Game> games)
        {
            var changedGames = new List<Game>();
            var gameIDs = games.Select(i => i.ID).ToList();
            var gameSpeedRunComViews = _gameRepo.GetGameSpeedRunComViews();
            bool isChanged;

            foreach (var game in games)
            {
                isChanged = false;
                var gameSpeedRunComView = gameSpeedRunComViews.FirstOrDefault(i => i.SpeedRunComID == game.ID);

                if (gameSpeedRunComView != null)
                {
                    isChanged = (game.Name != gameSpeedRunComView.Name
                                 || game.IsRomHack != gameSpeedRunComView.IsRomHack
                                 || game.YearOfRelease != gameSpeedRunComView.YearOfRelease
                                 || game.Assets?.CoverLarge?.Uri.ToString() != gameSpeedRunComView.CoverImageUrl);

                    if (!isChanged)
                    {
                        isChanged = (game.Categories.Select(h => h.ID).Except(gameSpeedRunComView.CategorySpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.CategorySpeedRunComIDArray.Except(game.Categories.Select(h => h.ID)).Any());
                    }

                    if (!isChanged)
                    {
                        isChanged = (game.Levels.Select(h => h.ID).Except(gameSpeedRunComView.LevelSpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.LevelSpeedRunComIDArray.Except(game.Levels.Select(h => h.ID)).Any());
                    }

                    if (!isChanged)
                    {
                        isChanged = (game.Variables.Select(h => h.ID).Except(gameSpeedRunComView.VariableSpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.VariableSpeedRunComIDArray.Except(game.Variables.Select(h => h.ID)).Any());
                    }

                    if (!isChanged)
                    {
                        isChanged = (game.Variables.SelectMany(h => h.Values.Select(n => n.ID)).Except(gameSpeedRunComView.VariableValueSpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.VariableValueSpeedRunComIDArray.Except(game.Variables.SelectMany(h => h.Values.Select(n => n.ID))).Any());
                    }

                    if (!isChanged)
                    {
                        isChanged = (game.PlatformIDs.Except(gameSpeedRunComView.PlatformSpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.PlatformSpeedRunComIDArray.Except(game.PlatformIDs.Select(h => h)).Any());
                    }

                    if (!isChanged)
                    {
                        isChanged = (game.ModeratorUsers.Select(h => h.ID).Except(gameSpeedRunComView.ModeratorSpeedRunComIDArray).Any()
                                     || gameSpeedRunComView.ModeratorSpeedRunComIDArray.Except(game.ModeratorUsers.Select(h => h.ID)).Any());
                    }

                    if (isChanged)
                    {
                        changedGames.Add(game);
                    }
                }
            }

            return changedGames;
        }
    }
}


