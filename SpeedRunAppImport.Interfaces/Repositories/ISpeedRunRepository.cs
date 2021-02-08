using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface ISpeedRunRepository
    {
        void CopySpeedRunTables();
        void RenameAndDropSpeedRunTables();
        void InsertSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunStatusEntity> speedRunStatuses, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunVideoEntity> videos);
        void SaveSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunStatusEntity> speedRunStatuses, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunVideoEntity> videos);
        //IEnumerable<SpeedRunEntity> GetSpeedRuns(Expression<Func<SpeedRunEntity, bool>> predicate);
        void UpdateSpeedRunRanks(DateTime lastImportDate);
        IEnumerable<string> GetExistingSpeedRunComIDs(IEnumerable<string> speedRunComIDs);
        //IEnumerable<string> GetExistingSpeedRunIDs(IEnumerable<string> runIDs);
        //IEnumerable<string> GetExistingSpeedRunPlayerIDs();
    }
}






