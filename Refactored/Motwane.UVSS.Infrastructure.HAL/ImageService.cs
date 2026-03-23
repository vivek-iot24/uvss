using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motwane.UVSS.HAL
{
    public class ImageService : IImageService
    {
        public byte[] GetUndersideImage() => new byte[0];
        public byte[] GetDriverImage() => new byte[0];
        public byte[] GetAnprImage() => new byte[0];
        public string GetVideoPath(int index) => "";
    }
}