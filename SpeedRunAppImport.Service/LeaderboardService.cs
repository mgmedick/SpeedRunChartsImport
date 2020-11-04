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

        public IEnumerable<Leaderboard> GetLeaderboards(IEnumerable<GameView> games)
        {
            var results = new List<Leaderboard>();

            foreach (var game in games)
            {
                foreach (var category in game.Categories)
                {
                    if (category.CategoryTypeID == (int)CategoryType.PerLevel)
                    {
                        foreach (var level in game.Levels)
                        {
                            var leaderboard = GetLeaderboardWithRetry(game.ID, category.ID, level.ID);
                            results.Add(leaderboard);
                        }
                    }
                    else
                    {
                        var leaderboard = GetLeaderboardWithRetry(game.ID, category.ID);
                        results.Add(leaderboard);
                    }
                }
            }

            return results;
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


