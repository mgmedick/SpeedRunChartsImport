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
        bool ProcessSpeedRuns(DateTime lastImportDateUtc, DateTime importLastRunDateUtc, bool isFullPull, bool isBulkReload, bool isUpdateSpeedRuns);
        void ProcessSpeedRunsDefault(DateTime lastImportDateUtc, bool isFullPull, bool isBulkReload);
        void ProcessSpeedRunsByGame(DateTime importLastRunDateUtc, bool isFullPull, bool isBulkReload);
        void ProcessSpeedRunsByScreenScrape();
        List<SpeedRun> GetSpeedRunsWithRetry(int elementsPerPage, int elementsOffset, string gameID, string categoryID, SpeedRunEmbeds embeds, RunsOrdering orderBy, RunStatusType? statusType = null, int retryCount = 0);
        void SaveSpeedRuns(IEnumerable<SpeedRun> runs, bool isBulkReload, bool isUpdateSpeedRuns);
        bool ReprocessSpeedRunVideoDetails();
        bool ProcessYouTubeSpeedRunVideoDetails();
    }
} 




