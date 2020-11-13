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

namespace SpeedRunAppImport.Service
{
    public class UserService : BaseService, IUserService
    {
        private readonly IGameRepository _gameRepo = null;
        private readonly IConfiguration _config = null;
        private readonly ILogger _logger;

        public UserService(IGameRepository gameRepo, IConfiguration config, ILogger logger)
        {
            _gameRepo = gameRepo;
            _config = config;
            _logger = logger;
        }

        public IEnumerable<User> GetUsers(DateTime lastImportDate, bool isFullImport)
        {
            _logger.Information("Started GetUsers: {@lastImportDate}, {@isFullImport}", lastImportDate, isFullImport);
            var results = new List<User>();
            List<User> users = null;
            UsersOrdering orderBy = isFullImport ? UsersOrdering.SignUpDate : UsersOrdering.SignUpDateDescending;

            do
            {
                users = GetUsersWithRetry(MaxElementsPerPage, results.Count, orderBy);
                if (users != null)
                {
                    results.AddRange(users);
                    _logger.Information("Pulled users: {@New}, total users: {@Total}", users.Count, results.Count);
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
            }
            while (users.Count == MaxElementsPerPage && users.Min(i => i.SignUpDate ?? SqlMinDateTime) >= lastImportDate);

            if (!isFullImport)
            {
                var userIDsToRemove = users.Where(i => (i.SignUpDate ?? SqlMinDateTime) < lastImportDate).Select(i => i.ID).ToList();
                results.RemoveAll(i => userIDsToRemove.Contains(i.ID));
            }

            _logger.Information("Completed GetUsers");
            return results.OrderBy(i => i.SignUpDate);
        }

        private List<User> GetUsersWithRetry(int elementsPerPage, int elementsOffset, UsersOrdering orderBy, int retryCount = 0)
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
    }
}


