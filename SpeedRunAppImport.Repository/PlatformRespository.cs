using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using System.Linq;
using SpeedRunApp.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

namespace SpeedRunAppImport.Repository
{
    public class PlatformRespository : BaseRepository, IPlatformRepository
    {
        private readonly ILogger _logger;

        public PlatformRespository(ILogger logger)
        {
            _logger = logger;
        }

        public void CopyPlatformTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"IF OBJECT_ID('dbo.tbl_Platform_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Platform_Full
                               
                                SELECT TOP 0 * INTO dbo.tbl_Platform_Full FROM dbo.tbl_Platform

                                ALTER TABLE [dbo].[tbl_Platform_Full] ADD CONSTRAINT [PK_tbl_Platform_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Platform_Full] ADD CONSTRAINT [DF_tbl_Platform_Full_ImportedDate] DEFAULT GETDATE() FOR [ImportedDate]");
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
                    db.Execute(@"EXEC sp_rename 'dbo.tbl_Platform', 'tbl_Platform_ToRemove'

                                EXEC sp_rename 'dbo.tbl_Platform_Full', 'tbl_Platform'

                                DROP TABLE dbo.tbl_Platform_ToRemove

                                EXEC sp_rename 'dbo.PK_tbl_Platform_Full', 'PK_tbl_Platform'
                                EXEC sp_rename 'dbo.DF_tbl_Platform_Full_ImportedDate', 'DF_tbl_Platform_ImportedDate'");
                    tran.Complete();
                }
            }
        }

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
                        tran.Complete();
                    }
                }

                _logger.Information("Saved platforms {@Count} / {@Total}", platformsBatch.Count, platformsList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertPlatforms");
        }
    }
}
