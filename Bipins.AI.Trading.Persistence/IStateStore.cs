using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Persistence;

public interface IStateStore
{
    StrategyState? Get(string strategyId, string symbol);
    void Set(string strategyId, string symbol, StrategyState state);
}
