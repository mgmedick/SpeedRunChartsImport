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
        bool ProcessPlatforms(bool isFullImport, bool isBulkReload);
        List<Platform> GetPlatformsWithRetry(int elementsPerPage, int elementsOffset, PlatformsOrdering orderBy, int retryCount = 0);
        void SavePlatforms(IEnumerable<Platform> platforms, bool isBulkReload);
    }
} 




