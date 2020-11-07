using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunApp.Client;
using SpeedRunApp.Model;
using SpeedRunApp.Model.Data;
using SpeedRunApp.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Serilog;

namespace SpeedRunAppImport.Service
{
    public class PlatformService : BaseService, IPlatformService
    {
        private readonly ILogger _logger;

        public PlatformService(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<Platform> GetAllPlatforms()
        {
            _logger.Information("Started GetAllPlatforms");
            var results = new List<Platform>();
            List<Platform> platforms = null;

            do
            {
                platforms = GetPlatformsWithRetry(MaxElementsPerPage, results.Count, PlatformsOrdering.YearOfRelease).ToList();
                results.AddRange(platforms);
                _logger.Information("Pulled platforms: {@New}, total platforms: {@Total}", platforms.Count, results.Count);
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
            }
            while (platforms.Count == MaxElementsPerPage);

            _logger.Information("Completed GetAllPlatforms");
            return results;
        }

        private IEnumerable<Platform> GetPlatformsWithRetry(int elementsPerPage, int elementsOffset, PlatformsOrdering orderBy, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            IEnumerable<Platform> platforms = null;
            try
            {
                platforms = clientContainer.Platforms.GetPlatforms(elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, orderBy: orderBy);
            }
            catch (Exception ex)
            {
                if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    GetPlatformsWithRetry(elementsPerPage, elementsOffset, orderBy, retryCount++);
                }
                else
                {
                    throw ex;
                }
            }

            return platforms;
        }

    }
}


