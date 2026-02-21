using AVP.ViewModels;
using System.Windows;
using LibVLCSharp.WPF;

namespace AVP.Views;

public partial class VideoWindow : Window
{
    public VideoWindow(VideoViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
