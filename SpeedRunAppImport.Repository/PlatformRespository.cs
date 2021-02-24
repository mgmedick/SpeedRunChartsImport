using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using System.Linq;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Repository
{
    public class PlatformRespository : BaseRepository, IPlatformRepository
    {
        private readonly ILogger _logger;

        public PlatformRespository(ILogger logger)
        {
            _logger = logger;
        }

        /*
        public void CopyPlatformTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"--tbl_Platform_Full
                                IF OBJECT_ID('dbo.tbl_Platform_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Platform_Full
                                END 

                                CREATE TABLE [dbo].[tbl_Platform_Full] 
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1), 
                                    [Name] [varchar] (50) NOT NULL,
                                    [YearOfRelease] [int] NULL,
                                    [ImportedDate] [datetime] NOT NULL CONSTRAINT [DF_tbl_Platform_Full_ImportedDate] DEFAULT(GETDATE())
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_Platform_Full] ADD CONSTRAINT [PK_tbl_Platform_Full] PRIMARY KEY NONCLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                --tbl_Platform_SpeedRunComID_Full
                                IF OBJECT_ID('dbo.tbl_Platform_SpeedRunComID_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_Platform_SpeedRunComID_Full
                                END 

                                CREATE TABLE [dbo].[tbl_Platform_SpeedRunComID_Full] 
                                (
	                                [PlatformID] [int] NOT NULL,
                                    [SpeedRunComID] [varchar] (10) NOT NULL
                                )
                                ALTER TABLE [dbo].[tbl_Platform_SpeedRunComID_Full] ADD CONSTRAINT [PK_tbl_Platform_SpeedRunComID_Full] PRIMARY KEY CLUSTERED ([PlatformID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
                    tran.Complete();
                }
            }
        }

        public void RenameAndDropPlatformTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.OneTimeCommandTimeout = 32767;
                    db.Execute(@"--tbl_Platform
                                ALTER TABLE [dbo].[tbl_Game_Platform] DROP CONSTRAINT [FK_tbl_Game_Platform_tbl_Platform]
                                ALTER TABLE [dbo].[tbl_SpeedRun_System] DROP CONSTRAINT [FK_tbl_SpeedRun_System_tbl_Platform]

                                DROP TABLE dbo.tbl_Platform

                                EXEC sp_rename 'dbo.PK_tbl_Platform_Full', 'PK_tbl_Platform'
                                EXEC sp_rename 'dbo.DF_tbl_Platform_Full_ImportedDate', 'DF_tbl_Platform_ImportedDate'
                                EXEC sp_rename 'dbo.tbl_Platform_Full', 'tbl_Platform'

                                DELETE FROM dbo.[tbl_Game_Platform] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_Platform p WHERE p.ID = PlatformID)                           
                                ALTER TABLE [dbo].[tbl_Game_Platform] ADD CONSTRAINT [FK_tbl_Game_Platform_tbl_Platform] FOREIGN KEY ([PlatformID]) REFERENCES [dbo].[tbl_Platform] ([ID])

                                DELETE FROM dbo.[tbl_SpeedRun_System] WHERE PlatformID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.tbl_Platform p WHERE p.ID = PlatformID)                           
                                ALTER TABLE [dbo].[tbl_SpeedRun_System] ADD CONSTRAINT [FK_tbl_SpeedRun_System_tbl_Platform] FOREIGN KEY ([PlatformID]) REFERENCES [dbo].[tbl_Platform] ([ID])

                                --tbl_Platform_SpeedRunComID
                                DROP TABLE dbo.tbl_Platform_SpeedRunComID
                                
                                EXEC sp_rename 'dbo.PK_tbl_Platform_SpeedRunComID_Full', 'PK_tbl_Platform_SpeedRunComID'                                
                                EXEC sp_rename 'dbo.tbl_Platform_SpeedRunComID_Full', 'tbl_Platform_SpeedRunComID'

                                CREATE NONCLUSTERED INDEX [IDX_tbl_Platform_SpeedRunComID_SpeedRunComID] ON [dbo].[tbl_Platform_SpeedRunComID] ([SpeedRunComID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
                    tran.Complete();
                }
            }
        }
        */

        public void InsertPlatforms(IEnumerable<PlatformEntity> platforms)
        {
            _logger.Information("Started InsertPlatforms");
            int batchCount = 0;
            var platformsList = platforms.ToList();

            while (batchCount < platformsList.Count)
            {
                var platformsBatch = platformsList.Skip(batchCount).Take(MaxBulkRows).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBulk<PlatformEntity>(platformsBatch);
                        var platformIDs = db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_Platform_Full ORDER BY ID DESC", platformsBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < platformsBatch.Count; i++)
                        {
                            platformsBatch[i].ID = platformIDs[i];
                        }

                        var platformSpeedRunComIDsBatch = platformsBatch.Select(i => new PlatformSpeedRunComIDEntity { PlatformID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<PlatformSpeedRunComIDEntity>(platformSpeedRunComIDsBatch);

                        tran.Complete();
                    }
                }

                _logger.Information("Saved platforms {@Count} / {@Total}", platformsBatch.Count, platformsList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertPlatforms");
        }

        public void SavePlatforms(IEnumerable<PlatformEntity> platforms)
        {
            int count = 1;
            var platformsList = platforms.ToList();

            foreach (var platform in platformsList)
            {
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        if (platform.ID != 0)
                        {
                            db.DeleteWhere<PlatformSpeedRunComIDEntity>("PlatformID = @platformID", new { platformID = platform.ID });
                        }

                        db.Save<PlatformEntity>(platform);

                        var platformSpeedRunComID = new PlatformSpeedRunComIDEntity { PlatformID = platform.ID, SpeedRunComID = platform.SpeedRunComID };
                        db.Insert<PlatformSpeedRunComIDEntity>(platformSpeedRunComID);
                    }
                }

                _logger.Information("Saved Platforms {@Count} / {@Total}", count, platformsList.Count);
                count++;
            }
        }

        public IEnumerable<PlatformSpeedRunComIDEntity> GetPlatformSpeedRunComIDs(Expression<Func<PlatformSpeedRunComIDEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<PlatformSpeedRunComIDEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }
    }
}
