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
        //void CopySpeedRunTables();
        //void RenameAndDropSpeedRunTables();
        void InsertSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunVideoEntity> videos);
        void SaveSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunVideoEntity> videos);
        IEnumerable<SpeedRunSpeedRunComIDEntity> GetSpeedRunSpeedRunComIDs(Expression<Func<SpeedRunSpeedRunComIDEntity, bool>> predicate = null);
        bool CreateFullTables();
        bool RenameFullTables();
        bool UpdateSpeedRunRanks(DateTime lastImportDateUtc);
        bool RebuildIndexes();
        IEnumerable<SpeedRunVideoEntity> GetSpeedRunVideos(Expression<Func<SpeedRunVideoEntity, bool>> predicate = null);
        void UpdateSpeedRunVideos(IEnumerable<SpeedRunVideoEntity> speedRunVideos);
    }
}






