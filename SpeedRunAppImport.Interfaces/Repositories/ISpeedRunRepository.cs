using System;
using System.Collections.Generic;
using SpeedRunApp.Model;
using SpeedRunApp.Model.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface ISpeedRunRepository
    {
        void InsertSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunVideoEntity> videos);
        void UpdateSpeedRunStatus(IEnumerable<SpeedRunEntity> speedRuns, RunStatusType statusType);
        void UpdateSpeedRunStatusAndRejectReason(IEnumerable<SpeedRunEntity> speedRuns);
        void CopySpeedRunTables();
        void RenameAndDropSpeedRunTables();
        IEnumerable<SpeedRunEntity> GetSpeedRuns(Expression<Func<SpeedRunEntity, bool>> predicate);
    }
}






