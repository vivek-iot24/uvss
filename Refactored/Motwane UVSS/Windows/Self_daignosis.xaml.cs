using Motwane.UVSS.Application.Interfaces.HAL;
using System;
using System.Threading.Tasks;
using System.Windows;
using Motwane.UVSS.Presentation.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Motwane.UVSS.Presentation.Windows
{
    public partial class Self_daignosis : Window
    {
        private readonly IDiagnosticService _diagnosticService;

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

        
        public Self_daignosis() : this(
            ((Motwane.UVSS.Presentation.App)System.Windows.Application.Current)
            .ServiceProvider
            .GetService(typeof(IDiagnosticService)) as IDiagnosticService)
        {
        }

        // DI constructor
        public Self_daignosis(IDiagnosticService diagnosticService)
        {
            InitializeComponent();

            _diagnosticService = diagnosticService;

            this.Loaded += async (s, e) =>
            {
                await Task.Delay(200);
                await CheckAllConnectionsAsync();
            };
        }

        public async Task CheckAllConnectionsAsync()
        {
            bool allOk = true;
            string errorMessage = "";

            foreach (var ip in ipAddresses)
            {
                bool pingResult = await _diagnosticService.PingAddressAsync(ip);
                if (!pingResult)
                {
                    allOk = false;
                    errorMessage += $"Ping failed: {ip}\n";
                }
            }

            bool serialOk = _diagnosticService.CheckSerialConnection(serialPortName);
            if (!serialOk)
            {
                allOk = false;
                errorMessage += $"Serial port not available: {serialPortName}\n";
            }

            System.Threading.Thread.Sleep(2000);

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

            ProgressBarStatus.IsIndeterminate = false;
            ProgressBarStatus.Visibility = Visibility.Collapsed;
        }

        public void method_to_go_next_page()
        {
            var main_Uvss_Page =
                ((Motwane.UVSS.Presentation.App )System.Windows. Application.Current)
                .ServiceProvider .GetRequiredService<Main_uvss_page>();

          

            this.Close();
            main_Uvss_Page.Show();
            this.Close();
        }

        public void method_to_go_back_page()
        {
            this.Close();
        }
    }
}