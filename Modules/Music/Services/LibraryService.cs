using Google.Cloud.Firestore;
using backend.Models;

namespace backend.Modules.Music.Services;

public class LibraryService : ILibraryService
{
    private readonly FirestoreDb _firestore;

    public LibraryService(Infrastructure.Firebase.FirebaseService firebaseService)
    {
        _firestore = firebaseService.GetFirestore();
    }

    private CollectionReference GetLikedSongsCollection(string firebaseUid) =>
        _firestore.Collection("users").Document(firebaseUid).Collection("liked_songs");

    private CollectionReference GetPlaylistsCollection(string firebaseUid) =>
        _firestore.Collection("users").Document(firebaseUid).Collection("playlists");

    public async Task<List<LikedSong>> GetLikedSongsAsync(string firebaseUid)
    {
        var snapshot = await GetLikedSongsCollection(firebaseUid).OrderByDescending("likedAt").GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<LikedSong>()).ToList();
    }

    public async Task ToggleLikedSongAsync(string firebaseUid, LikedSong song)
    {
        var doc = GetLikedSongsCollection(firebaseUid).Document(song.YoutubeId);
        var snapshot = await doc.GetSnapshotAsync();

        if (snapshot.Exists)
        {
            await doc.DeleteAsync(); // Unlike
        }
        else
        {
            await doc.SetAsync(song); // Like
        }
    }

    public async Task<bool> IsSongLikedAsync(string firebaseUid, string youtubeId)
    {
        var doc = GetLikedSongsCollection(firebaseUid).Document(youtubeId);
        var snapshot = await doc.GetSnapshotAsync();
        return snapshot.Exists;
    }

    public async Task<List<Playlist>> GetPlaylistsAsync(string firebaseUid)
    {
        var snapshot = await GetPlaylistsCollection(firebaseUid).OrderByDescending("updatedAt").GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Playlist>()).ToList();
    }

    public async Task<Playlist> CreatePlaylistAsync(string firebaseUid, string name, string? description = null, string? coverUrl = null)
    {
        var docRef = GetPlaylistsCollection(firebaseUid).Document(); // Auto-generate ID
        
        var playlist = new Playlist
        {
            Id = docRef.Id,
            Name = name,
            Description = description,
            CoverUrl = coverUrl,
            CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow),
            UpdatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
        };

        await docRef.SetAsync(playlist);
        return playlist;
    }

    public async Task DeletePlaylistAsync(string firebaseUid, string playlistId)
    {
        await GetPlaylistsCollection(firebaseUid).Document(playlistId).DeleteAsync();
    }

    public async Task<List<PlaylistTrack>> GetPlaylistTracksAsync(string firebaseUid, string playlistId)
    {
        var snapshot = await GetPlaylistsCollection(firebaseUid).Document(playlistId)
            .Collection("tracks").OrderBy("position").GetSnapshotAsync();
            
        return snapshot.Documents.Select(d => d.ConvertTo<PlaylistTrack>()).ToList();
    }

    public async Task AddTrackToPlaylistAsync(string firebaseUid, string playlistId, PlaylistTrack track)
    {
        // First get current max position to append to end
        var tracks = await GetPlaylistTracksAsync(firebaseUid, playlistId);
        track.Position = tracks.Count > 0 ? tracks.Max(t => t.Position) + 1 : 0;
        
        track.AddedAt = Timestamp.FromDateTime(DateTime.UtcNow);

        var docRef = GetPlaylistsCollection(firebaseUid).Document(playlistId)
            .Collection("tracks").Document(track.YoutubeId);
            
        track.Id = docRef.Id;
        await docRef.SetAsync(track);

        // Update playlist's updated timestamp
        await GetPlaylistsCollection(firebaseUid).Document(playlistId).UpdateAsync("updatedAt", Timestamp.FromDateTime(DateTime.UtcNow));
    }

    public async Task RemoveTrackFromPlaylistAsync(string firebaseUid, string playlistId, string youtubeId)
    {
        await GetPlaylistsCollection(firebaseUid).Document(playlistId)
            .Collection("tracks").Document(youtubeId).DeleteAsync();
            
        await GetPlaylistsCollection(firebaseUid).Document(playlistId).UpdateAsync("updatedAt", Timestamp.FromDateTime(DateTime.UtcNow));
    }
}
