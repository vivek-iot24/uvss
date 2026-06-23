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

        public User GetUserById(System.Guid userUUID)
        {
            return userRepository.GetUserById(userUUID);
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

        public DataTable GetUserLoginLog(System.Guid userUUID)
        {
            return userRepository.GetUserLoginLog(userUUID);
        }

        public void DeleteUser(System.Guid userUUID, string userName, string idNo)
        {
            userRepository.DeleteUser(userUUID, userName, idNo);
        }
    }
}