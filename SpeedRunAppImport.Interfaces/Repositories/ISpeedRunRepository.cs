using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;
using SpeedRunApp.Model.Data;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface ISpeedRunRepository
    {
        //void CopySpeedRunTables();
        //void RenameAndDropSpeedRunTables();
        void InsertSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunGuestEntity> guests, IEnumerable<SpeedRunVideoEntity> videos, IEnumerable<SpeedRunVideoDetailEntity> videoDetails);
        void SaveSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunGuestEntity> guests, IEnumerable<SpeedRunVideoEntity> videos, IEnumerable<SpeedRunVideoDetailEntity> videoDetails);
        void InsertSpeedRunVideoDetails(IEnumerable<SpeedRunVideoDetailEntity> videoDetails);
        void UpdateSpeedRunVideoDetailVideoCounts(IEnumerable<SpeedRunVideoDetailEntity> videoDetails);
        void UpdateSpeedRunVideoThumbnailLinkUrls(IEnumerable<SpeedRunVideoEntity> videos);
        void DeleteSpeedRuns(string predicate);
        void DeleteSpeedRuns(Expression<Func<SpeedRunEntity, bool>> predicate);
        void DeleteSpeedRunsByID(IEnumerable<int> speedRunIDsToDelete);
        IEnumerable<SpeedRunSpeedRunComIDEntity> GetSpeedRunSpeedRunComIDs(Expression<Func<SpeedRunSpeedRunComIDEntity, bool>> predicate = null);
        bool CreateFullTables();
        bool ReorderSpeedRuns();
        bool RenameFullTables();
        bool UpdateSpeedRunRanksFull();
        bool UpdateSpeedRunRanks(DateTime lastImportDateUtc);
        bool AnalyzeTables();
        bool OptimizeSpeedRunTables();
        bool OptimizeTables();
        bool RecreateSpeedRunIndexes();
        bool KillOtherProcesses(string InfoContains);
        bool UpdateStats();
        IEnumerable<SpeedRunVideoEntity> GetSpeedRunVideos(Expression<Func<SpeedRunVideoEntity, bool>> predicate = null);
        DateTime GetMaxSpeedRunVerifyDate();
        bool GetLatestSpeedRuns(int category, int topAmount, int? orderValueOffset, int? categoryTypeID);
        IEnumerable<SpeedRunVideoDetailEntity> GetSpeedRunVideoDetails(Expression<Func<SpeedRunVideoDetailEntity, bool>> predicate = null);
        IEnumerable<SpeedRunVideoView> GetSpeedRunVideoViews(Expression<Func<SpeedRunVideoView, bool>> predicate = null);
    }
}






