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
    public class SpeedRunService : BaseService, ISpeedRunService
    {
        private readonly ISpeedRunRepository _speedRunRepo = null;
        private readonly IConfiguration _config = null;
        private readonly ILogger _logger;

        public SpeedRunService(ISpeedRunRepository speedRunRepo, IConfiguration config, ILogger logger)
        {
            _speedRunRepo = speedRunRepo;
            _config = config;
            _logger = logger;
        }

        public IEnumerable<SpeedRun> GetSpeedRuns(DateTime lastImportDate, bool isFullImport, RunStatusType? statusType = null)
        {
            _logger.Information("Started GetSpeedRuns: {@lastImportDate}, {@isFullImport}, {@statusType}", lastImportDate, isFullImport, statusType);
            var results = new List<SpeedRun>();           
            List<SpeedRun> runs = null;
            RunsOrdering orderBy;
            if (isFullImport)
            {
                orderBy = RunsOrdering.DateSubmitted;
            }
            else
            {
                orderBy = (statusType == RunStatusType.Verified) ? RunsOrdering.VerifyDateDescending : RunsOrdering.DateSubmittedDescending;
            }

            do
            {
                runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count, orderBy, statusType).ToList();
                //runs = GetSpeedRunsWithRetry(MaxElementsPerPage, 62400, orderBy, statusType).ToList();
                results.AddRange(runs);
                _logger.Information("Pulled speedRuns: {@New}, total speedRuns: {@Total}", runs.Count, results.Count);
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
            }
            while (runs.Count == MaxElementsPerPage && ((statusType == RunStatusType.Verified && runs.Min(i => i.VerifyDate ?? DateTime.MinValue) >= lastImportDate) || (runs.Min(i => i.DateSubmitted ?? DateTime.MinValue) >= lastImportDate)));

            if (!isFullImport)
            {
                if (statusType == RunStatusType.Verified)
                {
                    var runIDsToRemove = runs.Where(i => (i.VerifyDate ?? DateTime.MinValue) < lastImportDate).Select(i => i.ID).ToList();
                    results.RemoveAll(i => runIDsToRemove.Contains(i.ID));
                }
                else
                {
                    var runIDsToRemove = runs.Where(i => (i.DateSubmitted ?? DateTime.MinValue) < lastImportDate).Select(i => i.ID).ToList();
                    results.RemoveAll(i => runIDsToRemove.Contains(i.ID));
                }
            }

            _logger.Information("Completed GetSpeedRuns");
            return results.OrderBy(i => i.DateSubmitted);
        }

        private IEnumerable<SpeedRun> GetSpeedRunsWithRetry(int elementsPerPage, int elementsOffset, RunsOrdering orderBy, RunStatusType? statusType = null, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            IEnumerable<SpeedRun> runs = null;
            try
            {
                runs = clientContainer.Runs.GetRuns(status: statusType, elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, orderBy: orderBy);
            }
            catch (Exception ex)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));

                if (retryCount <= MaxRetryCount)
                {
                    GetSpeedRunsWithRetry(elementsPerPage, elementsOffset, orderBy, statusType, retryCount++);
                }
                else
                {
                    _logger.Error(ex, "GetSpeedRunsWithRetry");
                    GetSpeedRunsWithRetry(elementsPerPage, elementsOffset + MaxElementsPerPage, orderBy, statusType, retryCount++);
                }
            }

            return runs;
        }
    }
}


