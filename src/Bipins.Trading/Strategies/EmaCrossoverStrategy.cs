using Bipins.Trading.Domain;
using Bipins.Trading.Indicators.MovingAverages;

namespace Bipins.Trading.Strategies;

public sealed class EmaCrossoverStrategy : StrategyBase
{
    private readonly int _fastPeriod;
    private readonly int _slowPeriod;

    public override string Name => "EmaCrossover";
    public override StrategyWarmup Warmup => StrategyWarmup.Bars(Math.Max(_fastPeriod, _slowPeriod) + 10);

    public EmaCrossoverStrategy(int fastPeriod = 12, int slowPeriod = 26)
    {
        _fastPeriod = fastPeriod;
        _slowPeriod = slowPeriod;
    }

    public override StrategyResult OnBar(StrategyContext ctx)
    {
        var signals = new List<SignalEvent>();
        var orders = new List<OrderIntent>();

        var fast = ctx.Indicators.Get(IndicatorKey.Ema(_fastPeriod, ctx.Timeframe), () => ComputeEma(ctx, _fastPeriod));
        var slow = ctx.Indicators.Get(IndicatorKey.Ema(_slowPeriod, ctx.Timeframe), () => ComputeEma(ctx, _slowPeriod));
        if (!fast.HasValue || !slow.HasValue || ctx.Candles.Count < 2) return new StrategyResult { Signals = signals, Orders = orders };

        var prevKeyFast = IndicatorKey.Ema(_fastPeriod, ctx.Timeframe).PreviousBar();
        var prevKeySlow = IndicatorKey.Ema(_slowPeriod, ctx.Timeframe).PreviousBar();
        var prevFast = ctx.Indicators.Get(prevKeyFast, () => ComputeEma(ctx, _fastPeriod, ctx.Candles.Count - 1));
        var prevSlow = ctx.Indicators.Get(prevKeySlow, () => ComputeEma(ctx, _slowPeriod, ctx.Candles.Count - 1));
        if (!prevFast.HasValue || !prevSlow.HasValue) return new StrategyResult { Signals = signals, Orders = orders };

        var current = ctx.Candles[^1];
        var price = current.Close;

        if (ctx.CurrentPosition.Side == PositionSide.Flat)
        {
            if (prevFast < prevSlow && fast > slow)
            {
                signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.EntryLong, price, "EMA cross up"));
                orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Buy, Domain.OrderType.Market, TimeInForce.GTC, null, null, null, null, "EMA cross", null, null));
            }
            else if (prevFast > prevSlow && fast < slow)
            {
                signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.EntryShort, price, "EMA cross down"));
                orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Sell, Domain.OrderType.Market, TimeInForce.GTC, null, null, null, null, "EMA cross", null, null));
            }
        }
        else if (ctx.CurrentPosition.Side == PositionSide.Long && prevFast > prevSlow && fast < slow)
        {
            signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.ExitLong, price, "EMA cross down"));
            orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Sell, Domain.OrderType.Market, TimeInForce.GTC, ctx.CurrentPosition.Quantity, null, null, null, "Exit long", null, null));
        }
        else if (ctx.CurrentPosition.Side == PositionSide.Short && prevFast < prevSlow && fast > slow)
        {
            signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.ExitShort, price, "EMA cross up"));
            orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Buy, Domain.OrderType.Market, TimeInForce.GTC, ctx.CurrentPosition.Quantity, null, null, null, "Exit short", null, null));
        }

        return new StrategyResult { Signals = signals, Orders = orders };
    }

    private static double? ComputeEma(StrategyContext ctx, int period, int? upToCount = null)
    {
        var list = ctx.Candles.Take(upToCount ?? ctx.Candles.Count).Select(CandleHelper.ToIndicator).ToList();
        if (list.Count == 0) return null;
        var ema = new Ema(period);
        double? last = null;
        foreach (var c in list) { var r = ema.Update(c); if (r.IsValid) last = r.Value; }
        return last;
    }
}
