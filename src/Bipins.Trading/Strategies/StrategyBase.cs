using Bipins.Trading.Domain;

namespace Bipins.Trading.Strategies;

public abstract class StrategyBase : IStrategy
{
    public abstract string Name { get; }
    public virtual StrategyWarmup Warmup => StrategyWarmup.None;

    public abstract StrategyResult OnBar(StrategyContext ctx);
}
