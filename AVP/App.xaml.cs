using AVP.Services;
using AVP.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Windows;
using System;

namespace AVP;

public partial class App : System.Windows.Application
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
                    services.AddSingleton<IWindowService, WindowService>();

                    // Register ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<VideoViewModel>();

                    // Register Views
                    services.AddTransient<MainWindow>();
                    services.AddTransient<VideoWindow>();
                });

            _host = hostBuilder.Build();
            await _host.StartAsync();

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
            await _host.StopAsync();
            _host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
