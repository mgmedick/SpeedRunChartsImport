using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using System.Linq;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;

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

                                EXEC sp_rename 'dbo.DF_tbl_User_Full_ImportedDate', 'DF_tbl_User_ImportedDate'

                                ALTER TABLE [dbo].[tbl_User] ADD CONSTRAINT [PK_tbl_User] PRIMARY KEY NONCLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE CLUSTERED INDEX [IDX_tbl_User_OrderValue] ON [dbo].[tbl_User] ([OrderValue]) WITH (FILLFACTOR=90) ON [PRIMARY]
                                CREATE NONCLUSTERED INDEX [IDX_tbl_User_Name] ON [dbo].[tbl_User] ([Name]) WITH (FILLFACTOR=90) ON [PRIMARY]");
                    tran.Complete();
                }
            }
        }

        public void InsertUsers(IEnumerable<UserEntity> users)
        {
            _logger.Information("Started InsertUsers");
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
            _logger.Information("Completed InsertUsers");
        }

        public void SaveUsers(IEnumerable<UserEntity> users)
        {
            int count = 1;
            var usersList = users.ToList();
            foreach (var user in usersList)
            {
                using (IDatabase db = DBFactory.GetDatabase())
                {
                    db.Save<UserEntity>(user);
                }

                _logger.Information("Saved users {@Count} / {@Total}", count, usersList.Count);
                count++;
            }
        }
    }
}
