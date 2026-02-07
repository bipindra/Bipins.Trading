using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.MovingAverages;

/// <summary>
/// Double Exponential Moving Average (DEMA). Reduces lag: DEMA = 2*EMA - EMA(EMA).
/// Formula: EMA1 = EMA(Close), EMA2 = EMA(EMA1), DEMA = 2*EMA1 - EMA2.
/// Reference: Patrick Mulloy; TASC.
/// </summary>
public sealed class Dema : IndicatorBase<SingleValueResult>
{
    private readonly Ema _ema1;
    private double _ema2;
    private bool _ema2Initialized;
    private readonly double _alpha;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Double Exponential Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Dema(int period = 14)
        : base("DEMA", "Double Exponential Moving Average. Reduced lag EMA.", period)
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
        double dema = 2 * ema1 - _ema2;
        return new SingleValueResult(dema, true);
    }
}
