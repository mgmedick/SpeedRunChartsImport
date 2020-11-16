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
        void ProcessSpeedRuns(DateTime lastImportDate, bool isFullImport);
        List<SpeedRun> GetSpeedRunsWithRetry(int elementsPerPage, int elementsOffset, RunsOrdering orderBy, RunStatusType? statusType = null, int retryCount = 0);
        void SaveSpeedRuns(IEnumerable<SpeedRun> runs, bool isFullImport);
        void SaveSpeedRuns(IEnumerable<SpeedRunEntity> runEntities, IEnumerable<SpeedRunVariableValueEntity> variableValueEntities, IEnumerable<SpeedRunPlayerEntity> playerEntities, IEnumerable<SpeedRunVideoEntity> videoEntities, bool isFullImport);
    }
} 




