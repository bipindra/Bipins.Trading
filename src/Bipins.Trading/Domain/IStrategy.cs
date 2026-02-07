namespace Bipins.Trading.Domain;

public interface IStrategy
{
    string Name { get; }
    StrategyWarmup Warmup { get; }
    StrategyResult OnBar(StrategyContext ctx);
    void OnTick(StrategyContext ctx, TradeTick tick) { }
}
