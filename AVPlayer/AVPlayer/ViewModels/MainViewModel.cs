using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AVPlayer.Models;
using AVPlayer.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AVPlayer.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IMediaPlayerService _mediaService;
        private readonly IScreenManager _screenManager;
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

        public MainViewModel(IMediaPlayerService mediaService, IScreenManager screenManager, ILogger<MainViewModel> logger)
        {
            _mediaService = mediaService;
            _screenManager = screenManager;
            _logger = logger;

            _mediaService.PropertyChanged += MediaService_PropertyChanged;
        }

        private void MediaService_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IMediaPlayerService.CurrentTime))
            {
                CurrentTime = _mediaService.CurrentTime;
                TimecodeDisplay = CurrentTime.ToString(@"hh\:mm\:ss\:ff");
            }
        }

        [RelayCommand]
        public void Load()
        {
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
