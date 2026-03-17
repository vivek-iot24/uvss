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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Motwane.UVSS
{
    /// <summary>
    /// Interaction logic for Self_daignostic_window.xaml
    /// </summary>
    public partial class Self_daignostic_window : Window
    {
        public Self_daignostic_window()
        {
            InitializeComponent();

            RunDiagnostics();
        }

        private async void RunDiagnostics()
        {
            LoaderBar.Visibility = Visibility.Visible;
            LoadingText.Visibility = Visibility.Visible;
            // StatusTextBlock.Text = "Starting self-diagnostics...\n";

            string cam1Status = await PingDevice("192.168.4.58") ? "OK" : "FAIL";
            Video_1_scan_StatusTextBlock.Text += $"(192.168.4.58): {cam1Status}\n";

            string cam1_1Status = await PingDevice("192.168.4.59") ? "OK" : "FAIL";
            Video_2_scan_StatusTextBlock.Text += $"(192.168.4.59): {cam1_1Status}\n";

            string cam1_2Status = await PingDevice("192.168.4.60") ? "OK" : "FAIL";
            Video_3_scan_StatusTextBlock.Text += $"(192.168.4.60): {cam1_2Status}\n";

            string cam2Status = await PingDevice("169.254.0.1") ? "OK" : "FAIL";
            area_scan_StatusTextBlock.Text += $"(169.254.0.1): {cam2Status}\n";

            string anprStatus = await PingDevice("192.168.4.56") ? "OK" : "FAIL";
            ANPR_StatusTextBlock.Text += $"(192.168.4.56): {anprStatus}\n";

            string driverStatus = await PingDevice("192.168.4.57") ? "OK" : "FAIL";
            Driver_StatusTextBlock.Text += $"(192.168.4.57): {driverStatus}\n";

            string serialStatus = await Task.Run(() => CheckSerialPort("COM4")) ? "OK" : "FAIL";
            com_scan_StatusTextBlock.Text += $"Serial COM4: {serialStatus}\n";

            // StatusTextBlock.Text += "\nSelf-diagnostics completed.";

            LoaderBar.Visibility = Visibility.Collapsed;
            LoadingText.Visibility = Visibility.Collapsed;
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
