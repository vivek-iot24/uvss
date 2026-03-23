using System;

namespace Motwane.UVSS.Domain.Entities
{
    public class VideoRecord
    {
        public int id { get; set; }

        public string vehicle_number { get; set; }

        public DateTime capture_date { get; set; }

        public TimeSpan capture_time { get; set; }

        public string video1_path { get; set; }

        public string video2_path { get; set; }

        public string video3_path { get; set; }

        public byte[] vehicle_image { get; set; }
    }
}