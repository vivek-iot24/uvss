using Motwane_UVSS.Application.Interfaces.HAL;
using OpenCvSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Motwane_UVSS.HAL.Cameras
{
    public class FakeLaptopCameraService : ICameraService
    {
        private VideoCapture _capture;
        private CancellationTokenSource _cts;

        public event Action<byte[], int, int, int> OnFrameReceived;

        public void Initialize()
        {
            _capture = new VideoCapture(0);
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                Mat frame = new Mat();

                while (!_cts.Token.IsCancellationRequested)
                {
                    if (_capture.Read(frame) && !frame.Empty())
                    {
                        int width = frame.Width;
                        int height = frame.Height;
                        int stride = (int)frame.Step();

                        byte[] buffer = new byte[frame.Rows * frame.Cols * frame.ElemSize()];
                        System.Runtime.InteropServices.Marshal.Copy(frame.Data, buffer, 0, buffer.Length);

                        OnFrameReceived?.Invoke(buffer, width, height, stride);
                    }
                }
            });
        }

        public void Stop()
        {
            _cts?.Cancel();
            _capture?.Dispose();
        }
    }
}