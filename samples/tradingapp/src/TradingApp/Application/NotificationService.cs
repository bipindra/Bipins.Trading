using TradingApp.Application.DTOs;
using TradingApp.Domain;

namespace TradingApp.Application;

public sealed class NotificationService
{
    private readonly INotificationRepository _repository;

    public NotificationService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetRecentAsync(int limit = 50, bool unreadOnly = false, CancellationToken ct = default)
    {
        var list = await _repository.GetRecentAsync(limit, unreadOnly, ct);
        return list.Select(n => new NotificationDto(n.Id, n.AlertId, n.Symbol, n.Message, n.TriggeredAt, n.ReadAt)).ToList();
    }

    public async Task MarkReadAsync(int id, CancellationToken ct = default)
    {
        await _repository.MarkReadAsync(id, ct);
    }
}
