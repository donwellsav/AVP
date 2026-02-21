namespace AVP.Services;

public interface IMediaPlayerService
{
    bool IsPlaying { get; }
    long Duration { get; }
    long Position { get; }
    int Volume { get; set; }

    void Load(string mediaPath);
    void Play();
    void Pause();
    void Stop();
    void SetPosition(float position);
}
