using System.Windows;
using AVPlayer.Services;
using Microsoft.Extensions.Logging;

namespace AVPlayer.Views
{
    public partial class OutputWindow : Window
    {
        private readonly ILogger<OutputWindow> _logger;

        public OutputWindow(ILogger<OutputWindow> logger)
        {
            InitializeComponent();
            _logger = logger;
            _logger.LogInformation("OutputWindow initialized.");
        }
    }
}
