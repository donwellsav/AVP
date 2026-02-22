using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace AVP.Services;

public class WindowService : IWindowService
{
    private readonly IServiceProvider _serviceProvider;
    private Window? _videoWindow;

    public WindowService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void ShowVideoWindow()
    {
        if (_videoWindow == null || !_videoWindow.IsLoaded)
        {
            _videoWindow = _serviceProvider.GetRequiredService<VideoWindow>();
            _videoWindow.Closed += (s, e) => _videoWindow = null;
            _videoWindow.Show();
        }
        else
        {
            _videoWindow.Activate();
        }
    }

    public void CloseVideoWindow()
    {
        if (_videoWindow != null)
        {
            _videoWindow.Close();
            _videoWindow = null;
        }
    }
}
