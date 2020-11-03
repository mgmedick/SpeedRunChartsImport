using System;
using System.Collections.Generic;
using System.Linq;
using SpeedRunApp.Client;
using SpeedRunApp.Model;
using SpeedRunApp.Model.Data;
using SpeedRunApp.Model.Entity;
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

            do
            {
                users = GetUsersWithRetry(MaxElementsPerPage, results.Count, UsersOrdering.SignUpDateDescending).ToList();
                results.AddRange(users);
                _logger.Information("Pulled users: {@New}, total users: {@Total}", users.Count, results.Count);
                Thread.Sleep(TimeSpan.FromMilliseconds(BaseService.PullDelayMS));
            }
            //while (users.Count == MaxElementsPerPage && users.Min(i => i.SignUpDate ?? DateTime.MinValue) >= lastImportDate);
            while (1 == 0);

            if (!isFullImport)
            {
                var userIDsToRemove = users.Where(i => (i.SignUpDate ?? DateTime.MinValue) < lastImportDate).Select(i => i.ID).ToList();
                results.RemoveAll(i => userIDsToRemove.Contains(i.ID));
            }

            _logger.Information("Completed GetUsers");
            return results;
        }

        private IEnumerable<User> GetUsersWithRetry(int elementsPerPage, int elementsOffset, UsersOrdering orderBy, int retryCount = 0)
        {
            ClientContainer clientContainer = new ClientContainer();
            IEnumerable<User> users = null;
            try
            {
                users = clientContainer.Users.GetUsers(elementsPerPage: elementsPerPage, elementsOffset: elementsOffset, orderBy: orderBy);
            }
            catch (Exception ex)
            {
                if (retryCount <= MaxRetryCount)
                {
                    GetUsersWithRetry(elementsPerPage, elementsOffset, orderBy, retryCount++);
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


