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
                runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count, orderBy, statusType);
                if (runs != null)
                {
                    results.AddRange(runs);
                    _logger.Information("Pulled speedRuns: {@New}, total speedRuns: {@Total}", runs.Count, results.Count);
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
            }
            while (runs.Count == MaxElementsPerPage && ((statusType == RunStatusType.Verified && runs.Min(i => i.VerifyDate ?? SqlMinDateTime) >= lastImportDate) || (runs.Min(i => i.DateSubmitted ?? SqlMinDateTime) >= lastImportDate)));

            if (!isFullImport)
            {
                if (statusType == RunStatusType.Verified)
                {
                    var runIDsToRemove = runs.Where(i => (i.VerifyDate ?? SqlMinDateTime) < lastImportDate).Select(i => i.ID).ToList();
                    results.RemoveAll(i => runIDsToRemove.Contains(i.ID));
                }
                else
                {
                    var runIDsToRemove = runs.Where(i => (i.DateSubmitted ?? SqlMinDateTime) < lastImportDate).Select(i => i.ID).ToList();
                    results.RemoveAll(i => runIDsToRemove.Contains(i.ID));
                }
            }

            _logger.Information("Completed GetSpeedRuns");
            return results.OrderBy(i => i.DateSubmitted);
        }

        private List<SpeedRun> GetSpeedRunsWithRetry(int elementsPerPage, int elementsOffset, RunsOrdering orderBy, RunStatusType? statusType = null, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            List<SpeedRun> runs = null;
            try
            {
                runs = clientContainer.Runs.GetRuns(status: statusType, elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, orderBy: orderBy).ToList();
            }
            catch (Exception ex)
            {
                if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    retryCount++;
                    _logger.Information("Retrying pull speedRuns: {@New}, total speedRuns: {@Total}, retry: {@RetryCount}", elementsPerPage, elementsOffset, retryCount);
                    runs = GetSpeedRunsWithRetry(elementsPerPage, elementsOffset, orderBy, statusType, retryCount);
                }
                else
                {
                    throw ex;
                }
            }

            return runs;
        }
    }
}


