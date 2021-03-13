using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface IPlatformRepository
    {
        void InsertPlatforms(IEnumerable<PlatformEntity> platforms);
        void SavePlatforms(IEnumerable<PlatformEntity> platforms);
        //void CopyPlatformTables();
        //void RenameAndDropPlatformTables();
        IEnumerable<PlatformSpeedRunComIDEntity> GetPlatformSpeedRunComIDs(Expression<Func<PlatformSpeedRunComIDEntity, bool>> predicate = null);
    }
}






