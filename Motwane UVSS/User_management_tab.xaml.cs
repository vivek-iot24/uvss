using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Motwane_UVSS
{
    /// <summary>
    /// Interaction logic for User_management_tab.xaml
    /// </summary>
    public partial class User_management_tab : Window
    {
        
        public User_management_tab()
        {
            InitializeComponent();
        }

        private void Exit_button_Click(object sender, RoutedEventArgs e)
        {
            Menu_screen menu_Screen = new Menu_screen();
            menu_Screen.Show(); 

            this.Close();
        }
        MainWindow mainWindow = new MainWindow();
        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            if (Password.Text == ConfPassword.Text)
            {
                // Get values from TextBoxes
                string userName = UserName.Text;
                string IdNo = Idno.Text;
                string Mobileno = MobNo.Text;
                string CompanyName = CompName.Text;
                string AgencyId = Agencyname.Text;
                string userId = UserId.Text;
                string password = Password.Text;
                string usertype = new_user_tab_user_type.Text;

                string connectionString = mainWindow.connectionString;

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // SQL query to insert data into the Users table
                        string query = "INSERT INTO dbo.Users (UserName, MobileNo, CompanyName, AgencyName, UserID, Password,UserType,IdNo) " +
                                       "VALUES (@UserName, @MobileNo, @CompanyName, @AgencyName, @UserID, @Password, @UserType, @IdNo)";

                        SqlCommand cmd = new SqlCommand(query, connection);

                        // Add parameters to prevent SQL injection
                        cmd.Parameters.AddWithValue("@UserName", userName);
                        cmd.Parameters.AddWithValue("@MobileNo", Mobileno);
                        cmd.Parameters.AddWithValue("@CompanyName", CompanyName);
                        cmd.Parameters.AddWithValue("@AgencyName", AgencyId);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@UserType", usertype);
                        cmd.Parameters.AddWithValue("@IdNo", IdNo);

                        // Execute the query to insert data
                        int rowsAffected = cmd.ExecuteNonQuery();

                        // Display success message
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Data inserted successfully!");
                        }
                        else
                        {
                            MessageBox.Show("Failed to insert data.");
                        }
                    }

                    UserName.Clear();
                    Idno.Clear();
                    MobNo.Clear();
                    CompName.Clear();
                    Agencyname.Clear();
                    UserId.Clear();
                    Password.Clear();
                    ConfPassword.Clear();
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Password & Confirm Password is different");
            }
        }

        private void search_button_Click(object sender, RoutedEventArgs e)
        {
            string userId = UserId_1.Text.Trim();
            string connectionString = mainWindow.connectionString;

            if (string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("Please enter User ID to search.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT UserName, MobileNo, CompanyName, AgencyName, UserID, Password, UserType, IdNo FROM dbo.Users WHERE UserID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);

                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            UserName_1.Text = reader["UserName"].ToString();
                            Idno_1.Text = reader["IdNo"].ToString();
                            MobNo_1.Text = reader["MobileNo"].ToString();
                            CompName_1.Text = reader["CompanyName"].ToString();
                            Agencyname_1.Text = reader["AgencyName"].ToString();
                            UserId_1.Text = reader["UserID"].ToString();
                            Password_1.Text = reader["Password"].ToString();
                            ConfPassword_1.Text = reader["Password"].ToString();
                            new_user_tab_user_type_1.Text = reader["UserType"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("User not found.");
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            
        }

        private void update_button_Click(object sender, RoutedEventArgs e)
        {
           
            string userName = UserName_1.Text.Trim();
            string IdNo = Idno_1.Text.Trim();
            string Mobileno = MobNo_1.Text.Trim();
            string CompanyName = CompName_1.Text.Trim();
            string AgencyId = Agencyname_1.Text.Trim();
            string userId = UserId_1.Text.Trim();
            string password = Password_1.Text.Trim();
            string usertype = new_user_tab_user_type_1.Text.Trim();

            string connectionString = mainWindow.connectionString;

            if (Password.Text == ConfPassword.Text)
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    MessageBox.Show("Please search for a user first.");
                    return;
                }

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string query = "UPDATE dbo.Users SET UserName = @UserName, MobileNo = @MobileNo, CompanyName = @CompanyName, AgencyName = @AgencyName, @IdNo = IdNo, Password = @Password, UserType = @UserType WHERE UserID = @UserID";


                        using (SqlCommand cmd = new SqlCommand(query, connection))
                        {
                            // Add parameters to prevent SQL injection
                            cmd.Parameters.AddWithValue("@UserName", userName);
                            cmd.Parameters.AddWithValue("@MobileNo", Mobileno);
                            cmd.Parameters.AddWithValue("@CompanyName", CompanyName);
                            cmd.Parameters.AddWithValue("@AgencyName", AgencyId);
                            cmd.Parameters.AddWithValue("@UserID", userId);
                            cmd.Parameters.AddWithValue("@Password", password);
                            cmd.Parameters.AddWithValue("@UserType", usertype);
                            cmd.Parameters.AddWithValue("@IdNo", IdNo);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                                MessageBox.Show("User details updated successfully!");
                            else
                                MessageBox.Show("No changes were made.");
                        }

                        UserName_1.Clear();
                        Idno_1.Clear();
                        MobNo_1.Clear();
                        CompName_1.Clear();
                        Agencyname_1.Clear();
                        UserId_1.Clear();
                        Password_1.Clear();
                        ConfPassword_1.Clear();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Password & Confirm Password is different");
            }
        }

        private void LoadUserTable()
        {
            string connectionString = mainWindow.connectionString;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM dbo.Users", con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dt.Rows[i][0] = 1+i;
                        }
                    }
                    UserDataGrid.ItemsSource = dt.DefaultView;
                    
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }
      
        private void list_of_user_tab_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LoadUserTable();
        }

        private void UserDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (UserDataGrid.SelectedItem is DataRowView rowView)
            {
                string userId = rowView["UserID"].ToString();
                string userName = rowView["UserName"].ToString();
                // Access other columns as needed

                MessageBox.Show($"Selected User ID: {userId}\nName: {userName}");
            }
        }

        private void Export_btn_Click(object sender, RoutedEventArgs e)
        {
            if (UserDataGrid.ItemsSource is DataView dataView)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF file (*.pdf)|*.pdf",
                    Title = "Save PDF File"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        PdfDocument document = new PdfDocument();
                        document.Info.Title = "Exported User Table";

                        PdfPage page = document.AddPage();
                        XGraphics gfx = XGraphics.FromPdfPage(page);
                        XFont font = new XFont("Verdana", 10);
                        XFont headingFont = new XFont("Verdana", 14);
                        double yPoint = 40;

                        // Draw heading in bold red
                        gfx.DrawString("List of User", headingFont, XBrushes.Red,
                            new XRect(30, yPoint, page.Width, page.Height),
                            XStringFormats.TopCenter);

                        yPoint += 30; // Add space after heading

                        // Draw date and time at the top right corner
                        string dateTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        gfx.DrawString(dateTimeText, font, XBrushes.Black,
                            new XRect(0, 10, page.Width - 40, page.Height),
                            XStringFormats.TopRight);

                        // Write header columns
                        int colCount = dataView.Table.Columns.Count;
                        for (int i = 0; i < colCount; i++)
                        {
                            gfx.DrawString(dataView.Table.Columns[i].ColumnName, font, XBrushes.Black,
                                new XRect(30 + i * 100, yPoint, page.Width, page.Height),
                                XStringFormats.TopLeft);
                        }

                        yPoint += 20;

                        // Write rows of data
                        foreach (DataRowView row in dataView)
                        {
                            for (int i = 0; i < colCount; i++)
                            {
                                string cellText = row[i].ToString();
                                gfx.DrawString(cellText, font, XBrushes.Black,
                                    new XRect(30 + i * 100, yPoint, page.Width, page.Height),
                                    XStringFormats.TopLeft);
                            }
                            yPoint += 20;

                            // Add a new page if needed
                            if (yPoint > page.Height - 40)
                            {
                                page = document.AddPage();
                                gfx = XGraphics.FromPdfPage(page);
                                yPoint = 40;

                                // Redraw heading on each new page
                                gfx.DrawString("List of User", headingFont, XBrushes.Red,
                                    new XRect(30, yPoint, page.Width, page.Height),
                                    XStringFormats.TopLeft);

                                yPoint += 30;

                                // Redraw date/time on new page
                                gfx.DrawString(dateTimeText, font, XBrushes.Black,
                                    new XRect(0, 10, page.Width - 40, page.Height),
                                    XStringFormats.TopRight);

                                // Redraw column headers
                                for (int i = 0; i < colCount; i++)
                                {
                                    gfx.DrawString(dataView.Table.Columns[i].ColumnName, font, XBrushes.Black,
                                        new XRect(30 + i * 100, yPoint, page.Width, page.Height),
                                        XStringFormats.TopLeft);
                                }

                                yPoint += 20;
                            }
                        }

                        document.Save(saveFileDialog.FileName);
                        MessageBox.Show("PDF exported successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting PDF: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("No data to export.");
            }
        }

        private void Delete_btn_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = mainWindow.connectionString;

            if (UserDataGrid.SelectedItem is DataRowView rowView)
            {
                string userId = rowView["UserID"].ToString();
                string userName = rowView["UserName"].ToString();
                string idNo = rowView["IdNo"].ToString();

                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete user ID: {userId}?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (SqlConnection con = new SqlConnection(connectionString))
                        {
                            con.Open();

                            // Begin a transaction to keep both operations atomic
                            using (SqlTransaction transaction = con.BeginTransaction())
                            {
                                try
                                {
                                    // Insert into deleted_user_history
                                    string insertQuery = @"
                                INSERT INTO deleted_user_history (UserName, UserID, IdNo, DeletedDate)
                                VALUES (@UserName, @UserID, @IdNo, GETDATE())";

                                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, con, transaction))
                                    {
                                        insertCmd.Parameters.AddWithValue("@UserName", userName);
                                        insertCmd.Parameters.AddWithValue("@UserID", userId);
                                        insertCmd.Parameters.AddWithValue("@IdNo", idNo);
                                        insertCmd.ExecuteNonQuery();
                                    }

                                    // Delete from Users table
                                    string deleteQuery = "DELETE FROM dbo.Users WHERE UserID = @UserID";

                                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, con, transaction))
                                    {
                                        deleteCmd.Parameters.AddWithValue("@UserID", userId);
                                        int rowsAffected = deleteCmd.ExecuteNonQuery();

                                        if (rowsAffected > 0)
                                        {
                                            transaction.Commit();
                                            MessageBox.Show("User deleted successfully.");
                                            LoadUserTable(); // Refresh the table
                                        }
                                        else
                                        {
                                            transaction.Rollback();
                                            MessageBox.Show("No user found or deletion failed.");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    MessageBox.Show("Error during deletion process: " + ex.Message);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting user: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a row to delete.");
            }
        }

        private void Export_btn_1_Click(object sender, RoutedEventArgs e)
        {
            if (UserDataGrid_1.ItemsSource is DataView dataView)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF file (*.pdf)|*.pdf",
                    Title = "Save PDF File"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        PdfDocument document = new PdfDocument();
                        document.Info.Title = "Exported User Table";

                        PdfPage page = document.AddPage();
                        XGraphics gfx = XGraphics.FromPdfPage(page);
                        XFont font = new XFont("Verdana", 10);
                        XFont headingFont = new XFont("Verdana", 14);
                        double yPoint = 40;

                        // Draw heading in bold red
                        gfx.DrawString("List of User", headingFont, XBrushes.Red,
                            new XRect(30, yPoint, page.Width, page.Height),
                            XStringFormats.TopCenter);

                        yPoint += 30; // Add space after heading

                        // Draw date and time at the top right corner
                        string dateTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        gfx.DrawString(dateTimeText, font, XBrushes.Black,
                            new XRect(0, 10, page.Width - 40, page.Height),
                            XStringFormats.TopRight);

                        // Write header columns
                        int colCount = dataView.Table.Columns.Count;
                        for (int i = 0; i < colCount; i++)
                        {
                            gfx.DrawString(dataView.Table.Columns[i].ColumnName, font, XBrushes.Black,
                                new XRect(30 + i * 100, yPoint, page.Width, page.Height),
                                XStringFormats.TopLeft);
                        }

                        yPoint += 20;

                        // Write rows of data
                        foreach (DataRowView row in dataView)
                        {
                            for (int i = 0; i < colCount; i++)
                            {
                                string cellText = row[i].ToString();
                                gfx.DrawString(cellText, font, XBrushes.Black,
                                    new XRect(30 + i * 100, yPoint, page.Width, page.Height),
                                    XStringFormats.TopLeft);
                            }
                            yPoint += 20;

                            // Add a new page if needed
                            if (yPoint > page.Height - 40)
                            {
                                page = document.AddPage();
                                gfx = XGraphics.FromPdfPage(page);
                                yPoint = 40;

                                // Redraw heading on each new page
                                gfx.DrawString("History", headingFont, XBrushes.Red,
                                    new XRect(30, yPoint, page.Width, page.Height),
                                    XStringFormats.TopLeft);

                                yPoint += 30;

                                // Redraw date/time on new page
                                gfx.DrawString(dateTimeText, font, XBrushes.Black,
                                    new XRect(0, 10, page.Width - 40, page.Height),
                                    XStringFormats.TopRight);

                                // Redraw column headers
                                for (int i = 0; i < colCount; i++)
                                {
                                    gfx.DrawString(dataView.Table.Columns[i].ColumnName, font, XBrushes.Black,
                                        new XRect(30 + i * 100, yPoint, page.Width, page.Height),
                                        XStringFormats.TopLeft);
                                }

                                yPoint += 20;
                            }
                        }

                        document.Save(saveFileDialog.FileName);
                        MessageBox.Show("PDF exported successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting PDF: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("No data to export.");
            }

        }

        private void history_tab_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            string connectionString = mainWindow.connectionString;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM dbo.deleted_user_history", con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            dt.Rows[i][0] = 1 + i;
                        }
                    }

                    UserDataGrid_1.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void search_btn_logbook_Click(object sender, RoutedEventArgs e)
        {
            string userId = LogUserIdTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(userId))
            {
                MessageBox.Show("Please enter a UserID.");
                return;
            }

            string connectionString = mainWindow.connectionString;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"SELECT LoginTime, LogoutTime FROM user_login_log WHERE UserID = @UserID ORDER BY LoginTime DESC";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        DataTable logTable = new DataTable();
                        adapter.Fill(logTable);

                        if (logTable.Rows.Count > 0)
                        {
                            UserLogDataGrid.ItemsSource = logTable.DefaultView;
                        }
                        else
                        {
                            MessageBox.Show("No log records found for this UserID.");
                            UserLogDataGrid.ItemsSource = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching log data: " + ex.Message);
            }
        }

        private void Export_btn_2_Click(object sender, RoutedEventArgs e)
        {
            if (UserLogDataGrid.ItemsSource is DataView dataView)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF file (*.pdf)|*.pdf",
                    Title = "Save PDF File"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        PdfDocument document = new PdfDocument();
                        document.Info.Title = "Exported User Table";

                        PdfPage page = document.AddPage();
                        XGraphics gfx = XGraphics.FromPdfPage(page);
                        XFont font = new XFont("Verdana", 10);
                        XFont headingFont = new XFont("Verdana", 14);
                        double yPoint = 40;

                        // Draw heading in bold red
                        gfx.DrawString("List of User", headingFont, XBrushes.Red,
                            new XRect(30, yPoint, page.Width, page.Height),
                            XStringFormats.TopCenter);

                        yPoint += 30; // Add space after heading

                        // Draw date and time at the top right corner
                        string dateTimeText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        gfx.DrawString(dateTimeText, font, XBrushes.Black,
                            new XRect(0, 10, page.Width - 40, page.Height),
                            XStringFormats.TopRight);

                        // Write header columns
                        int colCount = dataView.Table.Columns.Count;
                        for (int i = 0; i < colCount; i++)
                        {
                            gfx.DrawString(dataView.Table.Columns[i].ColumnName, font, XBrushes.Black,
                                new XRect(30 + i * 100, yPoint, page.Width, page.Height),
                                XStringFormats.TopLeft);
                        }

                        yPoint += 20;

                        // Write rows of data
                        foreach (DataRowView row in dataView)
                        {
                            for (int i = 0; i < colCount; i++)
                            {
                                string cellText = row[i].ToString();
                                gfx.DrawString(cellText, font, XBrushes.Black,
                                    new XRect(30 + i * 100, yPoint, page.Width, page.Height),
                                    XStringFormats.TopLeft);
                            }
                            yPoint += 20;

                            // Add a new page if needed
                            if (yPoint > page.Height - 40)
                            {
                                page = document.AddPage();
                                gfx = XGraphics.FromPdfPage(page);
                                yPoint = 40;

                                // Redraw heading on each new page
                                gfx.DrawString("Log Book", headingFont, XBrushes.Red,
                                    new XRect(30, yPoint, page.Width, page.Height),
                                    XStringFormats.TopLeft);

                                yPoint += 30;

                                // Redraw date/time on new page
                                gfx.DrawString(dateTimeText, font, XBrushes.Black,
                                    new XRect(0, 10, page.Width - 40, page.Height),
                                    XStringFormats.TopRight);

                                // Redraw column headers
                                for (int i = 0; i < colCount; i++)
                                {
                                    gfx.DrawString(dataView.Table.Columns[i].ColumnName, font, XBrushes.Black,
                                        new XRect(30 + i * 100, yPoint, page.Width, page.Height),
                                        XStringFormats.TopLeft);
                                }

                                yPoint += 20;
                            }
                        }

                        document.Save(saveFileDialog.FileName);
                        MessageBox.Show("PDF exported successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error exporting PDF: " + ex.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("No data to export.");
            }

        }

        private void list_of_user_tab_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
           
        }
    }
}
