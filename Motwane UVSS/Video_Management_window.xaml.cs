using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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

namespace Motwane_UVSS
{
    /// <summary>
    /// Interaction logic for Video_Management_window.xaml
    /// </summary>
    public partial class Video_Management_window : Window
    {
        MainWindow mainWindow = new MainWindow();
        private bool dataLoaded = false;

        public Video_Management_window()
        {
            InitializeComponent();

            Loaded += Window_Loaded;
        }

        private void LoadVideoData()
        {
            string connectionString = mainWindow.connectionString;
            var data = new List<VideoRecord>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM video_management_table";
                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        data.Add(new VideoRecord
                        {
                            id = reader["id"] != DBNull.Value ? (int)reader["id"] : 0,
                            vehicle_number = reader["vehicle_number"]?.ToString(),
                            capture_date = reader["capture_date"] != DBNull.Value ? (DateTime)reader["capture_date"] : DateTime.MinValue,
                            capture_time = reader["capture_time"] != DBNull.Value ? (TimeSpan)reader["capture_time"] : TimeSpan.Zero,
                            video1_path = reader["video1_path"]?.ToString(),
                            video2_path = reader["video2_path"]?.ToString(),
                            video3_path = reader["video3_path"]?.ToString(),
                            vehicle_image = reader["vehicle_image"] as byte[]
                        });
                    }
                }
            }

            VideoDataGrid.ItemsSource = data;
        }
        private void Exit_button_Click(object sender, RoutedEventArgs e)
        {
            Menu_screen menu_Screen = new Menu_screen();
            menu_Screen.Show();

            this.Close();
        }

        bool a = true;
        private void show_btn_Click(object sender, RoutedEventArgs e)
        {
            if (a == true)
            {
                if (VideoDataGrid.SelectedItem is VideoRecord record)
                {
                    // Set text values
                    txtVehicleNumber.Text = $"Vehicle: {record.vehicle_number}";
                    txtDate.Text = $"Date: {record.capture_date:yyyy-MM-dd}";
                    txtTime.Text = $"Time: {record.capture_time}";

                    // Set image
                   // imgVehicle.Source = record.VehicleImage;

                    // Set video sources
                    media1.Source = new Uri(record.video1_path, UriKind.Absolute);
                    media2.Source = new Uri(record.video2_path, UriKind.Absolute);
                    media3.Source = new Uri(record.video3_path, UriKind.Absolute);

                    // Reset and play
                    media1.SpeedRatio = 1.0;
                    media2.SpeedRatio = 1.0;
                    media3.SpeedRatio = 1.0;

                    media1.Play();
                    media2.Play();
                    media3.Play();
                }

                show_btn.Content = "Back";
                report_grid.Visibility = Visibility.Collapsed;
                video_grid.Visibility = Visibility.Visible;
                a = false;
            }
            else if (a == false)
            {
                show_btn.Content = "Show";
                report_grid.Visibility = Visibility.Visible;
                video_grid.Visibility = Visibility.Collapsed;
                a = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!dataLoaded)
            {
                LoadVideoData();
                dataLoaded = true;
            }
        }
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            media1.Play();
            media2.Play();
            media3.Play();
        }

        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            media1.Pause();
            media2.Pause();
            media3.Pause();
        }

        private void Fast_Click(object sender, RoutedEventArgs e)
        {
            //media1.SpeedRatio = 2.0;
            //media2.SpeedRatio = 2.0;
            //media3.SpeedRatio = 2.0;

            double newSpeed = media1.SpeedRatio + 0.5;
            if (newSpeed <= 5.0)
            {
                media1.SpeedRatio = newSpeed;
                media2.SpeedRatio = newSpeed;
                media3.SpeedRatio = newSpeed;
            }
        }

        private void Slow_Click(object sender, RoutedEventArgs e)
        {
            //media1.SpeedRatio = 0.5;
            //media2.SpeedRatio = 0.5;
            //media3.SpeedRatio = 0.5;

            double newSpeed = media1.SpeedRatio - 0.5;
            if (newSpeed >= 0.1)
            {
                media1.SpeedRatio = newSpeed;
                media2.SpeedRatio = newSpeed;
                media3.SpeedRatio = newSpeed;
            }
        }

        private void Replay_Click(object sender, RoutedEventArgs e)
        {
            media1.Position = TimeSpan.Zero;
            media2.Position = TimeSpan.Zero;
            media3.Position = TimeSpan.Zero;

            media1.Play();
            media2.Play();
            media3.Play();
        }

    }

    public class VideoRecord
    {
        public int id { get; set; }
        public string vehicle_number { get; set; }
        public DateTime capture_date { get; set; }
        public TimeSpan capture_time { get; set; }
        public string video1_path { get; set; }
        public string video2_path { get; set; }
        public string video3_path { get; set; }
        public byte[] vehicle_image { get; set; }

        public BitmapImage VehicleImage
        {
            get
            {
                if (vehicle_image == null || vehicle_image.Length == 0) return null;
                try
                {
                    var ms = new MemoryStream(vehicle_image);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = ms;
                    image.EndInit();
                    return image;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
