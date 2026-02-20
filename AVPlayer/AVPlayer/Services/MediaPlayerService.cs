using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LibVLCSharp.Shared;
using Microsoft.Extensions.Logging;

namespace AVPlayer.Services
{
    public class MediaPlayerService : IMediaPlayerService, IDisposable
    {
        private readonly ILogger<MediaPlayerService> _logger;
        private LibVLC? _libVLC;

        private LibVLCSharp.Shared.MediaPlayer? _playerA;
        private LibVLCSharp.Shared.MediaPlayer? _playerB;
        private LibVLCSharp.Shared.MediaPlayer? _activePlayer;
        private LibVLCSharp.Shared.MediaPlayer? _nextPlayer;

        private WriteableBitmap? _activeVideoSource;
        private TimeSpan _currentTime;
        private TimeSpan _totalDuration;
        private bool _isPlaying;

        private TimeSpan? _inPoint;
        private TimeSpan? _outPoint;

        private IntPtr _videoBuffer = IntPtr.Zero;
        private uint _videoWidth = 1920;
        private uint _videoHeight = 1080;
        private uint _videoPitch;
        private uint _videoLines;

        public event PropertyChangedEventHandler? PropertyChanged;

        public WriteableBitmap? ActiveVideoSource
        {
            get => _activeVideoSource;
            private set { if (_activeVideoSource != value) { _activeVideoSource = value; OnPropertyChanged(); } }
        }

        public TimeSpan CurrentTime
        {
            get => _currentTime;
            private set { if (_currentTime != value) { _currentTime = value; OnPropertyChanged(); } }
        }

        public TimeSpan TotalDuration
        {
            get => _totalDuration;
            private set { if (_totalDuration != value) { _totalDuration = value; OnPropertyChanged(); } }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            private set { if (_isPlaying != value) { _isPlaying = value; OnPropertyChanged(); } }
        }

        public MediaPlayerService(ILogger<MediaPlayerService> logger)
        {
            _logger = logger;
            _videoPitch = _videoWidth * 4;
            _videoLines = _videoHeight;
        }

        public void Initialize()
        {
             try
            {
                Core.Initialize();
                _libVLC = new LibVLC("--verbose=0", "--avcodec-hw=d3d11va", "--no-osd");

                _playerA = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
                _playerB = new LibVLCSharp.Shared.MediaPlayer(_libVLC);

                _activePlayer = _playerA;
                _nextPlayer = _playerB;

                SetupEvents(_playerA);
                SetupEvents(_playerB);

                _videoBuffer = Marshal.AllocHGlobal((int)(_videoPitch * _videoLines));

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                     _activeVideoSource = new WriteableBitmap((int)_videoWidth, (int)_videoHeight, 96, 96, PixelFormats.Bgra32, null);
                     OnPropertyChanged(nameof(ActiveVideoSource));
                });
                _logger.LogInformation("MediaPlayerService initialized.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize LibVLC.");
            }
        }

        private void SetupEvents(LibVLCSharp.Shared.MediaPlayer player)
        {
            player.TimeChanged += (s, e) =>
            {
                if (s == _activePlayer)
                {
                    var time = TimeSpan.FromMilliseconds(e.Time);
                    if (_outPoint.HasValue && time >= _outPoint.Value && IsPlaying)
                    {
                         System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => Stop());
                    }
                    else
                    {
                        System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => CurrentTime = time);
                    }
                }
            };
            player.LengthChanged += (s, e) => { if (s == _activePlayer) System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => TotalDuration = TimeSpan.FromMilliseconds(e.Length)); };
            player.EndReached += (s, e) => { if (s == _activePlayer) System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => { IsPlaying = false; _logger.LogInformation("Playback finished."); }); };
            player.Playing += (s, e) => { if (s == _activePlayer) System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => IsPlaying = true); };
            player.Paused += (s, e) => { if (s == _activePlayer) System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => IsPlaying = false); };
            player.Stopped += (s, e) => { if (s == _activePlayer) System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => { IsPlaying = false; CurrentTime = TimeSpan.Zero; }); };
        }

        public void Load(string path, bool isAudio = false)
        {
            if (_libVLC == null || _nextPlayer == null) return;
            if (!File.Exists(path)) return;

            var media = new Media(_libVLC, path, FromType.FromPath);
            media.AddOption(":network-caching=1000");

            if (_nextPlayer.Media != null) { /* Dispose logic if needed */ }
            _nextPlayer.Media = media;

            media.Parse(MediaParseOptions.ParseLocal);
            _logger.LogInformation("Loaded media into NEXT player: {Path}", path);
        }

        public void SetLimits(TimeSpan? inPoint, TimeSpan? outPoint)
        {
            _inPoint = inPoint;
            _outPoint = outPoint;
            _logger.LogInformation("Set Limits: In={In}, Out={Out}", inPoint, outPoint);
        }

        public void Take()
        {
             lock(_activePlayer!)
            {
                if (_activePlayer == null || _nextPlayer == null) return;

                // Stop active player with optional fade?
                // For hard cut, stop immediately.
                if (_activePlayer.IsPlaying) _activePlayer.Stop();

                _activePlayer.SetVideoCallbacks(null, null, null);

                var temp = _activePlayer;
                _activePlayer = _nextPlayer;
                _nextPlayer = temp;

                _activePlayer.SetVideoFormat("RV32", _videoWidth, _videoHeight, _videoPitch);
                _activePlayer.SetVideoCallbacks(LockVideo, UnlockVideo, DisplayVideo);

                _activePlayer.Volume = 100; // Reset Volume
                _activePlayer.Play();

                // Handle In-Point Seek
                if (_inPoint.HasValue && _inPoint.Value > TimeSpan.Zero)
                {
                     // Using ThreadPool to avoid blocking UI or Lock
                     Task.Run(async () =>
                     {
                         // Wait until state is Playing
                         int retries = 0;
                         while (!_activePlayer.IsPlaying && retries < 10) { await Task.Delay(50); retries++; }

                         if (_activePlayer.IsPlaying)
                         {
                            _activePlayer.Time = (long)_inPoint.Value.TotalMilliseconds;
                         }
                     });
                }

                System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => IsPlaying = true);
            }
        }

        public void Play()
        {
             if (_activePlayer != null && !_activePlayer.IsPlaying)
            {
                _activePlayer.SetVideoFormat("RV32", _videoWidth, _videoHeight, _videoPitch);
                _activePlayer.SetVideoCallbacks(LockVideo, UnlockVideo, DisplayVideo);
                _activePlayer.Play();
            }
        }

        public void Pause() { if (_activePlayer != null) _activePlayer.Pause(); }

        public void Stop()
        {
            if (_activePlayer != null && _activePlayer.IsPlaying)
            {
                // Anti-Pop Fade Out (250ms)
                // Need to capture the player instance to avoid race condition if swapped during fade
                var playerToStop = _activePlayer;

                Task.Run(async () =>
                {
                    int startVolume = playerToStop.Volume;
                    for (int i = 0; i < 10; i++)
                    {
                        if (!playerToStop.IsPlaying) break;
                        playerToStop.Volume = (int)(startVolume * (1.0 - (i / 10.0)));
                        await Task.Delay(25);
                    }
                    playerToStop.Stop();
                    playerToStop.Volume = 100; // Reset for next use
                });
            }
        }

        public void SetPosition(float position) { if (_activePlayer != null) _activePlayer.Position = position; }
        public void SetVolume(int volume) { if (_activePlayer != null) _activePlayer.Volume = volume; }

        // Callbacks
        private IntPtr LockVideo(IntPtr opaque, IntPtr planes)
        {
            if (_videoBuffer != IntPtr.Zero) Marshal.WriteIntPtr(planes, _videoBuffer);
            return IntPtr.Zero;
        }
        private void UnlockVideo(IntPtr opaque, IntPtr picture, IntPtr planes) {}
        private void DisplayVideo(IntPtr opaque, IntPtr picture)
        {
             System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                if (_activeVideoSource != null)
                {
                    _activeVideoSource.Lock();
                    unsafe
                    {
                        int stride = _activeVideoSource.BackBufferStride;
                        int height = _activeVideoSource.PixelHeight;
                        Buffer.MemoryCopy((void*)_videoBuffer, (void*)_activeVideoSource.BackBuffer, stride * height, stride * height);
                    }
                    _activeVideoSource.AddDirtyRect(new Int32Rect(0, 0, (int)_videoWidth, (int)_videoHeight));
                    _activeVideoSource.Unlock();
                }
            }, System.Windows.Threading.DispatcherPriority.Render);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _playerA?.Dispose();
            _playerB?.Dispose();
            _libVLC?.Dispose();
            if (_videoBuffer != IntPtr.Zero) Marshal.FreeHGlobal(_videoBuffer);
        }
    }
}
