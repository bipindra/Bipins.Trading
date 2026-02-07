using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.MovingAverages;

namespace Bipins.AI.Trading.Indicators.Oscillators;

/// <summary>
/// Awesome Oscillator. SMA(Mid, 5) - SMA(Mid, 34). Mid = (High+Low)/2.
/// Reference: Bill Williams; Investopedia.
/// </summary>
public sealed class AwesomeOscillator : IndicatorBase<SingleValueResult>
{
    private readonly Sma _smaFast;
    private readonly Sma _smaSlow;

    /// <summary>
    /// Awesome Oscillator.
    /// </summary>
    /// <param name="fastPeriod">Fast period (default 5).</param>
    /// <param name="slowPeriod">Slow period (default 34).</param>
    public AwesomeOscillator(int fastPeriod = 5, int slowPeriod = 34)
        : base("Awesome Oscillator", "SMA(5) - SMA(34) of median price.", slowPeriod)
    {
        _smaFast = new Sma(fastPeriod);
        _smaSlow = new Sma(slowPeriod);
        AddParameter("Fast", fastPeriod.ToString(), "Fast period");
        AddParameter("Slow", slowPeriod.ToString(), "Slow period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _smaFast.Reset();
        _smaSlow.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double mid = (double)(candle.High + candle.Low) / 2;
        var midCandle = new Candle(candle.Time, (decimal)mid, (decimal)mid, (decimal)mid, (decimal)mid, candle.Volume);
        var f = _smaFast.Update(midCandle);
        var s = _smaSlow.Update(midCandle);
        if (!f.IsValid || !s.IsValid)
            return SingleValueResult.Invalid;
        return new SingleValueResult(f.Value - s.Value, true);
    }
}
