using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using NPoco.Extensions;
using System.Linq;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Repository
{
    public class SpeedRunRespository : BaseRepository, ISpeedRunRepository
    {
        private readonly ILogger _logger;

        public SpeedRunRespository(ILogger logger)
        {
            _logger = logger;
        }

        public void CopySpeedRunTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"IF OBJECT_ID('dbo.tbl_SpeedRun_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_SpeedRun_Full

                                IF OBJECT_ID('dbo.tbl_SpeedRun_Full_Ordered') IS NOT NULL 
                                    DROP TABLE dbo.tbl_SpeedRun_Full_Ordered

                                IF OBJECT_ID('dbo.tbl_SpeedRun_Player_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_SpeedRun_Player_Full

                                IF OBJECT_ID('dbo.tbl_SpeedRun_VariableValue_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_SpeedRun_VariableValue_Full

                                IF OBJECT_ID('dbo.tbl_SpeedRun_Video_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_SpeedRun_Video_Full

                                SELECT TOP 0 * INTO dbo.tbl_SpeedRun_Full FROM dbo.tbl_SpeedRun
                                SELECT TOP 0 * INTO dbo.tbl_SpeedRun_Full_Ordered FROM dbo.tbl_SpeedRun
                                SELECT TOP 0 * INTO dbo.tbl_SpeedRun_Player_Full FROM dbo.tbl_SpeedRun_Player
                                SELECT TOP 0 * INTO dbo.tbl_SpeedRun_VariableValue_Full FROM dbo.tbl_SpeedRun_VariableValue
                                SELECT TOP 0 * INTO dbo.tbl_SpeedRun_Video_Full FROM dbo.tbl_SpeedRun_Video

                                ALTER TABLE [dbo].[tbl_SpeedRun_Full] ADD CONSTRAINT [DF_tbl_SpeedRun_Full_ImportedDate] DEFAULT GETDATE() FOR [ImportedDate]");
                    tran.Complete();
                }
            }
        }

        public void RenameAndDropSpeedRunTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.OneTimeCommandTimeout = 32767;
                    db.Execute(@"DECLARE @batch INT = 10000

                                WHILE @batch > 0
                                BEGIN
                                    INSERT INTO tbl_SpeedRun_Full_Ordered (ID, StatusTypeID, GameID, CategoryID, LevelID, PlatformID, RegionID, IsEmulated,
											                                PrimaryTime, RealTime, RealTimeWithoutLoads, GameTime, Comment, ExaminerUserID,
											                                RejectReason, SpeedRunComUrl, SplitsUrl, RunDate, DateSubmitted, VerifyDate,
											                                ImportedDate, ModifiedDate, [Rank], SubCategoryVariableValues, PlayerIDs)
                                    SELECT TOP (@batch)
	                                ID, StatusTypeID, GameID, CategoryID, LevelID, PlatformID, RegionID, IsEmulated,
	                                PrimaryTime, RealTime, RealTimeWithoutLoads, GameTime, Comment, ExaminerUserID,
	                                RejectReason, SpeedRunComUrl, SplitsUrl, RunDate, DateSubmitted, VerifyDate,
	                                ImportedDate, ModifiedDate, [Rank], SubCategoryVariableValues, PlayerIDs
                                    FROM dbo.tbl_SpeedRun_Full t1
                                    WHERE NOT EXISTS (SELECT 1 FROM tbl_SpeedRun_Full_Ordered t2 WHERE t2.ID = t1.ID)
                                    ORDER BY ISNULL(t1.DateSubmitted, t1.RunDate)

                                    SELECT @batch = @@ROWCOUNT
                                END

                                EXEC sp_rename 'dbo.tbl_SpeedRun', 'tbl_SpeedRun_ToRemove'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Full', 'tbl_SpeedRun_Full_ToRemove'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Player', 'tbl_SpeedRun_Player_ToRemove'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_VariableValue', 'tbl_SpeedRun_VariableValue_ToRemove'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Video', 'tbl_SpeedRun_Video_ToRemove'

                                EXEC sp_rename 'dbo.tbl_SpeedRun_Full_Ordered', 'tbl_SpeedRun'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Player_Full', 'tbl_SpeedRun_Player'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_VariableValue_Full', 'tbl_SpeedRun_VariableValue'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Video_Full', 'tbl_SpeedRun_Video'

                                DROP TABLE dbo.tbl_SpeedRun_ToRemove
                                DROP TABLE dbo.tbl_SpeedRun_Full_ToRemove
                                DROP TABLE dbo.tbl_SpeedRun_Player_ToRemove
                                DROP TABLE dbo.tbl_SpeedRun_VariableValue_ToRemove
                                DROP TABLE dbo.tbl_SpeedRun_Video_ToRemove

                                EXEC sp_rename 'dbo.DF_tbl_SpeedRun_Full_ImportedDate', 'DF_tbl_SpeedRun_ImportedDate'

                                ALTER TABLE [dbo].[tbl_SpeedRun] ADD CONSTRAINT [PK_tbl_SpeedRun] PRIMARY KEY NONCLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE CLUSTERED INDEX [IDX_tbl_SpeedRun_OrderValue] ON [dbo].[tbl_SpeedRun] ([OrderValue]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_GameID_CategoryID_PlusInclude] ON [dbo].[tbl_SpeedRun] ([StatusTypeID],[GameID],[CategoryID]) INCLUDE ([ID],[LevelID],[PrimaryTime],[SubCategoryVariableValues],[PlayerIDs])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_PlusInclude] ON [dbo].[tbl_SpeedRun] ([StatusTypeID]) INCLUDE ([GameID],[CategoryID],[LevelID],[Rank],[SubCategoryVariableValues])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_GameID_CategoryID_PrimaryTime_PlusInclude] ON [dbo].[tbl_SpeedRun] ([StatusTypeID],[GameID],[CategoryID],[PrimaryTime]) INCLUDE ([ID],[LevelID],[SubCategoryVariableValues])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_LevelID] ON [dbo].[tbl_SpeedRun] ([StatusTypeID],[LevelID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_CategoryID] ON [dbo].[tbl_SpeedRun] ([StatusTypeID],[CategoryID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_GameID_Rank] ON [dbo].[tbl_SpeedRun] ([StatusTypeID],[GameID],[Rank]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                ALTER TABLE [dbo].[tbl_SpeedRun_Player] ADD CONSTRAINT [PK_tbl_SpeedRun_Player] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_Player_SpeedRunID] ON [dbo].[tbl_SpeedRun_Player] ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_Player_UserID] ON [dbo].[tbl_SpeedRun_Player] ([UserID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                ALTER TABLE [dbo].[tbl_SpeedRun_VariableValue] ADD CONSTRAINT [PK_tbl_SpeedRun_VariableValue] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_VariableValue_SpeedRunID] ON [dbo].[tbl_SpeedRun_VariableValue] ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_VariableValue_VariableValueID_PlusInclude] ON [dbo].[tbl_SpeedRun_VariableValue] ([VariableValueID]) INCLUDE ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                ALTER TABLE [dbo].[tbl_SpeedRun_Video] ADD CONSTRAINT [PK_tbl_SpeedRun_Video] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_Video_SpeedRunID] ON [dbo].[tbl_SpeedRun_Video] ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
                    tran.Complete();
                }
            }
        }

        /*
        public void RenameAndDropSpeedRunTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.OneTimeCommandTimeout = 32767;
                    db.Execute(@"EXEC sp_rename 'dbo.tbl_SpeedRun', 'tbl_SpeedRun_ToRemove'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Player', 'tbl_SpeedRun_Player_ToRemove'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_VariableValue', 'tbl_SpeedRun_VariableValue_ToRemove'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Video', 'tbl_SpeedRun_Video_ToRemove'

                                EXEC sp_rename 'dbo.tbl_SpeedRun_Full', 'tbl_SpeedRun'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Player_Full', 'tbl_SpeedRun_Player'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_VariableValue_Full', 'tbl_SpeedRun_VariableValue'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Video_Full', 'tbl_SpeedRun_Video'

                                DROP TABLE dbo.tbl_SpeedRun_ToRemove
                                DROP TABLE dbo.tbl_SpeedRun_Player_ToRemove
                                DROP TABLE dbo.tbl_SpeedRun_VariableValue_ToRemove
                                DROP TABLE dbo.tbl_SpeedRun_Video_ToRemove

                                EXEC sp_rename 'dbo.DF_tbl_SpeedRun_Full_ImportedDate', 'DF_tbl_SpeedRun_ImportedDate'

                                ALTER TABLE [dbo].[tbl_SpeedRun] ADD CONSTRAINT [PK_tbl_SpeedRun] PRIMARY KEY NONCLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE CLUSTERED INDEX [IDX_tbl_SpeedRun_OrderValue] ON [dbo].[tbl_SpeedRun] ([OrderValue]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_GameID_CategoryID_PlusInclude] ON [dbo].[tbl_SpeedRun] ([StatusTypeID],[GameID],[CategoryID]) INCLUDE ([ID],[LevelID],[PrimaryTime],[SubCategoryVariableValues],[PlayerIDs])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_PlusInclude] ON [dbo].[tbl_SpeedRun] ([StatusTypeID]) INCLUDE ([GameID],[CategoryID],[LevelID],[Rank],[SubCategoryVariableValues])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_GameID_CategoryID_PrimaryTime_PlusInclude] ON [dbo].[tbl_SpeedRun] ([StatusTypeID],[GameID],[CategoryID],[PrimaryTime]) INCLUDE ([ID],[LevelID],[SubCategoryVariableValues])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_LevelID] ON [dbo].[tbl_SpeedRun] ([StatusTypeID],[LevelID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_CategoryID] ON [dbo].[tbl_SpeedRun] ([StatusTypeID],[CategoryID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_StatusTypeID_GameID_Rank] ON [dbo].[tbl_SpeedRun] ([StatusTypeID],[GameID],[Rank]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                ALTER TABLE [dbo].[tbl_SpeedRun_Player] ADD CONSTRAINT [PK_tbl_SpeedRun_Player] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_Player_SpeedRunID] ON [dbo].[tbl_SpeedRun_Player] ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_Player_UserID] ON [dbo].[tbl_SpeedRun_Player] ([UserID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                ALTER TABLE [dbo].[tbl_SpeedRun_VariableValue] ADD CONSTRAINT [PK_tbl_SpeedRun_VariableValue] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_VariableValue_SpeedRunID] ON [dbo].[tbl_SpeedRun_VariableValue] ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_VariableValue_VariableValueID_PlusInclude] ON [dbo].[tbl_SpeedRun_VariableValue] ([VariableValueID]) INCLUDE ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                ALTER TABLE [dbo].[tbl_SpeedRun_Video] ADD CONSTRAINT [PK_tbl_SpeedRun_Video] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_Video_SpeedRunID] ON [dbo].[tbl_SpeedRun_Video] ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
                    tran.Complete();
                }
            }
        }
        */

        public void InsertSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunVideoEntity> videos)
        {
            _logger.Information("Started InsertSpeedRuns");
            int batchCount = 0;
            var speedRunsList = speedRuns.ToList();
            while (batchCount < speedRunsList.Count)
            {
                var runsBatch = speedRunsList.Skip(batchCount).Take(MaxBulkRows).ToList();
                var runIDs = runsBatch.Select(i => i.ID).Distinct().ToList();
                var variableValuesBatch = variableValues.Where(i => runIDs.Contains(i.SpeedRunID)).ToList();
                var playersBatch = players.Where(i => runIDs.Contains(i.SpeedRunID)).ToList();
                var videosBatch = videos.Where(i => runIDs.Contains(i.SpeedRunID)).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBulk<SpeedRunEntity>(runsBatch);
                        db.InsertBulk<SpeedRunVariableValueEntity>(variableValuesBatch);
                        db.InsertBulk<SpeedRunPlayerEntity>(playersBatch);
                        db.InsertBulk<SpeedRunVideoEntity>(videosBatch);
                        tran.Complete();
                    }
                }

                _logger.Information("Saved speedRuns {@Count} / {@Total}", runsBatch.Count, speedRunsList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertSpeedRuns");
        }

        public void SaveSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunVideoEntity> videos)
        {
            int count = 1;
            var speedRunsList = speedRuns.ToList();
            foreach (var speedRun in speedRuns)
            {
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        if (db.Exists<SpeedRunEntity>(speedRun.ID))
                        {
                            speedRun.ModifiedDate = DateTime.Now;
                            db.DeleteWhere<SpeedRunVariableValueEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                            db.DeleteWhere<SpeedRunPlayerEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                            db.DeleteWhere<SpeedRunVideoEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                        }

                        db.Save<SpeedRunEntity>(speedRun);
                        db.InsertBulk<SpeedRunVariableValueEntity>(variableValues.Where(i => i.SpeedRunID == speedRun.ID).ToList());
                        db.InsertBulk<SpeedRunPlayerEntity>(players.Where(i => i.SpeedRunID == speedRun.ID).ToList());
                        db.InsertBulk<SpeedRunVideoEntity>(videos.Where(i => i.SpeedRunID == speedRun.ID).ToList());

                        tran.Complete();
                    }
                }

                _logger.Information("Saved speedRuns {@Count} / {@Total}", count, speedRunsList.Count);
                count++;
            }
        }

        /*
        public void UpdateSpeedRunStatus(IEnumerable<SpeedRunEntity> speedRuns, RunStatusType statusType)
        {
            try
            {
                int batchCount = 0;
                while (batchCount < speedRuns.Count())
                {
                    var runsBatch = speedRuns.Skip(batchCount).Take(MaxBulkRows).Select(i => i.ID).ToArray();

                    using (IDatabase db = DBFactory.GetDatabase())
                    {
                        using (var tran = db.GetTransaction())
                        {
                            db.UpdateMany<SpeedRunEntity>()
                              .Where(i => runsBatch.Contains(i.ID))
                              .OnlyFields(i => new { i.StatusTypeID, i.Comment })
                              .Execute(new SpeedRunEntity() { StatusTypeID = (int)statusType });
                            tran.Complete();
                        }
                    }

                    batchCount += MaxBulkRows;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UpdateSpeedRunStatus");
            }
        }

        public void UpdateSpeedRunStatusAndRejectReason(IEnumerable<SpeedRunEntity> speedRuns)
        {
            try
            {
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    foreach (var speedRun in speedRuns)
                    {
                        db.Update<SpeedRunEntity>(speedRun, i => new { i.StatusTypeID, i.Comment, i.RejectReason });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "UpdateSpeedRunStatusAndRejectReason");
            }
        }
        */

        public IEnumerable<SpeedRunEntity> GetSpeedRuns(Expression<Func<SpeedRunEntity, bool>> predicate)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<SpeedRunEntity>().Where(predicate).ToList();
            }
        }

        public IEnumerable<SpeedRunEntity> GetSpeedRunsBetweenDates(DateTime startDate, DateTime endDate)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<SpeedRunEntity>("SELECT ID, StatusTypeID, GameID, CategoryID, LevelID, PlatformID, RegionID, IsEmulated, " +
                                                    "PrimaryTime, RealTime, RealTimeWithoutLoads, GameTime, Comment, ExaminerUserID, " +
                                                    "RejectReason, SpeedRunComUrl, SplitsUrl, RunDate, DateSubmitted, VerifyDate, " +
                                                    "ImportedDate, ModifiedDate, [Rank], SubCategoryVariableValues, PlayerIDs " +
                                                    "FROM dbo.tbl_SpeedRun " +
                                                    "WHERE ImportedDate BETWEEN @0 AND @1", startDate, endDate).ToList();
            }
        }

        public void UpdateSpeedRunRanks(int importProcessID, DateTime gameLastImportDate, DateTime speedRunLastImportDate)
        {
            _logger.Information("Started UpdateSpeedRunRanks {@ImportProcessID}, {@GameLastImportDate}, {@SpeedRunLastImportDate}", importProcessID, gameLastImportDate, speedRunLastImportDate);

            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                db.Execute("EXEC dbo.UpdateSpeedRunRanks @0, @1", importProcessID, gameLastImportDate, speedRunLastImportDate);
            }

            _logger.Information("Completed UpdateSpeedRunRanks");
        }

        public void UpdateSpeedRunSubCategoryVariableValues(DateTime lastImportDate)
        {
            _logger.Information("Started UpdateSpeedRunSubCategoryVariableValues {@LastImportDate}", lastImportDate);

            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                db.Execute("EXEC dbo.UpdateSpeedRunSubCategoryVariableValues @0", lastImportDate);
            }

            _logger.Information("Completed UpdateSpeedRunSubCategoryVariableValues");
        }
    }
}
