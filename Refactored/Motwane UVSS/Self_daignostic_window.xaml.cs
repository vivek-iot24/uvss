using System.Diagnostics;
using LibVLCSharp.Shared;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Motwane.UVSS
{
    /// <summary>
    /// Interaction logic for Self_daignostic_window.xaml
    /// </summary>
    public partial class Self_daignostic_window : System.Windows.Window
    {
        private static string[] ipAddresses = new string[]
        {
            "192.168.4.56",
            "192.168.4.57",
            "192.168.4.58",
            "192.168.4.59",
            "192.168.4.60",
            "169.254.0.1"
        };
        public Self_daignostic_window()
        {
            InitializeComponent();

            StartAreaScanCamera();
            StartAnprCamera();
            StartDriverCamera();

            diagnosticTimer = new DispatcherTimer();
            diagnosticTimer.Interval = TimeSpan.FromSeconds(1);
            diagnosticTimer.Tick += DiagnosticTimer_Tick;
            diagnosticTimer.Start();

            _ = RunDiagnostics();
        }

        private DispatcherTimer diagnosticTimer;
        private Underside_cam_class1 undersideCamera;

        private VideoCapture anpr_capture;
        private bool anpr_isStreaming;
        private Thread anpr_cameraThread;

        private VideoCapture driver_capture;
        private bool driver_isStreaming;
        private Thread driver_cameraThread;

        private readonly string anpr_rtspUrl =
            "rtsp://admin:sefthS$2702@192.168.4.56:554/video/live?channel=1&subtype=0";

        private readonly string driver_rtspUrl =
            "rtsp://admin:sefthS$2702@192.168.4.57:554/video/live?channel=1&subtype=0";
        private async Task RunDiagnostics()
        {
            LoaderBar.Visibility = Visibility.Visible;
            LoadingText.Visibility = Visibility.Visible;
            await UpdatePingStatus("192.168.4.58", Video_1_scan_StatusTextBlock);
            await UpdatePingStatus("192.168.4.59", Video_2_scan_StatusTextBlock);
            await UpdatePingStatus("192.168.4.60", Video_3_scan_StatusTextBlock);
            await UpdatePingStatus("169.254.0.1", area_scan_StatusTextBlock);
            await UpdatePingStatus("192.168.4.56", ANPR_StatusTextBlock);
            await UpdatePingStatus("192.168.4.57", Driver_StatusTextBlock);
            string serialStatus = await Task.Run(() => CheckSerialPort("COM4")) ? "OK" : "FAIL";

            com_scan_StatusTextBlock.Text = $"COM4 : {serialStatus}";
            com_scan_StatusTextBlock.Foreground =
                serialStatus == "OK" ? Brushes.Green : Brushes.Red;

     

            bool anpr =
                anpr_capture != null &&
                anpr_capture.IsOpened();

            bool driver =
                driver_capture != null &&
                driver_capture.IsOpened();

            LoaderBar.Visibility = Visibility.Collapsed;
            LoadingText.Visibility = Visibility.Collapsed;
        }
        private bool _isRunning;

        private async void DiagnosticTimer_Tick(object sender, EventArgs e)
        {
            if (_isRunning)
                return;

            try
            {
                _isRunning = true;
                await RunDiagnostics();
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task UpdatePingStatus(string ip, TextBlock textBlock)
        {
            bool connected = await PingDevice(ip);

            textBlock.Text = $"{ip} : {(connected ? "OK" : "FAIL")}";
            textBlock.Foreground = connected ? Brushes.Green : Brushes.Red;
        }
        protected override void OnClosed(EventArgs e)
{
    anpr_isStreaming = false;

    driver_isStreaming = false;

    anpr_capture?.Release();

    driver_capture?.Release();

    undersideCamera?.StopAcquisition();

    base.OnClosed(e);
}
        private void UpdateSensorStatus()
        {
            bool sensor1 = true;
            bool sensor2 = false;
            bool pneumatic = true;

            Sensor1Toggle.IsChecked = sensor1;

            Sensor1Status.Text =
                sensor1 ? "ON" : "OFF";

            Sensor1Status.Foreground =
                sensor1
                    ? Brushes.Green
                    : Brushes.Red;



            Sensor2Toggle.IsChecked = sensor2;

            Sensor2Status.Text =
                sensor2 ? "ON" : "OFF";

            Sensor2Status.Foreground =
                sensor2
                    ? Brushes.Green
                    : Brushes.Red;



            PneumaticToggle.IsChecked =
                pneumatic;

            PneumaticStatus.Text =
                pneumatic ? "ON" : "OFF";

            PneumaticStatus.Foreground =
                pneumatic
                    ? Brushes.Green
                    : Brushes.Red;
        }
        private void StartAreaScanCamera()
        {
            try
            {
                undersideCamera = new Underside_cam_class1();

                undersideCamera.OnNewFrame += bitmap =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AreaScanImage.Source = bitmap;

                        AreaScanConnectionText.Text = "CONNECTED";

                        AreaScanConnectionText.Foreground =
                            Brushes.Green;
                    }));
                };

                string result = undersideCamera.InitCamera();

                if (result != null)
                {
                    AreaScanConnectionText.Text =
                        "NOT CONNECTED";

                    AreaScanConnectionText.Foreground =
                        Brushes.Red;

                    MessageBox.Show(result);
                }
                if (!undersideCamera.IsCapturing)
                    undersideCamera.StartAcquisition();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void StartAnprCamera()
        {
            if (anpr_isStreaming)
                return;

            anpr_isStreaming = true;

            anpr_cameraThread =
                new Thread(AnprCameraLoop);

            anpr_cameraThread.IsBackground = true;

            anpr_cameraThread.Start();
        }
        private void AnprCameraLoop()
        {
            try
            {
                Cv2.SetNumThreads(0);

                anpr_capture =
                    new VideoCapture(
                        anpr_rtspUrl,
                        VideoCaptureAPIs.FFMPEG);
                if (!anpr_capture.IsOpened())
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AnprConnectionText.Text =
                            "NOT CONNECTED";

                        AnprConnectionText.Foreground =
                            Brushes.Red;

                        AnprImage.Source = null;
                    }));

                    anpr_isStreaming = false;

                    return;
                }

                using (Mat frame = new Mat())
                {
                    while (anpr_isStreaming)
                    {
                        bool ok =
                            anpr_capture.Read(frame);

                        if (ok && !frame.Empty())
                        {

                            BitmapSource bmp =
                                BitmapSourceConverter
                                    .ToBitmapSource(frame);

                            bmp.Freeze();

                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                AnprConnectionText.Text =
                                    "CONNECTED";

                                AnprConnectionText.Foreground =
                                    Brushes.Green;

                                AnprImage.Source = bmp;
                            }));
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                }

              

                anpr_capture.Release();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        private void StartDriverCamera()
        {
            if (driver_isStreaming)
                return;

            driver_isStreaming = true;

            driver_cameraThread =
                new Thread(DriverCameraLoop);

            driver_cameraThread.IsBackground = true;

            driver_cameraThread.Start();
        }
        private void DriverCameraLoop()
        {
            try
            {
                Cv2.SetNumThreads(0);

                driver_capture =
                    new VideoCapture(
                        driver_rtspUrl,
                        VideoCaptureAPIs.FFMPEG);

                if (!driver_capture.IsOpened())
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DriverConnectionText.Text =
                            "NOT CONNECTED";

                        DriverConnectionText.Foreground =
                            Brushes.Red;

                        DriverImage.Source = null;
                    }));

                    driver_isStreaming = false;

                    return;
                }
                using (Mat frame = new Mat())
                {
                    while (driver_isStreaming)
                    {
                        bool ok =
                            driver_capture.Read(frame);

                        if (ok && !frame.Empty())
                        {
                            BitmapSource bmp =
                                BitmapSourceConverter
                                    .ToBitmapSource(frame);

                            bmp.Freeze();

                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                DriverConnectionText.Text =
                                    "CONNECTED";

                                DriverConnectionText.Foreground =
                                    Brushes.Green;

                                DriverImage.Source = bmp;
                            }));
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }

                    driver_capture.Release();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        
        private async Task<bool> PingDevice(string ip)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ip, 1000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool CheckSerialPort(string portName)
        {
            try
            {
                using (SerialPort serialPort = new SerialPort
                {
                    PortName = portName,
                    BaudRate = 9600,
                    Parity = Parity.None,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    Encoding = Encoding.ASCII,
                    ReadTimeout = 500,
                    WriteTimeout = 500
                })
                {
                    serialPort.Open();
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        private void Exit_button_Click(object sender, RoutedEventArgs e)
        {
            Menu_screen menu_Screen = new Menu_screen();
            menu_Screen.Show();

            this.Close();
        }

        private void Print_btn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = "SelfDaignostic.pdf"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            // 1. Render StackPanel to bitmap
            var rtb = new RenderTargetBitmap(
                (int)self_daigno_information_stack.ActualWidth,
                (int)self_daigno_information_stack.ActualHeight,
                96, 96, PixelFormats.Pbgra32);
            rtb.Render(self_daigno_information_stack);

            // 2. Save as PNG to memory stream
            using (MemoryStream imageStream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rtb));
                encoder.Save(imageStream);
                imageStream.Position = 0; // Rewind

                // 3. Create PDF and add image
                PdfDocument pdf = new PdfDocument();
                PdfPage page = pdf.AddPage();

                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                {
                    XImage img = XImage.FromStream(imageStream);

                    // Adjust page size to image size
                    page.Width = img.PixelWidth * 72 / 96;
                    page.Height = img.PixelHeight * 72 / 96;

                    gfx.DrawImage(img, 0, 0, page.Width, page.Height);
                }

                // 4. Save PDF to selected file
                pdf.Save(saveFileDialog.FileName);
                MessageBox.Show("PDF saved to: " + saveFileDialog.FileName);
            }

        }
    }
}
