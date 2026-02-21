using AVP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using LibVLCSharp.Shared;

namespace AVP.ViewModels;

public partial class VideoViewModel : ObservableObject
{
    private readonly IMediaPlayerService _mediaPlayerService;

    public VideoViewModel(IMediaPlayerService mediaPlayerService)
    {
        _mediaPlayerService = mediaPlayerService;
    }

    public MediaPlayer? MediaPlayer => _mediaPlayerService.GetMediaPlayer();
}
