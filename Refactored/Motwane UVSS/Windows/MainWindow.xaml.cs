using Microsoft.Extensions.DependencyInjection;
using Motwane.UVSS.Application.Interfaces.HAL;
using Motwane.UVSS.Application.Services;
using System;
using System.Windows;

namespace Motwane.UVSS.Presentation.Windows
{
    public partial class MainWindow : Window
    {
        private readonly AuthenticationService _authenticationService;
        private readonly IFileSystemService _fileSystemService;
        public static string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=UVSS_USER_DETAILS;Integrated Security=True;";
        public string USERID;

        public MainWindow(AuthenticationService authenticationService,
                   IFileSystemService fileSystemService)
        {
            InitializeComponent();

            _authenticationService = authenticationService;
            _fileSystemService = fileSystemService;

            _fileSystemService.DeleteAllFiles(@"D:\uvss\underside image");
        }
        public MainWindow() : this(
    ((Motwane.UVSS.Presentation.App)System.Windows.Application.Current).ServiceProvider.GetService<AuthenticationService>(),
    ((Motwane.UVSS.Presentation.App)System.Windows.Application.Current).ServiceProvider.GetService<IFileSystemService>())
        {
        }
        private void Login_btn_Click(object sender, RoutedEventArgs e)
        {
            string userId = txtUserID.Text.Trim();
            string password = txtPassword.Password.Trim();
            string userType = cmbUserType.Text.Trim();

            USERID = userId;

            try
            {
                int count = _authenticationService.ValidateUser(userId, password, userType);

                if (count > 0)
                {
                    Self_daignosis self_Daignosis = new Self_daignosis();
                    self_Daignosis.Show();

                    ((Motwane.UVSS.Presentation.App)System.Windows.Application.Current).LoggedInUserID = txtUserID.Text;
                    ((Motwane.UVSS.Presentation.App)System.Windows.Application.Current).LoggedInUSERTYPE = cmbUserType.Text;
                }
                else
                {
                    MessageBox.Show("Invalid User ID, Password or User Type.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _authenticationService.InsertLoginLog(userId, DateTime.Now);

            txtUserID.Clear();
            txtPassword.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                _authenticationService.UpdateLogoutLog(USERID, DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}