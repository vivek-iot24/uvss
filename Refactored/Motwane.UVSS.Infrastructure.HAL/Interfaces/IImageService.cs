using Motwane.UVSS.HAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Motwane.UVSS.HAL
{
    public interface IImageService
    {
        string GetUndersideImagePath();
        string GetDriverImagePath();
        string GetAnprImagePath();
        string GetVideoPath(int index);
    }
}