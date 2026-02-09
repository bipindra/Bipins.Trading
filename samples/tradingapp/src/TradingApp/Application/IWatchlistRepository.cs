using TradingApp.Domain;

namespace TradingApp.Application;

public interface IWatchlistRepository
{
    Task<IReadOnlyList<WatchlistItem>> GetAllAsync(CancellationToken ct = default);
    Task<WatchlistItem?> GetBySymbolAsync(string symbol, CancellationToken ct = default);
    Task<WatchlistItem> AddAsync(string symbol, CancellationToken ct = default);
    Task<bool> RemoveAsync(string symbol, CancellationToken ct = default);
}
