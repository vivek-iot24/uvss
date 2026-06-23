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
                string userId = rowView["User_ID"].ToString();
                string userName = rowView["User_Name"].ToString();

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
                Guid userTypeUUID = Guid.Empty;
                string userTypeText = "";

                if (new_user_tab_user_type.SelectedItem != null)
                {
                    ComboBoxItem selectedItem = (ComboBoxItem)new_user_tab_user_type.SelectedItem;
                    userTypeText = selectedItem.Content.ToString();

                    // Example mapping: Operator=1, Admin=2, Maintenance=3
                    switch (userTypeText)
                    {
                        case "Operator":
                             userTypeUUID = Guid.Parse("57F131DB-D7B1-445E-BD91-5CB602BC848B");
                            break;
                        case "Admin":
                            userTypeUUID = Guid.Parse("D0357D48-854E-4EC3-8001-E711C9436E50");          
                            break;
                        case "Service":
                            userTypeUUID = Guid.Parse("93F797F1-E258-4F26-BBC0-78653A6EE3F2");
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Please select a User Type!");
                    return;
                }

                User user = new User
                {
                    UserName = UserName.Text,
                    IdNo = Idno.Text,
                    MobileNo = MobNo.Text,
                    CompanyName = CompName.Text,
                    AgencyName = Agencyname.Text,
                    Password = Password.Text,
                    UserType = userTypeText,   
                    Usertype_UUID = userTypeUUID        
                };

                userService.InsertUser(user);

                MessageBox.Show("Data inserted successfully!");

                // Clear fields
                UserName.Clear();
                Idno.Clear();
                MobNo.Clear();
                CompName.Clear();
                Agencyname.Clear();
                Password.Clear();
                ConfPassword.Clear();
                new_user_tab_user_type.SelectedIndex = -1; // Reset ComboBox
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

            var user = userService.GetUserById(new System.Guid(userId));

            if (user == null)
            {
                MessageBox.Show("User not found.");
                return;
            }

            UserName_1.Text = user.UserName;
            Idno_1.Text = user.IdNo;
            MobNo_1.Text = user.MobileNo;
            CompName_1.Text = user.CompanyName;
            Agencyname_1.Text = user.AgencyName;
            UserId_1.Text = user.UserUUID.ToString();
            Password_1.Text = user.Password;
            ConfPassword_1.Text = user.Password;
            new_user_tab_user_type_1.Text = user.UserType;
        }

        private void update_button_Click(object sender, RoutedEventArgs e)
        {
            if (Password_1.Text != ConfPassword_1.Text)
            {
                MessageBox.Show("Password & Confirm Password is different");
                return;
            }

            User user = new User
            {
                UserName = UserName_1.Text,
                IdNo = Idno_1.Text,
                MobileNo = MobNo_1.Text,
                CompanyName = CompName_1.Text,
                AgencyName = Agencyname_1.Text,
                UserUUID = new System.Guid(UserId_1.Text),
                Password = Password_1.Text,
                UserType = new_user_tab_user_type_1.Text
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
                string userId = rowView["User_  UUID"].ToString();
                string userName = rowView["User_Name"].ToString();
                string idNo = rowView["IdNo"].ToString();

                var result = MessageBox.Show(
                    $"Are you sure you want to deactivate user ID: {userId}?",
                    "Confirm Deactivate",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    userService.DeleteUser(new System.Guid(userId), userName, idNo);

                    MessageBox.Show("User deactivated successfully.");

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

            DataTable dt = userService.GetUserLoginLog(new System.Guid(userId));

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