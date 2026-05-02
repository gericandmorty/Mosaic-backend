using backend.Modules.Music.DTOs;

namespace backend.Modules.Music.Services
{
    public interface IMusicService
    {
        Task<IEnumerable<TrackResponse>> SearchTracksAsync(string query, int limit = 20);
        Task<TrackResponse?> GetTrackDetailsAsync(string videoId);
        Task<string?> GetAudioStreamUrlAsync(string videoId);
    }
}
