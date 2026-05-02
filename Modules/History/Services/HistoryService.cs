using backend.Infrastructure.Firebase;
using backend.Models;
using Google.Cloud.Firestore;

namespace backend.Modules.History.Services;

public class HistoryService : IHistoryService
{
    private readonly FirestoreDb _firestore;

    public HistoryService(FirebaseService firebaseService)
    {
        _firestore = firebaseService.GetFirestore();
    }

    public async Task AddToHistoryAsync(string userId, HistoryTrack track)
    {
        var historyRef = _firestore.Collection("users").Document(userId).Collection("history");

        // Use YoutubeId as the document ID to prevent duplicate adjacent entries 
        // OR use a generated ID to allow the exact same song to appear multiple times in history.
        // For history, it's usually better to allow duplicates, but maybe we just update the timestamp 
        // if they play it again, moving it to the top. Let's do that! Update timestamp if exists, else create.

        var docRef = historyRef.Document(track.YoutubeId);
        
        track.PlayedAt = Timestamp.FromDateTime(DateTime.UtcNow);
        
        // This acts as an upsert
        await docRef.SetAsync(track);
    }

    public async Task<List<HistoryTrack>> GetRecentHistoryAsync(string userId, int limit = 10)
    {
        var historyRef = _firestore.Collection("users").Document(userId).Collection("history");
        
        var snapshot = await historyRef
            .OrderByDescending("playedAt")
            .Limit(limit)
            .GetSnapshotAsync();

        return snapshot.Documents.Select(doc => doc.ConvertTo<HistoryTrack>()).ToList();
    }

    public async Task ClearHistoryAsync(string userId)
    {
        var historyRef = _firestore.Collection("users").Document(userId).Collection("history");
        var snapshot = await historyRef.GetSnapshotAsync();
        
        var batch = _firestore.StartBatch();
        foreach (var doc in snapshot.Documents)
        {
            batch.Delete(doc.Reference);
        }
        
        await batch.CommitAsync();
    }
}
