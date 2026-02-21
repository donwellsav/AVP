using Wpf.Ui.Controls;
using AVPlayer.ViewModels;

namespace AVPlayer
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
