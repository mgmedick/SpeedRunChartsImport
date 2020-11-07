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
        IEnumerable<LeaderboardEntity> GetLeaderboards(Expression<Func<LeaderboardEntity, bool>> predicate);
        void UpdateLeaderboards(IEnumerable<LeaderboardEntity> leaderboards);
    }
}






