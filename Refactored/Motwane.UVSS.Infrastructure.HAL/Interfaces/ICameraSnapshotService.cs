using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motwane.UVSS.HAL
{
    public interface ICameraSnapshotService
    {
        Task<byte[]> CaptureAnprAsync();
        Task<byte[]> CaptureDriverAsync();
    }
}