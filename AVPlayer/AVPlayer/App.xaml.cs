using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;
using AVPlayer.Services;
using AVPlayer.ViewModels;
using AVPlayer.Views;

namespace AVPlayer
{
    public partial class App : System.Windows.Application
    {
        private IHost? _host;

        public App()
        {
            // Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global Exception Handling
            DispatcherUnhandledException += (s, args) =>
            {
                Log.Error(args.Exception, "Unhandled UI Exception");
                System.Windows.MessageBox.Show($"An unhandled error occurred: {args.Exception.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                args.Handled = true; // Attempt to keep running
            };

            AppDomain.CurrentDomain.UnhandledException += (s, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                Log.Fatal(ex, "Unhandled AppDomain Exception");
                System.Windows.MessageBox.Show($"A fatal error occurred: {ex?.Message}", "Fatal Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            };

            var builder = Host.CreateApplicationBuilder();

            // Register Services
            builder.Services.AddSingleton<IMediaPlayerService, MediaPlayerService>();
            builder.Services.AddSingleton<IScreenManager, ScreenManager>();
            builder.Services.AddSingleton<IShowfileService, ShowfileService>();
            builder.Services.AddSingleton<IOSLockService, OSLockService>();
            builder.Services.AddSingleton<IOscService, OscService>();

            // Register ViewModels
            builder.Services.AddSingleton<MainViewModel>();

            // Register Views
            builder.Services.AddSingleton<MainWindow>();
            builder.Services.AddTransient<OutputWindow>();

            // Logging
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();

            _host = builder.Build();
            await _host.StartAsync();

            var mediaService = _host.Services.GetRequiredService<IMediaPlayerService>();
            mediaService.Initialize();

            var screenManager = _host.Services.GetRequiredService<IScreenManager>();
            screenManager.Initialize();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            // Allow sleep on exit (if not crashed)
            // But OSLockService is singleton, we should probably resolve it and call AllowSleep
            // Or rely on OS cleaning up process handles.

            base.OnExit(e);
            Log.CloseAndFlush();
        }
    }
}
