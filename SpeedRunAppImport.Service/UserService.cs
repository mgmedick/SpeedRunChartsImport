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
                while (users.Count == MaxElementsPerPage && users.Min(i => i.SignUpDate ?? SqlMinDateTime) > lastImportDateUtc);

                if (!isFullImport)
                {
                    results.RemoveAll(i => (i.SignUpDate ?? SqlMinDateTime) <= lastImportDateUtc);
                }

                if (results.Any())
                {
                    SaveUsers(results, isBulkReload);
                    _settingService.UpdateSetting("UserLastImportDate", results.Max(i => i.SignUpDate ?? SqlMinDateTime));
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
                Name = i.Name
            })
            .ToList();

            if (isBulkReload)
            {
                _userRepo.InsertGuests(guestEntities);
            }
            else
            {
                _userRepo.SaveGuests(guestEntities);
            }
        }

        public void SaveUsers(IEnumerable<User> users, bool isBulkReload, IEnumerable<UserSpeedRunComIDEntity> userSpeedRunComIDs = null)
        {
            _logger.Information("Started SaveUsers: {@Count}, {@IsBulkReload}", users.Count(), isBulkReload);

            users = users.OrderBy(i => i.SignUpDate).ToList();
            var userIDs = users.Select(i => i.ID).ToList();
            if (userSpeedRunComIDs == null)
            {
                userSpeedRunComIDs = _userRepo.GetUserSpeedRunComIDs().Where(i => userIDs.Contains(i.SpeedRunComID)).ToList();
            }

            var userEntities = users.Select(i => new UserEntity {
                ID = userSpeedRunComIDs.Where(g => g.SpeedRunComID == i.ID).Select(g => g.UserID).FirstOrDefault(),
                SpeedRunComID = i.ID,
                Name = i.Name,
                UserRoleID = (int)i.Role,
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
                _userRepo.InsertUsers(userEntities, userLocationEntities, userLinkEntities);
            }
            else
            {
                _userRepo.SaveUsers(userEntities, userLocationEntities, userLinkEntities);
            }

            _logger.Information("Completed SaveUsers");
        }
    }
}


