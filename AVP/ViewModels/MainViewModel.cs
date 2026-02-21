using AVP.Services;
using AVP.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading.Tasks;

namespace AVP.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly IServiceProvider _serviceProvider;
    private VideoWindow? _videoWindow;

    [ObservableProperty]
    private string title = "AVP - Audio Video Playback";

    [ObservableProperty]
    private string mediaPath = string.Empty;

    public MainViewModel(IMediaPlayerService mediaPlayerService, IServiceProvider serviceProvider)
    {
        _mediaPlayerService = mediaPlayerService;
        _serviceProvider = serviceProvider;
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

    [RelayCommand]
    private void OpenVideoWindow()
    {
        if (_videoWindow == null)
        {
            Log.Information("Opening Video Window.");
            // We use DI to get a new instance, which will also resolve VideoViewModel
            _videoWindow = _serviceProvider.GetRequiredService<VideoWindow>();

            // Handle closure to reset our reference
            _videoWindow.Closed += (s, e) => _videoWindow = null;

            _videoWindow.Show();
        }
        else
        {
            Log.Information("Video Window already open, activating.");
            _videoWindow.Activate();
        }
    }

    [RelayCommand]
    private void CloseVideoWindow()
    {
        if (_videoWindow != null)
        {
            Log.Information("Closing Video Window.");
            _videoWindow.Close();
            _videoWindow = null;
        }
    }
}
