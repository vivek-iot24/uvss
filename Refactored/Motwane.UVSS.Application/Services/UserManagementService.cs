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

        public User GetUserById(string userId)
        {
            return userRepository.GetUserById(userId);
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

        public DataTable GetUserLoginLog(string userId)
        {
            int parsedUserId = 0;
            int.TryParse(userId, out parsedUserId);

            return userRepository.GetUserLoginLog(parsedUserId);
        }

        public void DeleteUser(string userId, string userName, string idNo)
        {
            userRepository.DeleteUser(userId, userName, idNo);
        }
    }
}