using System;
using System.Data;
using Motwane.UVSS.Domain.Entities;

namespace Motwane.UVSS.Application.Interfaces.DAL
{
    public interface IUserRepository
    {
        int ValidateUser(string userId, string password, string userType);

        void InsertLoginLog(string userId, DateTime loginTime);

        void UpdateLogoutLog(string UserName, DateTime logoutTime);

        void InsertUser(User user);

        User GetUserById(string userId);

        void UpdateUser(User user);

        DataTable GetAllUsers();

        DataTable GetDeletedUserHistory();

        DataTable GetUserLoginLog(string userId);

        void DeleteUser(string userId, string userName, string idNo);
    }
}