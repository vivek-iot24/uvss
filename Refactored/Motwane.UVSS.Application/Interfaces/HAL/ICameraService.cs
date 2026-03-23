    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Motwane.UVSS.Application.Interfaces.HAL
{
    public interface ICameraService
    {
        event Action<byte[], int, int, int> OnFrameReceived;

        void Initialize();
        void Start();
        void Stop();
    }
}