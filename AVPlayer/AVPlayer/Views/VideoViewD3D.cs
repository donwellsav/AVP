using System.Windows;
using System.Windows.Controls;

namespace AVPlayer.Views
{
    public class VideoViewD3D : System.Windows.Controls.Control
    {
        static VideoViewD3D()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VideoViewD3D), new FrameworkPropertyMetadata(typeof(VideoViewD3D)));
        }

        public static readonly DependencyProperty VideoSourceProperty =
            DependencyProperty.Register(nameof(VideoSource), typeof(System.Windows.Media.ImageSource), typeof(VideoViewD3D), new PropertyMetadata(null));

        public System.Windows.Media.ImageSource VideoSource
        {
            get { return (System.Windows.Media.ImageSource)GetValue(VideoSourceProperty); }
            set { SetValue(VideoSourceProperty, value); }
        }
    }
}
