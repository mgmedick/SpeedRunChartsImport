using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface IPlatformService
    {
        void ProcessPlatforms(bool isFullImport);
        List<Platform> GetPlatformsWithRetry(int elementsPerPage, int elementsOffset, PlatformsOrdering orderBy, int retryCount = 0);
        void SavePlatforms(IEnumerable<Platform> platforms, bool isFullImport);
        void SavePlatforms(IEnumerable<PlatformEntity> platformsEntities, bool isFullImport);
    }
} 




