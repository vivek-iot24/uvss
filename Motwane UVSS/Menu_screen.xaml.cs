using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace Motwane_UVSS
{
    /// <summary>
    /// Interaction logic for Menu_screen.xaml
    /// </summary>
    public partial class Menu_screen : Window
    {
        public Menu_screen()
        {
            InitializeComponent();
        }

        private void User_management_btn_Click(object sender, RoutedEventArgs e)
        {
            User_management_tab user_Management_Tab = new User_management_tab();
            user_Management_Tab.Show();

            this.Close();
        }

        private void About_btn_Click(object sender, RoutedEventArgs e)
        {
            About_screen about_Screen = new About_screen();
            about_Screen.Show();    

            this.Close();   
        }

        private void exit_btn_Click(object sender, RoutedEventArgs e)
        {
            Main_uvss_page main_Uvss_Page = new Main_uvss_page();
            main_Uvss_Page.Show();  

            this.Close();
        }

        private void Report_management_btn_Click(object sender, RoutedEventArgs e)
        {
            Report_management_tab report_Management_Tab = new Report_management_tab();
            report_Management_Tab.Show();   
            this.Close();
        }

        private void logout_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();
            
        }

        private void system_setting_btn_Click(object sender, RoutedEventArgs e)
        {
            System_settings_page system_Settings_Page = new System_settings_page();
            system_Settings_Page.Show();    
            this.Close();
        }

        private void video_management_btn_Click(object sender, RoutedEventArgs e)
        {
            Video_Management_window video_Management_Window = new Video_Management_window();
            video_Management_Window.Show();
            this.Close();
        }

        private void self_daignos_btn_Click(object sender, RoutedEventArgs e)
        {
            Self_daignostic_window self_Daignostic_Window = new Self_daignostic_window();   
            self_Daignostic_Window.Show();

            this.Close();
        }

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
                    // Only close user-visible windows
                    if (string.IsNullOrEmpty(p.MainWindowTitle)) continue;

                    // Gracefully ask the program to close
                    SendMessage(p.MainWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                }
                catch
                {
                    // Ignore exceptions (e.g., access denied)
                }
            }

            // Wait a few seconds for apps to close
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
