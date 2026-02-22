using AVP.ViewModels;
using System.Windows;

namespace AVP;

public partial class VideoWindow : Window
{
    public VideoWindow(VideoViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Connect the VideoView to the MediaPlayer
        VideoView.MediaPlayer = viewModel.MediaPlayer;
    }
}
