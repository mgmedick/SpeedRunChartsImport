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

        public void ProcessUsers(DateTime lastImportDate, bool isFullImport)
        {
            try
            {
                var lastImportDateUtc = lastImportDate.ToUniversalTime();
                _logger.Information("Started ProcessUsers: {@LastImportDate}, {@LastImportDateUtc}, {@IsFullImport}", lastImportDate, lastImportDateUtc, isFullImport);

                UsersOrdering orderBy = isFullImport ? UsersOrdering.SignUpDate : UsersOrdering.SignUpDateDescending;
                var results = new List<User>();
                var users = new List<User>();
                var prevTotal = 0;

                if (isFullImport)
                {
                    _userRepo.CopyUserTables();
                }

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
                        SaveUsers(results, isFullImport);
                        results.ClearMemory();
                    }
                }
                while (users.Count == MaxElementsPerPage && users.Min(i => i.SignUpDate ?? SqlMinDateTimeUtc) >= lastImportDateUtc);

                if (!isFullImport)
                {
                    results.RemoveAll(i => (i.SignUpDate ?? SqlMinDateTimeUtc) < lastImportDateUtc);
                }

                if (results.Any())
                {
                    SaveUsers(results, isFullImport);
                    results.ClearMemory();
                }

                if (isFullImport)
                {
                    _userRepo.RenameAndDropUserTables();
                }

                _settingService.UpdateSetting("UserLastImportDate", DateTime.Now);
                _logger.Information("Completed ProcessUsers");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "ProcessUsers");
            }
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

        public void SaveUsers(IEnumerable<User> users, bool isFullImport)
        {
            var userEntities = users.Select(i => new UserEntity { SpeedRunComID = i.ID, Name = i.Name, UserRoleID = (int)i.Role, SignUpDate = i.SignUpDate }).ToList();
            var userLocationEntities = users.Where(i => !string.IsNullOrWhiteSpace(i.Location?.ToString())).Select(i => new UserLocationEntity { UserSpeedRunComID = i.ID, Location = i.Location?.ToString() }).ToList();
            var userLinkEntities = users.Select(i => new UserLinkEntity
            {
                UserSpeedRunComID = i.ID,
                SpeedRunComUrl = i.WebLink.ToString(),
                ProfileImageUrl = i.ProfileImage?.ToString(),
                TwitchProfileUrl = i.TwitchProfile?.ToString(),
                HitboxProfileUrl = i.HitboxProfile?.ToString(),
                YoutubeProfileUrl = i.YoutubeProfile?.ToString(),
                TwitterProfileUrl = i.TwitterProfile?.ToString()
            }).ToList();

            SaveUsers(userEntities, userLocationEntities, userLinkEntities, isFullImport);
        }

        public void SaveUsers(IEnumerable<UserEntity> userEntities, IEnumerable<UserLocationEntity> userLocationEntities, IEnumerable<UserLinkEntity> userLinkEntities, bool isFullImport)
        {
            if (isFullImport)
            {
                _userRepo.InsertUsers(userEntities, userLocationEntities, userLinkEntities);
            }
            else
            {
                _userRepo.SaveUsers(userEntities, userLocationEntities, userLinkEntities);
            }
        }
    }
}


