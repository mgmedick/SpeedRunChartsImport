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

namespace SpeedRunAppImport.Service
{
    public class SpeedRunService : BaseService//, IUserService
    {
        private readonly IConfiguration _config = null;
        private readonly IGameRepository _gameRepo = null;

        public SpeedRunService(IConfiguration config, IGameRepository gameRepo)
        {
            _config = config;
            _gameRepo = gameRepo;
        }

        public IEnumerable<run> GetSpeedRuns()
        {
            var results = new List<User>();
            List<User> users = null;

            do
            {
                users = GetSpeedRunsWithRetry(MaxElementsPerPage, results.Count, UsersOrdering.SignUpDateDescending).ToList();
                results.AddRange(users);
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            while (users.Count == MaxElementsPerPage && (IsFullImport || users.Min(i => i.SignUpDate ?? DateTime.MinValue) >= UserLastImportDate));

            if (!IsFullImport)
            {
                results = results.Where(i => (i.SignUpDate ?? DateTime.MinValue) >= UserLastImportDate).ToList();
            }

            return results;
        }

        private IEnumerable<User> GetSpeedRunsWithRetry(int elementsPerPage, int elementsOffset, UsersOrdering orderBy, int retryCount = 0)
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


