using ClosedXML.Excel;
using Emgu.CV.XImgproc;
using Microsoft.Win32;
using Motwane_UVSS.Application.Services;
using Motwane_UVSS.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Motwane_UVSS.Presentation.Windows
{
    public partial class Report_management_tab : Window
    {
        private readonly VehicleEntryService _service;

        public ObservableCollection<VehicleEntryLogVM> VehicleEntries =
            new ObservableCollection<VehicleEntryLogVM>();

        public Report_management_tab(VehicleEntryService service)
        {
            InitializeComponent();
            filterTypeComboBox.ItemsSource = Enum.GetValues(typeof(DateFilterType));
            filterTypeComboBox.SelectedItem = DateFilterType.All;
            _service = service;
            LoadUsernames();
            dataGrid.ItemsSource = VehicleEntries;

            LoadData();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ReloadGridAsync();
        }
        private (DateTime from, DateTime to) Getdaterange()
        {
            DateTime from = fromDatePicker.SelectedDate ?? DateTime.Now;
            DateTime to = toDatePicker.SelectedDate ?? DateTime.Now;
            return (from, to);
        }
        private void LoadUsernames()
        {
            var users = _service.GetDistinctUsernames();

            usernameComboBox.ItemsSource = users;
        }
        private async void ReloadGridAsync()
        {
            VehicleEntries.Clear();

            dataGrid.IsEnabled = false;

            var range = Getdaterange();

            string user = usernameComboBox.SelectedItem as string;
            string plate = numberplateTextBox.Text;

            var entries = await Task.Run(() =>
                _service.GetEntries(range.Item1, range.Item2, user, plate));

            foreach (var model in entries)
            {
                VehicleEntries.Add(new VehicleEntryLogVM
                {
                    SrNo = model.SrNo,
                    Username = model.Username,
                    EntryDate = model.EntryDate,
                    EntryTime = model.EntryTime,
                    Status = model.Status,
                    Remark = model.Remark,
                    Numberplate = model.Numberplate,
                    UndersideBytes = model.UndersideBytes,
                    DriverCamBytes = model.DriverCamBytes,
                    AnprBytes = model.AnprBytes
                });
            }

            dataGrid.IsEnabled = true;
        }
        public enum DateFilterType
        {
            All,
            LastDay,
            LastWeek,
            LastMonth,
            SpecificDate,
            CustomRange
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            filterTypeComboBox.SelectedItem = DateFilterType.All;

            fromDatePicker.SelectedDate = null;
            toDatePicker.SelectedDate = null;

            usernameComboBox.SelectedItem = null;
            numberplateTextBox.Clear();

            VehicleEntries.Clear();
           
            ReloadGridAsync();
        }

        private void Exit_button_3_Click(object sender, RoutedEventArgs e)
        {
            Menu_screen menu = new Menu_screen();
            menu.Show();
            Close();
        }

        private async void Export_btn_1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Export button clicked.");
        }

        private void FilterTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            fromDatePicker.Visibility = Visibility.Collapsed;
            toDatePicker.Visibility = Visibility.Collapsed;
            fromTextBlock.Visibility = Visibility.Collapsed;
            toTextBlock.Visibility = Visibility.Collapsed;
            DateFilterType type = (DateFilterType)filterTypeComboBox.SelectedItem;
            if (type == DateFilterType.SpecificDate)
            {
                fromDatePicker.Visibility = Visibility.Visible;
                fromTextBlock.Visibility = Visibility.Visible;
            }
            if (type == DateFilterType.CustomRange)
            {
                fromDatePicker.Visibility = Visibility.Visible;
                toDatePicker.Visibility = Visibility.Visible;
                fromTextBlock.Visibility = Visibility.Visible;
                toTextBlock.Visibility = Visibility.Visible;
            }
        }
        private void LoadData()
        {
            var data = _service.GetEntries(null, null, null, null);

            VehicleEntries.Clear();

            foreach (var model in data)
            {
                VehicleEntries.Add(new VehicleEntryLogVM
                {
                    SrNo = model.SrNo,
                    Username = model.Username,
                    EntryDate = model.EntryDate,
                    EntryTime = model.EntryTime,
                    Status = model.Status,
                    Remark = model.Remark,
                    Numberplate = model.Numberplate,
                    UndersideBytes = model.UndersideBytes,
                    DriverCamBytes = model.DriverCamBytes,
                    AnprBytes = model.AnprBytes
                });
            }
        }
    }
}