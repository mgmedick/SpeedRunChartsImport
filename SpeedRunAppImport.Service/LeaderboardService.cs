using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunApp.Client;
using SpeedRunApp.Model;
using SpeedRunApp.Model.Data;
using SpeedRunApp.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Serilog;

namespace SpeedRunAppImport.Service
{
    public class LeaderboardService : BaseService, ILeaderboardService
    {
        private readonly ILogger _logger;

        public LeaderboardService(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<Leaderboard> GetLeaderboards(IEnumerable<LeaderboardKeyEntity> leaderboardKeys)
        {
            List<Leaderboard> leaderboards = new List<Leaderboard>();
            var leaderboardKeysList = leaderboardKeys.ToList();
            var keys = leaderboardKeys.Take(10);
            foreach (var leaderboardKey in leaderboardKeys.Take(10))
            {
                var leaderboard = GetLeaderboardWithRetry(leaderboardKey.GameID, leaderboardKey.CategoryID, leaderboardKey.LevelID);
                leaderboards.Add(leaderboard);
                _logger.Information("Pulled {@Count} / {@Total} leaderboards", leaderboards.Count, leaderboardKeysList.Count);
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
            }

            return leaderboards;
        }

        private Leaderboard GetLeaderboardWithRetry(string gameID, string categoryID, string levelID = null, int retryCount = 0)
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
                    GetLeaderboardWithRetry(gameID, categoryID, levelID, retryCount++);
                }
                else
                {
                    throw ex;
                }
            }

            return leaderboard;
        }
    }
}


