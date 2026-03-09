using System;
using System.Collections.Generic;
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
using System.Windows.Threading;

namespace Motwane_UVSS
{
    /// <summary>
    /// Interaction logic for Self_daignosis.xaml
    /// </summary>
    //public partial class Self_daignosis : Window
    //{
    //       private static string[] ipAddresses = new string[]
    //       {
    //            "192.168.4.56",
    //            "192.168.4.57",
    //            "192.168.4.58",
    //            "192.168.4.59",
    //            "192.168.4.60",
    //            "169.254.0.1"
    //       };

    //    private const string serialPortName = "COM3";

    //    public async Task CheckAllConnectionsAsync()
    //    {
    //        bool allOk = true;
    //        string errorMessage = "";

    //        // Check IPs
    //        foreach (var ip in ipAddresses)
    //        {
    //            bool pingResult = await PingAddressAsync(ip);
    //            if (!pingResult)
    //            {
    //                allOk = false;
    //                errorMessage += $"Ping failed: {ip}\n";
    //            }
    //        }

    //        // Check Serial Port
    //        bool serialOk = CheckSerialConnection(serialPortName);
    //        if (!serialOk)
    //        {
    //            allOk = false;
    //            errorMessage += $"Serial port not available or can't be opened: {serialPortName}\n";
    //        }

    //        if (allOk)
    //        {
    //            method_to_go_next_page();
    //        }
    //        else
    //        {
    //            MessageBox.Show("Connection issues found:\n" + errorMessage);
    //            method_to_go_back_page();
    //        }
    //    }

    //    private async Task<bool> PingAddressAsync(string ip)
    //    {
    //        try
    //        {
    //            using (Ping ping = new Ping())
    //            {
    //                PingReply reply = await ping.SendPingAsync(ip, 1000); // 1 second timeout
    //                return reply.Status == IPStatus.Success;
    //            }
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //    }

    //    private bool CheckSerialConnection(string portName)
    //    {
    //        try
    //        {
    //            using (SerialPort serialPort = new SerialPort(portName))
    //            {
    //                serialPort.Open();
    //                if (serialPort.IsOpen)
    //                {
    //                    serialPort.Close();
    //                    return true;
    //                }
    //            }
    //        }
    //        catch
    //        {
    //            return false;
    //        }
    //        return false;
    //    }

    //    public Self_daignosis()
    //    {
    //        InitializeComponent();

    //        CheckAllConnectionsAsync();

    //        //DispatcherTimer timer = new DispatcherTimer();
    //        //timer.Interval = TimeSpan.FromSeconds(5);
    //        //timer.Tick += Timer_Tick;
    //        //timer.Start();
    //    }

    //    //private void Timer_Tick(object sender, EventArgs e)
    //    //{
    //    //    (sender as DispatcherTimer).Stop();
    //    //    Main_uvss_page main_Uvss_Page = new Main_uvss_page();
    //    //    main_Uvss_Page.Show();  
    //    //    this.Close();

    //    //}

    //    public void method_to_daignose()
    //    {

    //    }

    //    public void method_to_go_next_page()
    //    {
    //        Main_uvss_page main_Uvss_Page = new Main_uvss_page();
    //        main_Uvss_Page.Show();
    //        this.Close();
    //    }

    //    public void method_to_go_back_page()
    //    {

    //        this.Close();
    //    }
    //}

    public partial class Self_daignosis : Window
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

        private const string serialPortName = "COM4";

        public Self_daignosis()
        {
            InitializeComponent();

            // Start the diagnostics after UI has rendered
            this.Loaded += async (s, e) =>
            {
                await Task.Delay(200); // Let the UI render first
                await CheckAllConnectionsAsync(); // Run the connection check once
            };
        }

        public async Task CheckAllConnectionsAsync()
        {
            bool allOk = true;
            string errorMessage = "";

            // Check each IP address
            foreach (var ip in ipAddresses)
            {
                bool pingResult = await PingAddressAsync(ip);
                if (!pingResult)
                {
                    allOk = false;
                    errorMessage += $"Ping failed: {ip}\n";
                }
            }

            // Check Serial Port
            bool serialOk = CheckSerialConnection(serialPortName);
            if (!serialOk)
            {
                allOk = false;
                errorMessage += $"Serial port not available: {serialPortName}\n";
            }
            System.Threading.Thread.Sleep(2000);

            // Show result in a MessageBox and keep the UI open
            if (allOk)
            {
                MessageBox.Show("All connections OK!", "Status", MessageBoxButton.OK, MessageBoxImage.Information);
                method_to_go_next_page();
            }
            else
            {
                MessageBox.Show("Connection issues found:\n" + errorMessage, "Status", MessageBoxButton.OK, MessageBoxImage.Warning);
                method_to_go_back_page();
            }

            // Optional: stop or hide progress bar after check
            ProgressBarStatus.IsIndeterminate = false;
            ProgressBarStatus.Visibility = Visibility.Collapsed;
        }

        public void method_to_go_next_page()
        {
            Main_uvss_page main_Uvss_Page = new Main_uvss_page();
            main_Uvss_Page.Show();
            this.Close();
        }

        public void method_to_go_back_page()
        {

            this.Close();
        }

        private async Task<bool> PingAddressAsync(string ip)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ip, 1000); // 1 second timeout
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool CheckSerialConnection(string portName)
        {
            try
            {
                using (SerialPort serialPort = new SerialPort(portName))
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
    }
}
