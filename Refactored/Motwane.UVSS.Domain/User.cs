using System;

namespace Motwane.UVSS.Domain.Entities
{
    public class User
    {
        public Guid UserUUID { get; set; }

        public string UserName { get; set; }

        public string MobileNo { get; set; }

        public string CompanyName { get; set; }

        public string AgencyName { get; set; }
        public string IsActive { get; set; }
        public string IdNo { get; set; }

        public string Password { get; set; }

        public string UserType { get; set; }

        // DB relation columns
        public Guid Usertype_UUID { get; set; }

        public int Authentication_ID { get; set; }

        // New DB columns
        public int Trials { get; set; }

        public string UserStatus { get; set; }
    }
}