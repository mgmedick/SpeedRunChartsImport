using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model;
using SpeedRunAppImport.Model.Data;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Services
{
    public interface IUserService
    {
        void ProcessUsers(DateTime lastImportDate, bool isFullImport);
        List<User> GetUsersWithRetry(int elementsPerPage, int elementsOffset, UsersOrdering orderBy, int retryCount = 0);
        void SaveUsers(IEnumerable<User> users, bool isFullImport);
        void SaveUsers(IEnumerable<UserEntity> userEntities, bool isFullImport);
    }
} 




