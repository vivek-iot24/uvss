using DocumentFormat.OpenXml.Drawing;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32; // ADDED THIS LINE FOR THE FILE DIALOG
using Motwane.UVSS.Application.Interfaces.HAL;
using Motwane.UVSS.Application.Services;
using Motwane.UVSS.HAL;
using Motwane.UVSS.Presentation;
using Motwane.UVSS.Presentation.ViewModels;
using Motwane.UVSS.Presentation.Windows;
using Onvif.Core.Client;
using Onvif.Core.Client.Media;
using Onvif.IP;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.Internal.Vectors;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Motwane.UVSS.Application.ComputerVision;
using System.Windows.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using MessageBox = System.Windows.MessageBox;

namespace Motwane.UVSS.Presentation.Windows
{
    /// <summary>
    /// Interaction logic for Main_uvss_page.xaml
    /// </summary>
    public partial class Main_uvss_page : System.Windows.Window
    {

        // private BitmapSource _originalBitmap;   // THIS NEW VARIABLE will hold the clean, original version of the current image.
        private readonly ICameraService _cameraService;
        private readonly AnprEngine _anprEngine;
        private DispatcherTimer timer;
        private readonly VehicleEntryService _vehicleEntryService;
        private Underside_cam_class Underside_cameraHandler;
        private readonly ISensorService _sensorService;
        #region using this to communicate with ir sensor


     

        #endregion

        #region using for video pinhole camera

        private const string basefolder_to_save_pinhole_videos = @"D:\uvss\under_vehicle_video_center";

        // ── 2) CAMERA CREDENTIALS & IP LIST ───────────────────────────────────────

        private const string CameraUser = "admin";
        private const string CameraPass = "sefthS$2702";

        //    Add or remove as many IPs as you want here:
        private readonly string[] CameraIPs =
        {
            "192.168.4.58",
            "192.168.4.59",
            "192.168.4.60"
        };


        private readonly List<string> RtspUrls = new List<string>();

        // ── 4) ONE MediaPlayer / Media per camera ─────────────────────────────────
        private readonly List<LibVLCSharp.Shared.MediaPlayer> _mediaPlayers = new List<LibVLCSharp.Shared.MediaPlayer>();
        private readonly List<LibVLCSharp.Shared.Media> _medias = new List<LibVLCSharp.Shared.Media>();

        // ── 5) OUTPUT FILEPATHS (camera1.mp4, camera2.mp4, …) ────────────────────
        private readonly List<string> _outputPaths = new List<string>();

        // ── 6) LibVLC CORE OBJECT ──────────────────────────────────────────────────
        private LibVLC _libVLC;

        // ── 7) TRACK WHETHER RECORDING HAS STARTED ─────────────────────────────────
        private bool _isRecordingAll = false;

        #endregion



        #region  using for ANPR and driver camera

        private const string anprsnapshotUrl = "https://192.168.4.56/cpapi/snapshot.cgi";
        // private const string driversnapshotUrl = "https://192.168.4.57/cpapi/snapshot.cgi";
        private const string driversnapshotUrl = "https://admin:sefthS$2702@192.168.4.57/cpapi/snapshot.cgi";
        private const string username = "admin";
        private const string password = "sefthS$2702";

        #endregion
        private readonly VehicleEntryService _vehicleService;
        private volatile bool isRecognitionCompleted = false;

        public Main_uvss_page(
     VehicleEntryService vehicleEntryService,
     ICameraService cameraService,AnprEngine anprEngine)
        {
            InitializeComponent();

            _vehicleEntryService = vehicleEntryService;
            _cameraService = cameraService;
            _anprEngine = anprEngine;
            StartClock();

            _cameraService.Initialize();
            _cameraService.Start();

            foreach (var ip in CameraIPs)
            {
                var rtsp = $"rtsp://{CameraUser}:{CameraPass}@{ip}:554/cam/realmonitor?channel=1&subtype=0";
                RtspUrls.Add(rtsp);
            }

            Core.Initialize();
            _libVLC = new LibVLC();
            
        }

        private void StartClock()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Update every second
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTextBlock.Text = DateTime.Now.ToString("MMMM dd, yyyy");
            TimeTextBlock.Text = DateTime.Now.ToString("hh:mm:ss tt");
        }

        private void menu_button_Click(object sender, RoutedEventArgs e)
        {
            Menu_screen menu_Screen = new Menu_screen();
            menu_Screen.Show();

            //this.Close();


         
        }


        private byte[] originalPixels;
        private int width, height, stride;

        private void load()
        {
            BitmapSource source = (BitmapSource)Sticked_image.Source;
            FormatConvertedBitmap formattedBitmap = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            width = formattedBitmap.PixelWidth;
            height = formattedBitmap.PixelHeight;
            stride = width * 4;

            originalPixels = new byte[height * stride];
            formattedBitmap.CopyPixels(originalPixels, stride, 0);

            Sticked_image.Source = formattedBitmap;


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            load();
            // numberplate_image.MaxWidth = numberplate_image.Width;
            numberplate_image.MaxHeight = 40;
            //MessageBox.Show(mainwindow_class.USERID);
            username_textbox.Text = ((App)System.Windows.Application.Current).LoggedInUserID;
            //usertype_textbox.Text = ((App)System.Windows.Application.Current).LoggedInUSERTYPE;

           

            //// ADD THIS LINE: Capture the initial hardcoded image as our first "master copy".
            //if (Sticked_image.Source != null)
            //{
            //    _originalBitmap = Sticked_image.Source as BitmapSource;
            //}

            // This code creates the "master copy" of the image for your sliders to use.

            // Step 1: Safety check to make sure the hardcoded image was loaded.
            if (Sticked_image.Source is BitmapSource bitmapSource)
            {
                // Step 2: Get the image's dimensions and format details.
                width = bitmapSource.PixelWidth;
                height = bitmapSource.PixelHeight;
                stride = width * (bitmapSource.Format.BitsPerPixel / 8);

                // Step 3: Create the byte array and copy the pixel data into it.
                originalPixels = new byte[height * stride];
                bitmapSource.CopyPixels(originalPixels, stride, 0);
            }

        }

        private string lastState = "0";

        private void HandleSensorSignal(string incoming)
        {
            Dispatcher.Invoke(() =>
            {
                if (incoming == "1" && lastState != "1")
                {
                    lastState = "1";

                    sensor_display.Fill = System.Windows.Media.Brushes.Red;
                    HOLD_BTN.IsEnabled = false;
                    PASS_BTN.IsEnabled = false;

                    CaptureAnpr();
                    CaptureDriver();

                    _ = StartRecordingAsync();
                    Underside_cameraHandler.StartAcquisition();
                }
                else if (incoming == "0" && lastState != "0")
                {
                    lastState = "0";

                    sensor_display.Fill = System.Windows.Media.Brushes.Green;
                    HOLD_BTN.IsEnabled = true;
                    PASS_BTN.IsEnabled = true;

                    StopAllRecordingsSafe();
                    Underside_cameraHandler.StopAcquisition();
                }
            });
        }

        //private async void anpr_imageCaptureSnapshotAndShow()
        //{
        //    try
        //    {
        //        var handler = new HttpClientHandler
        //        {
        //            Credentials = new NetworkCredential(username, password),
        //            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true // For HTTPS without valid cert
        //        };

        //        var client = new HttpClient(handler);
        //        var response = await client.GetAsync(anprsnapshotUrl);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            MessageBox.Show($"Failed to capture image: {response.StatusCode}");
        //            return;
        //        }

        //        var imageBytes = await response.Content.ReadAsByteArrayAsync();

        //        // Load image into WPF Image control
        //        var ms = new MemoryStream(imageBytes);
        //        var bitmap = new BitmapImage();
        //        bitmap.BeginInit();
        //        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmap.StreamSource = ms;
        //        bitmap.EndInit();
        //        bitmap.Freeze();

        //        Anpr_image.Source = bitmap; // Assuming you have an Image control named imgSnapshot

        //        StartNumberPlateRecognitionThread();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error: " + ex.Message);
        //    }
        //}

        //private async void driver_imageCaptureSnapshotAndShow()
        //{
        //    try
        //    {

        //        //var handler = new HttpClientHandler
        //        //{
        //        //    Credentials = new NetworkCredential("admin", "sefthS$2702")
        //        //};

        //        // var client = new HttpClient(handler);
        //        //var response = await client.GetAsync("http://192.168.4.57/cpapi/snapshot.cgi");




        //        var handler = new HttpClientHandler
        //        {
        //            Credentials = new NetworkCredential(username, password),
        //            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true // For HTTPS without valid cert
        //        };

        //        var client = new HttpClient(handler);

        //        var response = await client.GetAsync(driversnapshotUrl);

        //        if (!response.IsSuccessStatusCode)
        //        {
        //            MessageBox.Show($"Failed to capture image: {response.StatusCode}");
        //            return;
        //        }

        //        var imageBytes = await response.Content.ReadAsByteArrayAsync();

        //        // Load image into WPF Image control
        //        var ms = new MemoryStream(imageBytes);
        //        var bitmap = new BitmapImage();
        //        bitmap.BeginInit();
        //        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmap.StreamSource = ms;
        //        bitmap.EndInit();
        //        bitmap.Freeze();

        //        Driver_image.Source = bitmap; // Assuming you have an Image control named imgSnapshot
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Error: " + ex.Message);
        //    }
        //}

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {


            if (originalPixels == null) return;

            double brightness = BrightnessSlider.Value;
            double contrast = ContrastSlider.Value;


            byte[] adjustedPixels = new byte[originalPixels.Length];

            for (int i = 0; i < originalPixels.Length; i += 4)
            {
                for (int c = 0; c < 3; c++) // R, G, B
                {
                    // Normalize to 0-1 range
                    double color = originalPixels[i + c] / 255.0;

                    // Apply contrast (centered around 0.5)
                    color -= 0.5;
                    color *= contrast;
                    color += 0.5;

                    // Apply brightness
                    color *= brightness;

                    adjustedPixels[i + c] = Clamp(color * 255);
                }

                adjustedPixels[i + 3] = originalPixels[i + 3]; // Alpha
            }

            WriteableBitmap adjustedBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            adjustedBitmap.WritePixels(new Int32Rect(0, 0, width, height), adjustedPixels, stride, 0);


            Sticked_image.Source = adjustedBitmap;
        }

        private void SharpnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {


            if (originalPixels == null) return;

            double brightness = BrightnessSlider.Value;
            double contrast = ContrastSlider.Value;
            double sharpness = SharpnessSlider.Value;



            byte[] contrastBrightnessPixels = new byte[originalPixels.Length];

            // Step 1: Apply Brightness and Contrast first
            for (int i = 0; i < originalPixels.Length; i += 4)
            {
                for (int c = 0; c < 3; c++) // R, G, B
                {
                    double color = originalPixels[i + c] / 255.0;
                    color -= 0.5;
                    color *= contrast;
                    color += 0.5;
                    color *= brightness;
                    contrastBrightnessPixels[i + c] = Clamp(color * 255);
                }
                contrastBrightnessPixels[i + 3] = originalPixels[i + 3]; // A
            }

            // Step 2: Apply sharpness (skip if sharpness = 0)
            byte[] finalPixels = sharpness == 0 ? contrastBrightnessPixels : ApplySharpening(contrastBrightnessPixels, sharpness);

            // Write back to image
            WriteableBitmap adjustedBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            adjustedBitmap.WritePixels(new Int32Rect(0, 0, width, height), finalPixels, stride, 0);
            Sticked_image.Source = adjustedBitmap;
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {


            if (originalPixels == null) return;

            double brightness = e.NewValue;

            byte[] adjustedPixels = new byte[originalPixels.Length];
            for (int i = 0; i < originalPixels.Length; i += 4)
            {
                adjustedPixels[i] = Clamp(originalPixels[i] * brightness);     // B
                adjustedPixels[i + 1] = Clamp(originalPixels[i + 1] * brightness); // G
                adjustedPixels[i + 2] = Clamp(originalPixels[i + 2] * brightness); // R
                adjustedPixels[i + 3] = originalPixels[i + 3]; // A
            }

            WriteableBitmap adjustedBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
            adjustedBitmap.WritePixels(new Int32Rect(0, 0, width, height), adjustedPixels, stride, 0);

            Sticked_image.Source = adjustedBitmap;
        }

        private void reset_button_Click(object sender, RoutedEventArgs e)
        {
            BrightnessSlider.Value = 1.0;
            SharpnessSlider.Value = 0;
            ContrastSlider.Value = 1;

            string Anprimage = "D:\\muvss_name.png";
            string MainImage = "D:\\muvss_name.png";
            string NumberPlateImage = "D:\\muvss_name.png";
            string DriverImage = "D:\\muvss_name.png";


            // This line loads the new image and puts it in the display frame.
            Sticked_image.Source = LoadImageUnlocked(MainImage);
            Driver_image.Source = LoadImageUnlocked(DriverImage);
            Anpr_image.Source = LoadImageUnlocked(Anprimage);
            numberplate_image.Source = LoadImageUnlocked(NumberPlateImage);
            Numberplate_number_box.Text = "";
        }

        private byte Clamp(double value)
        {
            return (byte)(value > 255 ? 255 : (value < 0 ? 0 : value));
        }

        private byte[] ApplySharpening(byte[] pixels, double sharpness)
        {
            int kernelSize = 3;
            int radius = kernelSize / 2;

            // Sharpen kernel (classic with user strength)
            double[,] kernel = {
        { 0, -1,  0 },
        { -1, 4 + sharpness, -1 },
        { 0, -1,  0 }
    };

            byte[] output = new byte[pixels.Length];

            for (int y = radius; y < height - radius; y++)
            {
                for (int x = radius; x < width - radius; x++)
                {
                    double[] sum = new double[3]; // R, G, B

                    for (int ky = -radius; ky <= radius; ky++)
                    {
                        for (int kx = -radius; kx <= radius; kx++)
                        {
                            int px = x + kx;
                            int py = y + ky;
                            int index = (py * stride) + (px * 4);

                            double k = kernel[ky + radius, kx + radius];

                            sum[0] += pixels[index + 2] * k; // R
                            sum[1] += pixels[index + 1] * k; // G
                            sum[2] += pixels[index] * k;     // B
                        }
                    }

                    int centerIndex = (y * stride) + (x * 4);
                    output[centerIndex + 2] = Clamp(pixels[centerIndex + 2] + sum[0]); // R
                    output[centerIndex + 1] = Clamp(pixels[centerIndex + 1] + sum[1]); // G
                    output[centerIndex] = Clamp(pixels[centerIndex] + sum[2]); // B
                    output[centerIndex + 3] = pixels[centerIndex + 3]; // A
                }
            }

            return output;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                Underside_cameraHandler?.CloseCamera();

                base.OnClosed(e);

            }
            catch
            {

            }



        }

        private string drivercam_rtspUrl = "rtsp://admin:sefthS$2702@192.168.4.57:554/video/live?channel=1&subtype=0";

        private void Diver_camera_image_trigger()
        {
            Task.Run(() =>
            {
                try
                {
                    //string drivercam_rtspUrl = "D:\\young-indian-truck-driver-concept-260nw-2352219515.jpg";
                    CaptureRTSPFrame(drivercam_rtspUrl, @"D:\uvss\driver_camera_images\Image_2.jpg");
                    // To_capture_driver_image();
                }
                catch
                {
                    MessageBox.Show("Error occur in driver camera!!!");
                }

                a = 2;
                LoadImageWithoutLocking(@"D:\uvss\driver_camera_images\Image_2.jpg", Driver_image);
            });
        }


        private OpenCvSharp.VideoCapture driver_capture;
        private bool driver_isStreaming = false;
        private Thread driver_cameraThread;
        private volatile bool showImage = false; // make it accessible from outside

        private void StartdriverCamera()
        {
            try
            {
                Cv2.SetNumThreads(0); // Optional: avoid threading issues
                driver_capture = new OpenCvSharp.VideoCapture(drivercam_rtspUrl, VideoCaptureAPIs.FFMPEG);

                if (!driver_capture.IsOpened())
                {
                    Dispatcher.Invoke(() => MessageBox.Show("Failed to open RTSP stream."));
                    driver_isStreaming = false;
                    return;
                }

                OpenCvSharp.Mat frame = new OpenCvSharp.Mat();

                while (driver_isStreaming)
                {
                    bool readSuccess = driver_capture.Read(frame);

                    if (readSuccess && !frame.Empty())
                    {
                        if (showImage) // only show frame when flag is true
                        {
                            var bitmap = BitmapSourceConverter.ToBitmapSource(frame);
                            bitmap.Freeze();

                            Dispatcher.Invoke(() =>
                            {
                                Driver_image.Source = bitmap;
                            });

                            showImage = false; // reset flag after showing one frame
                        }
                    }
                    else
                    {
                        Thread.Sleep(100); // wait and try again
                    }
                }

                driver_capture.Release();
            }
            catch (System.Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Error: " + ex.Message);
                });
            }
        }


        OpenCvSharp.VideoCapture capture = new OpenCvSharp.VideoCapture();
        public static bool CaptureRTSPFrame(string rtspUrl, string savePath)
        {
            try
            {
                var cap = new OpenCvSharp.VideoCapture(rtspUrl, VideoCaptureAPIs.FFMPEG);
                if (!cap.IsOpened())
                    return false;

                Thread.Sleep(200); // allow buffer to fill

                var frame = new OpenCvSharp.Mat();
                cap.Read(frame);

                if (frame.Empty())
                    return false;

                Cv2.ImWrite(savePath, frame, new[] { new ImageEncodingParam(OpenCvSharp.ImwriteFlags.JpegQuality, 95) });
                return true;
            }
            catch
            {
                return false;
            }
        }

        static async Task DriverCamera()
        {
            string cameraIp = "192.168.4.57";
            string username = "admin";
            string password = "sefthS$2702"; // encode if needed ($ → %24)

            // Create Media client
            var mediaClient = await OnvifClientFactory.CreateMediaClientAsync(
            $"http://{cameraIp}/onvif/device_service",
            username,
            password
            );



            var profilesResponse = await mediaClient.GetProfilesAsync();
            var profile = profilesResponse.Profiles[0]; // use first available profile

            // Get snapshot URI
            var snapshotUri = await mediaClient.GetSnapshotUriAsync(profile.token);
            Console.WriteLine("Snapshot URI: " + snapshotUri.Uri);

            // Download snapshot
            using (var httpClient = new HttpClient(new HttpClientHandler
            {
                Credentials = new System.Net.NetworkCredential(username, password)
            }))
            {
                var imgBytes = await httpClient.GetByteArrayAsync(snapshotUri.Uri);
                File.WriteAllBytes("snapshot.jpg", imgBytes);
            }

            Console.WriteLine("Snapshot saved to snapshot.jpg");
        }

        private void anpr_camera_image_trigger()
        {
            Task.Run(() =>
            {
                try
                {
                    string vehicalcam_rtspUrl = "D:\\anpr-blog-1732110542";
                    // To_capture_anpr_image();
                    CaptureRTSPFrame(vehicalcam_rtspUrl, @"D:\uvss\anpr image\Image.jpg");
                }
                catch
                {
                    MessageBox.Show("Error occur in driver camera!!!");
                }

                a = 2;
                LoadImageWithoutLocking(@"D:\uvss\anpr image\Image.jpg", Anpr_image);
            });
        }

        public void fgyh()
        {

            string folderPath = @"D:\uvss\underside image";
            ProcessImagesFromFolder(folderPath);

            load();
        }

        private void try_btn_Click(object sender, RoutedEventArgs e)   //  just to try // For driver camera
        {
            string folderPath = @"D:\uvss\underside image";
            ProcessImagesFromFolder(folderPath);

            load();
        }

        public void ProcessImagesFromFolder(string folderPath)
        {
            // Ensure the folder exists
            if (Directory.Exists(folderPath))
            {
                // Get all image files in the folder (you can filter by specific file extensions)
                string[] imageFiles = Directory.GetFiles(folderPath, "*.jpg");

                // Call the original method with the list of image files
                // CreatePanoramicImage_Fast_2(imageFiles);

                CallPanoramaWhenReady();



                //  CreatePanoramicImage_Pro(imageFiles);
            }
            else
            {
                Console.WriteLine("Folder does not exist.");
            }
        }

        private async void CallPanoramaWhenReady()
        {
            // Wait until recognition is done
            while (!isRecognitionCompleted)
            {
                await Task.Delay(200); // wait 200ms and check again
            }

            // Once done, call method
            try
            {
                string[] imageFiles = Directory.GetFiles(@"D:\uvss\underside image", "*.jpg");
                if (imageFiles.Length > 0)
                {
                    CreatePanoramicImage_Fast_2(imageFiles);
                }
                else
                {
                    MessageBox.Show("No images found to stitch.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error in stitching: " + ex.Message);
            }
        }

        public void LoadImageWithoutLocking(string imagePath, System.Windows.Controls.Image imageControl)
        {
            // Load image into memory
            BitmapImage bitmap = new BitmapImage();
            using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Important: load the image into memory
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze(); // Optional: make it cross-thread accessible
            }

            // Set the image source
            // imageControl.Source = bitmap;

            imageControl.Dispatcher.Invoke(() =>
            {
                imageControl.Source = bitmap;
            });

        }

        public int a;
        private void driver_next_image_Click(object sender, RoutedEventArgs e)  // For driver camera
        {
            try
            {
                if (a < 5)
                {
                    a++;
                    LoadImageWithoutLocking(@"D:\uvss\driver_camera_images\Image_" + a + ".jpg", Driver_image);
                }
            }
            catch
            {

            }
        }

        private void try1_btn_Click(object sender, RoutedEventArgs e)
        {
            // Diver_camera_image_trigger();

            Task.Run(() => Dispatcher.Invoke(async () => await StartRecordingAsync()));


            Underside_cameraHandler.StartAcquisition();
        }

        private void try2_btn_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() => Dispatcher.Invoke(() => StopAllRecordingsSafe()));

            Underside_cameraHandler.StopAcquisition();
        }

        private void Sticked_image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            System.Windows.Point mousePos = e.GetPosition(Sticked_image);
            double zoomFactor = e.Delta > 0 ? 1.1 : 1 / 1.1;

            // Current scale
            double oldScale = imageScaleTransform.ScaleX;
            double newScale = oldScale * zoomFactor;

            // Optional: Clamp zoom level
            if (newScale < 1 || newScale > 5)
                return;

            // Calculate offsets
            double absX = mousePos.X * oldScale + imageTranslateTransform.X;
            double absY = mousePos.Y * oldScale + imageTranslateTransform.Y;

            // Apply new scale
            imageScaleTransform.ScaleX = newScale;
            imageScaleTransform.ScaleY = newScale;

            // Adjust translation so the point under the mouse stays under the mouse
            imageTranslateTransform.X = absX - mousePos.X * newScale;
            imageTranslateTransform.Y = absY - mousePos.Y * newScale;
        }
        private void driver_previous_image_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (a > 0)
                {
                    a--;
                    LoadImageWithoutLocking(@"D:\uvss\driver_camera_images\Image_" + a + ".jpg", Driver_image);
                }
            }
            catch
            {

            }
        }

     
        DateTime now = DateTime.Now;

        private void PASS_BTN_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MainViewModel)DataContext;
            vm.PassCommand.Execute(null);
        }

        private void HOLD_BTN_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MainViewModel)DataContext;
            vm.HoldCommand.Execute(null);
        }

        //private byte[] ImageToByteArray(System.Windows.Controls.Image imageControl)
        //{
        //    if (imageControl.Source == null)
        //        return null;

        //    var bitmapSource = imageControl.Source as BitmapSource;

        //    using (var stream = new MemoryStream())
        //    {
        //        BitmapEncoder encoder = new PngBitmapEncoder();
        //        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
        //        encoder.Save(stream);
        //        return stream.ToArray();
        //    }
        //}


      
        private void CreatePanoramicImage_2(string[] imageFiles)
        {
            var firstImage = new Bitmap(imageFiles[0]);
            int dynamicSliceWidth = Math.Max(5, firstImage.Width / 50); // Adaptive slice width

            int blendWidth = dynamicSliceWidth / 2; // 50% of the slit width used for blending
            int totalWidth = (imageFiles.Length - 1) * (dynamicSliceWidth - blendWidth) + dynamicSliceWidth;
            int totalHeight = firstImage.Width; // After 90-degree rotation

            using (Bitmap panoramicImage = new Bitmap(totalWidth, totalHeight))
            using (Graphics g = Graphics.FromImage(panoramicImage))
            {
                int currentX = 0;

                foreach (string file in imageFiles)
                {
                    using (var img = new Bitmap(file))
                    {
                        // Rotate the image 90 degrees left 
                        img.RotateFlip(RotateFlipType.Rotate270FlipNone);

                        // Extract a wider slit from the middle region
                        int middleColumn = img.Width / 2;
                        System.Drawing.Rectangle slit = new System.Drawing.Rectangle(middleColumn - (dynamicSliceWidth / 2), 0, dynamicSliceWidth, img.Height);
                        Bitmap slitImage = img.Clone(slit, img.PixelFormat);

                        // Apply non-linear blending at the edges of the slit image
                        if (currentX > 0) // Skip blending for the first image
                        {
                            for (int x = 0; x < blendWidth; x++)
                            {
                                // Using a sigmoid-like function for smoother blending
                                double t = (double)x / blendWidth;
                                double blendFactor = t * t * (3 - 2 * t); // Smooth step interpolation formula
                                blendFactor *= 255; // Scaling factor for color blending

                                for (int y = 0; y < slitImage.Height; y++)
                                {
                                    System.Drawing.Color prevColor = panoramicImage.GetPixel(currentX - blendWidth + x, y);
                                    System.Drawing.Color newColor = slitImage.GetPixel(x, y);

                                    int r = (int)((prevColor.R * (255 - blendFactor) + newColor.R * blendFactor) / 255);
                                    int gColor = (int)((prevColor.G * (255 - blendFactor) + newColor.G * blendFactor) / 255);
                                    int b = (int)((prevColor.B * (255 - blendFactor) + newColor.B * blendFactor) / 255);

                                    System.Drawing.Color blendedColor = System.Drawing.Color.FromArgb(r, gColor, b);
                                    panoramicImage.SetPixel(currentX - blendWidth + x, y, blendedColor);
                                }
                            }
                        }

                        // Draw the slit image in the final panoramic image, adjusting for the blended region
                        g.DrawImage(slitImage, new System.Drawing.Rectangle(currentX, 0, dynamicSliceWidth - blendWidth, totalHeight), new System.Drawing.Rectangle(blendWidth, 0, dynamicSliceWidth - blendWidth, totalHeight), GraphicsUnit.Pixel);
                        currentX += (dynamicSliceWidth - blendWidth);
                    }
                }

                string outputPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PanoramicImage.jpg");
                panoramicImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Jpeg);

                //Sticked_image.Source = 

                // System.Windows.MessageBox.Show($"Panoramic image saved to: {outputPath}");

                LoadImageWithoutLocking(outputPath, Sticked_image);
            }
        }

        private void CreatePanoramicImage_22(string[] imageFiles)
        {
            var firstImage = new Bitmap(imageFiles[0]);
            int dynamicSliceWidth = Math.Max(5, firstImage.Width / 50);
            int blendWidth = dynamicSliceWidth / 2;
            int totalHeight = firstImage.Width; // After rotation

            List<Bitmap> slitImages = new List<Bitmap>();

            Bitmap previousSlice = null;
            foreach (string file in imageFiles)
            {
                using (var img = new Bitmap(file))
                {
                    img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    int middleColumn = img.Width / 2;
                    System.Drawing.Rectangle slit = new System.Drawing.Rectangle(middleColumn - (dynamicSliceWidth / 2), 0, dynamicSliceWidth, img.Height);
                    Bitmap slitImage = img.Clone(slit, img.PixelFormat);

                    if (previousSlice != null)
                    {
                        double similarity = CalculateMSE(previousSlice, slitImage);
                        if (similarity < 50) // Threshold to filter duplicates; tweak as needed
                        {
                            slitImage.Dispose();
                            continue; // Skip similar image
                        }
                    }

                    previousSlice = (Bitmap)slitImage.Clone();
                    slitImages.Add(slitImage);
                }
            }

            int totalWidth = slitImages.Count * (dynamicSliceWidth - blendWidth) + blendWidth;

            using (Bitmap panoramicImage = new Bitmap(totalWidth, totalHeight))
            using (Graphics g = Graphics.FromImage(panoramicImage))
            {
                int currentX = 0;
                for (int i = 0; i < slitImages.Count; i++)
                {
                    Bitmap slitImage = slitImages[i];

                    if (i > 0)
                    {
                        for (int x = 0; x < blendWidth; x++)
                        {
                            double t = (double)x / blendWidth;
                            double blendFactor = t * t * (3 - 2 * t);
                            blendFactor *= 255;

                            for (int y = 0; y < slitImage.Height; y++)
                            {
                                System.Drawing.Color prevColor = panoramicImage.GetPixel(currentX - blendWidth + x, y);
                                System.Drawing.Color newColor = slitImage.GetPixel(x, y);

                                int r = (int)((prevColor.R * (255 - blendFactor) + newColor.R * blendFactor) / 255);
                                int gColor = (int)((prevColor.G * (255 - blendFactor) + newColor.G * blendFactor) / 255);
                                int b = (int)((prevColor.B * (255 - blendFactor) + newColor.B * blendFactor) / 255);

                                System.Drawing.Color blendedColor = System.Drawing.Color.FromArgb(r, gColor, b);
                                panoramicImage.SetPixel(currentX - blendWidth + x, y, blendedColor);
                            }
                        }
                    }

                    g.DrawImage(slitImage, new System.Drawing.Rectangle(currentX, 0, dynamicSliceWidth - blendWidth, totalHeight),
                        new System.Drawing.Rectangle(blendWidth, 0, dynamicSliceWidth - blendWidth, totalHeight), GraphicsUnit.Pixel);

                    currentX += (dynamicSliceWidth - blendWidth);
                }

                foreach (var slice in slitImages)
                    slice.Dispose();

                string outputPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PanoramicImage.jpg");
                panoramicImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                System.Windows.MessageBox.Show($"Panoramic image saved to: {outputPath}");

                LoadImageWithoutLocking(outputPath, Sticked_image);
            }
        }

        private double CalculateMSE(Bitmap img1, Bitmap img2)
        {
            if (img1.Width != img2.Width || img1.Height != img2.Height)
                return double.MaxValue;

            double sum = 0;
            for (int y = 0; y < img1.Height; y++)
            {
                for (int x = 0; x < img1.Width; x++)
                {
                    System.Drawing.Color c1 = img1.GetPixel(x, y);
                    System.Drawing.Color c2 = img2.GetPixel(x, y);

                    int gray1 = (c1.R + c1.G + c1.B) / 3;
                    int gray2 = (c2.R + c2.G + c2.B) / 3;

                    int diff = gray1 - gray2;
                    sum += diff * diff;
                }
            }

            return sum / (img1.Width * img1.Height);
        }

        private void CreatePanoramicImage_20(string[] imageFiles)
        {
            // Load first image just to compute sizes
            using (var firstImage = new Bitmap(imageFiles[0]))
            {
                int dynamicSliceWidth = Math.Max(5, firstImage.Width / 50);
                int blendWidth = dynamicSliceWidth / 2;
                int totalWidth = (imageFiles.Length - 1) * (dynamicSliceWidth - blendWidth) + dynamicSliceWidth;
                int totalHeight = firstImage.Width; // height after rotating 90°

                // Create output bitmap in 32bpp for fast LockBits access
                using (var panoramic = new Bitmap(totalWidth, totalHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    var outputRect = new System.Drawing.Rectangle(0, 0, totalWidth, totalHeight);
                    var outputData = panoramic.LockBits(outputRect, ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    int outputStride = outputData.Stride;
                    int bytesPerPixel = 4;
                    int outputBytes = outputStride * totalHeight;
                    var outputBuffer = new byte[outputBytes];
                    // No need to fill; zeroed buffer means transparent/black

                    int currentX = 0;

                    foreach (string file in imageFiles)
                    {
                        using (var img = new Bitmap(file))
                        {
                            // Rotate 90° CCW
                            img.RotateFlip(RotateFlipType.Rotate270FlipNone);

                            int sliceX = (img.Width / 2) - (dynamicSliceWidth / 2);
                            var rotatedRect = new System.Drawing.Rectangle(0, 0, img.Width, img.Height);
                            var rotatedData = img.LockBits(rotatedRect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                            int rotatedStride = rotatedData.Stride;
                            int rotatedBytes = rotatedStride * totalHeight;
                            var rotatedBuffer = new byte[rotatedBytes];
                            Marshal.Copy(rotatedData.Scan0, rotatedBuffer, 0, rotatedBytes);

                            // Blend and copy slice column by column
                            for (int x = 0; x < dynamicSliceWidth; x++)
                            {
                                if (currentX > 0 && x < blendWidth)
                                {
                                    double t = (double)x / blendWidth;
                                    double blendF = t * t * (3 - 2 * t); // smooth-step

                                    for (int y = 0; y < totalHeight; y++)
                                    {
                                        int destBlendX = currentX - blendWidth + x;
                                        int outIdx = y * outputStride + destBlendX * bytesPerPixel;
                                        int rotIdx = y * rotatedStride + (sliceX + x) * bytesPerPixel;

                                        byte prevB = outputBuffer[outIdx];
                                        byte prevG = outputBuffer[outIdx + 1];
                                        byte prevR = outputBuffer[outIdx + 2];
                                        // byte prevA = outputBuffer[outIdx + 3];

                                        byte newB = rotatedBuffer[rotIdx];
                                        byte newG = rotatedBuffer[rotIdx + 1];
                                        byte newR = rotatedBuffer[rotIdx + 2];
                                        // byte newA = rotatedBuffer[rotIdx + 3];

                                        byte r = (byte)((prevR * (255 - blendF) + newR * blendF) / 255);
                                        byte gC = (byte)((prevG * (255 - blendF) + newG * blendF) / 255);
                                        byte b = (byte)((prevB * (255 - blendF) + newB * blendF) / 255);

                                        outputBuffer[outIdx] = b;
                                        outputBuffer[outIdx + 1] = gC;
                                        outputBuffer[outIdx + 2] = r;
                                        outputBuffer[outIdx + 3] = 255;
                                    }
                                }
                                else if (x >= blendWidth)
                                {
                                    int destX = currentX + x - blendWidth;
                                    for (int y = 0; y < totalHeight; y++)
                                    {
                                        int outIdx = y * outputStride + destX * bytesPerPixel;
                                        int rotIdx = y * rotatedStride + (sliceX + x) * bytesPerPixel;

                                        outputBuffer[outIdx] = rotatedBuffer[rotIdx];
                                        outputBuffer[outIdx + 1] = rotatedBuffer[rotIdx + 1];
                                        outputBuffer[outIdx + 2] = rotatedBuffer[rotIdx + 2];
                                        outputBuffer[outIdx + 3] = 255;
                                    }
                                }
                            }

                            img.UnlockBits(rotatedData);
                        }

                        currentX += (dynamicSliceWidth - blendWidth);
                    }

                    // Copy back to the bitmap and save
                    Marshal.Copy(outputBuffer, 0, outputData.Scan0, outputBytes);
                    panoramic.UnlockBits(outputData);

                    string outputPath = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        "PanoramicImage.jpg"
                    );
                    panoramic.Save(outputPath, ImageFormat.Jpeg);
                    LoadImageWithoutLocking(outputPath, Sticked_image);
                    // System.Windows.MessageBox.Show($"Panoramic image saved to: {outputPath}");
                }
            }
        }

        public void CreatePanoramicImage_Fast(string[] imageFiles)
        {
            if (imageFiles == null || imageFiles.Length == 0)
                throw new ArgumentException("No images provided.");

            // Load the first image just to compute dynamicSliceWidth and totalHeight:
            var firstBmpOrig = new Bitmap(imageFiles[0]);
            int dynamicSliceWidth = Math.Max(5, firstBmpOrig.Width / 50);
            int blendWidth = dynamicSliceWidth / 2;
            int totalHeight = firstBmpOrig.Width; // after 90° left rotation, height == original width

            // We'll store each slit as a raw byte[] in 24bpp format:
            var slitBuffers = new List<byte[]>();
            int sliceHeight = totalHeight;
            int bytesPerPixel = 3; // for PixelFormat.Format24bppRgb

            // We'll keep track of the previous slit to compute MSE:
            byte[] previousSlit = null;

            // 1) For each input file: rotate, convert to 24bpp, extract the center‐column slice into a byte[]:
            foreach (string file in imageFiles)
            {
                var orig = new Bitmap(file);
                orig.RotateFlip(RotateFlipType.Rotate270FlipNone);

                // 1a) Copy the rotated image onto a 24bpp bitmap (so we know pixel format = 24bpp):
                var bmp24 = new Bitmap(orig.Width, orig.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(bmp24))
                {
                    g.DrawImage(orig, 0, 0, orig.Width, orig.Height);
                }

                // 1b) LockBits for fast byte access:
                var rect = new System.Drawing.Rectangle(0, 0, bmp24.Width, bmp24.Height);
                var data = bmp24.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                try
                {
                    int stride = data.Stride; // usually >= Width * 3
                    IntPtr scan0 = data.Scan0;

                    // Compute where to start copying the middle columns:
                    int middleCol = bmp24.Width / 2;
                    int startX = middleCol - (dynamicSliceWidth / 2);
                    if (startX < 0) startX = 0;
                    if (startX + dynamicSliceWidth > bmp24.Width)
                        throw new InvalidOperationException("dynamicSliceWidth too large for this image.");

                    // Allocate a buffer for this slit (dynamicSliceWidth × sliceHeight × 3 bytes):
                    byte[] thisSlit = new byte[dynamicSliceWidth * sliceHeight * bytesPerPixel];

                    // Copy each row’s dynamicSliceWidth pixels from the locked data:
                    for (int y = 0; y < sliceHeight; y++)
                    {
                        // address of row y in the locked data:
                        IntPtr rowPtr = scan0 + y * stride;
                        int srcOffset = startX * bytesPerPixel;   // byte‐offset of the middle‐slice in this row
                        int destOffset = y * (dynamicSliceWidth * bytesPerPixel);

                        // Copy dynamicSliceWidth*3 bytes from rowPtr + srcOffset into thisSlit at destOffset:
                        Marshal.Copy(rowPtr + srcOffset, thisSlit, destOffset, dynamicSliceWidth * bytesPerPixel);
                    }

                    // 1c) Compute MSE against previousSlit (if it exists). If MSE < threshold, skip adding this slice:
                    if (previousSlit != null)
                    {
                        double mse = ComputeMSE(previousSlit, thisSlit, dynamicSliceWidth, sliceHeight);
                        if (mse < 50.0) // tweak threshold as needed
                        {
                            // Too‐similar; skip this one
                            continue;
                        }
                    }

                    // Keep a clone of thisSlit as previous for the next iteration:
                    previousSlit = new byte[thisSlit.Length];
                    Array.Copy(thisSlit, previousSlit, thisSlit.Length);
                    slitBuffers.Add(thisSlit);
                }
                finally
                {
                    bmp24.UnlockBits(data);
                }
            }

            if (slitBuffers.Count == 0)
                throw new InvalidOperationException("No distinct slits found.");

            // 2) Compute total width of final panorama:
            //    Each new slice contributes (dynamicSliceWidth – blendWidth) columns,
            //    except we add back blendWidth at the end to account for the final half‐overlap.
            int totalWidth = slitBuffers.Count * (dynamicSliceWidth - blendWidth) + blendWidth;

            // 3) Create final 24bpp panorama and lock it for writing:
            var panorama = new Bitmap(totalWidth, totalHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var panoRect = new System.Drawing.Rectangle(0, 0, totalWidth, totalHeight);
            var panoData = panorama.LockBits(panoRect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                int panoStride = panoData.Stride;
                IntPtr panoScan0 = panoData.Scan0;

                // We'll write directly, row by row. We'll keep a “currentX” (in pixels) where the next slice’s non‐blended region starts:
                int currentX = 0;

                unsafe
                {
                    byte* panoPtr = (byte*)panoScan0.ToPointer();

                    for (int sliceIndex = 0; sliceIndex < slitBuffers.Count; sliceIndex++)
                    {
                        byte[] slit = slitBuffers[sliceIndex];

                        // 3a) If not the very first slice, blend blendWidth columns with what’s already in panorama:
                        if (sliceIndex > 0)
                        {
                            for (int y = 0; y < sliceHeight; y++)
                            {
                                byte* panoRow = panoPtr + y * panoStride + (currentX - blendWidth) * bytesPerPixel;
                                int slitRowStart = y * (dynamicSliceWidth * bytesPerPixel);

                                for (int x = 0; x < blendWidth; x++)
                                {
                                    // t = fractional position in [0..1), blendFactor in [0..255]
                                    double t = (double)x / blendWidth;
                                    double blendFactor = (t * t * (3 - 2 * t)) * 255.0;
                                    if (blendFactor < 0) blendFactor = 0;
                                    if (blendFactor > 255) blendFactor = 255;

                                    int byteOffsetSlit = slitRowStart + x * bytesPerPixel;

                                    // For each color channel (B,G,R):
                                    for (int channel = 0; channel < 3; channel++)
                                    {
                                        byte prevColor = panoRow[x * 3 + channel];
                                        byte newColor = slit[byteOffsetSlit + channel];
                                        int blended = (int)((prevColor * (255 - blendFactor) + newColor * blendFactor) / 255.0);
                                        panoRow[x * 3 + channel] = (byte)blended;
                                    }
                                }
                            }
                        }

                        // 3b) Copy the remainder of this slice (columns [blendWidth .. dynamicSliceWidth)) directly into panorama:
                        int copyCols = dynamicSliceWidth - blendWidth;
                        for (int y = 0; y < sliceHeight; y++)
                        {
                            byte* panoRow = panoPtr + y * panoStride + currentX * bytesPerPixel;
                            int slitRowStart = y * (dynamicSliceWidth * bytesPerPixel) + blendWidth * bytesPerPixel;

                            // Copy (copyCols * 3) bytes from slit into pano:
                            for (int b = 0; b < copyCols * bytesPerPixel; b++)
                            {
                                panoRow[b] = slitRowStart < slit.Length
                                    ? slit[slitRowStart + b]
                                    : (byte)0;
                            }
                        }

                        // Advance currentX by copyCols for the next slice:
                        currentX += copyCols;
                    }
                }
            }
            finally
            {
                panorama.UnlockBits(panoData);
            }

            // 4) Save to Desktop (or wherever you like):
            string outputPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "PanoramicImage_Fast.jpg"
            );
            panorama.Save(outputPath, ImageFormat.Jpeg);
            LoadImageWithoutLocking(outputPath, Sticked_image);
            // MessageBox.Show($"Panoramic image saved to: {outputPath}");
        }

        //---------------------------------------------------------------------
        // Compute mean‐squared‐error between two slits (both byte[], 24bpp-packed).
        // width × height × 3 bytes each. Assumes identical dimensions.
        //---------------------------------------------------------------------

        public void CreatePanoramicImage_Fast_1(string[] imageFiles)
        {
            if (imageFiles == null || imageFiles.Length == 0)
                throw new ArgumentException("No images provided.");

            // Sort files by name
            Array.Sort(imageFiles);

            // Track used image file names
            var usedFiles = new List<string>();

            var firstBmpOrig = new Bitmap(imageFiles[0]);
            int dynamicSliceWidth = Math.Max(5, firstBmpOrig.Width / 50);
            int blendWidth = dynamicSliceWidth / 2;
            int totalHeight = firstBmpOrig.Width;

            var slitBuffers = new List<byte[]>();
            int sliceHeight = totalHeight;
            int bytesPerPixel = 3;

            byte[] previousSlit = null;

            foreach (string file in imageFiles)
            {
                var orig = new Bitmap(file);
                orig.RotateFlip(RotateFlipType.Rotate270FlipNone);

                var bmp24 = new Bitmap(orig.Width, orig.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(bmp24))
                {
                    g.DrawImage(orig, 0, 0, orig.Width, orig.Height);
                }

                var rect = new System.Drawing.Rectangle(0, 0, bmp24.Width, bmp24.Height);
                var data = bmp24.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                try
                {
                    int stride = data.Stride;
                    IntPtr scan0 = data.Scan0;

                    int middleCol = bmp24.Width / 2;
                    int startX = middleCol - (dynamicSliceWidth / 2);
                    if (startX < 0) startX = 0;
                    if (startX + dynamicSliceWidth > bmp24.Width)
                        throw new InvalidOperationException("dynamicSliceWidth too large for this image.");

                    byte[] thisSlit = new byte[dynamicSliceWidth * sliceHeight * bytesPerPixel];

                    for (int y = 0; y < sliceHeight; y++)
                    {
                        IntPtr rowPtr = scan0 + y * stride;
                        int srcOffset = startX * bytesPerPixel;
                        int destOffset = y * (dynamicSliceWidth * bytesPerPixel);

                        Marshal.Copy(rowPtr + srcOffset, thisSlit, destOffset, dynamicSliceWidth * bytesPerPixel);
                    }

                    if (previousSlit != null)
                    {
                        double mse = ComputeMSE(previousSlit, thisSlit, dynamicSliceWidth, sliceHeight);
                        if (mse < 50.0)
                        {
                            continue;
                        }
                    }

                    previousSlit = new byte[thisSlit.Length];
                    Array.Copy(thisSlit, previousSlit, thisSlit.Length);
                    slitBuffers.Add(thisSlit);
                    usedFiles.Add(System.IO.Path.GetFileName(file)); // store used file name
                }
                finally
                {
                    bmp24.UnlockBits(data);
                }
            }

            if (slitBuffers.Count == 0)
                throw new InvalidOperationException("No distinct slits found.");

            int totalWidth = slitBuffers.Count * (dynamicSliceWidth - blendWidth) + blendWidth;

            var panorama = new Bitmap(totalWidth, totalHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var panoRect = new System.Drawing.Rectangle(0, 0, totalWidth, totalHeight);
            var panoData = panorama.LockBits(panoRect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                int panoStride = panoData.Stride;
                IntPtr panoScan0 = panoData.Scan0;

                int currentX = 0;

                unsafe
                {
                    byte* panoPtr = (byte*)panoScan0.ToPointer();

                    for (int sliceIndex = 0; sliceIndex < slitBuffers.Count; sliceIndex++)
                    {
                        byte[] slit = slitBuffers[sliceIndex];

                        if (sliceIndex > 0)
                        {
                            for (int y = 0; y < sliceHeight; y++)
                            {
                                byte* panoRow = panoPtr + y * panoStride + (currentX - blendWidth) * bytesPerPixel;
                                int slitRowStart = y * (dynamicSliceWidth * bytesPerPixel);

                                for (int x = 0; x < blendWidth; x++)
                                {
                                    double t = (double)x / blendWidth;
                                    double blendFactor = (t * t * (3 - 2 * t)) * 255.0;
                                    if (blendFactor < 0) blendFactor = 0;
                                    if (blendFactor > 255) blendFactor = 255;

                                    int byteOffsetSlit = slitRowStart + x * bytesPerPixel;

                                    for (int channel = 0; channel < 3; channel++)
                                    {
                                        byte prevColor = panoRow[x * 3 + channel];
                                        byte newColor = slit[byteOffsetSlit + channel];
                                        int blended = (int)((prevColor * (255 - blendFactor) + newColor * blendFactor) / 255.0);
                                        panoRow[x * 3 + channel] = (byte)blended;
                                    }
                                }
                            }
                        }

                        int copyCols = dynamicSliceWidth - blendWidth;
                        for (int y = 0; y < sliceHeight; y++)
                        {
                            byte* panoRow = panoPtr + y * panoStride + currentX * bytesPerPixel;
                            int slitRowStart = y * (dynamicSliceWidth * bytesPerPixel) + blendWidth * bytesPerPixel;

                            for (int b = 0; b < copyCols * bytesPerPixel; b++)
                            {
                                panoRow[b] = slitRowStart < slit.Length
                                    ? slit[slitRowStart + b]
                                    : (byte)0;
                            }
                        }

                        currentX += copyCols;
                    }
                }
            }
            finally
            {
                panorama.UnlockBits(panoData);
            }

            // Save final image
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string outputPath = System.IO.Path.Combine(desktop, "PanoramicImage_Fast.jpg");
            panorama.Save(outputPath, ImageFormat.Jpeg);

            // Save image sequence log
            string sequencePath = System.IO.Path.Combine(desktop, "StitchedSequence.txt");
            File.WriteAllLines(sequencePath, usedFiles);

            // Optional display
            LoadImageWithoutLocking(outputPath, Sticked_image);
        }

        public void CreatePanoramicImage_Fast_2(string[] imageFiles)
        {
            if (imageFiles == null || imageFiles.Length == 0)
                throw new ArgumentException("No images provided.");

            // Step 1: Sort image files based on numeric suffix (e.g., Image_55.jpg < Image_100.jpg)
            string[] sortedImageFiles = imageFiles
                .OrderBy(f =>
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(f);
                    string numberPart = new string(name.Where(char.IsDigit).ToArray());
                    return int.TryParse(numberPart, out int num) ? num : int.MaxValue;
                })
                .ToArray();

            // Optional: Save sorted sequence to text file on Desktop
            string sequencePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "StitchedImageSequence.txt");
            File.WriteAllLines(sequencePath, sortedImageFiles.Select(f => System.IO.Path.GetFileName(f)));

            // Now continue with the original logic using sortedImageFiles
            var firstBmpOrig = new Bitmap(sortedImageFiles[0]);
            int dynamicSliceWidth = Math.Max(5, firstBmpOrig.Width / 50);
            int blendWidth = dynamicSliceWidth / 2;
            int totalHeight = firstBmpOrig.Width;

            var slitBuffers = new List<byte[]>();
            int sliceHeight = totalHeight;
            int bytesPerPixel = 3;
            byte[] previousSlit = null;

            foreach (string file in sortedImageFiles)
            {
                var orig = new Bitmap(file);
                orig.RotateFlip(RotateFlipType.Rotate270FlipNone);

                var bmp24 = new Bitmap(orig.Width, orig.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(bmp24))
                {
                    g.DrawImage(orig, 0, 0, orig.Width, orig.Height);
                }

                var rect = new System.Drawing.Rectangle(0, 0, bmp24.Width, bmp24.Height);
                var data = bmp24.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                try
                {
                    int stride = data.Stride;
                    IntPtr scan0 = data.Scan0;
                    int middleCol = bmp24.Width / 2;
                    int startX = middleCol - (dynamicSliceWidth / 2);
                    if (startX < 0) startX = 0;
                    if (startX + dynamicSliceWidth > bmp24.Width)
                        throw new InvalidOperationException("dynamicSliceWidth too large for this image.");

                    byte[] thisSlit = new byte[dynamicSliceWidth * sliceHeight * bytesPerPixel];

                    for (int y = 0; y < sliceHeight; y++)
                    {
                        IntPtr rowPtr = scan0 + y * stride;
                        int srcOffset = startX * bytesPerPixel;
                        int destOffset = y * (dynamicSliceWidth * bytesPerPixel);
                        Marshal.Copy(rowPtr + srcOffset, thisSlit, destOffset, dynamicSliceWidth * bytesPerPixel);
                    }

                    if (previousSlit != null)
                    {
                        double mse = ComputeMSE(previousSlit, thisSlit, dynamicSliceWidth, sliceHeight);
                        if (mse < 50.0)
                            continue;
                    }

                    previousSlit = new byte[thisSlit.Length];
                    Array.Copy(thisSlit, previousSlit, thisSlit.Length);
                    slitBuffers.Add(thisSlit);
                }
                catch (Exception ex)
                {
                    bmp24.UnlockBits(data);
                }
                finally
                {
                    //firstBmpOrig.Dispose();
                    bmp24.UnlockBits(data);
                }
            }

            if (slitBuffers.Count == 0)
                throw new InvalidOperationException("No distinct slits found.");

            int totalWidth = slitBuffers.Count * (dynamicSliceWidth - blendWidth) + blendWidth;
            var panorama = new Bitmap(totalWidth, totalHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var panoRect = new System.Drawing.Rectangle(0, 0, totalWidth, totalHeight);
            var panoData = panorama.LockBits(panoRect, ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            try
            {
                int panoStride = panoData.Stride;
                IntPtr panoScan0 = panoData.Scan0;
                int currentX = 0;

                unsafe
                {
                    byte* panoPtr = (byte*)panoScan0.ToPointer();

                    for (int sliceIndex = 0; sliceIndex < slitBuffers.Count; sliceIndex++)
                    {
                        byte[] slit = slitBuffers[sliceIndex];

                        if (sliceIndex > 0)
                        {
                            for (int y = 0; y < sliceHeight; y++)
                            {
                                byte* panoRow = panoPtr + y * panoStride + (currentX - blendWidth) * bytesPerPixel;
                                int slitRowStart = y * (dynamicSliceWidth * bytesPerPixel);

                                for (int x = 0; x < blendWidth; x++)
                                {
                                    double t = (double)x / blendWidth;
                                    double blendFactor = (t * t * (3 - 2 * t)) * 255.0;
                                    blendFactor = blendFactor < 0 ? 0 : (blendFactor > 255 ? 255 : blendFactor);


                                    int byteOffsetSlit = slitRowStart + x * bytesPerPixel;

                                    for (int channel = 0; channel < 3; channel++)
                                    {
                                        byte prevColor = panoRow[x * 3 + channel];
                                        byte newColor = slit[byteOffsetSlit + channel];
                                        int blended = (int)((prevColor * (255 - blendFactor) + newColor * blendFactor) / 255.0);
                                        panoRow[x * 3 + channel] = (byte)blended;
                                    }
                                }
                            }
                        }

                        int copyCols = dynamicSliceWidth - blendWidth;
                        for (int y = 0; y < sliceHeight; y++)
                        {
                            byte* panoRow = panoPtr + y * panoStride + currentX * bytesPerPixel;
                            int slitRowStart = y * (dynamicSliceWidth * bytesPerPixel) + blendWidth * bytesPerPixel;

                            for (int b = 0; b < copyCols * bytesPerPixel; b++)
                            {
                                panoRow[b] = slitRowStart + b < slit.Length ? slit[slitRowStart + b] : (byte)0;
                            }
                        }

                        currentX += copyCols;
                    }
                }
            }
            finally
            {
                panorama.UnlockBits(panoData);
                firstBmpOrig.Dispose();
            }

            // panorama.SetResolution()
            string outputPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PanoramicImage_Fast.jpg");
            panorama.Save(outputPath, ImageFormat.Jpeg);
            LoadImageWithoutLocking(outputPath, Sticked_image);

            DeleteAllFiles(@"D:\uvss\underside image");

        }

        private double ComputeMSE(byte[] a, byte[] b, int sliceWidth, int sliceHeight)
        {
            long sumSq = 0;
            int totalPixels = sliceWidth * sliceHeight;
            int bytesPerRow = sliceWidth * 3;

            // We’ll compare in grayscale by averaging B,G,R, but only sample every 4th row/col for speed:
            int rowStep = Math.Max(1, sliceHeight / 50);
            int colStep = Math.Max(1, sliceWidth / 50);

            for (int y = 0; y < sliceHeight; y += rowStep)
            {
                int rowOffset = y * bytesPerRow;
                for (int x = 0; x < sliceWidth; x += colStep)
                {
                    int idx = rowOffset + x * 3;
                    if (idx + 2 >= a.Length || idx + 2 >= b.Length)
                        continue;

                    int grayA = (a[idx] + a[idx + 1] + a[idx + 2]) / 3;
                    int grayB = (b[idx] + b[idx + 1] + b[idx + 2]) / 3;
                    int diff = grayA - grayB;
                    sumSq += diff * diff;
                }
            }

            // Normalize by number of sampled points:
            double sampleCount = (double)((sliceHeight + rowStep - 1) / rowStep) * ((sliceWidth + colStep - 1) / colStep);
            return (double)sumSq / Math.Max(1, sampleCount);
        }

        #region Using to delete all images from folder after stitching the image

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

        private const uint DELETE = 0x10000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FILE_SHARE_DELETE = 0x00000004;
        private const uint OPEN_EXISTING = 3;
        public static void DeleteAllFiles(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Directory does not exist: " + folderPath);
                return;
            }

            string[] files = Directory.GetFiles(folderPath);

            foreach (var file in files)
            {
                try
                {
                    // Try to force-delete even if file is in use
                    IntPtr handle = CreateFile(
                        file,
                        DELETE,
                        FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
                        IntPtr.Zero,
                        OPEN_EXISTING,
                        0,
                        IntPtr.Zero);

                    if (handle != new IntPtr(-1))
                    {
                        File.Delete(file);
                        Console.WriteLine($"Deleted: {file}");
                    }
                    else
                    {
                        Console.WriteLine($"File in use or locked: {file}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete {file}: {ex.Message}");
                }
            }
        }

        #endregion

        public static string CreateTimestampedFolder(string basePath)
        {
            // Ensure base path exists
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            // Create timestamped folder name
            string folderName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fullFolderPath = System.IO.Path.Combine(basePath, folderName);

            // Create the folder
            Directory.CreateDirectory(fullFolderPath);

            // Return the full path of the created folder
            return fullFolderPath;
        }
        public string pinhole_came_path;

        #region // using for video pinhole camera 

        private async Task StartRecordingAsync()
        {
            //TxtStatus.Text = "Starting all cameras…";

            pinhole_came_path = CreateTimestampedFolder(basefolder_to_save_pinhole_videos);

            try
            {
                // if (!Directory.Exists(SAVE_FOLDER))
                if (!Directory.Exists(pinhole_came_path))
                {
                    Directory.CreateDirectory(pinhole_came_path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot create or access folder:\n{pinhole_came_path}\n\n{ex.Message}",
                                "Folder Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _outputPaths.Clear();
            for (int i = 0; i < CameraIPs.Length; i++)
            {
                string filename = $"camera{i + 1}.mp4";
                string fullPath = System.IO.Path.Combine(pinhole_came_path, filename);
                _outputPaths.Add(fullPath);
            }

            foreach (var mp in _mediaPlayers) mp.Dispose();
            foreach (var m in _medias) m.Dispose();
            _mediaPlayers.Clear();
            _medias.Clear();

            for (int i = 0; i < RtspUrls.Count; i++)
            {
                string rtsp = RtspUrls[i];
                string outPath = _outputPaths[i];

                string soutOpt = $":sout=#duplicate{{dst=std{{access=file,mux=mp4,dst={outPath}}}}}";
                string keepOpt = ":sout-keep";
                string cacheOpt = ":network-caching=300";

                var media = new LibVLCSharp.Shared.Media(_libVLC, rtsp, FromType.FromLocation);
                media.AddOption(soutOpt);
                media.AddOption(keepOpt);
                media.AddOption(cacheOpt);

                var player = new LibVLCSharp.Shared.MediaPlayer(_libVLC);

                if (i == 0)
                {
                    // PreviewView.MediaPlayer = player;
                }

                _medias.Add(media);
                _mediaPlayers.Add(player);
            }

            try
            {
                for (int i = 0; i < _mediaPlayers.Count; i++)
                {
                    bool ok = _mediaPlayers[i].Play(_medias[i]);
                    if (!ok)
                    {
                        throw new Exception($"Failed to start RTSP for camera #{i + 1}.");
                    }
                }

                _isRecordingAll = true;
                //BtnStartAll.IsEnabled = false;
                //BtnStopAll.IsEnabled = true;
                //TxtStatus.Text = "Connecting to cameras…";

                await Task.Delay(1000);

                if (_mediaPlayers[0].State != VLCState.Playing)
                {
                    ShowErrorAndCleanupAll("Unable to connect to camera #1. Check IP/credentials.");
                    return;
                }

                // TxtStatus.Text = "Recording all cameras…";
            }
            catch (Exception ex)
            {
                ShowErrorAndCleanupAll($"Error starting recordings:\n{ex.Message}");
            }
        }

        private void StopAllRecordingsSafe()
        {
            if (!_isRecordingAll) return;
            _isRecordingAll = false;

            for (int i = 0; i < _mediaPlayers.Count; i++)
            {
                var player = _mediaPlayers[i];
                var media = _medias[i];

                try
                {
                    if (player.IsPlaying)
                        player.Stop();
                }
                catch { }

                media.Dispose();
                player.Dispose();
            }

            _mediaPlayers.Clear();
            _medias.Clear();

            Dispatcher.Invoke(() =>
            {
                //TxtStatus.Text = $"Stopped. Files saved in:\n{SAVE_FOLDER}";
                //BtnStartAll.IsEnabled = true;
                //BtnStopAll.IsEnabled = false;
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void ShowErrorAndCleanupAll(string message)
        {
            foreach (var path in _outputPaths)
            {
                try
                {
                    if (!string.IsNullOrEmpty(path) && File.Exists(path))
                        File.Delete(path);
                }
                catch { }
            }

            foreach (var player in _mediaPlayers)
            {
                try
                {
                    if (player.IsPlaying)
                        player.Stop();
                }
                catch { }
                player.Dispose();
            }

            foreach (var media in _medias)
            {
                media.Dispose();
            }

            _mediaPlayers.Clear();
            _medias.Clear();
            _isRecordingAll = false;

            //BtnStartAll.IsEnabled = true;
            //BtnStopAll.IsEnabled = false;
            //TxtStatus.Text = "Ready";

            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void Window_Closed_1(object sender, EventArgs e)
        {

        }

        private void Window_Closed_2(object sender, EventArgs e)
        {

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            StopAllRecordingsSafe();
            _libVLC?.Dispose();
            base.OnClosing(e);
        }

        #endregion

        public void SaveRecordTovideo_management_Database(string vehicleNumber, string video1, string video2, string video3)
        {
            string connectionString = MainWindow.connectionString;

            // Convert WPF Image to byte[]
            byte[] imageData = null;
            if (Sticked_image.Source != null)
            {
                var bitmapSource = Sticked_image.Source as BitmapSource;
                if (bitmapSource != null)
                {
                    var encoder = new JpegBitmapEncoder(); // Or PngBitmapEncoder
                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    using (var ms = new MemoryStream())
                    {
                        encoder.Save(ms);
                        imageData = ms.ToArray();
                    }
                }
            }

            using (var conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO video_management_table
            (vehicle_number, capture_date, capture_time,
             video1_path, video2_path, video3_path, vehicle_image)
            VALUES (@vehicle_number, @date, @time,
                    @video1, @video2, @video3, @image)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@vehicle_number", vehicleNumber);
                    cmd.Parameters.AddWithValue("@date", DateTime.Now.Date);
                    cmd.Parameters.AddWithValue("@time", DateTime.Now.TimeOfDay);
                    cmd.Parameters.AddWithValue("@video1", video1);
                    cmd.Parameters.AddWithValue("@video2", video2);
                    cmd.Parameters.AddWithValue("@video3", video3);
                    cmd.Parameters.AddWithValue("@image", (object)imageData ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        // This new helper method will be our single, reliable way to create the "master copy"
        private void UpdateOriginalPixels()
        {
            // Safety check: If no image is displayed, do nothing.
            if (Sticked_image.Source == null)
            {
                originalPixels = null;
                return;
            }

            // This is YOUR existing logic for getting the pixel data, now in a reusable method.
            var bitmapSource = Sticked_image.Source as BitmapSource;
            width = bitmapSource.PixelWidth;
            height = bitmapSource.PixelHeight;
            stride = width * (bitmapSource.Format.BitsPerPixel / 8);
            originalPixels = new byte[height * stride];
            bitmapSource.CopyPixels(originalPixels, stride, 0);
        }

        bool btnload = false;
        // NEW METHOD FOR THE "Load Image" BUTTON
        #region Load Image Feature
        private void btnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            // openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
            // if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string Anprimage = @"C:\\Users\\Security\\Downloads\\uvss_images\\uvss_images\\ANPR\\MH41V 7911_28.png";
                    string MainImage = "D:\\PanoramicImage.jpg";
                    string NumberPlateImage = "D:\\Image2.png";
                    string DriverImage = "D:\\WhatsApp Image 2025-08-29 at 6.43.57 PM.jpeg";
                    // string selectedImagePath = openFileDialog.FileName;
                    if (!btnload)
                    {
                        btnload = !btnload;

                        // This line loads the new image and puts it in the display frame.
                        //Sticked_image.Source = LoadImageUnlocked(MainImage);
                        //Driver_image.Source = LoadImageUnlocked(DriverImage);
                        //Anpr_image.Source = LoadImageUnlocked(Anprimage);
                        //numberplate_image.Source = LoadImageUnlocked(NumberPlateImage);
                        // THE FIX: This single line updates the master copy with the new image's data.
                        UpdateOriginalPixels();
                        ComputerVisionResult result = _anprEngine.ProcessImage(Anprimage);
                        NumberPlateImage = result.PlateCropPath;
                        Numberplate_number_box.Text = result.RegistrationNumber;
                        Anpr_image.Source = LoadImageUnlocked(Anprimage);
                        numberplate_image.Source = LoadImageUnlocked(NumberPlateImage);

                        // THE FIX: This single line updates the master copy with the new image's data.
                        UpdateOriginalPixels();

                        // Optional but recommended: Reset sliders for the new image.
                        BrightnessSlider.Value = 1.0;
                        ContrastSlider.Value = 1.0;
                        SharpnessSlider.Value = 0;
                    }
                    else
                    {
                        btnload = !btnload;

                        Anprimage = "D:\\muvss_name.png";
                        MainImage = "D:\\muvss_name.png";
                        NumberPlateImage = "D:\\muvss_name.png";
                        DriverImage = "D:\\muvss_name.png";

                        // This line loads the new image and puts it in the display frame.
                        Sticked_image.Source = LoadImageUnlocked(MainImage);
                        Driver_image.Source = LoadImageUnlocked(DriverImage);
                        Anpr_image.Source = LoadImageUnlocked(Anprimage);
                        numberplate_image.Source = LoadImageUnlocked(NumberPlateImage);
                    }



                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading image: " + ex.Message);
                }
            }
        }
        #endregion





        private const int sliceWidth = 20;
        private const int minMatchesThreshold = 30;


        private bool _isZoomed;

        private Grid _origParent;
        private int _origRow, _origCol, _origRowSpan, _origColSpan, _origChildIndex;
        private double _origWidth, _origHeight;

        // public bool aic_on = false;
        private void AIC_btn_Click(object sender, RoutedEventArgs e)
        {
            //    if (aic_on == false)
            //    {
            //        SaveImagesForAIC();
            //        aic_on = true;
            //    }
            //    else
            //    {
            //        string imagePath = @"D:\uvss\tmp aic\source_image.jpg";
            //        BitmapImage bitmap = new BitmapImage();
            //        bitmap.BeginInit();
            //        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute); 
            //        bitmap.CacheOption = BitmapCacheOption.OnLoad;
            //        bitmap.EndInit();
            //        Sticked_image.Source = bitmap;
            //        aic_on = false;
            //    }

            SaveImagesForAIC();
        }

        //private void SaveImagesForAIC()
        //{
        //    string numberplate = Numberplate_number_box.Text.Trim();

        //    if (string.IsNullOrEmpty(numberplate))
        //    {
        //        MessageBox.Show("Please enter a number plate.");
        //        return;
        //    }

        //    try
        //    {
        //        string folderPath = @"D:\uvss\tmp aic";
        //        Directory.CreateDirectory(folderPath); // Ensure folder exists

        //        // Save the WPF image (Sticked_image) as source image
        //        SaveImageControlToFile(Sticked_image, System.IO.Path.Combine(folderPath, "source_image.jpg"));

        //        using (SqlConnection conn = new SqlConnection(mainWindow.connectionString))
        //        {
        //            conn.Open();

        //            string query = @"SELECT TOP 1 underside_image, entry_date, entry_time 
        //                     FROM vehicle_entry_log 
        //                     WHERE numberplate = @numberplate 
        //                     ORDER BY sr_no DESC";

        //            using (SqlCommand cmd = new SqlCommand(query, conn))
        //            {
        //                cmd.Parameters.AddWithValue("@numberplate", numberplate);

        //                using (SqlDataReader reader = cmd.ExecuteReader())
        //                {
        //                    if (reader.Read())
        //                    {
        //                        byte[] imageData = (byte[])reader["underside_image"];

        //                        string dbImagePath = System.IO.Path.Combine(folderPath, "db_image.jpg");
        //                        File.WriteAllBytes(dbImagePath, imageData); // Save DB image

        //                        DateTime date = reader.GetDateTime(reader.GetOrdinal("entry_date"));
        //                        TimeSpan time = (TimeSpan)reader["entry_time"];

        //                        MessageBox.Show($"Latest image timestamp:\nDate: {date:yyyy-MM-dd}\nTime: {time}");

        //                        TryCompare( @"D:\uvss\tmp aic\db_image.jpg", @"D:\uvss\tmp aic\source_image.jpg");
        //                    }
        //                    else
        //                    {
        //                        MessageBox.Show("no past image for this vehicle");
        //                    }
        //                }
        //            }
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Database error: " + ex.Message);
        //    }
        //}

        private void SaveImagesForAIC()  // This method saves the current image and retrieves the past image from the database,
                                         // then opens a new window for comparison.
        {
            string numberplate = Numberplate_number_box.Text.Trim();
            if (string.IsNullOrEmpty(numberplate))
            {
                MessageBox.Show("Please enter a number plate.");
                return;
            }
            try
            {
                string folderPath = @"D:\uvss\tmp aic";
                Directory.CreateDirectory(folderPath);
                string sourceImagePath = System.IO.Path.Combine(folderPath, "source_image.jpg");
                string dbImagePath = System.IO.Path.Combine(folderPath, "db_image.jpg");
                SaveImageControlToFile(Sticked_image, sourceImagePath);
                using (SqlConnection conn = new SqlConnection(MainWindow.connectionString))
                {
                    conn.Open();

                    // CHANGE 1: Update the SQL query to also select entry_date and entry_time
                    string query = @"SELECT TOP 1 underside_image, entry_date, entry_time 
                             FROM vehicle_entry_log 
                             WHERE numberplate = @numberplate ORDER BY sr_no DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@numberplate", numberplate);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                byte[] imageData = (byte[])reader["underside_image"];
                                File.WriteAllBytes(dbImagePath, imageData);

                                // CHANGE 2: Read the new date and time columns from the database
                                DateTime date = (DateTime)reader["entry_date"];
                                TimeSpan time = (TimeSpan)reader["entry_time"];

                                // Format the date and time into a nice string
                                string timestampInfo = $"Date: {date:yyyy-MM-dd}   Time: {time:hh\\:mm\\:ss}";

                                // --- FINAL INTEGRATION ---
                              var app = (App)System.Windows.Application.Current;
AicViewerWindow viewer = app.ServiceProvider.GetRequiredService<AicViewerWindow>();

                                // CHANGE 3: Pass the new timestamp string to the viewer window
                                viewer.LoadAndCompareImages(sourceImagePath, dbImagePath, timestampInfo);

                                viewer.ShowDialog();
                            }
                            else
                            {
                                MessageBox.Show("no past image for this vehicle");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }




        private void SaveImageControlToFile(System.Windows.Controls.Image imageControl, string filePath)
        {
            if (imageControl.Source is BitmapSource bitmapSource)
            {
                BitmapEncoder encoder = new JpegBitmapEncoder(); // You can use PngBitmapEncoder or others
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        //private void TryCompare(string firstImagePth, string secondImagePth)
        //{
        //    if (File.Exists(firstImagePth) && File.Exists(secondImagePth))
        //    {
        //        var img1 = new Image<Bgr, byte>(firstImagePth);
        //        var img2 = new Image<Bgr, byte>(secondImagePth);

        //        if (img1.Size != img2.Size)
        //            img2 = img2.Resize(img1.Width, img1.Height, Inter.Linear);

        //        var gray1 = img1.Convert<Emgu.CV.Structure.Gray, byte>();
        //        var gray2 = img2.Convert<Emgu.CV.Structure.Gray, byte>();

        //        var diff = gray1.AbsDiff(gray2);

        //        var thresh = diff.ThresholdBinary(new Emgu.CV.Structure.Gray(80), new Emgu.CV.Structure.Gray(255));

        //        CvInvoke.MorphologyEx(thresh, thresh, MorphOp.Open, CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(7, 7), new System.Drawing.Point(-1, -1)), new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

        //        var contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
        //        CvInvoke.FindContours(thresh, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

        //        for (int i = 0; i < contours.Size; i++)
        //        {
        //            var rect = CvInvoke.BoundingRectangle(contours[i]);

        //            if (rect.Width > 60 && rect.Height > 60)
        //            {
        //                CvInvoke.Rectangle(img2, rect, new MCvScalar(0, 0, 255), 2);
        //            }
        //        }

        //        Sticked_image.Source = TobitmapSource(img2);
        //    }
        //}

        //private BitmapSource TobitmapSource(Image<Bgr, byte> image)
        //{
        //    using (var bitmap = image.ToBitmap())
        //    {
        //        var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

        //        //  var bitmapSource = BitmapSource.Create(bitmapData.Width, bitmapData.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height * bitmapData.Stride);

        //        int width = bitmapData.Width;
        //        int height = bitmapData.Height;
        //        int stride = bitmapData.Stride;
        //        int bytes = stride * height;

        //        // Allocate managed byte array
        //        byte[] pixelData = new byte[bytes];

        //        // Copy unmanaged memory to managed array
        //        System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, pixelData, 0, bytes);

        //        // Create BitmapSource from managed byte array
        //        var bitmapSource = BitmapSource.Create(
        //            width,
        //            height,
        //            96,
        //            96,
        //            System.Windows.Media.PixelFormats.Bgr24,
        //            null,
        //            pixelData,
        //            stride);




        //        bitmap.UnlockBits(bitmapData);
        //        return bitmapSource;

        //    }
        //}

        private Thickness _origMargin;
        private System.Windows.HorizontalAlignment _origHAlign;
        private VerticalAlignment _origVAlign;
        private System.Windows.Media.Stretch _origStretch;

        private void Sticked_image_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // ImageControl.Visibility = Visibility.Collapsed;
            // FallbackText.Visibility = Visibility.Visible;
        }

        private void Sticked_image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // react only to a *double* left‑click
            if (e.ChangedButton != MouseButton.Left || e.ClickCount != 2)
                return;

            if (!_isZoomed)
            {
                //------------------ ZOOM IN ------------------
                // remember where and how the image lived
                _origParent = (Grid)Sticked_image.Parent;
                _origRow = Grid.GetRow(Sticked_image);
                _origCol = Grid.GetColumn(Sticked_image);
                _origRowSpan = Grid.GetRowSpan(Sticked_image);
                _origColSpan = Grid.GetColumnSpan(Sticked_image);
                _origChildIndex = _origParent.Children.IndexOf(Sticked_image);

                _origWidth = Sticked_image.Width;
                _origHeight = Sticked_image.Height;
                _origMargin = Sticked_image.Margin;
                _origHAlign = Sticked_image.HorizontalAlignment;
                _origVAlign = Sticked_image.VerticalAlignment;
                _origStretch = Sticked_image.Stretch;

                // lift the image out of its cell and place it on mother_grid
                _origParent.Children.Remove(Sticked_image);
                mother_grid.Children.Add(Sticked_image);

                // make it cover every cell of mother_grid
                Grid.SetRow(Sticked_image, 0);
                Grid.SetColumn(Sticked_image, 0);
                Grid.SetRowSpan(Sticked_image, Math.Max(1, mother_grid.RowDefinitions.Count));
                Grid.SetColumnSpan(Sticked_image, Math.Max(1, mother_grid.ColumnDefinitions.Count));

                // stretch to fill — keep aspect ratio
                Sticked_image.Width = double.NaN;
                Sticked_image.Height = double.NaN;
                Sticked_image.Margin = new Thickness(0);
                Sticked_image.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                Sticked_image.VerticalAlignment = VerticalAlignment.Stretch;
                Sticked_image.Stretch = System.Windows.Media.Stretch.Fill;   // or Fill if you prefer

                // keep it visually on top of everything
                System.Windows.Controls.Panel.SetZIndex(Sticked_image, 999);

                _isZoomed = true;
            }
            else
            {
                //---------------- RESTORE ----------------
                // remove from mother_grid and put back where it came from
                mother_grid.Children.Remove(Sticked_image);
                _origParent.Children.Insert(_origChildIndex, Sticked_image);

                Grid.SetRow(Sticked_image, _origRow);
                Grid.SetColumn(Sticked_image, _origCol);
                Grid.SetRowSpan(Sticked_image, _origRowSpan);
                Grid.SetColumnSpan(Sticked_image, _origColSpan);

                Sticked_image.Width = _origWidth;
                Sticked_image.Height = _origHeight;
                Sticked_image.Margin = _origMargin;
                Sticked_image.HorizontalAlignment = _origHAlign;
                Sticked_image.VerticalAlignment = _origVAlign;
                Sticked_image.Stretch = _origStretch;

                System.Windows.Controls.Panel.SetZIndex(Sticked_image, 0);
                _isZoomed = false;
            }
        }

        public void CreatePanoramicImage_Pro(string[] imageFiles)
        {
            if (imageFiles == null || imageFiles.Length == 0)
                throw new ArgumentException("No images provided.");

            var sortedImageFiles = imageFiles
                .OrderBy(f =>
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(f);
                    string numberPart = new string(name.Where(char.IsDigit).ToArray());
                    return int.TryParse(numberPart, out int num) ? num : int.MaxValue;
                })
                .ToArray();

            List<OpenCvSharp.Mat> sliceList = new List<OpenCvSharp.Mat>();
            OpenCvSharp.Mat previousSlice = null;

            foreach (string file in sortedImageFiles)
            {
                Bitmap bmp = new Bitmap(file);
                bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);
                OpenCvSharp.Mat mat = BitmapConverter.ToMat(bmp);

                // Enhance contrast
                Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2YCrCb);
                var channels = Cv2.Split(mat);
                Cv2.EqualizeHist(channels[0], channels[0]);
                Cv2.Merge(channels, mat);
                Cv2.CvtColor(mat, mat, ColorConversionCodes.YCrCb2BGR);

                // Extract vertical center slice
                int centerX = mat.Cols / 2 - sliceWidth / 2;
                var slice = new OpenCvSharp.Mat(mat, new OpenCvSharp.Rect(centerX, 0, sliceWidth, mat.Rows)).Clone();

                if (previousSlice != null)
                {
                    if (!IsDistinctSlice(previousSlice, slice))
                        continue;
                }

                previousSlice = slice;
                sliceList.Add(slice);
            }

            if (sliceList.Count == 0)
                throw new InvalidOperationException("No valid slices found.");

            // Stitch slices horizontally
            OpenCvSharp.Mat panorama = new OpenCvSharp.Mat(sliceList[0].Rows, sliceList.Count * sliceWidth, MatType.CV_8UC3);
            for (int i = 0; i < sliceList.Count; i++)
            {
                OpenCvSharp.Rect roi = new OpenCvSharp.Rect(i * sliceWidth, 0, sliceWidth, sliceList[i].Rows);
                sliceList[i].CopyTo(new OpenCvSharp.Mat(panorama, roi));
            }

            string outputPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PanoramicImage_Pro.jpg");
            Cv2.ImWrite(outputPath, panorama);

            LoadImageWithoutLocking(outputPath, Sticked_image);

            DeleteAllFiles(@"D:\uvss\underside image");
        }

        private bool IsDistinctSlice(OpenCvSharp.Mat sliceA, OpenCvSharp.Mat sliceB)
        {
            var grayA = new OpenCvSharp.Mat();
            var grayB = new OpenCvSharp.Mat();
            Cv2.CvtColor(sliceA, grayA, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(sliceB, grayB, ColorConversionCodes.BGR2GRAY);

            var orb = ORB.Create();
            KeyPoint[] kpA, kpB;
            OpenCvSharp.Mat desA = new OpenCvSharp.Mat(), desB = new OpenCvSharp.Mat();

            orb.DetectAndCompute(grayA, null, out kpA, desA);
            orb.DetectAndCompute(grayB, null, out kpB, desB);

            if (desA.Empty() || desB.Empty())
                return true;

            var bf = new BFMatcher(NormTypes.Hamming, crossCheck: true);
            var matches = bf.Match(desA, desB);

            return matches.Length < minMatchesThreshold;
        }

        #region using this for recognise and drop numberplate 

        private Thread recognitionThread;

        //private void StartNumberPlateRecognitionThread()
        //{
        //    if (recognitionThread != null && recognitionThread.IsAlive)
        //    {
        //        MessageBox.Show("Recognition is already running.");
        //        return;
        //    }

        //    recognitionThread = new Thread(() =>
        //    {
        //        Application.Current.Dispatcher.Invoke(async () =>
        //        {
        //            try
        //            {
        //                await CallNumberPlateRecognitionFromImageElement_1();
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show("❌ Error: " + ex.Message);
        //            }
        //            finally
        //            {
        //                recognitionThread = null; // ✅ Release the thread
        //            }
        //        });
        //    });

        //    recognitionThread.IsBackground = true;
        //    recognitionThread.Start();


        //}

        private void StartNumberPlateRecognitionThread()
        {
            if (recognitionThread != null && recognitionThread.IsAlive)
            {
                MessageBox.Show("Recognition is already running.");
                return;
            }

            isRecognitionCompleted = false; // ⬅️ Reset before starting new recognition

            recognitionThread = new Thread(() =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                {
                    try
                    {
                        await CallNumberPlateRecognitionFromImageElement_1();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("❌ Error: " + ex.Message);
                    }
                    finally
                    {
                        isRecognitionCompleted = true;  // ✅ Set flag once recognition is complete
                        recognitionThread = null;
                    }
                });
            });

            recognitionThread.IsBackground = true;
            recognitionThread.Start();
        }

        private async Task CallNumberPlateRecognitionFromImageElement_1()
        {
            string baseFolder = @"D:\uvss\anpr image";
            string tempInputPath = System.IO.Path.Combine(baseFolder, "car-number-plate-500x500.jpg");
            string fullImagePath = System.IO.Path.Combine(baseFolder, "detected_output.jpg");
            string croppedFolder = System.IO.Path.Combine(baseFolder, "cropped_plates");
            string resultTextPath = System.IO.Path.Combine(baseFolder, "result.txt");

            string pythonExe = @"C:\Users\ADMIN\miniconda3\python.exe";
            //string scriptPath = @"C:\Users\ADMIN\Desktop\anpr.py";
            string scriptPath = @"C:\Users\ADMIN\Desktop\anpr_standalone.py";

            try
            {
                // ✅ Save image from Image element to disk
                if (Anpr_image.Source is BitmapSource bitmapSource)
                {
                    using (var fileStream = new FileStream(tempInputPath, FileMode.Create))
                    {
                        var encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                        encoder.Save(fileStream);
                    }
                }
                else
                {
                    MessageBox.Show("⚠️ No image in Anpr_image control.");
                    return;
                }

                // Cleanup previous output
                if (File.Exists(fullImagePath)) File.Delete(fullImagePath);
                if (File.Exists(resultTextPath)) File.Delete(resultTextPath);
                if (Directory.Exists(croppedFolder)) Directory.Delete(croppedFolder, true);

                Numberplate_number_box.Text = "";
                numberplate_image.Source = null;

                await Task.Run(() =>
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = pythonExe,
                        Arguments = $"\"{scriptPath}\" \"{tempInputPath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (Process p = Process.Start(psi))
                    {
                        p.WaitForExit();
                    }
                });

                // ✅ Load results into UI
                if (File.Exists(fullImagePath))
                    Anpr_image.Source = LoadImageUnlocked(fullImagePath);

                if (Directory.Exists(croppedFolder))
                {
                    var plates = Directory.GetFiles(croppedFolder, "plate_*.jpg");
                    if (plates.Length > 0)
                        numberplate_image.Source = LoadImageUnlocked(plates[0]);
                }

                if (File.Exists(resultTextPath))
                {
                    string[] lines = File.ReadAllLines(resultTextPath);
                    string num = string.Join("\n", lines);
                    if (num[0] == 'E' || num[0] == 'F')
                    {
                        num = num.Remove(0, 1);
                    }
                    Numberplate_number_box.Text = num;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Error: " + ex.Message);
            }
            finally
            {
                // ✅ Delete all temporary files and folders
                try
                {
                    if (File.Exists(tempInputPath)) File.Delete(tempInputPath);
                    if (File.Exists(fullImagePath)) File.Delete(fullImagePath);
                    if (File.Exists(resultTextPath)) File.Delete(resultTextPath);
                    if (Directory.Exists(croppedFolder)) Directory.Delete(croppedFolder, true);

                }
                catch { /* ignore cleanup errors */ }
            }
        }

        // ✅ Unlocks and loads image safely (avoids file lock)
        public BitmapImage LoadImageUnlocked(string path)
        {
            var bitmap = new BitmapImage();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
            }
            bitmap.Freeze(); // Cross-thread safety
            return bitmap;
        }

        #endregion
        private async void CaptureAnpr() { }
        private async void CaptureDriver() { }
    }
}
