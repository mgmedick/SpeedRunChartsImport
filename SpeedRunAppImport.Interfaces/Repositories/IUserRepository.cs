using System;
using System.Collections.Generic;
using SpeedRunAppImport.Model.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace SpeedRunAppImport.Interfaces.Repositories
{
    public interface IUserRepository
    {
        //void CopyUserTables();
        //void RenameAndDropUserTables();
        void InsertUsers(IEnumerable<UserEntity> users, IEnumerable<UserLocationEntity> userLocations, IEnumerable<UserLinkEntity> userLinks);
        void SaveUsers(IEnumerable<UserEntity> users, IEnumerable<UserLocationEntity> userLocations, IEnumerable<UserLinkEntity> userLinks);
        IEnumerable<UserSpeedRunComIDEntity> GetUserSpeedRunComIDs(Expression<Func<UserSpeedRunComIDEntity, bool>> predicate = null);
        IEnumerable<GuestEntity> GetGuests(Expression<Func<GuestEntity, bool>> predicate = null);
        IEnumerable<UserSpeedRunComView> GetUserSpeedRunComViews(Expression<Func<UserSpeedRunComView, bool>> predicate = null);
        void InsertGuests(IEnumerable<GuestEntity> guests, IEnumerable<GuestLinkEntity> guestLinks);
        void SaveGuests(IEnumerable<GuestEntity> guests, IEnumerable<GuestLinkEntity> guestLinks);
    }
}






