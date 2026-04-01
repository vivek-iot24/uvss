using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Motwane.UVSS.Application.Services;
using Motwane.UVSS.Domain.Entities;

namespace Motwane.UVSS.Presentation.Windows
{
    public partial class User_management_tab : Window
    {
        private readonly UserManagementService userService;

        public User_management_tab(UserManagementService userService)
        {
            InitializeComponent();
            this.userService = userService;
        }

        private void Exit_button_Click(object sender, RoutedEventArgs e)
        {
            Menu_screen menu_Screen = new Menu_screen();
            menu_Screen.Show();
            this.Close();
        }
        private void UserDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (UserDataGrid.SelectedItem is DataRowView rowView)
            {
                string userId = rowView["UserID"].ToString();
                string userName = rowView["UserName"].ToString();

                MessageBox.Show($"Selected User ID: {userId}\nName: {userName}");
            }
        }
        private void Export_btn_1_Click(object sender, RoutedEventArgs e)
        {
            if (UserDataGrid_1.ItemsSource is DataView dataView)
            {
                ExportToPdf(dataView, "History");
            }
        }
        private void Export_btn_Click(object sender, RoutedEventArgs e)
        {
            if (UserDataGrid.ItemsSource is DataView dataView)
            {
                ExportToPdf(dataView, "List of User");
            }
        }
        private void Export_btn_2_Click(object sender, RoutedEventArgs e)
        {
            if (UserLogDataGrid.ItemsSource is DataView dataView)
            {
                ExportToPdf(dataView, "Log Book");
            }
        }
        private void ExportToPdf(DataView dataView, string title)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF file (*.pdf)|*.pdf",
                Title = "Save PDF File"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            PdfDocument document = new PdfDocument();
            document.Info.Title = title;

            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);

            XFont font = new XFont("Verdana", 10);
            XFont headingFont = new XFont("Verdana", 14);

            double yPoint = 40;

            gfx.DrawString(title, headingFont, XBrushes.Red,
                new XRect(30, yPoint, page.Width, page.Height),
                XStringFormats.TopCenter);

            yPoint += 30;

            int colCount = dataView.Table.Columns.Count;

            for (int i = 0; i < colCount; i++)
            {
                gfx.DrawString(dataView.Table.Columns[i].ColumnName, font, XBrushes.Black,
                    new XRect(30 + i * 100, yPoint, page.Width, page.Height),
                    XStringFormats.TopLeft);
            }

            yPoint += 20;

            foreach (DataRowView row in dataView)
            {
                for (int i = 0; i < colCount; i++)
                {
                    gfx.DrawString(row[i].ToString(), font, XBrushes.Black,
                        new XRect(30 + i * 100, yPoint, page.Width, page.Height),
                        XStringFormats.TopLeft);
                }

                yPoint += 20;
            }

            document.Save(saveFileDialog.FileName);

            MessageBox.Show("PDF exported successfully.");
        }
        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            if (Password.Text != ConfPassword.Text)
            {
                MessageBox.Show("Password & Confirm Password is different");
                return;
            }

            try
            {
                User user = new User
                {
                    UserName = UserName.Text,
                    MobileNo = MobNo.Text,
                    CompanyName = CompName.Text,
                    Usertype_ID = 1,          // default admin/operator type
                    Authentication_ID = 1,    // default auth type
                    PasswordHash = Password.Text,
                    PasswordSalt = "STATIC"
                };

                userService.InsertUser(user);

                MessageBox.Show("Data inserted successfully!");

                UserName.Clear();
                MobNo.Clear();
                CompName.Clear();
                Password.Clear();
                ConfPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void search_button_Click(object sender, RoutedEventArgs e)
        {
            string userId = UserId_1.Text.Trim();

            if (string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("Please enter User ID to search.");
                return;
            }

            var user = userService.GetUserById(userId);

            if (user == null)
            {
                MessageBox.Show("User not found.");
                return;
            }

            UserName_1.Text = user.UserName;
            MobNo_1.Text = user.MobileNo;
            CompName_1.Text = user.CompanyName;
            UserId_1.Text = user.UserID;
        }

        private void update_button_Click(object sender, RoutedEventArgs e)
        {
            User user = new User
            {
                UserID = UserId_1.Text,
                UserName = UserName_1.Text,
                MobileNo = MobNo_1.Text,
                CompanyName = CompName_1.Text
            };

            userService.UpdateUser(user);

            MessageBox.Show("User updated successfully.");
        }

        private void list_of_user_tab_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataTable dt = userService.GetAllUsers();
            UserDataGrid.ItemsSource = dt.DefaultView;
        }

        private void Delete_btn_Click(object sender, RoutedEventArgs e)
        {
            if (UserDataGrid.SelectedItem is DataRowView rowView)
            {
                string userName = rowView["User_Name"].ToString();
                string userId = rowView["User_ID"].ToString();
                string idNo = rowView["IdNo"].ToString();

                var result = MessageBox.Show(
                    $"Are you sure you want to delete user ID: {userId}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    userService.DeleteUser(userId, userName, idNo);
                    MessageBox.Show("User deleted successfully.");

                    DataTable dt = userService.GetAllUsers();
                    UserDataGrid.ItemsSource = dt.DefaultView;
                }
            }
        }

        private void history_tab_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataTable dt = userService.GetDeletedUserHistory();
            UserDataGrid_1.ItemsSource = dt.DefaultView;
        }
        private void search_btn_logbook_Click(object sender, RoutedEventArgs e)
        {
            string userId = LogUserIdTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("Please enter a UserID.");
                return;
            }

            DataTable dt = userService.GetUserLoginLog(userId);

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show("No log records found.");
                UserLogDataGrid.ItemsSource = null;
                return;
            }

            UserLogDataGrid.ItemsSource = dt.DefaultView;
        }
    }
}