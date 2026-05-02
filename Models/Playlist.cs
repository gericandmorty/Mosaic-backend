using Google.Cloud.Firestore;

namespace backend.Models;

[FirestoreData]
public class Playlist
{
    [FirestoreDocumentId]
    public string Id { get; set; } = string.Empty;
    
    [FirestoreProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [FirestoreProperty("description")]
    public string? Description { get; set; }
    
    [FirestoreProperty("coverUrl")]
    public string? CoverUrl { get; set; }
    
    [FirestoreProperty("createdAt")]
    public Timestamp CreatedAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);
    
    [FirestoreProperty("updatedAt")]
    public Timestamp UpdatedAt { get; set; } = Timestamp.FromDateTime(DateTime.UtcNow);
}
