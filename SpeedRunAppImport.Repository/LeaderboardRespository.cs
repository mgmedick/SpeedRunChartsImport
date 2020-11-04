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
    public class LeaderboardRespository : BaseRepository//, IUserRepository
    {
        private readonly ILogger _logger;

        public LeaderboardRespository(ILogger logger)
        {
            _logger = logger;
        }

        public void CopyLeaderboardTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"IF OBJECT_ID('dbo.tbl_Leaderboard_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_Leaderboard_Full
                               
                                SELECT TOP 0 * INTO dbo.tbl_Leaderboard_Full FROM dbo.tbl_Leaderboard

                                ALTER TABLE [dbo].[tbl_Leaderboard_Full] ADD CONSTRAINT [PK_tbl_Leaderboard_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_Leaderboard_Full] ADD CONSTRAINT [DF_tbl_Leaderboard_Full_ImportedDate] DEFAULT GETDATE() FOR [ImportedDate]");
                    tran.Complete();
                }
            }
        }

        public void RenameAndDropLeaderboardTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"EXEC sp_rename 'dbo.tbl_Leaderboard', 'tbl_Leaderboard_ToRemove'

                                EXEC sp_rename 'dbo.tbl_Leaderboard_Full', 'tbl_Leaderboard'

                                DROP TABLE dbo.tbl_Leaderboard_ToRemove

                                EXEC sp_rename 'dbo.PK_tbl_Leaderboard_Full', 'PK_tbl_Leaderboard_Full'
                                EXEC sp_rename 'dbo.DF_tbl_Leaderboard_Full_ImportedDate', 'DF_tbl_Leaderboard_Full_ImportedDate'");
                    tran.Complete();
                }
            }
        }

        public void InsertLeaderboards(IEnumerable<LeaderboardEntity> leaderboards)
        {
            _logger.Information("Started InsertLeaderboards");
            int batchCount = 0;
            var leaderboardsList = leaderboards.ToList();
            while (batchCount < leaderboardsList.Count)
            {
                var leaderboardsBatch = leaderboardsList.Skip(batchCount).Take(MaxBulkRows).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBulk<LeaderboardEntity>(leaderboardsBatch);
                        tran.Complete();
                    }
                }

                _logger.Information("Saved users {@Count} / {@Total}", leaderboardsBatch.Count, leaderboardsList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertLeaderboards");
        }
    }
}
