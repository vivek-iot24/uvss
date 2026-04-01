using System;
namespace Motwane.UVSS.Domain.Entities
{
    public class User
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string MobileNo { get; set; }
        public string CompanyName { get; set; }

        // required by repository
        public int Usertype_ID { get; set; }
        public int Authentication_ID { get; set; }

        // required for login
        public string PasswordHash { get; set; }
        public string PasswordSalt { get; set; }
    }
}