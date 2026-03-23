using Motwane.UVSS.Application.Interfaces.DAL;
using Motwane.UVSS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Motwane.UVSS.Presentation.Windows
{
    public partial class Video_Management_window : Window
    {
        private readonly IVideoRepository videoRepository;

        MainWindow mainWindow = new MainWindow();

        private bool dataLoaded = false;

        public Video_Management_window(IVideoRepository repository)
        {
            InitializeComponent();

            videoRepository = repository;

            Loaded += Window_Loaded;
        }

        private void LoadVideoData()
        {
            List<VideoRecord> data = videoRepository.GetAllVideos();

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
                    txtVehicleNumber.Text = $"Vehicle: {record.vehicle_number}";
                    txtDate.Text = $"Date: {record.capture_date:yyyy-MM-dd}";
                    txtTime.Text = $"Time: {record.capture_time}";

                    media1.Source = new Uri(record.video1_path, UriKind.Absolute);
                    media2.Source = new Uri(record.video2_path, UriKind.Absolute);
                    media3.Source = new Uri(record.video3_path, UriKind.Absolute);

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
            else
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
}