using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AVPlayer.Models;

namespace AVPlayer.Converters
{
    public class MediaStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MediaState state)
            {
                switch (state)
                {
                    case MediaState.Cued: return new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 100, 0)); // Dark Green
                    case MediaState.Playing: return new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 0, 0)); // Dark Red
                    case MediaState.Played: return new SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 50, 50)); // Grey
                    case MediaState.Offline: return new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 100, 0)); // Orange
                    default: return System.Windows.Media.Brushes.Transparent;
                }
            }
            return System.Windows.Media.Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
