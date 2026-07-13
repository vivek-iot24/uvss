using Microsoft.Extensions.DependencyInjection;
using Motwane.UVSS.Application.Common;
using Motwane.UVSS.Application.Interfaces.HAL;
using Motwane.UVSS.Application.Services;
using Motwane.UVSS.Domain;
using Motwane.UVSS.Presentation.Windows;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Motwane.UVSS.Windows
{
    public partial class LoginWindow : Window
    {
        private readonly AuthenticationService _authenticationService;
        private readonly IFileSystemService _fileSystemService;

        public string USERNAME = string.Empty;

        public LoginWindow(
            AuthenticationService authenticationService,
            IFileSystemService fileSystemService)
        {
            InitializeComponent();

            _authenticationService = authenticationService;
            _fileSystemService = fileSystemService;

            _fileSystemService.DeleteAllFiles(@"D:\uvss\underside image");

            PasswordField.PasswordChanged += PasswordField_PasswordChanged;
        }

        public LoginWindow() : this(((Motwane.UVSS.Presentation.App)System.Windows.Application.Current).ServiceProvider.GetService<AuthenticationService>(),
            ((Motwane.UVSS.Presentation.App)System.Windows.Application.Current).ServiceProvider.GetService<IFileSystemService>())
        { }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string userName = UsernameField.Text.Trim();
            string password = PasswordField.Password.Trim();

            string userType = "";

            if (TypeDropdown.SelectedItem is System.Windows.Controls.ComboBoxItem item)
                userType = item.Content.ToString();

            USERNAME = userName;

            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Please enter Username.");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter Password.");
                return;
            }

            if (string.IsNullOrWhiteSpace(userType))
            {
                MessageBox.Show("Please select User Type.");
                return;
            }

            try
            {
                int count = _authenticationService.ValidateUser(
                    userName,
                    password,
                    userType);

                if (count <= 0)
                {
                    MessageBox.Show(
                        "Invalid Username, Password or User Type.",
                        "Login Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                SessionManager.User_UUID =
                    _authenticationService.GetUserUUIDByUserName(userName);

                SessionManager.UserType_UUID =
                    _authenticationService.GetUserTypeUUIDByUserName(userName);

                SessionManager.Username = userName;

                Authentication auth =
                    _authenticationService.LoadPermissions(
                        SessionManager.UserType_UUID);

                if (auth != null)
                {
                    SessionManager.User_Settings = auth.User_Settings;
                    SessionManager.Application_settings = auth.Application_settings;
                    SessionManager.Diagnosis = auth.Diagnosis;
                    SessionManager.Camera_settings = auth.Camera_settings;
                    SessionManager.Reports = auth.Reports;
                    SessionManager.Menu = auth.Menu;
                    SessionManager.Aic = auth.Aic;
                }

                _authenticationService.InsertLoginLog(
                    userName,
                    Guid.NewGuid(),
                    DateTime.Now);

                var app = (Motwane.UVSS.Presentation.App)System.Windows.Application.Current;

                app.LoggedInUserID = userName;
                app.LoggedInUSERTYPE = userType;

                Self_daignosis selfDiagnosis = new Self_daignosis();
                selfDiagnosis.Show();

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void PasswordField_PasswordChanged(
            object sender,
            RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility =
                string.IsNullOrEmpty(PasswordField.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        private void UsernameField_TextChanged(object sender, TextChangedEventArgs e)
        {
            UsernamePlaceholder.Visibility =
                string.IsNullOrWhiteSpace(UsernameField.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(USERNAME))
                {
                    _authenticationService.UpdateLogoutLog(
                        USERNAME,
                        DateTime.Now);
                }
            }
            catch
            {
            }
        }
        private bool _showPassword = false;
        private void TogglePasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_showPassword)
            {
                PasswordField.Password = PasswordTextBox.Text;

                PasswordField.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                PasswordTextBox.Text = PasswordField.Password;

                PasswordField.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
            }

            _showPassword = !_showPassword;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ForgotPasswordOverlay.Visibility = Visibility.Visible;
        }
        private void CancelForgotBtn_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordOverlay.Visibility = Visibility.Collapsed;
        }

        private void ResetPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            string username = ForgotUsernameTextBox.Text;

           
            MessageBox.Show($"Admin will reset the password for {username}");

            ForgotPasswordOverlay.Visibility = Visibility.Collapsed;
            ForgotUsernameTextBox.Clear();
        }
    }
}