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
        IEnumerable<SpeedRun> GetSpeedRuns(DateTime lastImportDate, bool isFullImport, RunStatusType? statusType = null);
    }
} 




