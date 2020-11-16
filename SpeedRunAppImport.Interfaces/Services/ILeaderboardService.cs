using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface ILeaderboardService
    {
        void ProcessLeaderboards(DateTime lastImportDate, bool isFullImport);
        Leaderboard GetLeaderboardWithRetry(string gameID, string categoryID, string levelID = null, int retryCount = 0);
        void SaveLeaderboards(IEnumerable<Leaderboard> leaderboards, bool isFullImport);
        void SaveLeaderboards(IEnumerable<LeaderboardEntity> leaderboardEntities, bool isFullImport);
    }
} 




