using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
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

        // Video memory mapping (Simplified Software Renderer for Phase 3 Proof-of-Concept)
        private IntPtr _videoBuffer = IntPtr.Zero;
        private uint _videoWidth = 1920;
        private uint _videoHeight = 1080;
        private uint _videoPitch;
        private uint _videoLines;

        public event PropertyChangedEventHandler? PropertyChanged;

        public WriteableBitmap? ActiveVideoSource
        {
            get => _activeVideoSource;
            private set
            {
                if (_activeVideoSource != value)
                {
                    _activeVideoSource = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan CurrentTime
        {
            get => _currentTime;
            private set
            {
                if (_currentTime != value)
                {
                    _currentTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan TotalDuration
        {
            get => _totalDuration;
            private set
            {
                if (_totalDuration != value)
                {
                    _totalDuration = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged();
                }
            }
        }

        public MediaPlayerService(ILogger<MediaPlayerService> logger)
        {
            _logger = logger;
            _videoPitch = _videoWidth * 4; // BGRA 32-bit
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

                // Initial State: A is active, B is next
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


                _logger.LogInformation("MediaPlayerService initialized (LibVLC). Hardware decoding enabled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize LibVLC.");
            }
        }

        private void SetupEvents(LibVLCSharp.Shared.MediaPlayer player)
        {
            // Thread Safety: Ensure UI updates happen on Dispatcher

            player.TimeChanged += (s, e) =>
            {
                if (s == _activePlayer)
                {
                     System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => CurrentTime = TimeSpan.FromMilliseconds(e.Time));
                }
            };

            player.LengthChanged += (s, e) =>
            {
                if (s == _activePlayer)
                {
                    System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => TotalDuration = TimeSpan.FromMilliseconds(e.Length));
                }
            };

            player.EndReached += (s, e) =>
            {
                 if (s == _activePlayer)
                 {
                    System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
                    {
                        IsPlaying = false;
                        _logger.LogInformation("Playback finished.");
                    });
                 }
            };

            player.Playing += (s, e) =>
            {
                if (s == _activePlayer)
                {
                    System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => IsPlaying = true);
                    _logger.LogInformation("Active Player Started Playing.");
                }
            };

            player.Paused += (s, e) =>
            {
                if (s == _activePlayer) System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => IsPlaying = false);
            };

            player.Stopped += (s, e) =>
            {
                if (s == _activePlayer)
                {
                    System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
                    {
                        IsPlaying = false;
                        CurrentTime = TimeSpan.Zero;
                    });
                }
            };

            player.EncounteredError += (s, e) => _logger.LogError("Media Player Error.");
        }

        public void Load(string path, bool isAudio = false)
        {
            if (_libVLC == null || _nextPlayer == null) return;

            if (!File.Exists(path))
            {
                _logger.LogError("File not found: {Path}", path);
                return;
            }

            // Clean up old media
            if (_nextPlayer.Media != null)
            {
                 // _nextPlayer.Media.Dispose(); // Careful disposing if still in use by A? No, they are separate.
            }

            var media = new Media(_libVLC, path, FromType.FromPath);
            media.AddOption(":network-caching=1000");

            _nextPlayer.Media = media;

            // Just parse, don't set callbacks yet.
            media.Parse(MediaParseOptions.ParseLocal);
            _logger.LogInformation("Loaded media into NEXT player: {Path}", path);
        }

        public void Take()
        {
            // Critical Section
            lock(_activePlayer!)
            {
                if (_activePlayer == null || _nextPlayer == null) return;

                _logger.LogInformation("TAKE: Swapping players.");

                // 1. Stop Current Active Player
                if (_activePlayer.IsPlaying)
                {
                    _activePlayer.Stop();
                }

                // Unset callbacks
                _activePlayer.SetVideoCallbacks(null, null, null);

                // 2. Swap References
                var temp = _activePlayer;
                _activePlayer = _nextPlayer;
                _nextPlayer = temp;

                // 3. Set callbacks on NEW Active Player
                _activePlayer.SetVideoFormat("RV32", _videoWidth, _videoHeight, _videoPitch);
                _activePlayer.SetVideoCallbacks(LockVideo, UnlockVideo, DisplayVideo);

                // 4. Play
                _activePlayer.Play();

                // UI Update
                System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => IsPlaying = true);
            }
        }

        public void Play()
        {
            if (_activePlayer != null && !_activePlayer.IsPlaying)
            {
                // Ensure callbacks are set if we resume directly
                _activePlayer.SetVideoFormat("RV32", _videoWidth, _videoHeight, _videoPitch);
                _activePlayer.SetVideoCallbacks(LockVideo, UnlockVideo, DisplayVideo);
                _activePlayer.Play();
            }
        }

        public void Pause()
        {
            if (_activePlayer != null) _activePlayer.Pause();
        }

        public void Stop()
        {
            if (_activePlayer != null) _activePlayer.Stop();
        }

        public void SetPosition(float position)
        {
            if (_activePlayer != null) _activePlayer.Position = position;
        }

        public void SetVolume(int volume)
        {
            if (_activePlayer != null)
                _activePlayer.Volume = volume;
        }

        // ... Video Callbacks ...
        private IntPtr LockVideo(IntPtr opaque, IntPtr planes)
        {
            if (_videoBuffer != IntPtr.Zero)
                 Marshal.WriteIntPtr(planes, _videoBuffer);
            return IntPtr.Zero;
        }

        private void UnlockVideo(IntPtr opaque, IntPtr picture, IntPtr planes)
        {
        }

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
                        Buffer.MemoryCopy(
                            (void*)_videoBuffer,
                            (void*)_activeVideoSource.BackBuffer,
                            stride * height,
                            stride * height);
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
            if (_videoBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_videoBuffer);
                _videoBuffer = IntPtr.Zero;
            }
        }
    }
}
