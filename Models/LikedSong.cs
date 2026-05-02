using Google.Cloud.Firestore;

namespace backend.Models;

[FirestoreData]
public class LikedSong
{
    [FirestoreDocumentId]
    public string Id { get; set; } = string.Empty;
    
    [FirestoreProperty("youtubeId")]
    public string YoutubeId { get; set; } = string.Empty;
    
    [FirestoreProperty("title")]
    public string Title { get; set; } = string.Empty;
    
    [FirestoreProperty("artist")]
    public string Artist { get; set; } = string.Empty;
    
    [FirestoreProperty("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }
    
    [FirestoreProperty("duration")]
    public string? Duration { get; set; }
    
    [FirestoreProperty("likedAt")]
    public Timestamp LikedAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);
}
