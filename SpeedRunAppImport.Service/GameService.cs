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
using Microsoft.Extensions.Configuration;
using System.Threading;
using SpeedRunCommon;

namespace SpeedRunAppImport.Service
{
    public class GameService : BaseService, IGameService
    {
        private readonly ISettingService _settingService = null;
        private readonly IGameRepository _gameRepo = null;
        private readonly IConfiguration _config = null;
        private readonly ILogger _logger;

        public GameService(ISettingService settingService, IGameRepository gameRepo, IConfiguration config, ILogger logger)
        {
            _settingService = settingService;
            _gameRepo = gameRepo;
            _config = config;
            _logger = logger;
        }

        public void ProcessGames(DateTime lastImportDate, bool isFullImport)
        {
            try
            {
                _logger.Information("Started ProcessGames: {@LastImportDate}, {@IsFullImport}", lastImportDate, isFullImport);
                var newImportDate = DateTime.UtcNow;
                GamesOrdering orderBy = isFullImport ? GamesOrdering.CreationDate : GamesOrdering.CreationDateDescending;
                var gameEmbeds = new GameEmbeds { EmbedCategories = true, EmbedLevels = true, EmbedModerators = false, EmbedPlatforms = false, EmbedVariables = true };
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
                        prevTotal = results.Count;
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveGames(results.OrderBy(i => i.CreationDate), isFullImport);
                        results.ClearMemory();
                    }
                }
                while (games.Count == MaxElementsPerPage && games.Min(i => i.CreationDate ?? SqlMinDateTime) >= lastImportDate);

                if (results.Any())
                {
                    if (!isFullImport)
                    {
                        results.RemoveAll(i => (i.CreationDate ?? SqlMinDateTime) < lastImportDate);
                    }

                    SaveGames(results, isFullImport);
                    results.ClearMemory();
                }

                _settingService.UpdateSetting("GameLastImportDate", newImportDate);
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
            var gameEntities = games.Select(i => i.ConvertToEntity()).ToList();
            var levelEntities = games.SelectMany(i => i.Levels.Select(i => i.ConvertToEntity())).ToList();
            var categoryEntities = games.SelectMany(i => i.Categories.Select(i => i.ConvertToEntity())).ToList();
            var variableEntities = games.SelectMany(i => i.Variables.Select(i => i.ConvertToEntity())).ToList();
            var variableValueEntities = games.SelectMany(i => i.Variables.SelectMany(g => g.Values.Select(h => new VariableValueEntity { ID = h.ID, GameID = g.GameID, VariableID = h.VariableID, Value = h.Value, IsCustomValue = h.IsCustomValue }))).ToList();
            var gamePlatformEntities = games.SelectMany(i => i.PlatformIDs.Select(g => new GamePlatformEntity { GameID = i.ID, PlatformID = g })).ToList();
            var gameRegionEntities = games.SelectMany(i => i.RegionIDs.Select(g => new GameRegionEntity { GameID = i.ID, RegionID = g })).ToList();
            var gameModeratorEntities = games.SelectMany(i => i.Moderators.Select(g => new GameModeratorEntity { GameID = i.ID, UserID = g.UserID })).ToList();
            var gameRulesetEntities = games.Select(i => new GameRulesetEntity { GameID = i.ID, ShowMilliseconds = i.Ruleset.ShowMilliseconds, RequiresVerification = i.Ruleset.RequiresVerification, RequiresVideo = i.Ruleset.RequiresVideo, DefaultTimingMethodID = (int)i.Ruleset.DefaultTimingMethod, EmulatorsAllowed = i.Ruleset.EmulatorsAllowed }).ToList();
            var gameTimingMethodEntities = games.SelectMany(i => i.Ruleset.TimingMethods.Select(g => new GameTimingMethodEntity { GameID = i.ID, TimingMethodID = (int)g })).ToList();

            SaveGames(gameEntities, levelEntities, categoryEntities, variableEntities, variableValueEntities, gamePlatformEntities, gameRegionEntities, gameModeratorEntities, gameRulesetEntities, gameTimingMethodEntities, isFullImport);
        }

        public void SaveGames(IEnumerable<GameEntity> games, IEnumerable<LevelEntity> levels, IEnumerable<CategoryEntity> categories, IEnumerable<VariableEntity> variables, IEnumerable<VariableValueEntity> variableValues, IEnumerable<GamePlatformEntity> gamePlatforms, IEnumerable<GameRegionEntity> gameRegions, IEnumerable<GameModeratorEntity> gameModerators, IEnumerable<GameRulesetEntity> gameRulesets, IEnumerable<GameTimingMethodEntity> gameTimingMethods, bool isFullImport)
        {
            if (isFullImport)
            {
                _gameRepo.CopyGameTables();
                _gameRepo.InsertGames(games, levels, categories, variables, variableValues, gamePlatforms, gameRegions, gameModerators, gameRulesets, gameTimingMethods);
                _gameRepo.RenameAndDropGameTables();
            }
            else
            {
                _gameRepo.SaveGames(games, levels, categories, variables, variableValues, gamePlatforms, gameRegions, gameModerators, gameRulesets, gameTimingMethods);
            }
        }
    }
}


