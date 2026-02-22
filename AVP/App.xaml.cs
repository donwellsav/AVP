using AVP.Services;
using AVP.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Windows;
using System;

namespace AVP;

public partial class App : Application
{
    private IHost? _host;

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
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    // Register Services
                    // Using Singleton for MediaPlayerService as we likely want one playback engine instance
                    services.AddSingleton<IMediaPlayerService, LibVlcPlayerService>();

                    // OSC Service for remote control
                    services.AddSingleton<IOscService, OscService>(sp =>
                        new OscService(sp.GetRequiredService<IMediaPlayerService>(), 8000));

                    // Register ViewModels
                    services.AddTransient<MainViewModel>();

                    // Register Views
                    services.AddTransient<MainWindow>();
                });

            _host = hostBuilder.Build();
            await _host.StartAsync();

            // Start OSC Service
            _host.Services.GetRequiredService<IOscService>().Start();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application failed to start correctly");
            MessageBox.Show($"Fatal Error: {ex.Message}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            // Stop OSC Service
            try
            {
                _host.Services.GetService<IOscService>()?.Stop();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error stopping OSC Service");
            }

            await _host.StopAsync();
            _host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
