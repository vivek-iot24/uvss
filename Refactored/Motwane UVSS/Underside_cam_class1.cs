using SpinnakerNET;
using SpinnakerNET.GenApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Motwane.UVSS
{
    public class Underside_cam_class1
    {
        public event Action<BitmapSource> OnNewFrame;

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
                    return "No camera detected.";

                camera = camList[0];
                camera.Init();

                // Set pixel format to RGB8
                INodeMap nodeMap = camera.GetNodeMap();
                IEnum iPixelFormat = nodeMap.GetNode<IEnum>("PixelFormat");
                if (iPixelFormat != null && iPixelFormat.IsWritable)
                {
                    IEnumEntry iRgb8 = iPixelFormat.GetEntryByName("RGB8");
                    if (iRgb8 != null)
                        iPixelFormat.Value = iRgb8.Symbolic;
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
                try { camera.EndAcquisition(); } catch { }
            }
        }

        public void CloseCamera()
        {
            StopAcquisition();

            if (camera != null)
            {
                try
                {
                    camera.DeInit();
                    camera.Dispose();
                }
                catch { }

                camera = null;
            }

            camList?.Clear();
            system?.Dispose();
            IsInitialized = false;
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
                                // Live preview: convert to BitmapSource and send to UI
                                BitmapSource bitmap = BitmapSource.Create(
                                    (int)convertedImage.Width,
                                    (int)convertedImage.Height,
                                    96, 96,
                                    System.Windows.Media.PixelFormats.Rgb24,
                                    null,
                                    convertedImage.DataPtr,
                                    (int)convertedImage.BufferSize,
                                    (int)(convertedImage.Width * 3) // Stride: width × 3 for RGB8
                                );

                                bitmap.Freeze(); // Freeze for cross-thread access
                                OnNewFrame?.Invoke(bitmap);

                                // Save image (you can add condition if needed)
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
