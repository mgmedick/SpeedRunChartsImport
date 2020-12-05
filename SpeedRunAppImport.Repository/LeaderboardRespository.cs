using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using System.Linq;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Repository
{
    public class LeaderboardRespository : BaseRepository, ILeaderboardRepository
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
                                ALTER TABLE [dbo].[tbl_Leaderboard_Full] ADD CONSTRAINT [DF_tbl_Leaderboard_Full_ImportedDate] DEFAULT GETDATE() FOR [ImportedDate]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Leaderboard_Full_SpeedRunID] ON [dbo].[tbl_Leaderboard_Full] ([SpeedRunID]) WITH (FILLFACTOR=90) ON [PRIMARY] 
                                CREATE NONCLUSTERED INDEX [IDX_tbl_Leaderboard_Full_GameID_PlusInclude] ON [dbo].[tbl_Leaderboard_Full] ([GameID]) INCLUDE ([CategoryID], [LevelID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
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
                                EXEC sp_rename 'dbo.DF_tbl_Leaderboard_Full_ImportedDate', 'DF_tbl_Leaderboard_Full_ImportedDate'
                                EXEC sp_rename 'dbo.IDX_tbl_Leaderboard_Full_SpeedRunID', 'IDX_tbl_Leaderboard_SpeedRunID'
                                EXEC sp_rename 'dbo.IDX_tbl_Leaderboard_Full_GameID_PlusInclude', 'IDX_tbl_Leaderboard_GameID_PlusInclude'");
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

                _logger.Information("Saved leaderboards {@Count} / {@Total}", leaderboardsBatch.Count, leaderboardsList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertLeaderboards");
        }

        public void SaveLeaderboards(IEnumerable<LeaderboardEntity> leaderboards)
        {
            int count = 1;
            var leaderboardKeys = leaderboards.Select(g => new { g.GameID, g.CategoryID, g.LevelID }).Distinct().ToList();
            foreach (var leaderboardKey in leaderboardKeys)
            {
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.DeleteWhere<LeaderboardEntity>("GameID = @GameID AND CategoryID = @CategoryID AND ISNULL(LevelID,'') = ISNULL(@LevelID,'')", new { GameID = leaderboardKey.GameID, CategoryID = leaderboardKey.CategoryID, LevelID = leaderboardKey.LevelID });
                        db.InsertBulk<LeaderboardEntity>(leaderboards.Where(i => i.GameID == leaderboardKey.GameID && i.CategoryID == leaderboardKey.CategoryID && i.LevelID == leaderboardKey.LevelID).ToList());
                        tran.Complete();
                    }
                }

                _logger.Information("Saved leaderboards {@Count} / {@Total}", count, leaderboardKeys.Count);
                count++;
            }
        }

        public IEnumerable<LeaderboardKeyEntity> GetLeaderboardKeys(DateTime lastImportedDate, int statusID)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<LeaderboardKeyEntity>("SELECT GameID, CategoryID, LevelID " +
                                                    "FROM dbo.tbl_SpeedRun rn " +
                                                    "WHERE ISNULL(rn.ModifiedDate, rn.ImportedDate) >= @0 " +
                                                    "AND rn.StatusTypeID = @1 " +
                                                    "GROUP BY GameID, CategoryID, LevelID " +
                                                    "ORDER BY GameID, CategoryID, LevelID", lastImportedDate, statusID).ToList();
            }
        }
    }
}
