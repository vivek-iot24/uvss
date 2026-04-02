using System;
using System.Windows.Media.Imaging;
using Motwane.UVSS.Domain.Entities;

namespace Motwane.UVSS.Presentation.ViewModels
{
    public class VehicleEntryLogVM
    {
        public int SrNo { get; set; }
        public string Username { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? EntryTime { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string Numberplate { get; set; }

        public BitmapImage UndersideImage { get; set; }
        public BitmapImage DriverCamImage { get; set; }
        public BitmapImage AnprImage { get; set; }

        public string VideoCam1Path { get; set; }
        public string VideoCam2Path { get; set; }
        public string VideoCam3Path { get; set; }

        public byte[] UndersideBytes { get; set; }
        public byte[] DriverCamBytes { get; set; }
        public byte[] AnprBytes { get; set; }
    }
}