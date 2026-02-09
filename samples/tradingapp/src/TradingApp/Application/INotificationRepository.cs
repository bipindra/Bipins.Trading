using TradingApp.Domain;

namespace TradingApp.Application;

public interface INotificationRepository
{
    Task<Notification> AddAsync(Notification notification, CancellationToken ct = default);
    Task<IReadOnlyList<Notification>> GetRecentAsync(int limit = 50, bool unreadOnly = false, CancellationToken ct = default);
    Task MarkReadAsync(int id, CancellationToken ct = default);
}
