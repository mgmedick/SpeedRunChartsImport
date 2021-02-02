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

        public void ProcessGames(DateTime lastImportDate, bool isFullImport)
        {
            try
            {
                var lastImportDateUtc = lastImportDate.ToUniversalTime();
                _logger.Information("Started ProcessGames: {@LastImportDate}, {@LastImportDateUtc}, {@IsFullImport}", lastImportDate, lastImportDateUtc, isFullImport);

                GamesOrdering orderBy = isFullImport ? GamesOrdering.CreationDate : GamesOrdering.CreationDateDescending;
                var gameEmbeds = new GameEmbeds { EmbedCategories = true, EmbedLevels = true, EmbedModerators = false, EmbedPlatforms = false, EmbedVariables = true };
                var results = new List<Game>();
                var games = new List<Game>();
                var prevTotal = 0;

                if (isFullImport)
                {
                    _gameRepo.CopyGameTables();
                }

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
                        SaveGames(results, isFullImport);
                        results.ClearMemory();
                    }
                }
                while (games.Count == MaxElementsPerPage && games.Min(i => i.CreationDate ?? SqlMinDateTime) >= lastImportDateUtc);

                if (results.Any())
                {
                    if (!isFullImport)
                    {
                        results.RemoveAll(i => (i.CreationDate ?? SqlMinDateTime) < lastImportDateUtc);
                    }

                    SaveGames(results, isFullImport);
                    results.ClearMemory();
                }

                if (isFullImport)
                {
                    _gameRepo.RenameAndDropGameTables();
                }

                _settingService.UpdateSetting("GameLastImportDate", DateTime.Now);
                _logger.Information("Completed ProcessGames");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessGames");
            }
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

        public void SaveGames(IEnumerable<Game> games, bool isFullImport)
        {
            var platformSpeedRunComIDs = _platformRepo.GetPlatformSpeedRunComIDs();
            var regionSpeedRunComIDs = _gameRepo.GetRegionSpeedRunComIDs();
            var userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs();

            var gameEntities = games.Select(i => new GameEntity() { SpeedRunComID = i.ID, Name = i.Name, YearOfRelease = i.YearOfRelease, IsRomHack = i.IsRomHack, CreatedDate = i.CreationDate }).ToList();
            var levelEntities = games.SelectMany(i => i.Levels.Select(g => new LevelEntity { SpeedRunComID = g.ID, Name = g.Name, GameSpeedRunComID = i.ID })).ToList();
            var levelRuleEntities = games.SelectMany(i => i.Levels.Select(g => new LevelRuleEntity { LevelSpeedRunComID = g.ID, Rules = g.Rules })).ToList();
            var categoryEntities = games.SelectMany(i => i.Categories.Select(g => new CategoryEntity { SpeedRunComID = g.ID, Name = g.Name, GameSpeedRunComID = i.ID, CategoryTypeID = (int)g.Type } )).ToList();
            var categoryRuleEntities = games.SelectMany(i => i.Categories.Select(g => new CategoryRuleEntity { CategorySpeedRunComID = g.ID, Rules = g.Rules })).ToList();
            var variableEntities = games.SelectMany(i => i.Variables.Select(g => new VariableEntity { SpeedRunComID = g.ID, Name= g.Name, GameSpeedRunComID = i.ID, CategorySpeedRunComID = g.CategoryID, LevelSpeedRunComID = g.Scope.LevelID, IsSubCategory = g.IsSubCategory })).ToList();
            var variableValueEntities = games.SelectMany(i => i.Variables.SelectMany(g => g.Values.Select(h => new VariableValueEntity { SpeedRunComID = h.ID, GameSpeedRunComID = g.GameID, VariableSpeedRunComID = h.VariableID, Value = h.Value, IsCustomValue = h.IsCustomValue }))).ToList();
            var gamePlatformEntities = games.SelectMany(i => i.PlatformIDs.Select(g => new GamePlatformEntity { GameSpeedRunComID = i.ID, PlatformID = platformSpeedRunComIDs.Where(h => h.SpeedRunComID == g).Select(h => h.PlatformID).FirstOrDefault() })).ToList();
            var gameRegionEntities = games.SelectMany(i => i.RegionIDs.Select(g => new GameRegionEntity { GameSpeedRunComID = i.ID, RegionID = regionSpeedRunComIDs.Where(h => h.SpeedRunComID == g).Select(h => h.RegionID).FirstOrDefault() })).ToList();
            var gameModeratorEntities = games.SelectMany(i => i.Moderators.Select(g => new GameModeratorEntity { GameSpeedRunComID = i.ID, UserID = userSpeedRunComIDs.Where(h => h.SpeedRunComID == g.UserID).Select(h => h.UserID).FirstOrDefault() })).ToList();            
            var gameRulesetEntities = games.Select(i => new GameRulesetEntity { GameSpeedRunComID = i.ID, ShowMilliseconds = i.Ruleset.ShowMilliseconds, RequiresVerification = i.Ruleset.RequiresVerification, RequiresVideo = i.Ruleset.RequiresVideo, DefaultTimingMethodID = (int)i.Ruleset.DefaultTimingMethod, EmulatorsAllowed = i.Ruleset.EmulatorsAllowed }).ToList();
            var gameTimingMethodEntities = games.SelectMany(i => i.Ruleset.TimingMethods.Select(g => new GameTimingMethodEntity { GameSpeedRunComID = i.ID, TimingMethodID = (int)g })).ToList();

            SaveGames(gameEntities, levelEntities, categoryEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameRegionEntities, gameModeratorEntities, gameRulesetEntities, gameTimingMethodEntities, isFullImport);
        }

        public void SaveGames(IEnumerable<GameEntity> games, IEnumerable<LevelEntity> levels, IEnumerable<LevelRuleEntity> levelRules, IEnumerable<CategoryEntity> categories, IEnumerable<CategoryRuleEntity> CategoryRules, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods, bool isFullImport)
        {
            if (isFullImport)
            {
                _gameRepo.InsertGames(games, levels, levelRules, categories, CategoryRules, variables, variableValues, gamePlatforms, gameRegions, gameModerators, gameRulesets, gameTimingMethods);
            }
            else
            {
                _gameRepo.SaveGames(games, levels, categories, variables, variableValues, gamePlatforms, gameRegions, gameModerators, gameRulesets, gameTimingMethods);
            }
        }
    }
}


