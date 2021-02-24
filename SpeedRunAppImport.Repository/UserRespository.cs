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

        /*
        public void CopyUserTables()
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                using (var tran = db.GetTransaction())
                {
                    db.Execute(@"--tbl_User_Full
                                 IF OBJECT_ID('dbo.tbl_User_Full') IS NOT NULL
                                 BEGIN
                                    DROP TABLE dbo.tbl_User_Full
                                 END

                                CREATE TABLE [dbo].[tbl_User_Full]
                                ( 
                                    [ID] [int] NOT NULL IDENTITY(1,1), 
                                    [Name] [varchar] (100) NOT NULL,
	                                [UserRoleID] [int] NOT NULL,
                                    [SignUpDate] [datetime] NULL,  
                                    [ImportedDate] [datetime] NOT NULL CONSTRAINT [DF_tbl_User_Full_ImportedDate] DEFAULT(GETDATE()),
	                                [ModifiedDate] [datetime] NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_User_Full] ADD CONSTRAINT [PK_tbl_User_Full] PRIMARY KEY CLUSTERED ([ID]) WITH (FILLFACTOR=90) ON [PRIMARY] 

                                --tbl_User_SpeedRunComID_Full
                                IF OBJECT_ID('dbo.tbl_User_SpeedRunComID_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_User_SpeedRunComID_Full
                                END 

                                CREATE TABLE [dbo].[tbl_User_SpeedRunComID_Full] 
                                (
	                                [UserID] [int] NOT NULL,
                                    [SpeedRunComID] [varchar] (10) NOT NULL
                                )
                                ALTER TABLE [dbo].[tbl_User_SpeedRunComID_Full] ADD CONSTRAINT [PK_tbl_User_SpeedRunComID_Full] PRIMARY KEY CLUSTERED ([UserID]) WITH (FILLFACTOR=90) ON [PRIMARY]                               

                                --tbl_User_Location_Full
                                IF OBJECT_ID('dbo.tbl_User_Location_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_User_Location_Full
                                END 

                                CREATE TABLE [dbo].[tbl_User_Location_Full]
                                ( 
                                    [UserID] [int] NOT NULL,
                                    [Location] [varchar] (100) NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_User_Location_Full] ADD CONSTRAINT [PK_tbl_User_Location_Full] PRIMARY KEY CLUSTERED ([UserID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                --tbl_User_Link_Full
                                IF OBJECT_ID('dbo.tbl_User_Link_Full') IS NOT NULL 
                                BEGIN
                                    DROP TABLE dbo.tbl_User_Link_Full
                                END 

                                CREATE TABLE [dbo].[tbl_User_Link_Full] 
                                ( 
                                    [UserID] [int] NOT NULL,
	                                [SpeedRunComUrl] [varchar] (1000) NULL, 
                                    [ProfileImageUrl] [varchar] (1000) NULL,
                                    [TwitchProfileUrl] [varchar] (1000) NULL,
                                    [HitboxProfileUrl] [varchar] (1000) NULL,
                                    [YoutubeProfileUrl] [varchar] (1000) NULL,
                                    [TwitterProfileUrl] [varchar] (1000) NULL,
                                    [SpeedRunsLiveProfileUrl] [varchar] (1000) NULL
                                ) ON [PRIMARY] 
                                ALTER TABLE [dbo].[tbl_User_Link_Full] ADD CONSTRAINT [PK_tbl_User_Link_Full] PRIMARY KEY CLUSTERED ([UserID]) WITH (FILLFACTOR=90) ON [PRIMARY]");
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
                    db.Execute(@"--tbl_User
                                ALTER TABLE [dbo].[tbl_SpeedRun_Player] DROP CONSTRAINT [FK_tbl_SpeedRun_Player_tbl_User]
                                ALTER TABLE [dbo].[tbl_Game_Moderator] DROP CONSTRAINT [FK_tbl_Game_Moderator_tbl_User]
                                ALTER TABLE [dbo].[tbl_SpeedRun_Status] DROP CONSTRAINT [FK_tbl_SpeedRun_Status_tbl_User]
                                DROP TABLE dbo.tbl_User

                                EXEC sp_rename 'dbo.PK_tbl_User_Full', 'PK_tbl_User'                                
                                EXEC sp_rename 'dbo.DF_tbl_User_Full_ImportedDate', 'DF_tbl_User_ImportedDate'
                                EXEC sp_rename 'dbo.tbl_User_Full', 'tbl_User'

                                ALTER TABLE [dbo].[tbl_User] ADD CONSTRAINT [FK_tbl_User_tbl_UserRole] FOREIGN KEY ([UserRoleID]) REFERENCES [dbo].[tbl_UserRole] ([ID])
                                CREATE NONCLUSTERED INDEX [IDX_tbl_User_UserRoleID] ON [dbo].[tbl_User] ([UserRoleID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                DELETE FROM dbo.[tbl_SpeedRun_Player] WHERE UserID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.tbl_User u WHERE u.ID = UserID)                           
                                ALTER TABLE [dbo].[tbl_SpeedRun_Player] ADD CONSTRAINT [FK_tbl_SpeedRun_Player_tbl_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[tbl_User] ([ID])

                                DELETE FROM dbo.[tbl_Game_Moderator] WHERE NOT EXISTS (SELECT 1 FROM dbo.tbl_User u WHERE u.ID = UserID)                           
                                ALTER TABLE [dbo].[tbl_Game_Moderator] ADD CONSTRAINT [FK_tbl_Game_Moderator_tbl_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[tbl_User] ([ID])

                                DELETE FROM dbo.[tbl_SpeedRun_Status] WHERE ExaminerUserID IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.tbl_User u WHERE u.ID = ExaminerUserID)                           
                                ALTER TABLE [dbo].[tbl_SpeedRun_Status] ADD CONSTRAINT [FK_tbl_SpeedRun_Status_tbl_User] FOREIGN KEY ([ExaminerUserID]) REFERENCES [dbo].[tbl_User] ([ID])

                                --tbl_User_SpeedRunComID
                                DROP TABLE dbo.tbl_User_SpeedRunComID
                                
                                EXEC sp_rename 'dbo.PK_tbl_User_SpeedRunComID_Full', 'PK_tbl_User_SpeedRunComID'  
                                EXEC sp_rename 'dbo.tbl_User_SpeedRunComID_Full', 'tbl_User_SpeedRunComID'

                                CREATE NONCLUSTERED INDEX [IDX_tbl_User_SpeedRunComID_SpeedRunComID] ON [dbo].[tbl_User_SpeedRunComID] ([SpeedRunComID]) WITH (FILLFACTOR=90) ON [PRIMARY]

                                --tbl_User_Location
                                DROP TABLE dbo.tbl_User_Location
                                 
                                EXEC sp_rename 'dbo.PK_tbl_User_Location_Full', 'PK_tbl_User_Location'  
                                EXEC sp_rename 'dbo.tbl_User_Location_Full', 'tbl_User_Location'

                                --tbl_User_Link
                                DROP TABLE dbo.tbl_User_Link
                                 
                                EXEC sp_rename 'dbo.PK_tbl_User_Link_Full', 'PK_tbl_User_Link'  
                                EXEC sp_rename 'dbo.tbl_User_Link_Full', 'tbl_User_Link'");
                    tran.Complete();
                }
            }
        }
        */

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
                        //foreach (var user in usersBatch)
                        //{
                        //    db.Insert(user);
                        //}
                        db.InsertBulk<UserEntity>(usersBatch);
                        var userIDs = db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_User_Full ORDER BY ID DESC", usersBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < usersBatch.Count; i++)
                        {
                            usersBatch[i].ID = userIDs[i];
                        }

                        var userSpeedRunComIDsBatch = usersBatch.Select(i => new UserSpeedRunComIDEntity { UserID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBulk<UserSpeedRunComIDEntity>(userSpeedRunComIDsBatch);

                        userLocationsBatch.ForEach(i => i.UserID = usersBatch.Find(g => g.SpeedRunComID == i.UserSpeedRunComID).ID);
                        db.InsertBulk<UserLocationEntity>(userLocationsBatch);

                        userLinksBatch.ForEach(i => i.UserID = usersBatch.Find(g => g.SpeedRunComID == i.UserSpeedRunComID).ID);
                        db.InsertBulk<UserLinkEntity>(userLinksBatch);

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

            foreach (var user in usersList)
            {
                var userLocation = userLocations.FirstOrDefault(i => i.UserSpeedRunComID == user.SpeedRunComID);
                var userLink = userLinks.FirstOrDefault(i => i.UserSpeedRunComID == user.SpeedRunComID);

                using (IDatabase db = DBFactory.GetDatabase())
                {
                    using (var tran = db.GetTransaction())
                    {
                        if (user.ID != 0)
                        {
                            user.ModifiedDate = DateTime.UtcNow;
                            db.DeleteWhere<UserSpeedRunComIDEntity>("UserID = @userID", new { userID = user.ID });
                            db.DeleteWhere<UserLocationEntity>("UserID = @userID", new { userID = user.ID });
                            db.DeleteWhere<UserLinkEntity>("UserID = @userID", new { userID = user.ID });
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
                }

                _logger.Information("Saved users {@Count} / {@Total}", count, usersList.Count);
                count++;
            }
        }

        public IEnumerable<UserSpeedRunComIDEntity> GetUserSpeedRunComIDs(Expression<Func<UserSpeedRunComIDEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<UserSpeedRunComIDEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }
    }
}
