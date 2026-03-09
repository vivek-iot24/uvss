using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;


namespace Motwane_UVSS
{
    /// <summary>
    /// Interaction logic for Report_management_tab.xaml
    /// </summary>
    public partial class Report_management_tab : Window
    {
        public Report_management_tab()
        {
            InitializeComponent();

            LoadUsernames();
            dataGrid.ItemsSource = GetVehicleEntries();
           
        }
        private void LoadUsernames()
        {
            string connectionString = mainWindow.connectionString;
            var usernames = new List<string>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT username FROM vehicle_entry_log", conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usernames.Add(reader.GetString(0));
                    }
                }
            }

            usernameComboBox.ItemsSource = usernames;
            usernameComboBox.SelectedIndex = -1; // No selection by default
        }

        public BitmapImage ByteArrayToImage(byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
                return null;

            using (var stream = new MemoryStream(byteArray))
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return image;
            }
        }

        MainWindow mainWindow = new MainWindow();

        public List<VehicleEntryLog> GetVehicleEntries(DateTime? from = null, DateTime? to = null, string username = null, string numberplate = null)
        {
            var entries = new List<VehicleEntryLog>();
            string connectionString = mainWindow.connectionString;

            //var query = new StringBuilder("SELECT * FROM vehicle_entry_log WHERE 1=1");
            var query = new StringBuilder("Select * from vehicle_entry_log order by entry_date desc");

            var parameters = new List<SqlParameter>();

            if (from.HasValue)
            {
                query.Append(" AND entry_date >= @fromDate");
                parameters.Add(new SqlParameter("@fromDate", from.Value.Date));
            }

            if (to.HasValue)
            {
                query.Append(" AND entry_date <= @toDate");
                parameters.Add(new SqlParameter("@toDate", to.Value.Date));
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                query.Append(" AND username = @username");
                parameters.Add(new SqlParameter("@username", username));
            }

            if (!string.IsNullOrWhiteSpace(numberplate))
            {
                query.Append(" AND numberplate LIKE @numberplate");
                parameters.Add(new SqlParameter("@numberplate", "%" + numberplate + "%"));
            }

            int srr = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query.ToString(), conn))
            {
                cmd.Parameters.AddRange(parameters.ToArray());

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    
                    while (reader.Read())
                    {
                        srr++;
                        entries.Add(new VehicleEntryLog
                        {

                            //SrNo = reader.GetInt32(reader.GetOrdinal("sr_no")),
                            SrNo = srr,
                            Username = reader["username"] as string,
                            EntryDate = reader["entry_date"] as DateTime?,
                            EntryTime = reader["entry_time"] as TimeSpan?,
                            Status = reader["status"] as string,
                            Remark = reader["remark"] as string,
                            Numberplate = reader["numberplate"] as string,
                            UndersideImage = ByteArrayToImage(reader["underside_image"] as byte[]),
                            DriverCamImage = ByteArrayToImage(reader["driver_cam_image"] as byte[]),
                            AnprImage = ByteArrayToImage(reader["anpr_image"] as byte[])
                            
                        });
                    }
                }
            }

            return entries;
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is VehicleEntryLog entry)
            {
                // Pass the row data to your class 'maiiik'
                Main_uvss_page handler = new Main_uvss_page(entry);
                handler.Show();
                
            }
            this.Close();
        }


        private void Exit_button_3_Click(object sender, RoutedEventArgs e)
        {
            Menu_screen menu_Screen = new Menu_screen();
            menu_Screen.Show();
            this.Close();
        }

        private void Export_btn_1_Click(object sender, RoutedEventArgs e)
        {
            var entries = dataGrid.ItemsSource as List<VehicleEntryLog>;
            if (entries == null || entries.Count == 0)
            {
                System.Windows.MessageBox.Show("No data to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Workbook|*.xlsx",
                Title = "Save as Excel File",
                FileName = "VehicleEntryReport.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Vehicle Entries");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Sr No";
                    worksheet.Cell(1, 2).Value = "Username";
                    worksheet.Cell(1, 3).Value = "Entry Date";
                    worksheet.Cell(1, 4).Value = "Entry Time";
                    worksheet.Cell(1, 5).Value = "Status";
                    worksheet.Cell(1, 6).Value = "Remark";
                    worksheet.Cell(1, 7).Value = "Numberplate";
                    worksheet.Cell(1, 8).Value = "Underside Image";
                    worksheet.Cell(1, 9).Value = "Driver Cam Image";
                    worksheet.Cell(1, 10).Value = "ANPR Image";

                    int row = 2;
                    foreach (var entry in entries)
                    {
                        worksheet.Cell(row, 1).Value = entry.SrNo;
                        worksheet.Cell(row, 2).Value = entry.Username;
                        worksheet.Cell(row, 3).Value = entry.EntryDate?.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 4).Value = entry.EntryTime?.ToString();
                        worksheet.Cell(row, 5).Value = entry.Status;
                        worksheet.Cell(row, 6).Value = entry.Remark;
                        worksheet.Cell(row, 7).Value = entry.Numberplate;

                        if (entry.UndersideImage != null)
                            AddImageToExcel(entry.UndersideImage, worksheet, row, 8);

                        if (entry.DriverCamImage != null)
                            AddImageToExcel(entry.DriverCamImage, worksheet, row, 9);

                        if (entry.AnprImage != null)
                            AddImageToExcel(entry.AnprImage, worksheet, row, 10);

                        row++;
                    }

                    workbook.SaveAs(saveDialog.FileName);
                    System.Windows.MessageBox.Show("Data exported successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

        }
        private void AddImageToExcel(BitmapImage bitmapImage, IXLWorksheet worksheet, int row, int column)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);

                using (var original = new System.Drawing.Bitmap(ms))
                using (var resized = new System.Drawing.Bitmap(original, new System.Drawing.Size(150, 100)))
                using (var outputStream = new MemoryStream())
                {
                    resized.Save(outputStream, System.Drawing.Imaging.ImageFormat.Png);
                    outputStream.Seek(0, SeekOrigin.Begin);

                    var picture = worksheet.AddPicture(outputStream)
                                           .MoveTo(worksheet.Cell(row, column))
                                           .WithSize(150, 100); // ✅ Set image size to 150x100 px

                    // Adjust Excel cell size to match (approximate)
                    worksheet.Row(row).Height = 75;        // ~100px height
                    worksheet.Column(column).Width = 26;   // ~150px width
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime? fromDate = fromDatePicker.SelectedDate;
            DateTime? toDate = toDatePicker.SelectedDate;
            string selectedUser = usernameComboBox.SelectedItem as string;
            string numberplate = numberplateTextBox.Text;

            dataGrid.ItemsSource = GetVehicleEntries(fromDate, toDate, selectedUser, numberplate);
        }
    }

    public class VehicleEntryLog
    {
        public int SrNo { get; set; }
        public string Username { get; set; }
        public DateTime? EntryDate { get; set; }
        public TimeSpan? EntryTime { get; set; }
        public string Status { get; set; }
        public string Remark { get; set; }
        public string Numberplate { get; set; }

        public BitmapImage UndersideImage { get; set; }
        public BitmapImage DriverCamImage { get; set; }
        public BitmapImage AnprImage { get; set; }
    }

}
