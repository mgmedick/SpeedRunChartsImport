using System;
using System.Collections.Generic;
using SpeedRunApp.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface IPlatformRepository
    {
        void InsertPlatforms(IEnumerable<PlatformEntity> users);
        void CopyPlatformTables();
        void RenameAndDropPlatformTables();
    }
}






