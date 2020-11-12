using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface IPlatformRepository
    {
        void InsertPlatforms(IEnumerable<PlatformEntity> users);
        void CopyPlatformTables();
        void RenameAndDropPlatformTables();
        IEnumerable<string> GetAllPlatformIDs();
    }
}






