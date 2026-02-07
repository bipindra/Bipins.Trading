using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.MovingAverages;

namespace Bipins.AI.Trading.Indicators.Volume;

/// <summary>
/// Force Index. (Close - PrevClose) * Volume, optionally smoothed by EMA.
/// Reference: Alexander Elder; Investopedia.
/// </summary>
public sealed class ForceIndex : IndicatorBase<SingleValueResult>
{
    private readonly Ema? _ema;
    private double _prevClose;
    private bool _havePrev;

    /// <summary>EMA period (0 = raw).</summary>
    public int Period { get; }

    /// <summary>
    /// Force Index.
    /// </summary>
    /// <param name="period">EMA period for smoothing (0 = no smoothing, default 2).</param>
    public ForceIndex(int period = 2)
        : base("Force Index", "Force Index. (Close-PrevClose)*Volume.", period <= 0 ? 1 : period)
    {
        Period = period;
        _ema = period > 0 ? new Ema(period) : null;
        AddParameter("Period", period.ToString(), "EMA smoothing period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _ema?.Reset();
        _havePrev = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        double v = (double)candle.Volume;
        double force = _havePrev ? (c - _prevClose) * v : 0;
        _prevClose = c;
        _havePrev = true;
        if (_ema == null)
            return new SingleValueResult(force, true);
        var r = _ema.Update(new Candle(candle.Time, (decimal)force, (decimal)force, (decimal)force, (decimal)force, candle.Volume));
        return r.IsValid ? new SingleValueResult(r.Value, true) : SingleValueResult.Invalid;
    }
}
