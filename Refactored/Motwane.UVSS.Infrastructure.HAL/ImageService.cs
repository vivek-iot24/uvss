using System.IO;

namespace Motwane.UVSS.HAL
{
    public class ImageService : IImageService
    {
        private readonly string _baseFolder;

        public ImageService()
        {
            _baseFolder = @"D:\UVSS_MEDIA";
        }

        public string GetUndersideImagePath()
        {
            return Path.Combine(_baseFolder, "Entry Media", "Underside Images");
        }

        public string GetDriverImagePath()
        {
            return Path.Combine(_baseFolder, "Entry Media", "Driver Images");
        }

        public string GetAnprImagePath()
        {
            return Path.Combine(_baseFolder, "Entry Media", "ANPR Images");
        }

        public string GetVideoPath(int index)
        {
            return Path.Combine(_baseFolder, "Entry Media", $"Video Camera{index}");
        }
    }
}