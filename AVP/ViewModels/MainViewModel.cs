using AVP.Services;
using AVP.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Rug.Osc;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace AVP.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly IOscService _oscService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string title = "AVP - Audio Video Playback";

    [ObservableProperty]
    private string mediaPath = string.Empty;

    [ObservableProperty]
    private int oscPort = 8000;

    [ObservableProperty]
    private bool isOscRunning;

    public MainViewModel(IMediaPlayerService mediaPlayerService, IOscService oscService, IServiceProvider serviceProvider)
    {
        _mediaPlayerService = mediaPlayerService;
        _oscService = oscService;
        _serviceProvider = serviceProvider;

        _oscService.MessageReceived += OnOscMessageReceived;

        // Auto-start OSC
        StartOsc();
    }

    private void OnOscMessageReceived(object? sender, OscMessage e)
    {
        Log.Information("OSC Message Received: {OscAddress}", e.Address);

        Application.Current.Dispatcher.Invoke(() =>
        {
            switch (e.Address)
            {
                case "/play":
                    Play();
                    break;
                case "/pause":
                    Pause();
                    break;
                case "/stop":
                    Stop();
                    break;
                case "/load":
                    if (e.Count > 0 && e[0] is string path)
                    {
                        MediaPath = path;
                        _mediaPlayerService.Load(path);
                        _mediaPlayerService.Play();
                    }
                    break;
            }
        });
    }

    private bool CanStartOsc() => !IsOscRunning;
    private bool CanStopOsc() => IsOscRunning;

    [RelayCommand(CanExecute = nameof(CanStartOsc))]
    private void StartOsc()
    {
        if (_oscService.Start(OscPort))
        {
            IsOscRunning = true;
            StartOscCommand.NotifyCanExecuteChanged();
            StopOscCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanStopOsc))]
    private void StopOsc()
    {
        _oscService.Stop();
        IsOscRunning = false;
        StartOscCommand.NotifyCanExecuteChanged();
        StopOscCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    private void OpenVideoWindow()
    {
        try
        {
            var window = _serviceProvider.GetRequiredService<VideoWindow>();
            window.Show();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open Video Window");
        }
    }

    [RelayCommand]
    private void Play()
    {
        Log.Information("Play command executed.");

        if (!string.IsNullOrEmpty(MediaPath))
        {
             _mediaPlayerService.Load(MediaPath);
        }

        _mediaPlayerService.Play();
    }

    [RelayCommand]
    private void Pause()
    {
        Log.Information("Pause command executed.");
        _mediaPlayerService.Pause();
    }

    [RelayCommand]
    private void Stop()
    {
        Log.Information("Stop command executed.");
        _mediaPlayerService.Stop();
    }
}
