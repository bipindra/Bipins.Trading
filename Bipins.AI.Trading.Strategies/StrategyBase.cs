using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Strategies;

public abstract class StrategyBase : IStrategy
{
    public abstract string Name { get; }
    public virtual StrategyWarmup Warmup => StrategyWarmup.None;

    public abstract StrategyResult OnBar(StrategyContext ctx);
}
