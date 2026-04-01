using System;

namespace Motwane.UVSS.Application.DTOs
{
    public class VehicleEntryRequest
    {
        public string Username { get; set; }

        public DateTime EntryDate { get; set; }

        public TimeSpan EntryTime { get; set; }

        public string Status { get; set; }

        public string Remark { get; set; }

        public string NumberPlate { get; set; }

        public string UndersideImagePath { get; set; }

        public string DriverImagePath { get; set; }

        public string AnprImagePath { get; set; }

        public string Video1Path { get; set; }

        public string Video2Path { get; set; }

        public string Video3Path { get; set; }
    }
}