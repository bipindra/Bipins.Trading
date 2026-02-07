using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.MovingAverages;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Trend;

/// <summary>
/// TRIX. Triple-smoothed EMA rate of change. TRIX = percent change of triple EMA(Close).
/// Reference: Jack Hutson; Investopedia.
/// </summary>
public sealed class Trix : IndicatorBase<SingleValueResult>
{
    private readonly Ema _ema1;
    private readonly Ema _ema2;
    private readonly Ema _ema3;
    private double _prevE3;
    private bool _havePrevE3;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// TRIX.
    /// </summary>
    /// <param name="period">EMA period (default 14).</param>
    public Trix(int period = 14)
        : base("TRIX", "TRIX. Triple-smoothed EMA ROC.", period * 3)
    {
        Period = period;
        _ema1 = new Ema(period);
        _ema2 = new Ema(period);
        _ema3 = new Ema(period);
        AddParameter("Period", period.ToString(), "EMA period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _ema1.Reset();
        _ema2.Reset();
        _ema3.Reset();
        _havePrevE3 = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var e1 = _ema1.Update(candle);
        if (!e1.IsValid) return SingleValueResult.Invalid;
        var e2 = _ema2.Update(new Candle(candle.Time, (decimal)e1.Value, (decimal)e1.Value, (decimal)e1.Value, (decimal)e1.Value, candle.Volume));
        if (!e2.IsValid) return SingleValueResult.Invalid;
        var e3 = _ema3.Update(new Candle(candle.Time, (decimal)e2.Value, (decimal)e2.Value, (decimal)e2.Value, (decimal)e2.Value, candle.Volume));
        if (!e3.IsValid) return SingleValueResult.Invalid;
        if (!_havePrevE3)
        {
            _prevE3 = e3.Value;
            _havePrevE3 = true;
            return new SingleValueResult(0, true);
        }
        double trix = Math.Abs(_prevE3) < 1e-20 ? 0 : 100 * (e3.Value - _prevE3) / _prevE3;
        _prevE3 = e3.Value;
        return new SingleValueResult(trix, true);
    }
}
