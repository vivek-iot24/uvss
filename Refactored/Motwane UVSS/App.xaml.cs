using Microsoft.Extensions.DependencyInjection;
using Motwane_UVSS.Application.Interfaces.DAL;
using Motwane_UVSS.Application.Interfaces.HAL;
using Motwane_UVSS.Application.Services;
using Motwane_UVSS.DAL.Repositories;
using Motwane_UVSS.HAL.Cameras;
using Motwane_UVSS.HAL.ExternalServices;
using Motwane_UVSS.HAL.Hardware;
using Motwane_UVSS.Presentation.Windows;
using System;
using OpenCvSharp;

using System.Windows;

namespace Motwane_UVSS.Presentation
{
    public partial class App : System.Windows.Application
    {
        private string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=UVSS_DB;Integrated Security=True;";

        // Change this to "Real" when hardware is available
        private const string HardwareMode = "Test";

        public string LoggedInUserID { get; set; }
        public string LoggedInUSERTYPE { get; set; }

        public IServiceProvider ServiceProvider { get; private set; }
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            ConfigureServices(services);

            ServiceProvider = services.BuildServiceProvider();
            Services = ServiceProvider;

            var mainWindow = ServiceProvider.GetRequiredService<Main_uvss_page>();
            mainWindow.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // ---------------- DAL ----------------

            services.AddSingleton<IUserRepository>(sp =>
                new UserRepository(connectionString));

            services.AddSingleton<IVideoRepository>(sp =>
                new VideoRepository(connectionString));

            services.AddSingleton<IVehicleEntryRepository>(sp =>
                new VehicleEntryRepository(connectionString));


            // ---------------- APPLICATION SERVICES ----------------

            services.AddSingleton<UserManagementService>();
            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<VehicleEntryService>();


            // ---------------- HAL SERVICES ----------------

            services.AddSingleton<IFileSystemService, FileSystemService>();

            if (HardwareMode == "Test")
            {
                services.AddSingleton<IDiagnosticService, FakeDiagnosticService>();
            }
            else
            {
                services.AddSingleton<IDiagnosticService, DiagnosticService>();
            }

            services.AddSingleton<IAicComparisonService, AicComparisonService>();


            // ---------------- CAMERA SERVICE SWITCH ----------------

            if (HardwareMode == "Test")
            {
                // Laptop webcam simulation
                services.AddSingleton<ICameraService, FakeLaptopCameraService>();
            }
            else
            {
                // Real FLIR / Spinnaker camera
                services.AddSingleton<ICameraService, UndersideCamera_HAL>();
            }


            // ---------------- WINDOWS ----------------

            services.AddSingleton<MainWindow>();

            services.AddTransient<Self_daignosis>();
            services.AddTransient<Main_uvss_page>();
            services.AddTransient<AicViewerWindow>();
            services.AddTransient<Menu_screen>();
            services.AddTransient<User_management_tab>();
            services.AddTransient<Video_Management_window>();
            services.AddTransient<System_settings_page>();
            services.AddTransient<Self_daignostic_window>();
            services.AddTransient<Report_management_tab>();
            services.AddTransient<About_screen>();
        }
    }
}