using Microsoft.EntityFrameworkCore;
using TradingApp.Application;
using TradingApp.Domain;

namespace TradingApp.Infrastructure;

public sealed class WatchlistRepository : IWatchlistRepository
{
    private readonly AppDbContext _db;

    public WatchlistRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<WatchlistItem>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.WatchlistItems
            .OrderBy(x => x.AddedAt)
            .ToListAsync(ct);
    }

    public async Task<WatchlistItem?> GetBySymbolAsync(string symbol, CancellationToken ct = default)
    {
        var normalized = symbol.Trim().ToUpperInvariant();
        return await _db.WatchlistItems.FirstOrDefaultAsync(x => x.Symbol == normalized, ct);
    }

    public async Task<WatchlistItem> AddAsync(string symbol, CancellationToken ct = default)
    {
        var normalized = symbol.Trim().ToUpperInvariant();
        var existing = await GetBySymbolAsync(normalized, ct);
        if (existing != null)
            return existing;

        var item = new WatchlistItem { Symbol = normalized, AddedAt = DateTime.UtcNow };
        _db.WatchlistItems.Add(item);
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public async Task<bool> RemoveAsync(string symbol, CancellationToken ct = default)
    {
        var normalized = symbol.Trim().ToUpperInvariant();
        var item = await GetBySymbolAsync(normalized, ct);
        if (item == null) return false;
        _db.WatchlistItems.Remove(item);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
