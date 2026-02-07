using Bipins.Trading.Domain;
using Bipins.Trading.Indicators.Momentum;

namespace Bipins.Trading.Strategies;

public sealed class RsiMeanReversionStrategy : StrategyBase
{
    private readonly int _period;
    private readonly double _oversold;
    private readonly double _overbought;

    public override string Name => "RsiMeanReversion";
    public override StrategyWarmup Warmup => StrategyWarmup.Bars(_period + 20);

    public RsiMeanReversionStrategy(int period = 14, double oversold = 30, double overbought = 70)
    {
        _period = period;
        _oversold = oversold;
        _overbought = overbought;
    }

    public override StrategyResult OnBar(StrategyContext ctx)
    {
        var signals = new List<SignalEvent>();
        var orders = new List<OrderIntent>();

        var rsi = ctx.Indicators.Get(IndicatorKey.Rsi(_period, ctx.Timeframe), () => ComputeRsi(ctx, _period));
        if (!rsi.HasValue || ctx.Candles.Count < 2) return new StrategyResult { Signals = signals, Orders = orders };

        var current = ctx.Candles[^1];
        var price = current.Close;

        if (ctx.CurrentPosition.Side == PositionSide.Flat)
        {
            if (rsi.Value <= _oversold)
            {
                signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.EntryLong, price, "RSI oversold"));
                orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Buy, Domain.OrderType.Market, TimeInForce.GTC, null, null, null, null, "RSI oversold", null, null));
            }
            else if (rsi.Value >= _overbought)
            {
                signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.EntryShort, price, "RSI overbought"));
                orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Sell, Domain.OrderType.Market, TimeInForce.GTC, null, null, null, null, "RSI overbought", null, null));
            }
        }
        else if (ctx.CurrentPosition.Side == PositionSide.Long && rsi.Value >= _overbought)
        {
            signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.ExitLong, price, "RSI overbought exit"));
            orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Sell, Domain.OrderType.Market, TimeInForce.GTC, ctx.CurrentPosition.Quantity, null, null, null, "Exit long", null, null));
        }
        else if (ctx.CurrentPosition.Side == PositionSide.Short && rsi.Value <= _oversold)
        {
            signals.Add(new SignalEvent(Name, ctx.Symbol, current.Time, SignalType.ExitShort, price, "RSI oversold exit"));
            orders.Add(new OrderIntent(Name, ctx.Symbol, current.Time, OrderSide.Buy, Domain.OrderType.Market, TimeInForce.GTC, ctx.CurrentPosition.Quantity, null, null, null, "Exit short", null, null));
        }

        return new StrategyResult { Signals = signals, Orders = orders };
    }

    private static double? ComputeRsi(StrategyContext ctx, int period)
    {
        var list = ctx.Candles.Select(CandleHelper.ToIndicator).ToList();
        if (list.Count == 0) return null;
        var rsi = new Rsi(period);
        double? last = null;
        foreach (var c in list) { var r = rsi.Update(c); if (r.IsValid) last = r.Value; }
        return last;
    }
}
