using System;
using System.Collections.Generic;
using SpeedRunApp.Model;
using SpeedRunApp.Model.Data;
using SpeedRunApp.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface ISpeedRunService
    {
        IEnumerable<SpeedRun> GetSpeedRuns(DateTime lastImportDate, bool isFullImport, RunStatusType? statusType = null);
    }
} 




