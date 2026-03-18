using Motwane.UVSS.Infrastructure.HAL.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Motwane.UVSS.HAL
{
    public class ImageProcessingService : IImageProcessingService
    {
        public byte[] ConvertToBytes(Image imageControl)
        {
            if (imageControl.Source == null)
                return null;

            var bitmapSource = imageControl.Source as BitmapSource;

            using (var stream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(stream);
                return stream.ToArray();
            }
        }
    }
}