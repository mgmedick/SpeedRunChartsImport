using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunAppImport.Client;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using SpeedRunAppImport.Interfaces.Services;
using SpeedRunAppImport.Interfaces.Repositories;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Serilog;
using SpeedRunCommon;

namespace SpeedRunAppImport.Service
{
    public class UserService : BaseService, IUserService
    {
        private readonly ISettingService _settingService = null;
        private readonly IUserRepository _userRepo = null;
        private readonly IConfiguration _config = null;
        private readonly ILogger _logger;

        public UserService(ISettingService settingService, IUserRepository userRepo, IConfiguration config, ILogger logger)
        {
            _settingService = settingService;
            _userRepo = userRepo;
            _config = config;
            _logger = logger;
        }

        public void ProcessUsers(DateTime lastImportDate, bool isFullImport)
        {
            try
            {
                _logger.Information("Started ProcessUsers: {@LastImportDate}, {@IsFullImport}", lastImportDate, isFullImport);
                var newImportDate = DateTime.UtcNow;
                UsersOrdering orderBy = isFullImport ? UsersOrdering.SignUpDate : UsersOrdering.SignUpDateDescending;
                var results = GetUsersWithRetry(MaxElementsPerPage, 0, orderBy);
                _logger.Information("Pulled users: {@New}, total users: {@Total}", results.Count, results.Count);

                //while (results.Count == MaxElementsPerPage && results.Min(i => i.SignUpDate ?? SqlMinDateTime) >= lastImportDate)
                while (results.Count <= 200)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
                    var users = GetUsersWithRetry(MaxElementsPerPage, results.Count, orderBy);
                    results.AddRange(users);
                    _logger.Information("Pulled users: {@New}, total users: {@Total}", users.Count, results.Count);

                    var memorySize = GC.GetTotalMemory(false);
                    if (memorySize > MaxMemorySizeBytes)
                    {
                        _logger.Information("Saving to clear memory, results: {@Count}, size: {@Size}", results.Count, memorySize);
                        SaveUsers(results, isFullImport);
                        results.ClearMemory();
                    }
                }

                if (results.Any())
                {
                    SaveUsers(results, isFullImport);
                }

                _settingService.UpdateSetting("UserLastImportDate", newImportDate);
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
            var userEntities = users.Select(i => i.ConvertToEntity()).ToList();
            SaveUsers(userEntities, isFullImport);
        }

        public void SaveUsers(IEnumerable<UserEntity> userEntities, bool isFullImport)
        {
            if (isFullImport)
            {
                _userRepo.CopyUserTables();
                _userRepo.InsertUsers(userEntities);
                _userRepo.RenameAndDropUserTables();
            }
            else
            {
                _userRepo.SaveUsers(userEntities);
            }
        }
    }
}


