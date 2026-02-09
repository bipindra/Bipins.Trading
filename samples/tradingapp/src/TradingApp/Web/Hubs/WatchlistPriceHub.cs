using Microsoft.AspNetCore.SignalR;

namespace TradingApp.Web.Hubs;

public sealed class WatchlistPriceHub : Hub
{
    private readonly IWatchlistPriceSubscriptionStore _store;

    public WatchlistPriceHub(IWatchlistPriceSubscriptionStore store)
    {
        _store = store;
    }

    public Task Subscribe(string[] symbols)
    {
        if (symbols?.Length > 0)
            _store.SetSymbols(Context.ConnectionId, symbols);
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _store.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}

public interface IWatchlistPriceSubscriptionStore
{
    void SetSymbols(string connectionId, string[] symbols);
    void Remove(string connectionId);
    IReadOnlySet<string> GetSubscribedSymbols();
}

public sealed class WatchlistPriceSubscriptionStore : IWatchlistPriceSubscriptionStore
{
    private readonly Dictionary<string, HashSet<string>> _byConnection = new();
    private readonly object _lock = new();

    public void SetSymbols(string connectionId, string[] symbols)
    {
        lock (_lock)
        {
            var set = new HashSet<string>(symbols.Select(s => (s ?? "").Trim().ToUpperInvariant()).Where(s => s.Length > 0), StringComparer.OrdinalIgnoreCase);
            _byConnection[connectionId] = set;
        }
    }

    public void Remove(string connectionId)
    {
        lock (_lock)
            _byConnection.Remove(connectionId);
    }

    public IReadOnlySet<string> GetSubscribedSymbols()
    {
        lock (_lock)
        {
            var combined = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var set in _byConnection.Values)
            {
                foreach (var s in set)
                    combined.Add(s);
            }
            return combined;
        }
    }
}
