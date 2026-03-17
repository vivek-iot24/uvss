using Motwane.UVSS.Application.Interfaces.HAL;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Motwane.UVSS.HAL.Cameras
{
    public class FakeLaptopCameraService : ICameraService
    {
        private CancellationTokenSource _cts;

        public event Action<byte[], int, int, int> OnFrameReceived;

        public void Initialize()
        {
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                int width = 640;
                int height = 480;
                int stride = width * 3;

                while (!_cts.Token.IsCancellationRequested)
                {
                    byte[] frame = new byte[width * height * 3];

                    // generate dummy image data
                    new Random().NextBytes(frame);
                    OnFrameReceived?.Invoke(frame, width, height, stride);

                    await Task.Delay(33);
                }

            }, _cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
        }
    }
}