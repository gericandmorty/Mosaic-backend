using backend.Models;

namespace backend.Modules.Music.Services;

public interface ILibraryService
{
    // Liked Songs
    Task<List<LikedSong>> GetLikedSongsAsync(string firebaseUid);
    Task ToggleLikedSongAsync(string firebaseUid, LikedSong song);
    Task<bool> IsSongLikedAsync(string firebaseUid, string youtubeId);

    // Playlists
    Task<List<Playlist>> GetPlaylistsAsync(string firebaseUid);
    Task<Playlist> CreatePlaylistAsync(string firebaseUid, string name, string? description = null, string? coverUrl = null);
    Task DeletePlaylistAsync(string firebaseUid, string playlistId);
    
    // Playlist Tracks
    Task<List<PlaylistTrack>> GetPlaylistTracksAsync(string firebaseUid, string playlistId);
    Task AddTrackToPlaylistAsync(string firebaseUid, string playlistId, PlaylistTrack track);
    Task RemoveTrackFromPlaylistAsync(string firebaseUid, string playlistId, string youtubeId);
}
