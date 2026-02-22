using LibVLCSharp.Shared;

namespace AVP.Services;

public interface IMediaPlayerService
{
    MediaPlayer MediaPlayer { get; }
    bool IsPlaying { get; }
    long Duration { get; }
    long Position { get; }
    int Volume { get; set; }
    string? CurrentMediaPath { get; }

    void Load(string mediaPath);
    void Play();
    void Pause();
    void Stop();
    void SetPosition(float position);
}
