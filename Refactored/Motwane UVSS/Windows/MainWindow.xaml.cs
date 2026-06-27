using Microsoft.Extensions.DependencyInjection;
using Motwane.UVSS.Application.Common;
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
        public static string connectionString => App.ConnectionString;

        public string USERNAME = string.Empty;

        public MainWindow(AuthenticationService authenticationService,
            IFileSystemService fileSystemService)
        {
            InitializeComponent();

            _authenticationService = authenticationService;
            _fileSystemService = fileSystemService;

            _fileSystemService.DeleteAllFiles(@"D:\uvss\underside image");
        }

        public MainWindow() : this(
            ((Motwane.UVSS.Presentation.App)System.Windows.Application.Current)
                .ServiceProvider.GetService<AuthenticationService>(),
            ((Motwane.UVSS.Presentation.App)System.Windows.Application.Current)
                .ServiceProvider.GetService<IFileSystemService>())
        {
        }

        private void Login_btn_Click(object sender, RoutedEventArgs e)
        {
            string userName = txtUserName.Text.Trim();
            string password = txtPassword.Password.Trim();
            string userType = cmbUserType.Text.Trim();

            USERNAME = userName;

            try
            {
                int count = _authenticationService.ValidateUser(userName, password, userType);

                if (count > 0)
                {
                    _authenticationService.InsertLoginLog(
     userName,
     Guid.NewGuid(),
     DateTime.Now
 );
                    Self_daignosis selfDiagnosis = new Self_daignosis();
                    selfDiagnosis.Show();

                    ((Motwane.UVSS.Presentation.App)System.Windows.Application.Current)
                        .LoggedInUserID = userName;

                    ((Motwane.UVSS.Presentation.App)System.Windows.Application.Current)
                        .LoggedInUSERTYPE = userType;
                   
                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Invalid User ID, Password or User Type.",
                        "Login Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error: " + ex.Message,
                    "Exception",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            txtUserName.Clear();
            txtPassword.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(USERNAME))
                {
                    _authenticationService.UpdateLogoutLog(USERNAME, DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}