using System;
using System.Data;
using Motwane.UVSS.Domain.Entities;

namespace Motwane.UVSS.Application.Interfaces.DAL
{
    public interface IUserRepository
    {
        int ValidateUser(string userId, string password, string userType);

        void InsertLoginLog(string userName, Guid machineUUID, DateTime loginTime);

        void UpdateLogoutLog(string UserName, DateTime logoutTime);

        void InsertUser(User user);

        User GetUserById(Guid userUUID);


        void UpdateUser(User user);

        DataTable GetAllUsers();

        DataTable GetDeletedUserHistory();

        DataTable GetUserLoginLog(Guid userUUID);


        void DeleteUser(Guid userUUID, string userName, string idNo);
    }
}