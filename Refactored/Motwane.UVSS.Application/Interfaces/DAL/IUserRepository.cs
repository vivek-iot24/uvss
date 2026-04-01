using System;
using System.Data;
using Motwane.UVSS.Domain.Entities;

namespace Motwane.UVSS.Application.Interfaces.DAL
{
    public interface IUserRepository
    {
        int ValidateUser(string userName, string passwordHash, string userType);

        void InsertLoginLog(int userId, int machineId, DateTime loginTime);

        void UpdateLogoutLog(int userId, DateTime logoutTime);

        void InsertUser(User user);

        User GetUserByName(string userName);

        // ✅ compatibility methods for existing services
        User GetUserById(string userId);

        void UpdateUser(User user);

        DataTable GetAllUsers();

        DataTable GetDeletedUserHistory();

        DataTable GetUserLoginLog(int userId);

        void DeleteUser(int userId);

        // ✅ compatibility overload
        void DeleteUser(string userId, string userName, string idNo);
    }
}