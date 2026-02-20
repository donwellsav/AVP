using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AVPlayer.Models
{
    public enum MediaState
    {
        Cued,       // Green (Next)
        Playing,    // Red (Live)
        Played,     // Grey (Past)
        Offline,    // Orange/Red (Missing)
        Ready       // Default
    }

    public partial class MediaClip : ObservableObject
    {
        [ObservableProperty]
        private int _id;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _filePath = string.Empty;

        [ObservableProperty]
        private TimeSpan _duration;

        [ObservableProperty]
        private MediaState _state = MediaState.Ready;

        [ObservableProperty]
        private TimeSpan _inPoint = TimeSpan.Zero;

        [ObservableProperty]
        private TimeSpan _outPoint = TimeSpan.Zero; // Zero means full length

        [ObservableProperty]
        private double _volume = 1.0; // 0.0 to 1.0+ (Gain)

        [ObservableProperty]
        private bool _isImage;

        // Helper for display
        public string DurationDisplay => Duration.ToString(@"hh\:mm\:ss");
    }
}
