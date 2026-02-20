namespace AVPlayer.Services
{
    /// <summary>
    /// Service for media playback operations.
    /// See AGENTS.md for architecture details (LibVLCSharp + D3D11 Interop).
    /// </summary>
    public interface IMediaPlayerService
    {
        void Initialize();
    }
}
