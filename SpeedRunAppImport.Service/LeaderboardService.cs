using System;
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
using Serilog;
using SpeedRunCommon;

namespace SpeedRunAppImport.Service
{
    public class LeaderboardService : BaseService, ILeaderboardService
    {
        private readonly ISettingService _settingService = null;
        private readonly ILeaderboardRepository _leaderboardRepo = null;
        private readonly IConfiguration _config = null;
        private readonly ILogger _logger;

        public LeaderboardService(ISettingService settingService, ILeaderboardRepository leaderboardRepo, IConfiguration config, ILogger logger)
        {
            _settingService = settingService;
            _leaderboardRepo = leaderboardRepo;
            _config = config;
            _logger = logger;
        }

        public void ProcessLeaderboards(DateTime lastImportDate, bool isFullImport)
        {
            try
            {
                _logger.Information("Started ProcessLeaderboards: {@LastImportDate}, {@IsFullImport}", lastImportDate, isFullImport);
                var newImportDate = DateTime.UtcNow;
                var leaderboardKeys = _leaderboardRepo.GetLeaderboardKeys(lastImportDate, (int)RunStatusType.Verified).ToList();
                List<Leaderboard> results = new List<Leaderboard>();
                var prevTotal = 0;

                foreach (var leaderboardKey in leaderboardKeys)
                {
                    var leaderboard = GetLeaderboardWithRetry(leaderboardKey.GameID, leaderboardKey.CategoryID, leaderboardKey.LevelID);
                    results.Add(leaderboard);
                    _logger.Information("Pulled {@Count} / {@Total} leaderboards", results.Count + prevTotal, leaderboardKeys.Count);
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        prevTotal += results.Count;
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveLeaderboards(results, isFullImport);
                        results.ClearMemory();
                    }
                }

                if (results.Any())
                {
                    SaveLeaderboards(results, isFullImport);
                    results.ClearMemory();
                }

                _settingService.UpdateSetting("LeaderboardLastImportDate", newImportDate);
                _logger.Information("Completed ProcessLeaderboards");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessLeaderboards");
            }
        }

        public Leaderboard GetLeaderboardWithRetry(string gameID, string categoryID, string levelID = null, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            Leaderboard leaderboard = null;
            try
            {
                if(string.IsNullOrWhiteSpace(levelID))
                {
                    leaderboard = clientContainer.Leaderboards.GetLeaderboardForFullGameCategory(gameId: gameID, categoryId: categoryID);
                }
                else
                {
                    leaderboard = clientContainer.Leaderboards.GetLeaderboardForLevel(gameId: gameID, categoryId: categoryID, levelId: levelID);
                }
            }
            catch (Exception ex)
            {
                if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    retryCount++;
                    _logger.Information("Retrying pull leaderboard, gameID: {@GameID}, categoryID: {@CategoryID}, levelID: {@LevelID}, retry: {@RetryCount}", gameID, categoryID, levelID, retryCount);
                    leaderboard = GetLeaderboardWithRetry(gameID, categoryID, levelID, retryCount);
                }
                else
                {
                    throw ex;
                }
            }

            return leaderboard;
        }

        public void SaveLeaderboards(IEnumerable<Leaderboard> leaderboards, bool isFullImport)
        {
            var leaderboardEntities = leaderboards.SelectMany(i => i.Records.Select(g => new LeaderboardEntity { GameID = i.GameID, CategoryID = i.CategoryID, LevelID = i.LevelID, Rank = g.Rank, SpeedRunID = g.ID }));

            SaveLeaderboards(leaderboardEntities, isFullImport);
        }

        public void SaveLeaderboards(IEnumerable<LeaderboardEntity> leaderboardEntities, bool isFullImport)
        {
            if (isFullImport)
            {
                _leaderboardRepo.CopyLeaderboardTables();
                _leaderboardRepo.InsertLeaderboards(leaderboardEntities);
                _leaderboardRepo.RenameAndDropLeaderboardTables();
            }
            else
            {
                _leaderboardRepo.SaveLeaderboards(leaderboardEntities);
            }
        }
    }
}


