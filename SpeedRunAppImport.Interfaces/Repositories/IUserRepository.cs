using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface IUserRepository
    {
        void InsertUsers(IEnumerable<UserEntity> users);
        void CopyUserTables();
        void RenameAndDropUserTables();
    }
}






