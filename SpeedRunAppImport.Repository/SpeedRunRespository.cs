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

        /*
        public void CopySpeedRunTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"--tbl_SpeedRun_Full
                                IF OBJECT_ID('dbo.tbl_SpeedRun_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_SpeedRun_Full
                                END 

                                CREATE TABLE [dbo].[tbl_SpeedRun_Full] 
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1), 
	                                [GameID] [int] NOT NULL,
	                                [CategoryID] [int] NOT NULL,
	                                [LevelID] [int] NULL,
	                                [Rank] [int] NULL,
	                                [PrimaryTime] [bigint] NOT NULL,
	                                [RunDate] [datetime] NULL,
	                                [DateSubmitted] [datetime] NULL,
	                                [ImportedDate] [datetime] NOT NULL CONSTRAINT [DF_tbl_SpeedRun_Full_ImportedDate] DEFAULT(GETDATE()),
	                                [ModifiedDate] [datetime] NULL
                                ) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_SpeedRun_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_SpeedRunComID_Full
                                IF OBJECT_ID('dbo.tbl_SpeedRun_SpeedRunComID_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_SpeedRun_SpeedRunComID_Full
                                END 

                                CREATE TABLE [dbo].[tbl_SpeedRun_SpeedRunComID_Full] 
                                (
	                                [SpeedRunID] [int] NOT NULL,
                                    [SpeedRunComID] [varchar] (10) NOT NULL
                                )
                                ALTER TABLE [dbo].[tbl_SpeedRun_SpeedRunComID_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_SpeedRunComID_Full] PRIMARY KEY CLUSTERED ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_Status_Full
                                IF OBJECT_ID('dbo.tbl_SpeedRun_Status_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_SpeedRun_Status_Full
                                END 

                                CREATE TABLE [dbo].[tbl_SpeedRun_Status_Full] 
                                ( 
                                    [SpeedRunID] [int] NOT NULL,
	                                [StatusTypeID] [int] NOT NULL,
	                                [ExaminerUserID] [int] NULL,
	                                [VerifyDate] [datetime] NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun_Status_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_Status_Full] PRIMARY KEY CLUSTERED ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_System_Full
                                IF OBJECT_ID('dbo.tbl_SpeedRun_System_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_SpeedRun_System_Full
                                END 

                                CREATE TABLE [dbo].[tbl_SpeedRun_System_Full] 
                                ( 
                                    [SpeedRunID] [int] NOT NULL,
	                                [PlatformID] [int] NULL,
	                                [RegionID] [int] NULL,
 	                                [IsEmulated] [bit] NOT NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun_System_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_System_Full] PRIMARY KEY CLUSTERED ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_Time_Full
                                IF OBJECT_ID('dbo.tbl_SpeedRun_Time_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_SpeedRun_Time_Full
                                END 

                                CREATE TABLE [dbo].[tbl_SpeedRun_Time_Full] 
                                ( 
                                    [SpeedRunID] [int] NOT NULL,
	                                [PrimaryTime] [bigint] NOT NULL,
	                                [RealTime] [bigint] NULL,
	                                [RealTimeWithoutLoads] [bigint] NULL,
	                                [GameTime] [bigint] NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun_Time_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_Time_Full] PRIMARY KEY CLUSTERED ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_Link_Full
                                IF OBJECT_ID('dbo.tbl_SpeedRun_Link_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_SpeedRun_Link_Full
                                END 

                                CREATE TABLE [dbo].[tbl_SpeedRun_Link_Full] 
                                ( 
                                    [SpeedRunID] [int] NOT NULL,
	                                [SpeedRunComUrl] [varchar](1000) NOT NULL,
	                                [SplitsUrl] [varchar](1000) NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun_Link_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_Link_Full] PRIMARY KEY CLUSTERED ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_Comment_Full
                                IF OBJECT_ID('dbo.tbl_SpeedRun_Comment_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_SpeedRun_Comment_Full
                                END 

                                CREATE TABLE [dbo].[tbl_SpeedRun_Comment_Full] 
                                ( 
                                    [SpeedRunID] [int] NOT NULL,
	                                [Comment] [varchar](MAX) NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun_Comment_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_Comment_Full] PRIMARY KEY CLUSTERED ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_Player_Full
                                IF OBJECT_ID('dbo.tbl_SpeedRun_Player_Full') IS NOT NULL 
                                BEGIN 
                                    DROP TABLE dbo.tbl_SpeedRun_Player_Full
                                END
                                CREATE TABLE [dbo].[tbl_SpeedRun_Player_Full] 
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1),
                                    [SpeedRunID] [int] NOT NULL,
                                    [IsUser] [bit] NOT NULL,
                                    [UserID] [int] NULL,
                                    [GuestName] [varchar] (255) NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun_Player_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_Player_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_VariableValue_Full
                                IF OBJECT_ID('dbo.tbl_SpeedRun_VariableValue_Full') IS NOT NULL 
                                BEGIN 
                                    DROP TABLE dbo.tbl_SpeedRun_VariableValue_Full
                                END

                                CREATE TABLE [dbo].[tbl_SpeedRun_VariableValue_Full] 
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1),
                                    [SpeedRunID] [int] NOT NULL,
                                    [VariableID] [int] NOT NULL,
                                    [VariableValueID] [int] NOT NULL 
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun_VariableValue_Full] ADD CONSTRAINT [PK_tbl_SpeedRun_VariableValue_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_Video_Full
                                IF OBJECT_ID('dbo.tbl_SpeedRun_Video_Full') IS NOT NULL 
                                BEGIN 
                                    DROP TABLE dbo.tbl_SpeedRun_Video_Full
                                END

                                CREATE TABLE [dbo].[tbl_SpeedRun_Video_Full] 
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1),
                                    [SpeedRunID] [int] NOT NULL,
                                    [VideoLinkUrl] [varchar] (1000) NOT NULL
                                ) ON [PRIMARY] 
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
                    db.OneTimeCommandTimeout = 32767;
                    db.Execute(@"--tbl_SpeedRun
                                ALTER TABLE [dbo].[tbl_SpeedRun_Player] DROP CONSTRAINT [FK_tbl_SpeedRun_Player_tbl_SpeedRun]
                                ALTER TABLE [dbo].[tbl_SpeedRun_VariableValue] DROP CONSTRAINT [FK_tbl_SpeedRun_VariableValue_tbl_SpeedRun]
                                ALTER TABLE [dbo].[tbl_SpeedRun_Video] DROP CONSTRAINT [FK_tbl_SpeedRun_Video_tbl_SpeedRun]

                                DROP TABLE dbo.tbl_SpeedRun

                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_Full', 'PK_tbl_SpeedRun'                                
                                EXEC sp_rename 'dbo.DF_tbl_SpeedRun_Full_ImportedDate', 'DF_tbl_SpeedRun_ImportedDate'
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Full', 'tbl_SpeedRun'

                                ALTER TABLE [dbo].[tbl_SpeedRun] ADD CONSTRAINT [FK_tbl_SpeedRun_tbl_Game] FOREIGN KEY ([GameID]) REFERENCES [dbo].[tbl_Game] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_tbl_Game] ON [dbo].[tbl_SpeedRun] ([GameID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun] ADD CONSTRAINT [FK_tbl_SpeedRun_tbl_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[tbl_Category] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_tbl_Category] ON [dbo].[tbl_SpeedRun] ([CategoryID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun] ADD CONSTRAINT [FK_tbl_SpeedRun_tbl_Level] FOREIGN KEY ([LevelID]) REFERENCES [dbo].[tbl_Level] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_tbl_Level] ON [dbo].[tbl_SpeedRun] ([LevelID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_GameID_Rank_CategoryID_LevelID_PrimaryTime_DateSubmitted_VerifyDate] ON [dbo].[tbl_SpeedRun] ([GameID],[Rank]) INCLUDE ([CategoryID],[LevelID],[PrimaryTime],[DateSubmitted]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                DELETE FROM dbo.[tbl_SpeedRun_Player] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_SpeedRun rn WHERE rn.ID = SpeedRunID)                           
                                ALTER TABLE [dbo].[tbl_SpeedRun_Player] ADD CONSTRAINT [FK_tbl_SpeedRun_Player_tbl_SpeedRun] FOREIGN KEY ([SpeedRunID]) REFERENCES [dbo].[tbl_SpeedRun] ([ID])                           

                                DELETE FROM dbo.[tbl_SpeedRun_VariableValue] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_SpeedRun rn WHERE rn.ID = SpeedRunID)                           
                                ALTER TABLE [dbo].[tbl_SpeedRun_VariableValue] ADD CONSTRAINT [FK_tbl_SpeedRun_VariableValue_tbl_SpeedRun] FOREIGN KEY ([SpeedRunID]) REFERENCES [dbo].[tbl_SpeedRun] ([ID])

                                DELETE FROM dbo.[tbl_SpeedRun_Video] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_SpeedRun rn WHERE rn.ID = SpeedRunID)                           
                                ALTER TABLE [dbo].[tbl_SpeedRun_Video] ADD CONSTRAINT [FK_tbl_SpeedRun_Video_tbl_SpeedRun] FOREIGN KEY ([SpeedRunID]) REFERENCES [dbo].[tbl_SpeedRun] ([ID])

                                --tbl_SpeedRun_SpeedRunComID
                                DROP TABLE dbo.tbl_SpeedRun_SpeedRunComID
                                
                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_SpeedRunComID_Full', 'PK_tbl_SpeedRun_SpeedRunComID'                                
                                EXEC sp_rename 'dbo.tbl_SpeedRun_SpeedRunComID_Full', 'tbl_SpeedRun_SpeedRunComID'

                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_SpeedRunComID_SpeedRunComID] ON [dbo].[tbl_SpeedRun_SpeedRunComID] ([SpeedRunComID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_Status
                                DROP TABLE dbo.tbl_SpeedRun_Status
                                
                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_Status_Full', 'PK_tbl_SpeedRun_Status'                                
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Status_Full', 'tbl_SpeedRun_Status'

                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_Status_StatusTypeID] ON [dbo].[tbl_SpeedRun_Status] ([StatusTypeID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun_Status] ADD CONSTRAINT [FK_tbl_SpeedRun_Status_tbl_User] FOREIGN KEY ([ExaminerUserID]) REFERENCES [dbo].[tbl_User] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_Status_ExaminerUserID] ON [dbo].[tbl_SpeedRun_Status] ([ExaminerUserID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_System
                                DROP TABLE dbo.tbl_SpeedRun_System
                                
                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_System_Full', 'PK_tbl_SpeedRun_System'                                
                                EXEC sp_rename 'dbo.tbl_SpeedRun_System_Full', 'tbl_SpeedRun_System'

                                ALTER TABLE [dbo].[tbl_SpeedRun_System] ADD CONSTRAINT [FK_tbl_SpeedRun_System_tbl_Platform] FOREIGN KEY ([PlatformID]) REFERENCES [dbo].[tbl_Platform] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_System_tbl_Platform] ON [dbo].[tbl_SpeedRun_System] ([PlatformID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun_System] ADD CONSTRAINT [FK_tbl_SpeedRun_System_tbl_Region] FOREIGN KEY ([RegionID]) REFERENCES [dbo].[tbl_Region] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_System_tbl_Region] ON [dbo].[tbl_SpeedRun_System] ([RegionID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_Time
                                DROP TABLE dbo.tbl_SpeedRun_Time
                                
                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_Time_Full', 'PK_tbl_SpeedRun_Time'                                
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Time_Full', 'tbl_SpeedRun_Time'

                                --tbl_SpeedRun_Link
                                DROP TABLE dbo.tbl_SpeedRun_Link
                                
                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_Link_Full', 'PK_tbl_SpeedRun_Link'                                
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Link_Full', 'tbl_SpeedRun_Link'

                                --tbl_SpeedRun_Comment
                                DROP TABLE dbo.tbl_SpeedRun_Comment
                                
                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_Comment_Full', 'PK_tbl_SpeedRun_Comment'                                
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Comment_Full', 'tbl_SpeedRun_Comment'

                                --tbl_SpeedRun_Player
                                DROP TABLE dbo.tbl_SpeedRun_Player
                                
                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_Player_Full', 'PK_tbl_SpeedRun_Player'                                
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Player_Full', 'tbl_SpeedRun_Player'

                                ALTER TABLE [dbo].[tbl_SpeedRun_Player] ADD CONSTRAINT [FK_tbl_SpeedRun_Player_tbl_SpeedRun] FOREIGN KEY ([SpeedRunID]) REFERENCES [dbo].[tbl_SpeedRun] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_Player_tbl_SpeedRun] ON [dbo].[tbl_SpeedRun_Player] ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_SpeedRun_Player] ADD CONSTRAINT [FK_tbl_SpeedRun_Player_tbl_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[tbl_User] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_Player_tbl_User] ON [dbo].[tbl_SpeedRun_Player] ([UserID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_SpeedRun_Video
                                DROP TABLE dbo.tbl_SpeedRun_Video
                                
                                EXEC sp_rename 'dbo.PK_tbl_SpeedRun_Video_Full', 'PK_tbl_SpeedRun_Video'                                
                                EXEC sp_rename 'dbo.tbl_SpeedRun_Video_Full', 'tbl_SpeedRun_Video'

                                ALTER TABLE [dbo].[tbl_SpeedRun_Video] ADD CONSTRAINT [FK_tbl_SpeedRun_Video_tbl_SpeedRun] FOREIGN KEY ([SpeedRunID]) REFERENCES [dbo].[tbl_SpeedRun] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_SpeedRun_Video_tbl_SpeedRun] ON [dbo].[tbl_SpeedRun_Video] ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
                    tran.Complete();
                }
            }
        }
        */

        public void InsertSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunStatusEntity> speedRunStatuses, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunVideoEntity> videos)
        {
            _logger.Information("Started InsertSpeedRuns");
            int batchCount = 0;
            var speedRunsList = speedRuns.ToList();

            while (batchCount < speedRunsList.Count)
            {
                var runsBatch = speedRunsList.Skip(batchCount).Take(MaxBulkRows).ToList();
                var runSpeedRunComIDs = runsBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var speedRunLinksBatch = speedRunLinks.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                var speedRunStatusesBatch = speedRunStatuses.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                var speedRunSystemsBatch = speedRunSystems.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                var speedRunTimesBatch = speedRunTimes.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                var speedRunCommentsBatch = speedRunComments.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                var variableValuesBatch = variableValues.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                var playersBatch = players.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();
                var videosBatch = videos.Where(i => runSpeedRunComIDs.Contains(i.SpeedRunSpeedRunComID)).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBulk<SpeedRunEntity>(runsBatch);
                        var runIDs = db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_SpeedRun_Full ORDER BY ID DESC", runsBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < runsBatch.Count; i++)
                        {
                            runsBatch[i].ID = runIDs[i];
                        }

                        var speedRunSpeedRunComIDsBatch = runsBatch.Select(i => new SpeedRunSpeedRunComIDEntity { SpeedRunID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBulk<SpeedRunSpeedRunComIDEntity>(speedRunSpeedRunComIDsBatch);

                        speedRunLinksBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBulk<SpeedRunLinkEntity>(speedRunLinksBatch);

                        speedRunStatusesBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBulk<SpeedRunStatusEntity>(speedRunStatusesBatch);

                        speedRunSystemsBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBulk<SpeedRunSystemEntity>(speedRunSystemsBatch);

                        speedRunTimesBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBulk<SpeedRunTimeEntity>(speedRunTimesBatch);

                        speedRunCommentsBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBulk<SpeedRunCommentEntity>(speedRunCommentsBatch);

                        variableValuesBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBulk<SpeedRunVariableValueEntity>(variableValuesBatch);

                        playersBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBulk<SpeedRunPlayerEntity>(playersBatch);

                        videosBatch.ForEach(i => i.SpeedRunID = runsBatch.Find(g => g.SpeedRunComID == i.SpeedRunSpeedRunComID).ID);
                        db.InsertBulk<SpeedRunVideoEntity>(videosBatch);

                        tran.Complete();
                    }
                }

                _logger.Information("Saved speedRuns {@Count} / {@Total}", runsBatch.Count, speedRunsList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertSpeedRuns");
        }

        public void SaveSpeedRuns(IEnumerable<SpeedRunEntity> speedRuns, IEnumerable<SpeedRunLinkEntity> speedRunLinks, IEnumerable<SpeedRunStatusEntity> speedRunStatuses, IEnumerable<SpeedRunSystemEntity> speedRunSystems, IEnumerable<SpeedRunTimeEntity> speedRunTimes, IEnumerable<SpeedRunCommentEntity> speedRunComments, IEnumerable<SpeedRunVariableValueEntity> variableValues, IEnumerable<SpeedRunPlayerEntity> players, IEnumerable<SpeedRunVideoEntity> videos)
        {
            int count = 1;
            var speedRunsList = speedRuns.ToList();

            foreach (var speedRun in speedRuns)
            {
                var speedRunLink = speedRunLinks.FirstOrDefault(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID);
                var speedRunStatus = speedRunStatuses.FirstOrDefault(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID);
                var speedRunSystem = speedRunSystems.FirstOrDefault(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID);
                var speedRunTime = speedRunTimes.FirstOrDefault(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID);
                var speedRunComment = speedRunComments.FirstOrDefault(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID);
                var variableValuesBatch = variableValues.Where(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID).ToList();
                var playersBatch = players.Where(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID).ToList();
                var videosBatch = videos.Where(i => i.SpeedRunSpeedRunComID == speedRun.SpeedRunComID).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        if (speedRun.ID != 0)
                        {
                            speedRun.ModifiedDate = DateTime.Now;
                            db.DeleteWhere<SpeedRunSpeedRunComIDEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                            db.DeleteWhere<SpeedRunLinkEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                            db.DeleteWhere<SpeedRunStatusEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                            db.DeleteWhere<SpeedRunSystemEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                            db.DeleteWhere<SpeedRunTimeEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                            db.DeleteWhere<SpeedRunCommentEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                            db.DeleteWhere<SpeedRunVariableValueEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                            db.DeleteWhere<SpeedRunPlayerEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                            db.DeleteWhere<SpeedRunVideoEntity>("SpeedRunID = @speedRunID", new { speedRunID = speedRun.ID });
                        }

                        db.Save<SpeedRunEntity>(speedRun);

                        var speedRunSpeedRunComID = new SpeedRunSpeedRunComIDEntity { SpeedRunID = speedRun.ID, SpeedRunComID = speedRun.SpeedRunComID };
                        db.Insert<SpeedRunSpeedRunComIDEntity>(speedRunSpeedRunComID);

                        if (speedRunLink != null)
                        {
                            speedRunLink.SpeedRunID = speedRun.ID;
                            db.Insert<SpeedRunLinkEntity>(speedRunLink);
                        }

                        if (speedRunStatus != null)
                        {
                            speedRunStatus.SpeedRunID = speedRun.ID;
                            db.Insert<SpeedRunStatusEntity>(speedRunStatus);
                        }

                        if (speedRunSystem != null)
                        {
                            speedRunSystem.SpeedRunID = speedRun.ID;
                            db.Insert<SpeedRunSystemEntity>(speedRunSystem);
                        }

                        if (speedRunTime != null)
                        {
                            speedRunTime.SpeedRunID = speedRun.ID;
                            db.Insert<SpeedRunTimeEntity>(speedRunTime);
                        }

                        if (speedRunComment != null)
                        {
                            speedRunComment.SpeedRunID = speedRun.ID;
                            db.Insert<SpeedRunCommentEntity>(speedRunComment);
                        }

                        variableValuesBatch.ForEach(i => i.SpeedRunID = speedRun.ID);
                        db.InsertBatch<SpeedRunVariableValueEntity>(variableValuesBatch);

                        playersBatch.ForEach(i => i.SpeedRunID = speedRun.ID);
                        db.InsertBatch<SpeedRunPlayerEntity>(playersBatch);

                        videosBatch.ForEach(i => i.SpeedRunID = speedRun.ID);
                        db.InsertBatch<SpeedRunVideoEntity>(videosBatch);

                        tran.Complete();
                    }
                }

                _logger.Information("Saved speedRuns {@Count} / {@Total}", count, speedRunsList.Count);
                count++;
            }
        }

        public IEnumerable<SpeedRunSpeedRunComIDEntity> GetSpeedRunSpeedRunComIDs(Expression<Func<SpeedRunSpeedRunComIDEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<SpeedRunSpeedRunComIDEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public void CreateFullTables()
        {
            _logger.Information("Started CreateFullTables");

            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.Execute("EXEC dbo.ImportCreateFullTables");
            }

            _logger.Information("Completed CreateFullTables");
        }

        public void RenameFullTables()
        {
            _logger.Information("Started RenameFullTables");

            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.OneTimeCommandTimeout = 32767;
                    db.Execute("EXEC dbo.ImportRenameFullTables");
                    tran.Complete();
                }
            }

            _logger.Information("Completed RenameFullTables");
        }

        public void RebuildIndexes()
        {
            _logger.Information("Started RebuildIndexes");

            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                db.Execute("EXEC ImportRebuildIndexes");
            }

            _logger.Information("Completed RebuildIndexes");
        }

        public void UpdateSpeedRunRanks(DateTime lastImportDate)
        {
            _logger.Information("Started UpdateSpeedRunRanks {@LastImportDate}", lastImportDate);

            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                db.Execute("EXEC dbo.UpdateSpeedRunRanks @0", lastImportDate);
            }

            _logger.Information("Completed UpdateSpeedRunRanks");
        }
    }
}
