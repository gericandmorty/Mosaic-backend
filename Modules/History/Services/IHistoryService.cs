using backend.Models;

namespace backend.Modules.History.Services;

public interface IHistoryService
{
    Task AddToHistoryAsync(string userId, HistoryTrack track);
    Task<List<HistoryTrack>> GetRecentHistoryAsync(string userId, int limit = 10);
    Task ClearHistoryAsync(string userId);
}
