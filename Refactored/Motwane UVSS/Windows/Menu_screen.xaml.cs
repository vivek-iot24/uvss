using Microsoft.Extensions.DependencyInjection;
using Motwane.UVSS.Presentation;
using Motwane.UVSS.Presentation.Windows;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace Motwane.UVSS
{
    public partial class Menu_screen : Window
    {
        private readonly IServiceProvider _services;

        public Menu_screen()
        {
            InitializeComponent();
            _services = ((App)System.Windows. Application.Current).ServiceProvider;
        }

        private void OpenWindow<T>() where T : Window
        {
            var window = _services.GetRequiredService<T>();
            window.Show();
            Close();
        }

        private void User_management_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<User_management_tab>();
        }

        private void About_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<About_screen>();
        }

        private void exit_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<Main_uvss_page>();
        }

        private void Report_management_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<Report_management_tab>();
        }

        private void system_setting_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<System_settings_page>();
        }

        private void video_management_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<Video_Management_window>();
        }

        private void self_daignos_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<Self_daignostic_window>();
        }

        private void logout_btn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ---------------------------
        // SYSTEM CONTROL FUNCTIONS
        // ---------------------------

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        const uint WM_CLOSE = 0x0010;

        private void Restart_btn_Click(object sender, RoutedEventArgs e)
        {
            CloseAllUserPrograms();
            RestartSystem();
        }

        private void Shutdown_btn_Click(object sender, RoutedEventArgs e)
        {
            CloseAllUserPrograms();
            ShutdownSystem();
        }

        static void CloseAllUserPrograms()
        {
            Process[] processes = Process.GetProcesses();

            foreach (Process p in processes)
            {
                try
                {
                    if (string.IsNullOrEmpty(p.MainWindowTitle)) continue;

                    SendMessage(p.MainWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
                catch
                {
                }
            }

            System.Threading.Thread.Sleep(5000);
        }

        static void ShutdownSystem()
        {
            Process.Start(new ProcessStartInfo("shutdown", "/s /f /t 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        static void RestartSystem()
        {
            Process.Start(new ProcessStartInfo("shutdown", "/r /f /t 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
    }
}