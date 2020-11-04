using System;
using System.Collections.Generic;
using SpeedRunApp.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface ILeaderboardRepository
    {
        void InsertLeaderboards(IEnumerable<PlatformEntity> users);
        void CopyLeaderboardTables();
        void RenameAndDropLeaderboardTables();
    }
}






