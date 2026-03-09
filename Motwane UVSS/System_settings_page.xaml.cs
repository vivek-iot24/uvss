using LibVLCSharp.Shared;
using NAudio.CoreAudioApi;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WMPLib;
using VLCMediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace Motwane_UVSS
{
    /// <summary>
    /// Interaction logic for System_settings_page.xaml
    /// </summary>
    public partial class System_settings_page : System.Windows.Window
    {

        #region using to read and write audio volume and status

        private MMDeviceEnumerator deviceEnumerator;
        private MMDevice audioDevice;
        private DispatcherTimer updateTimer;
        private bool isUpdatingUI = false;

        #endregion

        private Underside_cam_class1 undersideCamera; // using to show live of underside image

        #region Using to show live of pinhole cameras

        private LibVLC _libVLC;
        private LibVLCSharp.Shared.MediaPlayer _player1;
        private LibVLCSharp.Shared.MediaPlayer _player2;
        private LibVLCSharp.Shared.MediaPlayer _player3;

        private string CameraUser = "admin";
        private string CameraPass = "sefthS$2702";
        public static bool AudioOn = true;
        #endregion

        public System_settings_page()
        {
            InitializeComponent();
            _libVLC = new LibVLC();
            #region Using to show live of underside image

            undersideCamera = new Underside_cam_class1();
            undersideCamera.OnNewFrame += UpdateLiveImage;

            string result = undersideCamera.InitCamera();
            if (result != null)
                MessageBox.Show(result);

            if (!undersideCamera.IsCapturing)
                undersideCamera.StartAcquisition();
            _player1 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            _player2 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            _player3 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            #endregion

        }

        private void UpdateAudioStatus(object sender, EventArgs e)
        {
            isUpdatingUI = true;

            // Sync ComboBox with mute state
            bool isMuted = audioDevice.AudioEndpointVolume.Mute;
            MuteComboBox.SelectedIndex = isMuted ? 1 : 0; // 0: On (Unmuted), 1: Off (Muted)

            // Sync Slider with volume
            float volume = audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
            VolumeSlider.Value = volume;

            isUpdatingUI = false;
        }

        private void MuteComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (isUpdatingUI) return;

                string selected = (MuteComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content as string;
                if (selected == "Off")
                {
                    audioDevice.AudioEndpointVolume.Mute = true; AudioOn = false;
                }
                else if (selected == "On")
                {
                    audioDevice.AudioEndpointVolume.Mute = false;
                    AudioOn = true;
                }
                    
            }
            catch (Exception ex)
            {
                MessageBox.Show("No default audio device found.\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try { 
            if (isUpdatingUI) return;

            float volume = (float)(VolumeSlider.Value / 100.0);
            audioDevice.AudioEndpointVolume.MasterVolumeLevelScalar = volume;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No default audio device found.\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region using to read and write audio volume and status
            try
            {
                deviceEnumerator = new MMDeviceEnumerator();
                audioDevice = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No default audio device found.\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            updateTimer.Tick += UpdateAudioStatus;
            updateTimer.Start();

            UpdateAudioStatus(null, null);

            #endregion

           
        }

        WindowsMediaPlayer player = new WindowsMediaPlayer();

        private void tune_selector_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageBox.Show(tune_selector_combobox.SelectedItem.ToString());
            player.URL = @"D:\alarm_tunes\Tune 1.mp3";
            player.controls.play();
        }

        private void PlayCamera(VLCMediaPlayer player, string ip)
        {
            string rtsp = $"rtsp://{CameraUser}:{CameraPass}@{ip}:554/cam/realmonitor?channel=1&subtype=0";
            var media = new Media(_libVLC, rtsp, FromType.FromLocation);
            player.Play(media);
            
        }
        private void UpdateLiveImage(BitmapSource bitmap)
        {
            Dispatcher.Invoke(() =>
            {
                LiveImage.Source = bitmap;
            });
        }

        private void save_btn_Click(object sender, RoutedEventArgs e)
        {
            Menu_screen menu_Screen = new Menu_screen();
            menu_Screen.Show();

            this.Close();
        }

        private void Exit_button_Click(object sender, RoutedEventArgs e)
        {
            undersideCamera.StopAcquisition();

            
            _player1.Stop();
            _player1.Dispose();
            _player2.Stop();
            _player2.Dispose();
            _libVLC.Dispose();

            Menu_screen menu_Screen = new Menu_screen();
            menu_Screen.Show();

            this.Close();
        }

       

        private void anpr_view_tab_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MessageBox.Show("hello");
            StartanprCamera();
        }

        private VideoCapture anpr_capture;
        private bool anpr_isStreaming = false;
        private Thread anpr_cameraThread;

        private string anpr_rtspUrl = "rtsp://admin:sefthS$2702@192.168.4.56:554/video/live?channel=1&subtype=0";
        private void StartanprCamera()
        {
            try
            {
                Cv2.SetNumThreads(0); // Optional: to avoid threading issues
                anpr_capture = new VideoCapture(anpr_rtspUrl, VideoCaptureAPIs.FFMPEG);

                if (!anpr_capture.IsOpened())
                {
                    Dispatcher.Invoke(() => MessageBox.Show("Failed to open RTSP stream."));
                    anpr_isStreaming = false;
                    return;
                }

                Mat frame = new Mat();

                while (anpr_isStreaming)
                {
                    bool readSuccess = anpr_capture.Read(frame);

                    if (readSuccess && !frame.Empty())
                    {
                        var bitmap = BitmapSourceConverter.ToBitmapSource(frame);
                        bitmap.Freeze();

                        Dispatcher.Invoke(() =>
                        {
                            Anpr_LiveImage.Source = bitmap;
                        });
                    }
                    else
                    {
                        Thread.Sleep(100); // Wait and try again
                    }
                }

                anpr_capture.Release();
            }
            catch (System.Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Error: " + ex.Message);
                });
            }
        }
        

        private void anpr_go_live_button_Click(object sender, RoutedEventArgs e)
        {
            if (anpr_isStreaming) return;

            anpr_isStreaming = true;
            anpr_cameraThread = new Thread(StartanprCamera);
            anpr_cameraThread.IsBackground = true;
            anpr_cameraThread.Start();
        }

        private VideoCapture driver_capture;
        private bool driver_isStreaming = false;
        private Thread driver_cameraThread;

        private string driver_rtspUrl = "rtsp://admin:sefthS$2702@192.168.4.57:554/video/live?channel=1&subtype=0";

        private void StartdriverCamera()
        {
            try
            {
                Cv2.SetNumThreads(0); // Optional: to avoid threading issues
                driver_capture = new VideoCapture(driver_rtspUrl, VideoCaptureAPIs.FFMPEG);

                if (!driver_capture.IsOpened())
                {
                    Dispatcher.Invoke(() => MessageBox.Show("Failed to open RTSP stream."));
                    driver_isStreaming = false;
                    return;
                }

                Mat frame = new Mat();

                while (driver_isStreaming)
                {
                    bool readSuccess = driver_capture.Read(frame);

                    if (readSuccess && !frame.Empty())
                    {
                        var bitmap = BitmapSourceConverter.ToBitmapSource(frame);
                        bitmap.Freeze();

                        Dispatcher.Invoke(() =>
                        {
                            Driver_LiveImage.Source = bitmap;
                        });
                    }
                    else
                    {
                        Thread.Sleep(100); // Wait and try again
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


        private void driver_go_live_button_Click(object sender, RoutedEventArgs e)
        {
            if (driver_isStreaming) return;

            driver_isStreaming = true;
            driver_cameraThread = new Thread(StartdriverCamera);
            driver_cameraThread.IsBackground = true;
            driver_cameraThread.Start();
        }

        private void to_show_pinhole_cameraa_live_Click(object sender, RoutedEventArgs e)
        {
            #region using to show live of pinhole cameras 

            Core.Initialize();

            _libVLC = new LibVLC();

            _player1 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            _player2 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
            _player3 = new LibVLCSharp.Shared.MediaPlayer(_libVLC);

            VideoView1.MediaPlayer = _player1;
            VideoView2.MediaPlayer = _player2;
            VideoView3.MediaPlayer = _player3;

            // 🔒 Wait until VideoView is loaded (Handle ready)
            //VideoView1.Loaded += (s, e) => PlayCamera(_player1, "192.168.4.58");
            //VideoView2.Loaded += (s, e) => PlayCamera(_player2, "192.168.4.59");
            // VideoView3.Loaded += (s, e) => PlayCamera(_player3, "192.168.1.103");

            PlayCamera(_player1, "192.168.4.58");
            PlayCamera(_player2, "192.168.4.59");
            PlayCamera(_player3, "192.168.4.60");
            #endregion

        }
    }
}
