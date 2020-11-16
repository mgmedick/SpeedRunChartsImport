using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface ILeaderboardRepository
    {
        void CopyLeaderboardTables();
        void RenameAndDropLeaderboardTables();
        IEnumerable<LeaderboardKeyEntity> GetLeaderboardKeys(DateTime lastImportedDate, int statusID);
        void InsertLeaderboards(IEnumerable<LeaderboardEntity> leaderboards);
        void SaveLeaderboards(IEnumerable<LeaderboardEntity> leaderboards);
    }
}






