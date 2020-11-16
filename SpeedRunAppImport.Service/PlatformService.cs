using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Client;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Serilog;
using SpeedRunCommon;

namespace SpeedRunAppImport.Service
{
    public class PlatformService : BaseService, IPlatformService
    {
        private readonly ISettingService _settingService = null;
        private readonly IPlatformRepository _platformRepo = null;
        private readonly IConfiguration _config = null;
        private readonly ILogger _logger;

        public PlatformService(ISettingService settingService, IPlatformRepository platformRepo, IConfiguration config, ILogger logger)
        {
            _settingService = settingService;
            _platformRepo = platformRepo;
            _config = config;
            _logger = logger;
        }

        public void ProcessPlatforms(bool isFullImport)
        {
            try
            {
                _logger.Information("Started ProcessPlatforms: {@IsFullImport}", isFullImport);
                var newImportDate = DateTime.UtcNow;
                var orderBy = PlatformsOrdering.YearOfRelease;
                var platformIDs = _platformRepo.GetAllPlatformIDs().ToList();
                var results = GetPlatformsWithRetry(MaxElementsPerPage, 0, PlatformsOrdering.YearOfRelease);
                _logger.Information("Pulled platforms: {@New}, total platforms: {@Total}", results.Count, results.Count);

                while (results.Count == MaxElementsPerPage)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
                    var platforms = GetPlatformsWithRetry(MaxElementsPerPage, results.Count, orderBy);
                    results.AddRange(platforms);
                    _logger.Information("Pulled platforms: {@New}, total platforms: {@Total}", platforms.Count, results.Count);

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SavePlatforms(results, isFullImport);
                        results.ClearMemory();
                    }
                }

                if (results.Any())
                {
                    SavePlatforms(results, isFullImport);
                }

                _settingService.UpdateSetting("PlatformLastImportDate", newImportDate);
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

        public void SavePlatforms(IEnumerable<Platform> platforms, bool isFullImport)
        {
            var platformEntities = platforms.Select(i => i.ConvertToEntity()).ToList();
            SavePlatforms(platformEntities, isFullImport);
        }

        public void SavePlatforms(IEnumerable<PlatformEntity> platformsEntities, bool isFullImport)
        {
            var platformIDs = _platformRepo.GetAllPlatformIDs().ToList();
            var newPlatforms = platformsEntities.Where(i => !platformIDs.Contains(i.ID)).ToList();
            if (isFullImport)
            {
                _platformRepo.CopyPlatformTables();
                _platformRepo.InsertPlatforms(newPlatforms);
                _platformRepo.RenameAndDropPlatformTables();
            }
            else
            {
                _platformRepo.InsertPlatforms(newPlatforms);
            }
        }
    }
}


