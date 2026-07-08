using Motwane.UVSS.Application.Services;
using Motwane.UVSS.Domain;
using Motwane.UVSS.Domain.Entities;
using System;
using System.Data;

namespace Motwane.UVSS.Application.Interfaces.DAL
{
    public interface IUserRepository
    {
        int ValidateUser(string userId, string password, string userType);

        void InsertLoginLog(string userName, Guid machineUUID, DateTime loginTime);
       
        void UpdateLogoutLog(string UserName, DateTime logoutTime);

        void InsertUser(User user);

        User GetUserByName(string userName);


        void UpdateUser(User user);

        DataTable GetAllUsers();

        DataTable GetDeletedUserHistory();

        DataTable GetUserLoginLog(string username);
        Authentication LoadPermissions(Guid userTypeUUID);
        void DeleteUser(Guid userUUID, string userName, string idNo);
    }
}