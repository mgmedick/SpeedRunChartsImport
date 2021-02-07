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
                    db.OneTimeCommandTimeout = 32767;
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

        public void InsertUsers(IEnumerable<UserEntity> users, IEnumerable<UserLocationEntity> userLocations, IEnumerable<UserLinkEntity> userLinks)
        {
            _logger.Information("Started InsertUsers");
            int batchCount = 0;
            var usersList = users.ToList();

            while (batchCount < usersList.Count)
            {
                var usersBatch = usersList.Skip(batchCount).Take(MaxBulkRows).ToList();
                var userSpeedRunComIDs = usersBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                var userLocationsBatch = userLocations.Where(i => userSpeedRunComIDs.Contains(i.UserSpeedRunComID)).ToList();
                var userLinksBatch = userLinks.Where(i => userSpeedRunComIDs.Contains(i.UserSpeedRunComID)).ToList();

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBatch<UserEntity>(usersBatch);

                        var userSpeedRunComIDsBatch = usersBatch.Select(i => new UserSpeedRunComIDEntity { UserID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<UserSpeedRunComIDEntity>(userSpeedRunComIDsBatch);

                        userLocationsBatch.ForEach(i => i.UserID = usersBatch.Find(g => g.SpeedRunComID == i.UserSpeedRunComID).ID);
                        db.InsertBatch<UserLocationEntity>(userLocationsBatch);

                        userLinksBatch.ForEach(i => i.UserID = usersBatch.Find(g => g.SpeedRunComID == i.UserSpeedRunComID).ID);
                        db.InsertBatch<UserLinkEntity>(userLinksBatch);

                        tran.Complete();
                    }
                }

                _logger.Information("Saved users {@Count} / {@Total}", usersBatch.Count, usersList.Count);
                batchCount += MaxBulkRows;
            }
            _logger.Information("Completed InsertUsers");
        }

        public void SaveUsers(IEnumerable<UserEntity> users, IEnumerable<UserLocationEntity> userLocations, IEnumerable<UserLinkEntity> userLinks)
        {
            int count = 1;
            var usersList = users.ToList();
            var userSpeedRunComIDs = GetUserSpeedRunComIDs();

            foreach (var user in usersList)
            {
                var userLocation = userLocations.FirstOrDefault(i => i.UserSpeedRunComID == user.SpeedRunComID);
                var userLink = userLinks.FirstOrDefault(i => i.UserSpeedRunComID == user.SpeedRunComID);

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    var userSpeedRunCom = userSpeedRunComIDs.FirstOrDefault(i => i.SpeedRunComID == user.SpeedRunComID);
                    if (userSpeedRunCom != null)
                    {
                        user.ModifiedDate = DateTime.Now;
                        db.DeleteWhere<UserSpeedRunComIDEntity>("UserID = @userID", new { userID = userSpeedRunCom.UserID });
                        db.DeleteWhere<UserLocationEntity>("UserID = @userID", new { userID = userSpeedRunCom.UserID });
                        db.DeleteWhere<UserLinkEntity>("UserID = @userID", new { userID = userSpeedRunCom.UserID });
                    }

                    db.Save<UserEntity>(user);

                    var userSpeedRunComID = new UserSpeedRunComIDEntity { UserID = user.ID, SpeedRunComID = user.SpeedRunComID };
                    db.Insert<UserSpeedRunComIDEntity>(userSpeedRunComID);

                    if (userLocation != null)
                    {
                        userLocation.UserID = user.ID;
                        db.Insert<UserLocationEntity>(userLocation);
                    }

                    if (userLink != null)
                    {
                        userLink.UserID = user.ID;
                        db.Insert<UserLinkEntity>(userLink);
                    }
                }

                _logger.Information("Saved users {@Count} / {@Total}", count, usersList.Count);
                count++;
            }
        }

        public IEnumerable<UserSpeedRunComIDEntity> GetUserSpeedRunComIDs()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                return db.Query<UserSpeedRunComIDEntity>("SELECT UserID, SpeedRunComID FROM dbo.tbl_User_SpeedRunComID WITH(NOLOCK)").ToList();
            }
        }
    }
}
