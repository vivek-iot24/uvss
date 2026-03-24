using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motwane.UVSS.Infrastructure.Infrastructure.HAl.Services
{
    public interface IImageProcessingService
    {
        byte[] ConvertToBytes(System.Windows.Controls.Image image);
    }
}
