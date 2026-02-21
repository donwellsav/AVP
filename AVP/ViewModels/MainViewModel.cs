using AVP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System.Threading.Tasks;

namespace AVP.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IMediaPlayerService _mediaPlayerService;

    [ObservableProperty]
    private string title = "AVP - Audio Video Playback";

    [ObservableProperty]
    private string mediaPath = string.Empty;

    public MainViewModel(IMediaPlayerService mediaPlayerService)
    {
        _mediaPlayerService = mediaPlayerService;
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
