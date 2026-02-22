using AVP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using LibVLCSharp.Shared;

namespace AVP.ViewModels;

public partial class VideoViewModel : ObservableObject
{
    private readonly IMediaPlayerService _mediaPlayerService;

    public MediaPlayer? MediaPlayer => _mediaPlayerService.MediaPlayer;

    public VideoViewModel(IMediaPlayerService mediaPlayerService)
    {
        _mediaPlayerService = mediaPlayerService;
    }
}
