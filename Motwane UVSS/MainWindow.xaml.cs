using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Motwane_UVSS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        

        // Define your connection string here
       // public string connectionString = "Server=SEC-MANDAR;Database=UVSS_USER_DETAILS;Integrated Security=True;";
        public string connectionString = "Server=DESKTOP-TJ0UIHH;Database=UVSS_USER_DETAILS;Integrated Security=True;";
        public string USERID;
        

        public MainWindow()
        {
            InitializeComponent();

            DeleteAllFiles(@"D:\uvss\underside image");
        }

        private void Login_btn_Click(object sender, RoutedEventArgs e)
        {
            string userId = txtUserID.Text.Trim();
            string password = txtPassword.Password.Trim();
            string userType = cmbUserType.Text.Trim();

            USERID = userId;    

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT COUNT(*) FROM dbo.Users WHERE UserID = @UserID AND Password = @Password AND UserType = @UserType";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@UserType", userType);

                        int count = (int)cmd.ExecuteScalar();

                        if (count > 0)
                        {
                           // MessageBox.Show("Login successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            Self_daignosis self_Daignosis = new Self_daignosis();
                            self_Daignosis.Show();

                           
                            ((App)Application.Current).LoggedInUserID = txtUserID.Text;
                            ((App)Application.Current).LoggedInUSERTYPE = cmbUserType.Text;


                            // MessageBox.Show(USERID_1);
                        }
                        else
                        {
                            MessageBox.Show("Invalid User ID, Password or User Type.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // self_Daignosis.Show();
            

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string loginQuery = @" INSERT INTO user_login_log (UserID, LoginTime)VALUES (@UserID, @LoginTime)";


                using (SqlCommand cmd = new SqlCommand(loginQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@LoginTime", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }

            txtUserID.Clear();
            txtPassword.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string logoutQuery = @" UPDATE user_login_log SET LogoutTime = @LogoutTime WHERE UserID = @UserID AND LogoutTime IS NULL";


                    using (SqlCommand cmd = new SqlCommand(logoutQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserID", USERID);
                        cmd.Parameters.AddWithValue("@LogoutTime", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
          
            
            
        }

        #region Using to delete all images from folder after stitching the image

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

        private const uint DELETE = 0x10000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint FILE_SHARE_WRITE = 0x00000002;
        private const uint FILE_SHARE_DELETE = 0x00000004;
        private const uint OPEN_EXISTING = 3;
        public static void DeleteAllFiles(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Directory does not exist: " + folderPath);
                return;
            }

            string[] files = Directory.GetFiles(folderPath);

            foreach (var file in files)
            {
                try
                {
                    // Try to force-delete even if file is in use
                    IntPtr handle = CreateFile(
                        file,
                        DELETE,
                        FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE,
                        IntPtr.Zero,
                        OPEN_EXISTING,
                        0,
                        IntPtr.Zero);

                    if (handle != new IntPtr(-1))
                    {
                        File.Delete(file);
                        Console.WriteLine($"Deleted: {file}");
                    }
                    else
                    {
                        Console.WriteLine($"File in use or locked: {file}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete {file}: {ex.Message}");
                }
            }
        }

        #endregion

    }
}
