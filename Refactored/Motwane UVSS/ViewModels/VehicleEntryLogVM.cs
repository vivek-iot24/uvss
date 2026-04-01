using System.IO;
using System.Windows.Media.Imaging;

namespace Motwane.UVSS.Presentation.ViewModels
{
    public class VehicleEntryLogVM
    {
        public int SrNo { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string Numberplate { get; set; }

        public string UndersideImagePath { get; set; }
        public string DriverImagePath { get; set; }
        public string AnprImagePath { get; set; }

    
        public BitmapImage UndersideImage => LoadImage(UndersideImagePath);
        public BitmapImage DriverCamImage => LoadImage(DriverImagePath);
        public BitmapImage AnprImage => LoadImage(AnprImagePath);

        private BitmapImage LoadImage(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return null;

            var bitmap = new BitmapImage();

            using (var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();
            }

            return bitmap;
        }
    }
}