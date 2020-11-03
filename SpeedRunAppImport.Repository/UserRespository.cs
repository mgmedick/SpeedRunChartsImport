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
    public class UserRespository : BaseRepository, IUserRepository
    {
        private readonly ILogger _logger;

        public UserRespository(ILogger logger)
        {
            _logger = logger;
        }

        public void CopyUserTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"IF OBJECT_ID('dbo.tbl_User_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_User_Full
                               
                                SELECT TOP 0 * INTO dbo.tbl_User_Full FROM dbo.tbl_User

                                ALTER TABLE [dbo].[tbl_User_Full] ADD CONSTRAINT [PK_tbl_User_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                ALTER TABLE [dbo].[tbl_User_Full] ADD CONSTRAINT [DF_tbl_User_Full_ImportedDate] DEFAULT GETDATE() FOR [ImportedDate]");
                    tran.Complete();
                }
            }
        }

        public void RenameAndDropUserTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"EXEC sp_rename 'dbo.tbl_User', 'tbl_User_ToRemove'

                                EXEC sp_rename 'dbo.tbl_User_Full', 'tbl_User'

                                DROP TABLE dbo.tbl_User_ToRemove

                                EXEC sp_rename 'dbo.PK_tbl_User_Full', 'PK_tbl_User'
                                EXEC sp_rename 'dbo.DF_tbl_User_Full_ImportedDate', 'DF_tbl_User_ImportedDate'");
                    tran.Complete();
                }
            }
        }

        public void InsertUsers(IEnumerable<UserEntity> users)
        {
            _logger.Information("Started InsertGames");
            int batchCount = 0;
            var usersList = users.ToList();
            while (batchCount < usersList.Count)
            {
                var usersBatch = usersList.Skip(batchCount).Take(MaxBulkRows).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBulk<UserEntity>(usersBatch);
                        tran.Complete();
                    }
                }

                _logger.Information("Saved users {@Count} / {@Total}", usersBatch.Count, usersList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertGames");
        }
    }
}
