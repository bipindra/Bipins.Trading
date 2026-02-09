using Microsoft.EntityFrameworkCore;
using TradingApp.Application;
using TradingApp.Domain;

namespace TradingApp.Infrastructure;

public sealed class ActivityLogRepository : IActivityLogRepository
{
    private readonly AppDbContext _db;

    public ActivityLogRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(ActivityLog log, CancellationToken ct = default)
    {
        // Skip saving Microsoft.* logs
        if (log.Category.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase))
            return;
            
        log.Timestamp = DateTime.UtcNow;
        _db.ActivityLogs.Add(log);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ActivityLog>> GetRecentAsync(int limit = 100, CancellationToken ct = default)
    {
        return await _db.ActivityLogs
            .Where(x => !x.Category.StartsWith("Microsoft."))
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ActivityLog>> GetByCategoryAsync(string category, int limit = 100, CancellationToken ct = default)
    {
        return await _db.ActivityLogs
            .Where(x => x.Category == category && !x.Category.StartsWith("Microsoft."))
            .OrderByDescending(x => x.Timestamp)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task ClearOldLogsAsync(TimeSpan olderThan, CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow - olderThan;
        var oldLogs = await _db.ActivityLogs
            .Where(x => x.Timestamp < cutoff)
            .ToListAsync(ct);
        _db.ActivityLogs.RemoveRange(oldLogs);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAllAsync(CancellationToken ct = default)
    {
        var allLogs = await _db.ActivityLogs.ToListAsync(ct);
        _db.ActivityLogs.RemoveRange(allLogs);
        await _db.SaveChangesAsync(ct);
    }
}
