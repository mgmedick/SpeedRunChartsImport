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

        public bool ProcessPlatforms(bool isFullPull, bool isBulkReload)
        {
            bool result = true;

            try
            {
                _logger.Information("Started ProcessPlatforms: {@IsFullPull}, {@IsBulkReload}", isFullPull, isBulkReload);
                var orderBy = PlatformsOrdering.YearOfRelease;
                var results = new List<Platform>();
                var platforms = new List<Platform>();
                var prevTotal = 0;
                var speedRunComIDs = _platformRepo.GetPlatformSpeedRunComIDs().Select(i => i.SpeedRunComID).ToList();

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

                if (!isFullPull)
                {
                    results.RemoveAll(i => speedRunComIDs.Contains(i.ID));
                }

                if (results.Any())
                {
                    SavePlatforms(results, isBulkReload);
                    results.ClearMemory();
                }

                _settingService.UpdateSetting("PlatformLastImportDate", DateTime.UtcNow);
                _logger.Information("Completed ProcessPlatforms");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "ProcessPlatforms");
            }

            return result;
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
            
            var platformIDs = platforms.Select(i => i.ID).ToList();
            var platformSpeedRunComIDs = _platformRepo.GetPlatformSpeedRunComIDs().Where(i => platformIDs.Contains(i.SpeedRunComID)).ToList();

            //TODO: if !isBulkReload narrow to only platforms that changed.

            var platformEntities = platforms.Select(i => new PlatformEntity {
                ID = platformSpeedRunComIDs.Where(g => g.SpeedRunComID == i.ID).Select(g => g.PlatformID).FirstOrDefault(),
                SpeedRunComID = i.ID,
                Name = i.Name,
                YearOfRelease = i.YearOfRelease
            }).ToList();

            if (isBulkReload)
            {
                _platformRepo.InsertPlatforms(platformEntities);
            }
            else
            {
                _platformRepo.SavePlatforms(platformEntities);
            }

            _logger.Information("Completed SavePlatforms");
        }
    }
}


