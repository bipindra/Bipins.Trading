using Microsoft.EntityFrameworkCore;
using TradingApp.Application;
using TradingApp.Domain;

namespace TradingApp.Infrastructure;

public sealed class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _db;

    public AlertRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Alert> AddAsync(Alert alert, CancellationToken ct = default)
    {
        alert.CreatedAt = DateTime.UtcNow;
        _db.Alerts.Add(alert);
        await _db.SaveChangesAsync(ct);
        return alert;
    }

    public async Task<IReadOnlyList<Alert>> GetBySymbolAsync(string symbol, CancellationToken ct = default)
    {
        var normalized = symbol.Trim().ToUpperInvariant();
        return await _db.Alerts
            .Where(x => x.Symbol == normalized)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Alert>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Alerts.Where(x => x.TriggeredAt == null).OrderBy(x => x.Symbol).ToListAsync(ct);
    }

    public async Task SetTriggeredAtAsync(int alertId, DateTime triggeredAt, CancellationToken ct = default)
    {
        var alert = await _db.Alerts.FirstOrDefaultAsync(x => x.Id == alertId, ct);
        if (alert != null)
        {
            alert.TriggeredAt = triggeredAt;
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> DeleteAsync(int alertId, CancellationToken ct = default)
    {
        var alert = await _db.Alerts.FirstOrDefaultAsync(x => x.Id == alertId, ct);
        if (alert == null) return false;
        _db.Alerts.Remove(alert);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
