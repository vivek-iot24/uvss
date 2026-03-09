using LibVLCSharp.Shared;
using Motwane_UVSS.Application.Interfaces.HAL;
using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WMPLib;

namespace Motwane_UVSS.Presentation.Windows
{
    public partial class AicViewerWindow : Window
    {
        private LibVLC _libVLC;
        private LibVLCSharp.Shared.MediaPlayer _player1;

        private MMDeviceEnumerator deviceEnumerator;
        private MMDevice audioDevice;

        private readonly IAicComparisonService _aicService;

        bool audioPlaying = false;
        Stopwatch sw = new Stopwatch();

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        public AicViewerWindow(IAicComparisonService aicService)
        {
            InitializeComponent();

            _aicService = aicService;

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

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (sw.ElapsedMilliseconds > 3000)
            {
                sw.Stop();
            }
        }
        private BitmapImage ConvertToBitmap(byte[] imageData)
        {
            if (imageData == null) return null;

            using (MemoryStream ms = new MemoryStream(imageData))
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.StreamSource = ms;
                img.EndInit();
                img.Freeze();
                return img;
            }
        }
        public void LoadAndCompareImages(string sourceImagePath, string dbImagePath, string timestampInfo)
        {
            var refImage = _aicService.LoadImageUnlocked(dbImagePath);
            imageRef.Source = ConvertToBitmap(refImage);

            labelRefInfo.Content = "Reference Image: " + timestampInfo;

            var result = _aicService.PerformComparison(dbImagePath, sourceImagePath);
            imageAicResult.Source = ConvertToBitmap(result);
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}