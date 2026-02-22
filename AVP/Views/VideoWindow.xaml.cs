using AVP.Services;
using LibVLCSharp.Shared;
using System.Windows;

namespace AVP.Views;

public partial class VideoWindow : Window
{
    private readonly IMediaPlayerService _playerService;

    public VideoWindow(IMediaPlayerService playerService)
    {
        InitializeComponent();
        _playerService = playerService;

        // Attach MediaPlayer to VideoView
        if (_playerService != null)
        {
            VideoView.MediaPlayer = _playerService.MediaPlayer;
        }

        Closing += VideoWindow_Closing;
    }

    private void VideoWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // When closing, detach the media player to avoid disposing it if the service lives on
        VideoView.MediaPlayer = null;
    }
}
