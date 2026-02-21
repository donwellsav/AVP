using System.Windows;
using System.Windows.Data;
using AVPlayer.Services;
using Microsoft.Extensions.Logging;

namespace AVPlayer.Views
{
    public partial class OutputWindow : Window
    {
        private readonly ILogger<OutputWindow> _logger;
        private readonly IMediaPlayerService _mediaService;

        public OutputWindow(ILogger<OutputWindow> logger, IMediaPlayerService mediaService)
        {
            InitializeComponent();
            _logger = logger;
            _mediaService = mediaService;

            _logger.LogInformation("OutputWindow initialized.");

            // Bind VideoViewD3D.VideoSource to MediaPlayerService.ActiveVideoSource
            var binding = new System.Windows.Data.Binding(nameof(IMediaPlayerService.ActiveVideoSource))
            {
                Source = _mediaService,
                Mode = BindingMode.OneWay
            };
            MainVideoView.SetBinding(VideoViewD3D.VideoSourceProperty, binding);
        }
    }
}
