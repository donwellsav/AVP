using AVP.Models;
using AVP.Services;
using AVP.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AVP.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IMediaPlayerService _mediaPlayerService;
    private readonly IOscService _oscService;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppSettings _appSettings;
    private VideoWindow? _videoWindow;

    [ObservableProperty]
    private string title = "AVP - Audio Video Playback";

    [ObservableProperty]
    private string mediaPath = string.Empty;

    [ObservableProperty]
    private int oscPort;

    [ObservableProperty]
    private string mediaDirectory = string.Empty;

    public MainViewModel(
        IMediaPlayerService mediaPlayerService,
        IOscService oscService,
        IServiceProvider serviceProvider,
        IOptions<AppSettings> appSettings)
    {
        _mediaPlayerService = mediaPlayerService;
        _oscService = oscService;
        _serviceProvider = serviceProvider;
        _appSettings = appSettings.Value;

        OscPort = _appSettings.OscPort;
        MediaDirectory = _appSettings.MediaDirectory;

        if (Directory.Exists(MediaDirectory))
        {
            Log.Information($"Media Directory found: {MediaDirectory}");
        }
    }

    partial void OnOscPortChanged(int value)
    {
        Log.Information($"OSC Port changed to {value}. Restarting service...");
        _oscService.Stop();
        _oscService.Start(value);
        _appSettings.OscPort = value;
    }

    [RelayCommand]
    private void BrowseMediaDirectory()
    {
        using (var dialog = new FolderBrowserDialog())
        {
            dialog.Description = "Select Media Directory";
            dialog.UseDescriptionForTitle = true;
            dialog.SelectedPath = MediaDirectory;

            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                MediaDirectory = dialog.SelectedPath;
                _appSettings.MediaDirectory = MediaDirectory;
                Log.Information($"Media Directory updated: {MediaDirectory}");
            }
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

    [RelayCommand]
    private void OpenVideoWindow()
    {
        if (_videoWindow == null)
        {
            Log.Information("Opening Video Window.");
            _videoWindow = _serviceProvider.GetRequiredService<VideoWindow>();
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
