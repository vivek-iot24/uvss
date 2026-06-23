using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motwane.UVSS.Application.DTOs
{
 
    public class VehicleEntryRequest
    {
        public string Username { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string NumberPlate { get; set; }

        public DateTime EntryDate { get; set; }
        public DateTime EntryTime { get; set; }

        public byte[] UndersideImage { get; set; }
        public byte[] DriverImage { get; set; }
        public byte[] AnprImage { get; set; }
        public string UndersideImagePath { get; set; }
        public string DriverImagePath { get; set; }
        public string AnprImagePath { get; set; }
        public string Video1Path { get; set; }
        public string Video2Path { get; set; }
        public string Video3Path { get; set; }
    }
}
