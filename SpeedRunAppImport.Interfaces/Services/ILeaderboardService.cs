using System;
using System.Collections.Generic;
using SpeedRunApp.Model.Data;
using SpeedRunApp.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface ILeaderboardService
    {
        IEnumerable<Leaderboard> GetLeaderboards(IEnumerable<LeaderboardKeyEntity> leaderboardKeys);
    }
} 




