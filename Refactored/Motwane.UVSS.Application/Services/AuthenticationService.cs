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
            int parsedUserId = 0;
            int.TryParse(userId, out parsedUserId);

            // temporary default machine id
            int machineId = 1;

            _userRepository.InsertLoginLog(
                parsedUserId,
                machineId,
                loginTime
            );
        }

        public void UpdateLogoutLog(string userId, DateTime logoutTime)
        {
            int parsedUserId = 0;
            int.TryParse(userId, out parsedUserId);

            _userRepository.UpdateLogoutLog(
                parsedUserId,
                logoutTime
            );
        }
    }
}