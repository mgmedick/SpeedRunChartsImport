using System;
using System.Collections.Generic;
using SpeedRunApp.Model.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface ILeaderboardRepository
    {
        void InsertLeaderboards(IEnumerable<LeaderboardEntity> leaderboards);
        void CopyLeaderboardTables();
        void RenameAndDropLeaderboardTables();
        IEnumerable<LeaderboardKeyEntity> GetLeaderboardKeys(DateTime lastImportedDate, int statusID);
        void UpdateLeaderboards(IEnumerable<LeaderboardEntity> leaderboards);
    }
}






