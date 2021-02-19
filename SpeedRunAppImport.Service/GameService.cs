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
        private readonly IGameRepository _gameRepo = null;
        private readonly IPlatformRepository _platformRepo = null;
        private readonly IUserRepository _userRepo = null;
        private readonly ISpeedRunRepository _speedRunRepo = null;
        private readonly ILogger _logger;

        public GameService(ISettingService settingService, IGameRepository gameRepo, IPlatformRepository platformRepo, IUserRepository userRepo, ISpeedRunRepository speedRunRepo, ILogger logger)
        {
            _settingService = settingService;
            _gameRepo = gameRepo;
            _platformRepo = platformRepo;
            _userRepo = userRepo;
            _speedRunRepo = speedRunRepo;
            _logger = logger;
        }

        public bool ProcessGames(DateTime lastImportDate, bool isFullImport, bool isBulkReload)
        {
            bool result = true;

            try
            {
                var lastImportDateUtc = lastImportDate.ToUniversalTime();
                _logger.Information("Started ProcessGames: {@LastImportDate}, {@LastImportDateUtc}, {@IsFullImport}, {@IsBulkReload}", lastImportDate, lastImportDateUtc, isFullImport, isBulkReload);

                GamesOrdering orderBy = isFullImport ? GamesOrdering.CreationDate : GamesOrdering.CreationDateDescending;
                var gameEmbeds = new GameEmbeds { EmbedCategories = true, EmbedLevels = true, EmbedModerators = false, EmbedPlatforms = false, EmbedVariables = true };
                var results = new List<Game>();
                var games = new List<Game>();
                var prevTotal = 0;

                do
                {
                    games = GetGamesWithRetry(MaxElementsPerPage, results.Count + prevTotal, gameEmbeds, orderBy);
                    //games = new List<Game> { new ClientContainer().Games.GetGame("268eg076", gameEmbeds) };
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
                while (games.Count == MaxElementsPerPage && games.Min(i => i.CreationDate ?? SqlMinDateTimeUtc) >= lastImportDateUtc);

                if (!isFullImport)
                {
                    results.RemoveAll(i => (i.CreationDate ?? SqlMinDateTimeUtc) < lastImportDateUtc);
                }

                if (results.Any())
                {
                    SaveGames(results, isBulkReload);
                    results.ClearMemory();
                }

                _settingService.UpdateSetting("GameLastImportDate", DateTime.Now);
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

            var gameIDs = games.Select(i => i.ID).ToList();
            var gameSpeedRunComIDs = _gameRepo.GetGameSpeedRunComIDs().Where(i => gameIDs.Contains(i.SpeedRunComID)).ToList();
            var userIDs = games.SelectMany(i => i.Moderators.Select(i => i.UserID)).Distinct().ToList();
            var userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs().Where(i => userIDs.Contains(i.SpeedRunComID)).ToList();
            var levelIDs = games.SelectMany(i => i.Levels.Select(i => i.ID)).Distinct().ToList();
            var levelSpeedRunComIDs = _gameRepo.GetLevelSpeedRunComIDs().Where(i => levelIDs.Contains(i.SpeedRunComID)).ToList();
            var categoryIDs = games.SelectMany(i => i.Categories.Select(i => i.ID)).Distinct().ToList();
            var categorySpeedRunComIDs = _gameRepo.GetCategorySpeedRunComIDs().Where(i => categoryIDs.Contains(i.SpeedRunComID)).ToList();
            var variableIDs = games.SelectMany(i => i.Variables.Select(i => i.ID)).Distinct().ToList();
            var variableSpeedRunComIDs = _gameRepo.GetVaraibleSpeedRunComIDs().Where(i => variableIDs.Contains(i.SpeedRunComID)).ToList();
            var variableValueIDs = games.SelectMany(i => i.Variables.SelectMany(g => g.Values.Select(h => h.ID))).Distinct().ToList();
            var variableValueSpeedRunComIDs = _gameRepo.GetVariableValueSpeedRunComIDs().Where(i => variableValueIDs.Contains(i.SpeedRunComID)).ToList();
            var platformIDs = games.SelectMany(i => i.PlatformIDs).Distinct().ToList();
            var platformSpeedRunComIDs = _platformRepo.GetPlatformSpeedRunComIDs(i => platformIDs.Contains(i.SpeedRunComID)).ToList();
            var regionIDs = games.SelectMany(i => i.RegionIDs).Distinct().ToList();
            var regionSpeedRunComIDs = _gameRepo.GetRegionSpeedRunComIDs(i => regionIDs.Contains(i.SpeedRunComID)).ToList();

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
            var gameModeratorEntities = games.SelectMany(i => i.Moderators.Select(g => new GameModeratorEntity
            {
                GameSpeedRunComID = i.ID,
                UserID = userSpeedRunComIDs.Where(h => h.SpeedRunComID == g.UserID).Select(h => h.UserID).FirstOrDefault()
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
    }
}


