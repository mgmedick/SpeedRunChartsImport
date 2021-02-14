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
        //void ProcessSpeedRunUpdates(DateTime lastImportDate);
        List<SpeedRun> GetSpeedRunsWithRetry(int elementsPerPage, int elementsOffset, string gameID, RunsOrdering orderBy, RunStatusType? statusType = null, int retryCount = 0);
        void SaveSpeedRuns(IEnumerable<SpeedRun> runs, bool isFullImport);
        void SaveSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunStatusEntity> speedRunStatuses, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunVideoEntity> videos, bool isFullImport);
    }
} 




