using Microsoft.Extensions.DependencyInjection;
using Motwane.UVSS;
using Motwane.UVSS.Application.ComputerVision;
using Motwane.UVSS.Application.Interfaces.DAL;
using Motwane.UVSS.Application.Interfaces.HAL;
using Motwane.UVSS.Application.Services;
using Motwane.UVSS.DAL.Repositories;
using Motwane.UVSS.Infrastructure.HAL.Cameras;
using Motwane.UVSS.Infrastructure.HAL.ExternalServices;
using Motwane.UVSS.Infrastructure.HAL.Hardware;
using Motwane.UVSS.Presentation.Windows;
using Motwane.UVSS.ViewModels;
using System;
using System.Windows;

namespace Motwane.UVSS.Presentation
{
    public partial class App : System.Windows.Application
    {
        string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=UVSS_USER_DETAILS;Integrated Security=True;";
        public const string HardwareMode = "Test";
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

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            var mediaService = new MediaFolderService();
            mediaService.EnsureStructure();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // DAL 
            services.AddSingleton<IUserRepository>(sp =>
                new UserRepository(connectionString));

            services.AddSingleton<IVideoRepository>(sp =>
                new VideoRepository(connectionString));

            services.AddSingleton<IVehicleEntryRepository>(sp =>
                new VehicleEntryRepository(connectionString));

            // Application Services
            services.AddSingleton<UserManagementService>();
            services.AddSingleton<AuthenticationService>();
            services.AddSingleton<VehicleEntryService>();
            services.AddSingleton<AnprEngine>(sp =>
            {
                var basePath = AppDomain.CurrentDomain.BaseDirectory;

                var cvDir = System.IO.Path.Combine(basePath,
                    @"..\..\..\..\Motwane.UVSS.Application\ComputerVision");

                var outputDir = System.IO.Path.Combine(basePath, "AnprOutput");

                return new AnprEngine(
                    System.IO.Path.Combine(cvDir, "best_plate.onnx"),
                    System.IO.Path.Combine(cvDir, "encoder.onnx"),
                    System.IO.Path.Combine(cvDir, "decoder.onnx"),
                    System.IO.Path.Combine(cvDir, "vocab.json"),
                    outputDir
                );
            });

            // HAL Services
            services.AddSingleton<IFileSystemService, FileSystemService>();
            services.AddSingleton<IDiagnosticService, DiagnosticService>();
            services.AddSingleton<IAicComparisonService, AicComparisonService>();


            // CAMERA SWITCH
            if (HardwareMode == "Test")
            {
                services.AddSingleton<ICameraService, FakeLaptopCameraService>();
                // ADD THIS
                services.AddSingleton<Underside_cam_class>();
            }
            else
            {
                services.AddSingleton<DiagnosticService>();
                services.AddSingleton<Underside_cam_class>();
            }


            // Windows
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