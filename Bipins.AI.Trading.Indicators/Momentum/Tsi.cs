using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.MovingAverages;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Momentum;

/// <summary>
/// True Strength Index (TSI). Double-smoothed momentum. Uses EMA of price change and EMA of absolute price change.
/// Formula: PC = Close - PrevClose, TSI = 100 * EMA(EMA(PC, long)) / EMA(EMA(|PC|, long)) with double smoothing.
/// Reference: William Blau; "Momentum, Direction, and Divergence".
/// </summary>
public sealed class Tsi : IndicatorBase<SingleValueResult>
{
    private readonly Ema _emaPc;
    private readonly Ema _emaAbsPc;
    private readonly Ema _emaPc2;
    private readonly Ema _emaAbsPc2;
    private double _prevClose;
    private bool _havePrev;

    /// <summary>Long period.</summary>
    public int LongPeriod { get; }

    /// <summary>Short period (inner EMA).</summary>
    public int ShortPeriod { get; }

    /// <summary>
    /// True Strength Index.
    /// </summary>
    /// <param name="longPeriod">Outer EMA period (default 25).</param>
    /// <param name="shortPeriod">Inner EMA period (default 13).</param>
    public Tsi(int longPeriod = 25, int shortPeriod = 13)
        : base("TSI", "True Strength Index. Double-smoothed momentum.", longPeriod + shortPeriod)
    {
        LongPeriod = longPeriod;
        ShortPeriod = shortPeriod;
        _emaPc = new Ema(shortPeriod);
        _emaAbsPc = new Ema(shortPeriod);
        _emaPc2 = new Ema(longPeriod);
        _emaAbsPc2 = new Ema(longPeriod);
        AddParameter("LongPeriod", longPeriod.ToString(), "Outer smoothing");
        AddParameter("ShortPeriod", shortPeriod.ToString(), "Inner smoothing");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _emaPc.Reset();
        _emaAbsPc.Reset();
        _emaPc2.Reset();
        _emaAbsPc2.Reset();
        _havePrev = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        double pc = _havePrev ? c - _prevClose : 0;
        _havePrev = true;
        _prevClose = c;
        var e1 = _emaPc.Update(new Candle(candle.Time, (decimal)pc, (decimal)pc, (decimal)pc, (decimal)pc, candle.Volume));
        var e2 = _emaAbsPc.Update(new Candle(candle.Time, (decimal)Math.Abs(pc), (decimal)Math.Abs(pc), (decimal)Math.Abs(pc), (decimal)Math.Abs(pc), candle.Volume));
        if (!e1.IsValid || !e2.IsValid)
            return SingleValueResult.Invalid;
        var e3 = _emaPc2.Update(new Candle(candle.Time, (decimal)e1.Value, (decimal)e1.Value, (decimal)e1.Value, (decimal)e1.Value, candle.Volume));
        var e4 = _emaAbsPc2.Update(new Candle(candle.Time, (decimal)e2.Value, (decimal)e2.Value, (decimal)e2.Value, (decimal)e2.Value, candle.Volume));
        if (!e3.IsValid || !e4.IsValid)
            return SingleValueResult.Invalid;
        if (e4.Value < 1e-20) return new SingleValueResult(0, true);
        double tsi = 100 * e3.Value / e4.Value;
        return new SingleValueResult(tsi, true);
    }
}
