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

        public void InsertLoginLog(string userId, DateTime loginTime)
        {
            _userRepository.InsertLoginLog(userId, loginTime);
        }

        public void UpdateLogoutLog(string userId, DateTime logoutTime)
        {
            _userRepository.UpdateLogoutLog(userId, logoutTime);
        }
    }
}