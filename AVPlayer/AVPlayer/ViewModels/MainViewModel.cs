// Imports...
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AVPlayer.Models;
using AVPlayer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;

namespace AVPlayer.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IMediaPlayerService _mediaService;
        private readonly IScreenManager _screenManager;
        private readonly IShowfileService _showfileService;
        private readonly IOSLockService _osLockService;
        private readonly IOscService _oscService; // Injected
        private readonly ILogger<MainViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<MediaClip> _mediaItems = new();
        [ObservableProperty]
        private MediaClip? _selectedClip;
        [ObservableProperty]
        private MediaClip? _cuedClip;
        [ObservableProperty]
        private MediaClip? _playingClip;
        [ObservableProperty]
        private TimeSpan _currentTime;
        [ObservableProperty]
        private string _timecodeDisplay = "00:00:00:00";

        [ObservableProperty]
        private bool _showMode = false;

        public MainViewModel(IMediaPlayerService mediaService, IScreenManager screenManager, IShowfileService showfileService, IOSLockService osLockService, IOscService oscService, ILogger<MainViewModel> logger)
        {
            _mediaService = mediaService;
            _screenManager = screenManager;
            _showfileService = showfileService;
            _osLockService = osLockService;
            _oscService = oscService;
            _logger = logger;

            _mediaService.PropertyChanged += MediaService_PropertyChanged;

            // OSC
            _oscService.CommandReceived += OnOscCommand;
            _oscService.Start(8000); // Default port

            Task.Run(async () =>
            {
                var clips = await _showfileService.LoadAsync("showfile.json");
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    MediaItems = clips;
                    if (MediaItems.Count > 0)
                    {
                        CuedClip = MediaItems[0];
                        CuedClip.State = MediaState.Cued;
                        _mediaService.Load(CuedClip.FilePath, false);
                    }
                });
            });

            _osLockService.PreventSleep();
        }

        private void OnOscCommand(object? sender, string address)
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                switch (address)
                {
                    case "/take": TakeCommand.Execute(null); break;
                    case "/stop": StopCommand.Execute(null); break;
                    case "/panic": PanicBlackCommand.Execute(null); break;
                    case "/load": LoadCommand.Execute(null); break;
                    case "/next":
                        // Logic to select next clip
                        break;
                }
            });
        }

        // ... Rest of ViewModel ...
        private void MediaService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IMediaPlayerService.CurrentTime))
            {
                CurrentTime = _mediaService.CurrentTime;
                TimecodeDisplay = CurrentTime.ToString(@"hh\:mm\:ss\:ff");
            }
        }

        [RelayCommand]
        public void PanicBlack()
        {
            _logger.LogWarning("PANIC BLACK TRIGGERED");
            _mediaService.Stop();
            if (PlayingClip != null)
            {
                PlayingClip.State = MediaState.Played;
                PlayingClip = null;
            }
        }

        [RelayCommand]
        public void FadeToBlack()
        {
             _logger.LogInformation("Fade To Black Triggered");
             StopCommand.Execute(null);
        }

        [RelayCommand]
        public void TestPattern()
        {
             _logger.LogInformation("Test Pattern Triggered (Placeholder)");
        }

        [RelayCommand]
        public async Task SaveShow()
        {
            await _showfileService.SaveAsync("showfile.json", MediaItems);
        }

        [RelayCommand]
        public void Load()
        {
            if (ShowMode) return;

            if (SelectedClip != null && File.Exists(SelectedClip.FilePath))
            {
                if (CuedClip != null && CuedClip != SelectedClip)
                {
                    CuedClip.State = MediaState.Ready;
                }

                CuedClip = SelectedClip;
                CuedClip.State = MediaState.Cued;

                _logger.LogInformation("Loading Clip: {Name}", CuedClip.Name);
                _mediaService.Load(CuedClip.FilePath, false);
            }
        }

        [RelayCommand]
        public void Take()
        {
            if (CuedClip == null)
            {
                 if (PlayingClip != null && !_mediaService.IsPlaying)
                 {
                     _mediaService.Play();
                 }
                 return;
            }

            _logger.LogInformation("Taking Clip: {Name}", CuedClip.Name);

            if (PlayingClip != null)
            {
                PlayingClip.State = MediaState.Played;
            }

            PlayingClip = CuedClip;
            PlayingClip.State = MediaState.Playing;

            _mediaService.Take();

            var index = MediaItems.IndexOf(PlayingClip);
            if (index >= 0 && index < MediaItems.Count - 1)
            {
                CuedClip = MediaItems[index + 1];
                CuedClip.State = MediaState.Cued;
                 _mediaService.Load(CuedClip.FilePath, false);
            }
            else
            {
                CuedClip = null;
            }
        }

        [RelayCommand]
        public void Stop()
        {
            _mediaService.Stop();
            if (PlayingClip != null)
            {
                PlayingClip.State = MediaState.Ready;
                PlayingClip = null;
            }
        }

        [RelayCommand]
        public void OnDragOver(System.Windows.DragEventArgs e)
        {
            if (ShowMode) { e.Effects = System.Windows.DragDropEffects.None; e.Handled = true; return; }

            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                e.Effects = System.Windows.DragDropEffects.Copy;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            e.Handled = true;
        }

        [RelayCommand]
        public void OnDrop(System.Windows.DragEventArgs e)
        {
             if (ShowMode) return;

            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                foreach (var file in files)
                {
                    if (File.Exists(file))
                    {
                        AddMediaItem(file);
                    }
                }
                SaveShowCommand.Execute(null);
            }
        }

        private void AddMediaItem(string filePath)
        {
            var ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (new[] { ".mp4", ".mov", ".mkv", ".webm", ".mp3", ".wav", ".jpg", ".png" }.Contains(ext))
            {
                var clip = new MediaClip
                {
                    Id = MediaItems.Count + 1,
                    Name = Path.GetFileName(filePath),
                    FilePath = filePath,
                    State = MediaState.Ready,
                    IsImage = new[] { ".jpg", ".png" }.Contains(ext)
                };
                MediaItems.Add(clip);
            }
        }
    }
}
