using System.Data;
using Motwane.UVSS.Application.Interfaces.DAL;
using Motwane.UVSS.Domain.Entities;

namespace Motwane.UVSS.Application.Services
{
    public class UserManagementService
    {
        private readonly IUserRepository userRepository;

        public UserManagementService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public void InsertUser(User user)
        {
            userRepository.InsertUser(user);
        }

        public User GetUserByName(string userName)
        {
            return userRepository.GetUserByName(userName);
        }

        public void UpdateUser(User user)
        {
            userRepository.UpdateUser(user);
        }

        public DataTable GetAllUsers()
        {
            return userRepository.GetAllUsers();
        }

        public DataTable GetDeletedUserHistory()
        {
            return userRepository.GetDeletedUserHistory();
        }

        public DataTable GetUserLoginLog(string username)
        {
            return userRepository.GetUserLoginLog( username);
        }

        public void DeleteUser(System.Guid userUUID, string userName, string idNo)
        {
            userRepository.DeleteUser(userUUID, userName, idNo);
        }
    }
}