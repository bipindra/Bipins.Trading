using Bipins.AI.Trading.Domain;
using Bipins.AI.Trading.Indicators.Volatility;

namespace Bipins.AI.Trading.Strategies;

public sealed class BreakoutDonchianStrategy : StrategyBase
{
    private readonly int _period;

    public override string Name => "BreakoutDonchian";
    public override StrategyWarmup Warmup => StrategyWarmup.Bars(_period + 5);

    public BreakoutDonchianStrategy(int period = 20)
    {
        _period = period;
    }

    public override StrategyResult OnBar(StrategyContext ctx)
    {
        var signals = new List<SignalEvent>();
        var orders = new List<OrderIntent>();

        var donchian = ctx.Indicators.GetMulti(IndicatorKey.Donchian(_period, ctx.Timeframe), () => ComputeDonchian(ctx, _period));
        if (donchian == null || donchian.Count < 3 || ctx.Candles.Count < 2) return new StrategyResult { Signals = signals, Orders = orders };

        var upper = donchian[0];
        var lower = donchian[2];
        var current = ctx.Candles[^1];
        var prev = ctx.Candles[^2];
        var price = current.Close;

        if (ctx.CurrentPosition.Side == PositionSide.Flat)
        {
            if (current.Close > (decimal)upper && prev.Close <= (decimal)upper)
            {
                signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.EntryLong, price, "Donchian breakout up"));
                orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Buy, Domain.OrderType.Market, TimeInForce.GTC, null, null, null, null, "Breakout long", null, null));
            }
            else if (current.Close < (decimal)lower && prev.Close >= (decimal)lower)
            {
                signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.EntryShort, price, "Donchian breakout down"));
                orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Sell, Domain.OrderType.Market, TimeInForce.GTC, null, null, null, null, "Breakout short", null, null));
            }
        }
        else if (ctx.CurrentPosition.Side == PositionSide.Long && current.Close < (decimal)lower)
        {
            signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.ExitLong, price, "Donchian exit long"));
            orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Sell, Domain.OrderType.Market, TimeInForce.GTC, ctx.CurrentPosition.Quantity, null, null, null, "Exit long", null, null));
        }
        else if (ctx.CurrentPosition.Side == PositionSide.Short && current.Close > (decimal)upper)
        {
            signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.ExitShort, price, "Donchian exit short"));
            orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Buy, Domain.OrderType.Market, TimeInForce.GTC, ctx.CurrentPosition.Quantity, null, null, null, "Exit short", null, null));
        }

        return new StrategyResult { Signals = signals, Orders = orders };
    }

    private static IReadOnlyList<double>? ComputeDonchian(StrategyContext ctx, int period)
    {
        var list = ctx.Candles.Select(CandleHelper.ToIndicator).ToList();
        if (list.Count == 0) return null;
        var donch = new DonchianChannels(period);
        double[]? last = null;
        foreach (var c in list)
        {
            var r = donch.Update(c);
            if (r.IsValid) last = new[] { r.Upper, r.Middle, r.Lower };
        }
        return last;
    }
}
