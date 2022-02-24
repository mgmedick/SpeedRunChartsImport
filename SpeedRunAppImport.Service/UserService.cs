using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Client;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using System.Threading;
using Serilog;
using SpeedRunCommon;
using System.IO;
using System.Net;

namespace SpeedRunAppImport.Service
{
    public class UserService : BaseService, IUserService
    {
        private readonly ISettingService _settingService = null;
        private readonly IUserRepository _userRepo = null;
        private readonly ISpeedRunRepository _speedRunRepo = null;
        private readonly ILogger _logger;

        public UserService(ISettingService settingService, IUserRepository userRepo, ISpeedRunRepository speedRunRepo, ILogger logger)
        {
            _settingService = settingService;
            _userRepo = userRepo;
            _speedRunRepo = speedRunRepo;
            _logger = logger;
        }

        public bool ProcessUsers(DateTime lastImportDateUtc, bool isFullImport, bool isBulkReload)
        {
            bool result = true;

            try
            {
                _logger.Information("Started ProcessUsers: {@LastImportDateUtc}, {@IsFullImport}, {@IsBulkReload}", lastImportDateUtc, isFullImport, isBulkReload);
                UsersOrdering orderBy = isFullImport ? UsersOrdering.SignUpDate : UsersOrdering.SignUpDateDescending;
                var results = new List<User>();
                var users = new List<User>();
                var prevTotal = 0;

                do
                {
                    users = GetUsersWithRetry(MaxElementsPerPage, results.Count + prevTotal, orderBy);
                    results.AddRange(users);
                    _logger.Information("Pulled users: {@New}, total users: {@Total}", users.Count, results.Count + prevTotal);
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        prevTotal += results.Count;
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveUsers(results, isBulkReload);
                        results.ClearMemory();
                    }
                }
                while (users.Count == MaxElementsPerPage && (isFullImport || users.Min(i => i.SignUpDate ?? SqlMinDateTime) >= lastImportDateUtc));

                if (!isFullImport)
                {
                    results.RemoveAll(i => (i.SignUpDate ?? SqlMinDateTime) <= lastImportDateUtc);
                }

                if (results.Any())
                {
                    SaveUsers(results, isBulkReload);
                    var lastUpdateDate = results.Max(i => i.SignUpDate) ?? DateTime.UtcNow;
                    _settingService.UpdateSetting("UserLastImportDate", lastUpdateDate);
                    results.ClearMemory();
                }

                _logger.Information("Completed ProcessUsers");
            }
            catch (Exception ex)
            {
                result = false;
                _logger.Error(ex, "ProcessUsers");
            }

            return result;
        }

        public List<User> GetUsersWithRetry(int elementsPerPage, int elementsOffset, UsersOrdering orderBy, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            List<User> users = null;
            try
            {
                users = clientContainer.Users.GetUsers(elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, orderBy: orderBy).ToList();
            }
            catch (Exception ex)
            {
                if (retryCount <= MaxRetryCount)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.ErrorPullDelayMS));
                    retryCount++;
                    _logger.Information("Retrying pull users: {@New}, total users: {@Total}, retry: {@RetryCount}", elementsPerPage, elementsOffset, retryCount);
                    users = GetUsersWithRetry(elementsPerPage, elementsOffset, orderBy, retryCount);
                }
                else
                {
                    throw ex;
                }
            }

            return users;
        }

        public void SaveGuests(IEnumerable<Guest> guests, bool isBulkReload, IEnumerable<GuestEntity> guestSpeedRunComIDs = null)
        {
            var guestIDs = guests.Select(i => i.Name).ToList();
            if (guestSpeedRunComIDs == null)
            {
                guestSpeedRunComIDs = _userRepo.GetGuests().Where(i => guestIDs.Contains(i.Name)).ToList();
            }

            var guestEntities = guests.Select(i => new GuestEntity
            {
                ID = guestSpeedRunComIDs.Where(g => g.Name == i.Name).Select(g => g.ID).FirstOrDefault(),
                Name = i.Name,
                Abbr = i.WebLink?.Segments.LastOrDefault()
            })
            .ToList();

            var guestLinkEntities = guests.Select(i => new GuestLinkEntity
            {
                GuestSpeedRunComID = i.Name,
                SpeedRunComUrl = i.WebLink.ToString()
            })
            .ToList();

            if (isBulkReload)
            {
                _userRepo.InsertGuests(guestEntities, guestLinkEntities);
            }
            else
            {
                _userRepo.SaveGuests(guestEntities, guestLinkEntities);
            }
        }

        public void SaveUsers(IEnumerable<User> users, bool isBulkReload, IEnumerable<UserSpeedRunComIDEntity> userSpeedRunComIDs = null)
        {
            _logger.Information("Started SaveUsers: {@Count}, {@IsBulkReload}", users.Count(), isBulkReload);

            users = users.OrderBy(i => i.SignUpDate).ToList();
            var userIDs = users.Select(i => i.ID).ToList();
            if (userSpeedRunComIDs == null)
            {
                userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs();
                userSpeedRunComIDs = userSpeedRunComIDs.Join(userIDs, o => o.SpeedRunComID, id => id, (o, id) => o).ToList();
            }

            var userEntities = users.Select(i => new UserEntity
            {
                ID = userSpeedRunComIDs.Where(g => g.SpeedRunComID == i.ID).Select(g => g.UserID).FirstOrDefault(),
                SpeedRunComID = i.ID,
                Name = i.Name,
                UserRoleID = (int)i.Role,
                Abbr = i.WebLink?.Segments.LastOrDefault(),
                SignUpDate = i.SignUpDate
            })
            .ToList();
            var userLocationEntities = users.Where(i => !string.IsNullOrWhiteSpace(i.Location?.ToString()))
                                            .Select(i => new UserLocationEntity
                                            {
                                                UserSpeedRunComID = i.ID,
                                                Location = i.Location?.ToString()
                                            })
                                            .ToList();
            var userLinkEntities = users.Select(i => new UserLinkEntity
            {
                UserSpeedRunComID = i.ID,
                SpeedRunComUrl = i.WebLink.ToString(),
                ProfileImageUrl = i.ProfileImage?.ToString(),
                TwitchProfileUrl = i.TwitchProfile?.ToString(),
                HitboxProfileUrl = i.HitboxProfile?.ToString(),
                YoutubeProfileUrl = i.YoutubeProfile?.ToString(),
                TwitterProfileUrl = i.TwitterProfile?.ToString()
            })
            .ToList();

            if (isBulkReload)
            {
                var newUserEntities = userEntities.Where(i => i.ID == 0).ToList();
                userLocationEntities = userLocationEntities.Where(i => userEntities.Any(g => g.ID == i.UserID)).ToList();
                userLinkEntities = userLinkEntities.Where(i => userEntities.Any(g => g.ID == i.UserID)).ToList();
                _userRepo.InsertUsers(newUserEntities, userLocationEntities, userLinkEntities);
            }
            else
            {
                var newUserEntities = userEntities.Where(i => i.ID == 0).ToList();
                var changedUserIDs = GetChangedUserIDs(userEntities, userLocationEntities, userLinkEntities);
                var changedUserEntities = userEntities.Where(i => changedUserIDs.Contains(i.ID)).ToList();
                var totalUsers = userEntities.Count();
                userEntities = newUserEntities.Concat(changedUserEntities).ToList();
                userLocationEntities = userLocationEntities.Where(i => userEntities.Any(g => g.SpeedRunComID == i.UserSpeedRunComID)).ToList();
                userLinkEntities = userLinkEntities.Where(i => userEntities.Any(g => g.SpeedRunComID == i.UserSpeedRunComID)).ToList();

                _logger.Information("Found NewUsers: {@New}, ChangedUsers: {@Changed}, TotalUsers: {@Total}", newUserEntities.Count(), changedUserEntities.Count(), totalUsers);

                _userRepo.SaveUsers(userEntities, userLocationEntities, userLinkEntities);
            }

            _logger.Information("Completed SaveUsers");
        }

        public IEnumerable<int> GetChangedUserIDs(List<UserEntity> users, IEnumerable<UserLocationEntity> userLocations, List<UserLinkEntity> userLinks)
        {
            _logger.Information("Started GetChangedUserIDs: {@Count}", users.Count());

            var changedUserIDs = new List<int>();
            var userSpeedRunComViews = new List<UserSpeedRunComView>();
            
            var maxBatchCount = 500;
            var batchCount = 0;
            while (batchCount < users.Count())
            {
                var userSpeedRunComIDsBatch = users.Skip(batchCount).Take(maxBatchCount).Select(i => i.SpeedRunComID).ToList();
                var userSpeedRunComViewsBatch = _userRepo.GetUserSpeedRunComViews(i => userSpeedRunComIDsBatch.Contains(i.SpeedRunComID));
                userSpeedRunComViews.AddRange(userSpeedRunComViewsBatch);
                batchCount += maxBatchCount;
            }

            bool isChanged;
            foreach (var user in users)
            {
                isChanged = false;
                var changeReasons = new List<string>();
                var userSpeedRunComView = userSpeedRunComViews.FirstOrDefault(i => i.SpeedRunComID == user.SpeedRunComID);

                if (userSpeedRunComView != null)
                {
                    isChanged = (user.Name != userSpeedRunComView.Name);

                    if (isChanged)
                    {
                        changeReasons.Add("Name changed");
                    }

                    if (!isChanged)
                    {
                        var userLink = userLinks.FirstOrDefault(i => i.UserSpeedRunComID == userSpeedRunComView.SpeedRunComID);
                        isChanged = userLink?.SpeedRunComUrl != userSpeedRunComView.SpeedRunComUrl
                                    || userLink?.ProfileImageUrl != userSpeedRunComView.ProfileImageUrl
                                    || userLink?.TwitchProfileUrl != userSpeedRunComView.TwitchProfileUrl
                                    || userLink?.HitboxProfileUrl != userSpeedRunComView.HitboxProfileUrl
                                    || userLink?.YoutubeProfileUrl != userSpeedRunComView.YoutubeProfileUrl
                                    || userLink?.TwitterProfileUrl != userSpeedRunComView.TwitterProfileUrl;

                        if (isChanged)
                        {
                            changeReasons.Add("Urls changed");
                        }
                    }

                    //if (!isChanged)
                    //{
                    //    var userLocation = userLocations.FirstOrDefault(i => i.UserSpeedRunComID == userSpeedRunComView.SpeedRunComID);
                    //    isChanged = userLocation?.Location != userSpeedRunComView.Location;

                    //    if (isChanged)
                    //    {
                    //        changeReasons.Add("Location changed");
                    //    }
                    //}

                    if (isChanged)
                    {
                        changedUserIDs.Add(user.ID);
                        _logger.Information("UserID: {@UserID}, ChangeReason: {@ChangeReason}", user.ID, string.Join("; ", changeReasons));
                    }
                }
            }

            _logger.Information("Completed GetChangedUserIDs");

            return changedUserIDs;
        }
    }
}


