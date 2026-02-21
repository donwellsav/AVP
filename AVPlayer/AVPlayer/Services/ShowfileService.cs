using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AVPlayer.Models;
using Microsoft.Extensions.Logging;

namespace AVPlayer.Services
{
    public interface IShowfileService
    {
        Task SaveAsync(string path, ObservableCollection<MediaClip> clips);
        Task<ObservableCollection<MediaClip>> LoadAsync(string path);
    }

    public class ShowfileService : IShowfileService
    {
        private readonly ILogger<ShowfileService> _logger;

        public ShowfileService(ILogger<ShowfileService> logger)
        {
            _logger = logger;
        }

        public async Task SaveAsync(string path, ObservableCollection<MediaClip> clips)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                using var fs = File.Create(path);
                await JsonSerializer.SerializeAsync(fs, clips, options);
                _logger.LogInformation("Showfile saved to {Path}", path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save showfile.");
            }
        }

        public async Task<ObservableCollection<MediaClip>> LoadAsync(string path)
        {
            if (!File.Exists(path)) return new ObservableCollection<MediaClip>();

            try
            {
                using var fs = File.OpenRead(path);
                var clips = await JsonSerializer.DeserializeAsync<ObservableCollection<MediaClip>>(fs);
                _logger.LogInformation("Showfile loaded from {Path}", path);
                return clips ?? new ObservableCollection<MediaClip>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load showfile.");
                return new ObservableCollection<MediaClip>();
            }
        }
    }
}
