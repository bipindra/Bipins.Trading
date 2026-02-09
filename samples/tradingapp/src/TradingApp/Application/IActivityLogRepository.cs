using TradingApp.Domain;

namespace TradingApp.Application;

public interface IActivityLogRepository
{
    Task AddAsync(ActivityLog log, CancellationToken ct = default);
    Task<IReadOnlyList<ActivityLog>> GetRecentAsync(int limit = 100, CancellationToken ct = default);
    Task<IReadOnlyList<ActivityLog>> GetByCategoryAsync(string category, int limit = 100, CancellationToken ct = default);
    Task ClearOldLogsAsync(TimeSpan olderThan, CancellationToken ct = default);
    Task DeleteAllAsync(CancellationToken ct = default);
}
