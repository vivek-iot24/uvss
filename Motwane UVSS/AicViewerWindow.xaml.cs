using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using LibVLCSharp.Shared;
using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WMPLib;
using VLCMediaPlayer = LibVLCSharp.Shared.MediaPlayer;


namespace Motwane_UVSS
{
    public partial class AicViewerWindow : Window
    {
        private LibVLC _libVLC;
        private LibVLCSharp.Shared.MediaPlayer _player1;
        private MMDeviceEnumerator deviceEnumerator;
        private MMDevice audioDevice;
        WindowsMediaPlayer player = new WindowsMediaPlayer();
        public AicViewerWindow()
        {
            InitializeComponent();
            _libVLC = new LibVLC();
            _player1 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            deviceEnumerator = new MMDeviceEnumerator();
            audioDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (System_settings_page.AudioOn == true)
            {
                audioDevice.AudioEndpointVolume.Mute = false;
            }
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
            timer.Enabled = true;
        }

        bool audioPlaying = false;
        Stopwatch sw = new Stopwatch();
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (sw.ElapsedMilliseconds > 3000)
            {
                player.controls.pause();
                sw.Stop();
            }
        }

        // Public method to receive image PATHS from the main window
        public void LoadAndCompareImages(string sourceImagePath, string dbImagePath, string timestampInfo)
        {
            imageRef.Source = LoadImageUnlocked(dbImagePath);
            labelRefInfo.Content = "Reference Image: " + timestampInfo;
            imageAicResult.Source = PerformComparison(dbImagePath, sourceImagePath);
        }

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        // ... [PerformComparison and TobitmapSource methods ] ...
        private BitmapSource PerformComparison(string firstImagePth, string secondImagePth)
        {
            if (File.Exists(firstImagePth) && File.Exists(secondImagePth))
            {
                var img1 = new Image<Bgr, byte>(firstImagePth);
                var img2 = new Image<Bgr, byte>(secondImagePth);
                if (img1.Size != img2.Size)
                    img2 = img2.Resize(img1.Width, img1.Height, Inter.Linear);
                var gray1 = img1.Convert<Gray, byte>();
                var gray2 = img2.Convert<Gray, byte>();
                var diff = gray1.AbsDiff(gray2);
                var thresh = diff.ThresholdBinary(new Gray(80), new Gray(255));
                CvInvoke.MorphologyEx(thresh, thresh, MorphOp.Open, CvInvoke.GetStructuringElement(ElementShape.Rectangle, new System.Drawing.Size(7, 7), new System.Drawing.Point(-1, -1)), new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                var contours = new VectorOfVectorOfPoint();
                CvInvoke.FindContours(thresh, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                for (int i = 0; i < contours.Size; i++)
                {
                    var rect = CvInvoke.BoundingRectangle(contours[i]);
                    if (rect.Width > 60 && rect.Height > 60)
                    {
                        CvInvoke.Rectangle(img2, rect, new MCvScalar(0, 0, 255), 2);
                    }
                }
                if (contours.Size > 15)
                {
                    if(System_settings_page.AudioOn)
                    {
                        player.URL = @"D:\alarm_tunes\Tune 1.mp3";
                        player.controls.play();
                       sw =  Stopwatch.StartNew();
                        //Thread.Sleep(3000);
                       audioPlaying = true;
                       
                    }
                   
                }

                return TobitmapSource(img2);
            }
            return null;
        }

        private BitmapSource TobitmapSource(Image<Bgr, byte> image)
        {
            using (var bitmap = image.ToBitmap())
            {
                var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int width = bitmapData.Width;
                int height = bitmapData.Height;
                int stride = bitmapData.Stride;
                int bytes = stride * height;
                byte[] pixelData = new byte[bytes];
                System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, pixelData, 0, bytes);
                var bitmapSource = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr24, null, pixelData, stride);
                bitmap.UnlockBits(bitmapData);
                return bitmapSource;
            }
        }

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
            bitmap.Freeze();
            return bitmap;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // =========================================================================
        // NEW: Method for the Exit Button
        // =========================================================================
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            // The logic is the same as the 'X' button.
            this.Close();
        }
    }
}

