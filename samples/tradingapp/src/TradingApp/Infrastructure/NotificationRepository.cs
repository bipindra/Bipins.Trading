using Microsoft.EntityFrameworkCore;
using TradingApp.Application;
using TradingApp.Domain;

namespace TradingApp.Infrastructure;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;

    public NotificationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Notification> AddAsync(Notification notification, CancellationToken ct = default)
    {
        notification.TriggeredAt = DateTime.UtcNow;
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);
        return notification;
    }

    public async Task<IReadOnlyList<Notification>> GetRecentAsync(int limit = 50, bool unreadOnly = false, CancellationToken ct = default)
    {
        var q = _db.Notifications.AsNoTracking()
            .Where(x => !unreadOnly || x.ReadAt == null)
            .OrderByDescending(x => x.TriggeredAt)
            .Take(limit);
        return await q.ToListAsync(ct);
    }

    public async Task MarkReadAsync(int id, CancellationToken ct = default)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (n != null)
        {
            n.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
    }
}
