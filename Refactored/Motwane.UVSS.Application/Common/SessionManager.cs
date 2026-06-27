using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motwane.UVSS.Application.Common
{
    public static class SessionManager
    {
        public static Guid User_UUID { get; set; }
        public static Guid UserType_UUID { get; set; }
        public static Guid Machine_UUID { get; set; }
        public static string Username { get; set; }


        public static void SetUserSession(
         Guid userUuid,
         Guid userTypeUuid,
         string username)
        {
            User_UUID = userUuid;
            UserType_UUID = userTypeUuid;
            Username = username;
        }

        public static void SetMachineSession(
            Guid machineUuid)
        {
            Machine_UUID = machineUuid;
        }

        public static void Clear()
        {
            User_UUID = Guid.Empty;
            UserType_UUID = Guid.Empty;
            Machine_UUID = Guid.Empty;
            Username = string.Empty;
        }
    }
}
