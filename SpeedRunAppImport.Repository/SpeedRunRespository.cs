using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using NPoco.Extensions;
using System.Linq;
using SpeedRunApp.Model;
using SpeedRunApp.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

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

                                IF OBJECT_ID('dbo.tbl_SpeedRun_Player_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_SpeedRun_Player_Full

                                IF OBJECT_ID('dbo.tbl_SpeedRun_VariableValue_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_SpeedRun_VariableValue_Full

                                IF OBJECT_ID('dbo.tbl_SpeedRun_Video_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_SpeedRun_Video_Full

                                SELECT TOP 0 * INTO dbo.tbl_SpeedRun_Full FROM dbo.tbl_SpeedRun
                                SELECT TOP 0 * INTO dbo.tbl_SpeedRun_Player_Full FROM dbo.tbl_SpeedRun_Player
                                SELECT TOP 0 * INTO dbo.tbl_SpeedRun_VariableValue_Full FROM dbo.tbl_SpeedRun_VariableValue
                                SELECT TOP 0 * INTO dbo.tbl_SpeedRun_Video_Full FROM dbo.tbl_SpeedRun_Video

                                ALTER TABLE [dbo].[tbl_SpeedRun_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_SpeedRun_Full] ADD CONSTRAINT [DF_tbl_SpeedRun_Full_ImportedDate] DEFAULT GETDATE() FOR [ImportedDate]
                                ALTER TABLE [dbo].[tbl_SpeedRun_Player_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_Player_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_SpeedRun_VariableValue_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_VariableValue_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_SpeedRun_Video_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_Video_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
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

                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_Full', 'PK_tbl_SpeedRun'
                                EXEC sp_rename 'dbo.DF_tbl_SpeedRun_Full_ImportedDate', 'DF_tbl_SpeedRun_ImportedDate'
                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_Player_Full', 'PK_tbl_SpeedRun_Player'
                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_VariableValue_Full', 'PK_tbl_SpeedRun_VariableValue'
                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_Video_Full', 'PK_tbl_SpeedRun_Video'");
                    tran.Complete();
                }
            }
        }

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

                _logger.Information("Saved games {@Count} / {@Total}", runsBatch.Count, speedRunsList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertSpeedRuns");
        }

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
    }
}
