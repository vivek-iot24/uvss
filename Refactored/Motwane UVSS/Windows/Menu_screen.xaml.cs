using Microsoft.Extensions.DependencyInjection;
using Motwane.UVSS.Application.Common;
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
            if (SessionManager.User_Settings == 0)
            {
                ShowAccessDenied();
                return;
            }

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
            if (SessionManager.Reports == 0)
            {
                ShowAccessDenied();
                return;
            }

            OpenWindow<Report_management_tab>();
        }

        private void system_setting_btn_Click(object sender, RoutedEventArgs e)
        {
            if (SessionManager.Application_settings == 0)
            {
                ShowAccessDenied();
                return;
            }

            OpenWindow<System_settings_page>();
        }

        private void video_management_btn_Click(object sender, RoutedEventArgs e)
        {
            OpenWindow<Video_Management_window>();
        }

        private void self_daignos_btn_Click(object sender, RoutedEventArgs e)
        {
            if (SessionManager.Diagnosis == 0)
            {
                ShowAccessDenied();
                return;
            }

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
        private void ShowAccessDenied()
        {
            MessageBox.Show(
                "You do not have permission to access this module.",
                "Access Denied",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        static void ShutdownSystem()
        {
            Process.Start(new ProcessStartInfo("shutdown", "/s /f /t 0")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            User_management_btn.IsEnabled = SessionManager.User_Settings == 1;
            system_setting_btn.IsEnabled = SessionManager.Application_settings == 1;
            self_daignos_btn.IsEnabled = SessionManager.Diagnosis == 1;
            Report_management_btn.IsEnabled = SessionManager.Reports == 1;
            video_management_btn.IsEnabled = SessionManager.Camera_settings == 1;
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