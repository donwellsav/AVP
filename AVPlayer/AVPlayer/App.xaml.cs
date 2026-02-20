using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Windows;
using AVPlayer.Services;
using AVPlayer.ViewModels;

namespace AVPlayer
{
    public partial class App : Application
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

            var builder = Host.CreateApplicationBuilder();

            // Register Services
            builder.Services.AddSingleton<IMediaPlayerService, MediaPlayerService>();

            // Register ViewModels
            builder.Services.AddSingleton<MainViewModel>();

            // Register Views
            builder.Services.AddSingleton<MainWindow>();

            // Logging
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();

            _host = builder.Build();
            await _host.StartAsync();

            var mediaService = _host.Services.GetRequiredService<IMediaPlayerService>();
            mediaService.Initialize();

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
            base.OnExit(e);
            Log.CloseAndFlush();
        }
    }
}
