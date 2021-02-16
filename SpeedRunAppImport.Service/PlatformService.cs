using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Client;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using System.Threading;
using Serilog;
using SpeedRunCommon;

namespace SpeedRunAppImport.Service
{
    public class PlatformService : BaseService, IPlatformService
    {
        private readonly ISettingService _settingService = null;
        private readonly IPlatformRepository _platformRepo = null;
        private readonly ILogger _logger;

        public PlatformService(ISettingService settingService, IPlatformRepository platformRepo, ILogger logger)
        {
            _settingService = settingService;
            _platformRepo = platformRepo;
            _logger = logger;
        }

        public void ProcessPlatforms(bool isBulkReload)
        {
            try
            {
                _logger.Information("Started ProcessPlatforms: {@IsBulkReload}", isBulkReload);
                var orderBy = PlatformsOrdering.YearOfRelease;
                var results = new List<Platform>();
                var platforms = new List<Platform>();
                var prevTotal = 0;

                do
                {
                    platforms = GetPlatformsWithRetry(MaxElementsPerPage, results.Count + prevTotal, orderBy);
                    results.AddRange(platforms);
                    _logger.Information("Pulled platforms: {@New}, total platforms: {@Total}", platforms.Count, results.Count + prevTotal);
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        prevTotal += results.Count;
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SavePlatforms(results, isBulkReload);
                        results.ClearMemory();
                    }
                }
                while (platforms.Count == MaxElementsPerPage);

                if (results.Any())
                {
                    SavePlatforms(results, isBulkReload);
                    results.ClearMemory();
                }

                _settingService.UpdateSetting("PlatformLastImportDate", DateTime.Now);
                _logger.Information("Completed ProcessPlatforms");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessPlatforms");
            }
        }

        public List<Platform> GetPlatformsWithRetry(int elementsPerPage, int elementsOffset, PlatformsOrdering orderBy, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            List<Platform> platforms = null;
            try
            {
                platforms = clientContainer.Platforms.GetPlatforms(elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, orderBy: orderBy).ToList();
            }
            catch (Exception ex)
            {
                if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    retryCount++;
                    _logger.Information("Retrying pull platforms: {@New}, total platforms: {@Total}, retry: {@RetryCount}", elementsPerPage, elementsOffset, retryCount);
                    platforms = GetPlatformsWithRetry(elementsPerPage, elementsOffset, orderBy, retryCount);
                }
                else
                {
                    throw ex;
                }
            }

            return platforms;
        }

        public void SavePlatforms(IEnumerable<Platform> platforms, bool isBulkReload)
        {
            _logger.Information("Started SavePlatforms: {@Count}, {@IsBulkReload}", platforms.Count(), isBulkReload);

            var platformEntities = platforms.Select(i => new PlatformEntity { SpeedRunComID = i.ID, Name = i.Name, YearOfRelease = i.YearOfRelease }).ToList();
            
            if (!isBulkReload)
            {
                var platformSpeedRunComIDs = _platformRepo.GetPlatformSpeedRunComIDs();
                platformEntities = platformEntities.Where(i => !platformSpeedRunComIDs.Any(g => g.SpeedRunComID == i.SpeedRunComID)).ToList();
            }

            if (platformEntities.Any())
            {
                _platformRepo.InsertPlatforms(platformEntities);
            }

            _logger.Information("Completed SavePlatforms");
        }
    }
}


