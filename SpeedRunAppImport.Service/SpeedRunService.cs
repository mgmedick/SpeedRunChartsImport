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

namespace SpeedRunAppImport.Service
{
    public class SpeedRunService : BaseService, ISpeedRunService
    {
        private readonly IConfiguration _config = null;
        private readonly ISpeedRunRepository _speedRunRepo = null;

        public SpeedRunService(IConfiguration config, ISpeedRunRepository speedRunRepo)
        {
            _config = config;
            _speedRunRepo = speedRunRepo;
        }

        public IEnumerable<SpeedRun> GetSpeedRuns(DateTime lastImportDate, bool isFullImport, RunStatusType? statusType = null)
        {
            var results = new List<SpeedRun>();           
            List<SpeedRun> runs = null;
            var ordering = (statusType == RunStatusType.Verified) ? RunsOrdering.VerifyDateDescending : RunsOrdering.DateSubmittedDescending;

            do
            {
                runs = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count, ordering, statusType).ToList();
                results.AddRange(runs);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            //while (runs.Count == MaxElementsPerPage && ((statusType == RunStatusType.Verified && runs.Min(i => i.VerifyDate ?? DateTime.MinValue) >= lastImportDate) || (runs.Min(i => i.DateSubmitted ?? DateTime.MinValue) >= lastImportDate)));
            while (1 == 0);

            if (!IsFullImport)
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

            return results;
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
                if (retryCount <= MaxRetryCount)
                {
                    GetSpeedRunsWithRetry(elementsPerPage, elementsOffset, orderBy, statusType, retryCount++);
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


