using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface ISpeedRunService
    {
        void ProcessSpeedRuns(DateTime lastImportDate, bool isFullImport, bool isBulkReload, bool isProcessSpeedRunsByGame);
        void ProcessSpeedRunsDefault(DateTime lastImportDateUtc, bool isFullImport, bool isBulkReload);
        void ProcessSpeedRunsByGameFullImport(bool isBulkReload);
        void ProcessSpeedRunsByScreenScrape();
        List<SpeedRun> GetSpeedRunsWithRetry(int elementsPerPage, int elementsOffset, string gameID, RunsOrdering orderBy, RunStatusType? statusType = null, int retryCount = 0);
        void SaveSpeedRuns(IEnumerable<SpeedRun> runs, bool isFullImport);
    }
} 




