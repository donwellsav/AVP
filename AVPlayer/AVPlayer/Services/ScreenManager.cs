using System;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using AVPlayer.Views;
using Screen = System.Windows.Forms.Screen;

namespace AVPlayer.Services
{
    public class ScreenManager : IScreenManager
    {
        private readonly ILogger<ScreenManager> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Window? _outputWindow;

        public event EventHandler? OutputWindowClosed;

        public ScreenManager(ILogger<ScreenManager> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public void Initialize()
        {
            SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
            _logger.LogInformation("ScreenManager initialized. Monitoring display changes.");
            // Explicitly use System.Windows.Application
            System.Windows.Application.Current.Dispatcher.Invoke(() => ShowOutputWindow());
        }

        private void OnDisplaySettingsChanged(object? sender, EventArgs e)
        {
            _logger.LogInformation("Display settings changed detected.");
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var screens = Screen.AllScreens;
                if (screens.Length < 2 && _outputWindow != null)
                {
                    _logger.LogWarning("Secondary display disconnected (Screens: {Count}). Closing Output Window.", screens.Length);
                    CloseOutputWindow();
                }
            });
        }

        public void ShowOutputWindow()
        {
            if (_outputWindow != null)
            {
                _outputWindow.Activate();
                return;
            }

            var screens = Screen.AllScreens;
            _logger.LogInformation("Detected {Count} screens.", screens.Length);

            if (screens.Length > 1)
            {
                var secondaryScreen = screens.FirstOrDefault(s => !s.Primary) ?? screens[1];
                var bounds = secondaryScreen.Bounds;

                // Resolve from DI
                var window = _serviceProvider.GetService<OutputWindow>();

                if (window != null)
                {
                    _outputWindow = window;

                    _outputWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                    _outputWindow.Left = bounds.Left;
                    _outputWindow.Top = bounds.Top;
                    _outputWindow.Width = bounds.Width;
                    _outputWindow.Height = bounds.Height;

                    _outputWindow.Closed += (s, e) =>
                    {
                        _outputWindow = null;
                        OutputWindowClosed?.Invoke(this, EventArgs.Empty);
                    };

                    _outputWindow.Show();
                    _logger.LogInformation("Output Window opened on Screen {DeviceName} at {Bounds}", secondaryScreen.DeviceName, bounds);
                }
                else
                {
                    _logger.LogError("Failed to resolve OutputWindow from service provider.");
                }
            }
            else
            {
                _logger.LogInformation("Only one screen detected. Output Window not opened.");
            }
        }

        public void CloseOutputWindow()
        {
            if (_outputWindow != null)
            {
                _outputWindow.Close();
                _outputWindow = null;
                _logger.LogInformation("Output Window closed.");
            }
        }
    }
}
