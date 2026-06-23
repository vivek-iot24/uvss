using System;
using Motwane.UVSS.Application.Interfaces.DAL;

namespace Motwane.UVSS.Application.Services
{
    public class AuthenticationService
    {
        private readonly IUserRepository _userRepository;

        public AuthenticationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public int ValidateUser(string userId, string password, string userType)
        {
            return _userRepository.ValidateUser(userId, password, userType);
        }

        public void InsertLoginLog(string userName, Guid machineUUID, DateTime loginTime)
        {
            _userRepository.InsertLoginLog(userName, machineUUID, loginTime);
        }

        public void UpdateLogoutLog(string UserName, DateTime logoutTime)
        {
            _userRepository.UpdateLogoutLog(UserName, logoutTime);
        }
    }
}