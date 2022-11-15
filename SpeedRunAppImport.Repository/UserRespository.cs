using System;
using System.Collections.Generic;
using NPoco;
using Serilog;
using System.Linq;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Repositories;
//using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using System.IO;
using SpeedRunAppImport.Model.Data;

namespace SpeedRunAppImport.Repository
{
    public class UserRespository : BaseRepository, IUserRepository
    {
        private readonly ILogger _logger;

        public UserRespository(ILogger logger)
        {
            _logger = logger;
        }

        public void InsertUsers(IEnumerable<UserEntity> users, IEnumerable<UserLocationEntity> userLocations, IEnumerable<UserLinkEntity> userLinks)
        {
            _logger.Information("Started InsertUsers");
            int batchCount = 0;
            var usersList = users.ToList();

            using (IDatabase db = DBFactory.GetDatabase())
            {
                while (batchCount < usersList.Count)
                {
                    var usersBatch = usersList.Skip(batchCount).Take(MaxBulkRows).ToList();
                    var userSpeedRunComIDs = usersBatch.Select(i => i.SpeedRunComID).Distinct().ToList();
                    var userLocationsBatch = userLocations.Where(i => userSpeedRunComIDs.Contains(i.UserSpeedRunComID)).ToList();
                    var userLinksBatch = userLinks.Where(i => userSpeedRunComIDs.Contains(i.UserSpeedRunComID)).ToList();

                    using (var tran = db.GetTransaction())
                    {
                        db.InsertBatch<UserEntity>(usersBatch);
                        var userIDs = IsMySQL ? db.Query<int>("SELECT ID FROM tbl_User_Full ORDER BY ID DESC LIMIT @0;", usersBatch.Count).Reverse().ToArray() :
                                                db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_User_Full ORDER BY ID DESC", usersBatch.Count).Reverse().ToArray();
                        for (int i = 0; i < usersBatch.Count; i++)
                        {
                            usersBatch[i].ID = userIDs[i];
                        }

                        var userSpeedRunComIDsBatch = usersBatch.Select(i => new UserSpeedRunComIDEntity { UserID = i.ID, SpeedRunComID = i.SpeedRunComID }).ToList();
                        db.InsertBatch<UserSpeedRunComIDEntity>(userSpeedRunComIDsBatch);

                        userLocationsBatch.ForEach(i => i.UserID = usersBatch.Find(g => g.SpeedRunComID == i.UserSpeedRunComID).ID);
                        db.InsertBatch<UserLocationEntity>(userLocationsBatch);

                        userLinksBatch.ForEach(i => i.UserID = usersBatch.Find(g => g.SpeedRunComID == i.UserSpeedRunComID).ID);
                        db.InsertBatch<UserLinkEntity>(userLinksBatch);

                        tran.Complete();
                    }

                    _logger.Information("Saved users {@Count} / {@Total}", usersBatch.Count, usersList.Count);
                    batchCount += MaxBulkRows;
                }
            }
            _logger.Information("Completed InsertUsers");
        }

        public void InsertGuests(IEnumerable<GuestEntity> guests, IEnumerable<GuestLinkEntity> guestLinks)
        {
            _logger.Information("Started InsertGuests");
            int batchCount = 0;
            var guestList = guests.ToList();

            using (IDatabase db = DBFactory.GetDatabase())
            {
                while (batchCount < guestList.Count)
                {
                    var guestsBatch = guestList.Skip(batchCount).Take(MaxBulkRows).ToList();
                    var guestSpeedRunComIDs = guestsBatch.Select(i => i.Name).Distinct().ToList();
                    var guestAbbrs = guestsBatch.Select(i => i.Abbr).Distinct().ToList();
                    var guestLinksBatch = guestLinks.Where(i => guestSpeedRunComIDs.Contains(i.GuestSpeedRunComID)).ToList();

                    using (var tran = db.GetTransaction())
                    {
                        try
                        { 
                            db.InsertBatch<GuestEntity>(guestsBatch);
                            var guestIDs = IsMySQL ? db.Query<int>("SELECT ID FROM tbl_Guest_Full ORDER BY ID DESC LIMIT @0;", guestsBatch.Count).Reverse().ToArray() :
                                                     db.Query<int>("SELECT TOP (@0) ID FROM dbo.tbl_Guest_Full ORDER BY ID DESC", guestsBatch.Count).Reverse().ToArray();
                            for (int i = 0; i < guestsBatch.Count; i++)
                            {
                                guestsBatch[i].ID = guestIDs[i];
                            }

                            guestLinksBatch.ForEach(i => i.GuestID = guestsBatch.Find(g => g.Name == i.GuestSpeedRunComID).ID);
                            db.InsertBatch<GuestLinkEntity>(guestLinksBatch);

                            tran.Complete();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "InsertGuests GuestNames: {@GuestSpeedRunComID}, GuestAbbrs: {@GuestAbbrs}", string.Join(",", guestSpeedRunComIDs), string.Join(",", guestAbbrs));
                        }
                    }

                    _logger.Information("Saved guests {@Count} / {@Total}", guestsBatch.Count, guestList.Count);
                    batchCount += MaxBulkRows;
                }
            }
            _logger.Information("Completed InsertGuests");
        }

        public void SaveGuests(IEnumerable<GuestEntity> guests, IEnumerable<GuestLinkEntity> guestLinks)
        {
            _logger.Information("Started SaveGuests");
            int count = 1;
            var guestList = guests.ToList();

            using (IDatabase db = DBFactory.GetDatabase())
            {
                foreach (var guest in guestList)
                {
                    var guestLink = guestLinks.FirstOrDefault(i => i.GuestSpeedRunComID == guest.Name);

                    using (var tran = db.GetTransaction())
                    {
                        try
                        {
                            db.Save<GuestEntity>(guest);

                            if (guestLink != null)
                            {
                                guestLink.GuestID = guest.ID;
                                db.Save<GuestLinkEntity>(guestLink);
                            }
                            tran.Complete();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "SaveGuests GuestID: {@GuestID}, GameSpeedRunComID: {@GuestSpeedRunComID}", guest.ID, guest.Name);
                        }
                    }

                    _logger.Information("Saved guests {@Count} / {@Total}", count, guestList.Count);
                    count++;
                }
            }
            _logger.Information("Completed SaveGuests");
        }

        public void SaveUsers(IEnumerable<UserEntity> users, IEnumerable<UserLocationEntity> userLocations, IEnumerable<UserLinkEntity> userLinks)
        {
            int count = 1;
            var usersList = users.ToList();

            using (IDatabase db = DBFactory.GetDatabase())
            {
                foreach (var user in usersList)
                {
                    var userLocation = userLocations.FirstOrDefault(i => i.UserSpeedRunComID == user.SpeedRunComID);
                    var userLink = userLinks.FirstOrDefault(i => i.UserSpeedRunComID == user.SpeedRunComID);

                    using (var tran = db.GetTransaction())
                    {
                        try
                        {
                            if (user.ID != 0)
                            {
                                user.ModifiedDate = DateTime.UtcNow;
                                user.IsChanged = null;
                                //db.DeleteWhere<UserSpeedRunComIDEntity>("UserID = @userID", new { userID = user.ID });
                                //db.DeleteWhere<UserLocationEntity>("UserID = @userID", new { userID = user.ID });
                                //db.DeleteWhere<UserLinkEntity>("UserID = @userID", new { userID = user.ID });
                            }

                            db.Save<UserEntity>(user);

                            var userSpeedRunComID = new UserSpeedRunComIDEntity { UserID = user.ID, SpeedRunComID = user.SpeedRunComID };
                            db.Save<UserSpeedRunComIDEntity>(userSpeedRunComID);

                            if (userLocation != null)
                            {
                                userLocation.UserID = user.ID;
                                db.Save<UserLocationEntity>(userLocation);
                            }

                            if (userLink != null)
                            {
                                userLink.UserID = user.ID;
                                db.Save<UserLinkEntity>(userLink);
                            }

                            tran.Complete();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "SaveUsers UserID: {@UserID}, UserSpeedRunComID: {@UserSpeedRunComID}", user.ID, user.SpeedRunComID);
                        }
                    }

                    _logger.Information("Saved users {@Count} / {@Total}", count, usersList.Count);
                    count++;
                }
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

        public IEnumerable<UserEntity> GetUsers(Expression<Func<UserEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<UserEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public IEnumerable<GuestEntity> GetGuests(Expression<Func<GuestEntity, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<GuestEntity>().Where(predicate ?? (x => true)).ToList();
            }
        }

        public IEnumerable<UserSpeedRunComView> GetUserSpeedRunComViews(Expression<Func<UserSpeedRunComView, bool>> predicate = null)
        {
            using (IDatabase db = DBFactory.GetDatabase())
            {
                db.OneTimeCommandTimeout = 32767;
                return db.Query<UserSpeedRunComView>().Where(predicate ?? (x => true)).ToList();
            }
        }
    }
}
