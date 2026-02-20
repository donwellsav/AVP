using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace AVPlayer.Services
{
    public interface IMediaPlayerService : INotifyPropertyChanged
    {
        void Initialize();
        void Load(string path, bool isAudio = false);
        void Take();
        void Play();
        void Pause();
        void Stop();
        void SetPosition(float position);
        void SetVolume(int volume);

        WriteableBitmap? ActiveVideoSource { get; }
        TimeSpan CurrentTime { get; }
        TimeSpan TotalDuration { get; }
        bool IsPlaying { get; }
    }
}
