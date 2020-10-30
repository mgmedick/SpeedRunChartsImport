using System;
using System.Collections.Generic;
using NPoco;
using System.Linq;
using SpeedRunApp.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;

namespace SpeedRunAppImport.Repository
{
    public class UserRespository : BaseRepository, IUserRepository
    {
        public UserRespository()
        {
        }

        public void CopyUserTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"IF OBJECT_ID('dbo.tbl_User_Full') IS NOT NULL 
                                    DROP TABLE dbo.tbl_User_Full
                               
                                SELECT TOP 0 * INTO dbo.tbl_User_Full FROM dbo.tbl_User");
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
                                ALTER TABLE [dbo].[tbl_User] ADD CONSTRAINT [PK_tbl_User] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
                    tran.Complete();
                }
            }
        }

        public void InsertUsers(IEnumerable<UserEntity> users)
        {
            try
            {
                int batchCount = 0;
                while (batchCount < users.Count())
                {
                    var usersBatch = users.Skip(batchCount).Take(MaxBulkRows).ToList();

                    using (IDatabase db = DBFactory.GetDatabase())
                    {
                        using (var tran = db.GetTransaction())
                        {
                            db.InsertBatch<UserEntity>(usersBatch);
                            tran.Complete();
                        }
                    }

                    batchCount += MaxBulkRows;
                }
            }
            catch (Exception ex)
            {

            }            
        }
    }
}
