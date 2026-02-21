using AVP.Models;
using AVP.Services;
using AVP.ViewModels;
using AVP.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System.Windows;
using System;
using System.IO;

namespace AVP;

// Explicitly use System.Windows.Application to resolve ambiguity with System.Windows.Forms
public partial class App : System.Windows.Application
{
    private IHost? _host;
    private IConfiguration? _configuration;

    public App()
    {
        Log.Logger = SerilogFactory.CreateLogger();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var hostBuilder = Host.CreateDefaultBuilder(e.Args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    _configuration = context.Configuration;

                    // Bind AppSettings
                    services.Configure<AppSettings>(context.Configuration.GetSection("AppSettings"));

                    // Register Services
                    services.AddSingleton<IMediaPlayerService, LibVlcPlayerService>();
                    services.AddSingleton<IOscService, OscService>();

                    // Register ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<VideoViewModel>();

                    // Register Views
                    services.AddTransient<MainWindow>();
                    services.AddTransient<VideoWindow>();
                });

            _host = hostBuilder.Build();
            await _host.StartAsync();

            // Start OSC Service with configured port
            try
            {
                var appSettings = _host.Services.GetRequiredService<IOptions<AppSettings>>().Value;
                var oscService = _host.Services.GetRequiredService<IOscService>();
                oscService.Start(appSettings.OscPort);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to start OSC Service on startup.");
            }

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start correctly");
            System.Windows.MessageBox.Show($"Fatal Error: {ex.Message}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            var oscService = _host.Services.GetService<IOscService>();
            oscService?.Stop();

            await _host.StopAsync();
            _host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
