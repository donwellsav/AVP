using System;

namespace AVPlayer.Services
{
    public interface IScreenManager
    {
        void Initialize();
        void ShowOutputWindow();
        void CloseOutputWindow();
        event EventHandler OutputWindowClosed;
    }
}
