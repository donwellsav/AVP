using CommunityToolkit.Mvvm.ComponentModel;

namespace AVP.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "AVP - Audio Video Playback";

    public MainViewModel()
    {
    }
}
