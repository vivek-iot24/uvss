using Motwane.UVSS.Application.Interfaces.HAL;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Motwane.UVSS.HAL.Cameras
{
    public class UndersideCamera_HAL : ICameraService
    {
        private readonly Underside_cam_class _camera;
        private CancellationTokenSource _cts;

        public event Action<byte[], int, int, int> OnFrameReceived;

        public UndersideCamera_HAL()
        {
            _camera = new Underside_cam_class();
        }

        public void Initialize()
        {
            string result = _camera.InitCamera();

            if (result != null)
                throw new Exception(result);
        }

        public void Start()
        {
            _camera.StartAcquisition();

            _cts = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    // Your current camera saves images to disk
                    // Here you would normally convert camera frames
                    // to byte[] and raise the event.

                    Thread.Sleep(30);
                }

            }, _cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _camera.StopAcquisition();
        }
    }
}