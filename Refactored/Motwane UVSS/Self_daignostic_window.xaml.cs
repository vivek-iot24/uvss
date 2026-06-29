using Microsoft.Win32;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Threading;
using LibVLCSharp.Shared;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Motwane.UVSS
{
    /// <summary>
    /// Interaction logic for Self_daignostic_window.xaml
    /// </summary>
    public partial class Self_daignostic_window : System.Windows.Window
    {
        public Self_daignostic_window()
        {
            InitializeComponent();

            StartAreaScanCamera();

            StartAnprCamera();

            StartDriverCamera();

            RunDiagnostics();
        }
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
        private async void RunDiagnostics()
        {
            LoaderBar.Visibility = Visibility.Visible;
            LoadingText.Visibility = Visibility.Visible;

            bool area =
                undersideCamera != null &&
                undersideCamera.IsCapturing;

            bool anpr =
                anpr_capture != null &&
                anpr_capture.IsOpened();

            bool driver =
                driver_capture != null &&
                driver_capture.IsOpened();

            bool serial =
                await Task.Run(() =>
                    CheckSerialPort("COM4"));



            area_scan_StatusTextBlock.Text =
                area ? "ONLINE" : "OFFLINE";

            ANPR_StatusTextBlock.Text =
                anpr ? "ONLINE" : "OFFLINE";

            Driver_StatusTextBlock.Text =
                driver ? "ONLINE" : "OFFLINE";

            com_scan_StatusTextBlock.Text =
                serial ? "ONLINE" : "OFFLINE";



            area_scan_StatusTextBlock.Foreground =
                area ? Brushes.Green : Brushes.Red;

            ANPR_StatusTextBlock.Foreground =
                anpr ? Brushes.Green : Brushes.Red;

            Driver_StatusTextBlock.Foreground =
                driver ? Brushes.Green : Brushes.Red;

            com_scan_StatusTextBlock.Foreground =
                serial ? Brushes.Green : Brushes.Red;



            AreaScanIPText.Text =
                "IP : 169.254.0.1";

            AnprIPText.Text =
                "IP : 192.168.4.56";

            DriverIPText.Text =
                "IP : 192.168.4.57";



            UpdateSensorStatus();



            LoaderBar.Visibility =
                Visibility.Collapsed;

            LoadingText.Visibility =
                Visibility.Collapsed;
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
                    Dispatcher.Invoke(() =>
                    {
                        AreaScanImage.Source = bitmap;
                    });
                };

                string result =
                    undersideCamera.InitCamera();

                if (result != null)
                    MessageBox.Show(result);

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
                    Dispatcher.Invoke(() =>
                    {
                        ANPR_StatusTextBlock.Text = "OFFLINE";
                        ANPR_StatusTextBlock.Foreground = Brushes.Red;
                    });

                    anpr_isStreaming = false;
                    return;
                }

                Mat frame = new Mat();

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

                        Dispatcher.Invoke(() =>
                        {
                            ANPR_StatusTextBlock.Text = "ONLINE";
                            ANPR_StatusTextBlock.Foreground = Brushes.Green;

                            AnprImage.Source = bmp;
                        });
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }

                anpr_capture.Release();
            }
            catch
            {

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
                    driver_isStreaming = false;
                    return;
                }

                Mat frame = new Mat();

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

                        Dispatcher.Invoke(() =>
                        {
                            DriverImage.Source = bmp;
                        });
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }

                driver_capture.Release();
            }
            catch
            {

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
