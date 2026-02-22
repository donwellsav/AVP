using LibVLCSharp.Shared;
using Serilog;

namespace AVP.Services;

public class MediaPlayerService : IMediaPlayerService, IDisposable
{
    private LibVLC _libVlc;
    private MediaPlayer _mediaPlayer;
    private bool _disposed;

    // Use property with custom getter/setter for Volume as defined in interface
    public int Volume
    {
        get => _mediaPlayer?.Volume ?? 0;
        set { if (_mediaPlayer != null) _mediaPlayer.Volume = value; }
    }

    // Properties implementation
    public MediaPlayer MediaPlayer => _mediaPlayer;
    public bool IsPlaying => _mediaPlayer?.IsPlaying ?? false;
    public long Duration => _mediaPlayer?.Length ?? 0;
    public long Position => (long)(_mediaPlayer?.Position ?? 0 * (_mediaPlayer?.Length ?? 0));

    public MediaPlayerService()
    {
        Log.Information("Initializing LibVLC Service...");

        try
        {
            // Initialize Core. Requires LibVLC binaries to be present or located.
            Core.Initialize();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to initialize LibVLC Core. Ensure LibVLC binaries are available.");
            // In a real scenario, we might want to fail gracefully or show a message,
            // but for now, rethrowing or handling as critical error is appropriate.
        }

        // Enable hardware decoding using --avcodec-hw=any
        // This is a crucial requirement for performance.
        _libVlc = new LibVLC("--avcodec-hw=any");

        _mediaPlayer = new MediaPlayer(_libVlc);

        // Subscribe to events for logging/debugging
        _mediaPlayer.LengthChanged += (sender, e) => Log.Debug($"Media Duration Changed: {e.Length} ms");
        _mediaPlayer.EndReached += (sender, e) => Log.Information("Media playback finished.");
        _mediaPlayer.EncounteredError += (sender, e) => Log.Error("LibVLC encountered an error during playback.");
    }

    public void Load(string mediaPath)
    {
        if (string.IsNullOrWhiteSpace(mediaPath))
        {
            Log.Warning("Attempted to load empty media path.");
            return;
        }

        bool isUrl = mediaPath.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                     mediaPath.StartsWith("rtsp", StringComparison.OrdinalIgnoreCase);

        if (!File.Exists(mediaPath) && !isUrl)
        {
            Log.Error($"Media file not found: {mediaPath}");
            return;
        }

        try
        {
            // Create media resource
            // Note: Media creation must be disposed after assignment or when player is done?
            // In LibVLCSharp, Media is IDisposable. Assigning it to MediaPlayer.Media transfers ownership? No, usually not.
            // We should manage Media lifecycle properly.

            using var media = new Media(_libVlc, mediaPath, isUrl ? FromType.FromLocation : FromType.FromPath);

            // Add options for low latency or specific buffering if needed (can be parameterized later)
            media.AddOption(":network-caching=300");
            media.AddOption(":file-caching=300");

            _mediaPlayer.Media = media;

            Log.Information($"Loaded media: {mediaPath}");
        }
        catch (Exception ex)
        {
             Log.Error(ex, $"Failed to load media: {mediaPath}");
        }
    }

    public void Play()
    {
        if (_mediaPlayer.Media == null)
        {
            Log.Warning("Play called but no media is loaded.");
            return;
        }

        var success = _mediaPlayer.Play();
        if (success)
            Log.Information("Playback started.");
        else
            Log.Warning("Playback failed to start.");
    }

    public void Pause()
    {
        if (_mediaPlayer.CanPause)
        {
            _mediaPlayer.Pause();
            Log.Information("Playback paused.");
        }
    }

    public void Stop()
    {
        _mediaPlayer.Stop();
        Log.Information("Playback stopped.");
    }

    public void SetPosition(float position)
    {
        // Position is a float between 0.0 and 1.0
        var safePosition = Math.Clamp(position, 0.0f, 1.0f);
        _mediaPlayer.Position = safePosition;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _mediaPlayer?.Stop();
            _mediaPlayer?.Dispose();
            _libVlc?.Dispose();
        }

        _disposed = true;
    }
}
