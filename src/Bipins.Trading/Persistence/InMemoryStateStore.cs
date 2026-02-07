using Bipins.Trading.Domain;
using System.Collections.Concurrent;

namespace Bipins.Trading.Persistence;

public sealed class InMemoryStateStore : IStateStore
{
    private readonly ConcurrentDictionary<string, StrategyState> _store = new();

    private static string Key(string strategyId, string symbol) => $"{strategyId}|{symbol}";

    public StrategyState? Get(string strategyId, string symbol) =>
        _store.TryGetValue(Key(strategyId, symbol), out var s) ? s : null;

    public void Set(string strategyId, string symbol, StrategyState state) =>
        _store[Key(strategyId, symbol)] = state;
}
