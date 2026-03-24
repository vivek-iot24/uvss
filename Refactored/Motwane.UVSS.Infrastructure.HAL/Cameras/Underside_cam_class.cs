using System;
using System.Threading.Tasks;
using SpinnakerNET.GenApi;
using SpinnakerNET;

namespace Motwane.UVSS.Infrastructure.HAL.Cameras
{
    public class Underside_cam_class
    {
        public bool IsCapturing => isCapturing;

        private ManagedSystem system;
        private ManagedCameraList camList;
        private IManagedCamera camera;
        private bool isCapturing;
        public bool IsInitialized { get; private set; } = false;
        public int ImageCount { get; private set; }

        private IManagedImageProcessor processor = new ManagedImageProcessor();

        public string InitCamera()
        {
            try
            {
                system = new ManagedSystem();
                camList = system.GetCameras();

                if (camList.Count == 0)
                {
                    return "No camera detected.";
                }

                camera = camList[0];
                camera.Init();

                INodeMap nodeMap = camera.GetNodeMap();
                IEnum iPixelFormat = nodeMap.GetNode<IEnum>("PixelFormat");

                if (iPixelFormat != null && iPixelFormat.IsWritable)
                {
                    IEnumEntry iRgb8 = iPixelFormat.GetEntryByName("RGB8");
                    if (iRgb8 != null)
                    {
                        iPixelFormat.Value = iRgb8.Symbolic;
                    }
                }

                IsInitialized = true;
                return null;
            }
            catch (Exception ex)
            {
                return $"Camera initialization failed: {ex.Message}";
            }
        }

        public bool StartAcquisition()
        {
            if (!IsInitialized || camera == null)
                return false;

            camera.BeginAcquisition();
            isCapturing = true;
            ImageCount = 0;

            Task.Run(() => CaptureImages());
            return true;
        }

        public void StopAcquisition()
        {
            isCapturing = false;

            if (camera != null)
            {
                camera.EndAcquisition();
            }
        }

        public void CloseCamera()
        {
            if (camera != null)
            {
                StopAcquisition();
            }

            if (camera != null)
            {
                camera.DeInit();
                camera.Dispose();
                camera = null;
            }

            if (camList.Count > 0)
            {
                camList?.Clear();
                system?.Dispose();
                IsInitialized = false;
            }
        }

        private void CaptureImages()
        {
            try
            {
                while (isCapturing)
                {
                    using (IManagedImage rawImage = camera.GetNextImage(1000))
                    {
                        if (!rawImage.IsIncomplete)
                        {
                            using (IManagedImage convertedImage = processor.Convert(rawImage, PixelFormatEnums.RGB8))
                            {
                                string filename = $@"D:\uvss\underside image\Image_{ImageCount}.jpg";
                                convertedImage.Save(filename);
                                ImageCount++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing images: {ex.Message}");
            }
        }
    }
}