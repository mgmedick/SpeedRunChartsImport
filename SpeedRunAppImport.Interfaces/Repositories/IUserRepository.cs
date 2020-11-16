using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface IUserRepository
    {
        void CopyUserTables();
        void RenameAndDropUserTables();
        void InsertUsers(IEnumerable<UserEntity> users);
        void SaveUsers(IEnumerable<UserEntity> users);
    }
}






