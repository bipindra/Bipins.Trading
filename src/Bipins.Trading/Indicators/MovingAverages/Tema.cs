using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.MovingAverages;

/// <summary>
/// Triple Exponential Moving Average (TEMA). Further reduces lag: TEMA = 3*EMA1 - 3*EMA2 + EMA3.
/// Reference: Patrick Mulloy; TASC.
/// </summary>
public sealed class Tema : IndicatorBase<SingleValueResult>
{
    private readonly Ema _ema1;
    private double _ema2, _ema3;
    private bool _ema2Initialized, _ema3Initialized;
    private readonly double _alpha;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Triple Exponential Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Tema(int period = 14)
        : base("TEMA", "Triple Exponential Moving Average. Minimal lag triple smoothing.", period)
    {
        Period = period;
        _alpha = 2.0 / (period + 1);
        _ema1 = new Ema(period);
        AddParameter("Period", period.ToString(), "Number of periods");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _ema1.Reset();
        _ema2Initialized = false;
        _ema3Initialized = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var r = _ema1.Update(candle);
        if (!r.IsValid)
            return SingleValueResult.Invalid;
        double ema1 = r.Value;
        if (!_ema2Initialized)
        {
            _ema2 = ema1;
            _ema2Initialized = true;
        }
        else
        {
            _ema2 = _alpha * ema1 + (1 - _alpha) * _ema2;
        }
        if (!_ema3Initialized)
        {
            _ema3 = _ema2;
            _ema3Initialized = true;
        }
        else
        {
            _ema3 = _alpha * _ema2 + (1 - _alpha) * _ema3;
        }
        double tema = 3 * ema1 - 3 * _ema2 + _ema3;
        return new SingleValueResult(tema, true);
    }
}
