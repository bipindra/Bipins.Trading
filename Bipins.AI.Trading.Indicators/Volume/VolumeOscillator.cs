using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Volume;

/// <summary>
/// Volume Oscillator. (Short MA(Volume) - Long MA(Volume)) / Long MA(Volume) * 100 or difference.
/// Reference: Standard.
/// </summary>
public sealed class VolumeOscillator : IndicatorBase<SingleValueResult>
{
    private readonly MovingAverages.Sma _shortMa;
    private readonly MovingAverages.Sma _longMa;

    /// <summary>
    /// Volume Oscillator.
    /// </summary>
    /// <param name="shortPeriod">Short period (default 5).</param>
    /// <param name="longPeriod">Long period (default 10).</param>
    public VolumeOscillator(int shortPeriod = 5, int longPeriod = 10)
        : base("Volume Oscillator", "Volume short MA minus long MA.", longPeriod)
    {
        _shortMa = new MovingAverages.Sma(shortPeriod);
        _longMa = new MovingAverages.Sma(longPeriod);
        AddParameter("Short", shortPeriod.ToString(), "Short period");
        AddParameter("Long", longPeriod.ToString(), "Long period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _shortMa.Reset();
        _longMa.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var volCandle = new Candle(candle.Time, candle.Volume, candle.Volume, candle.Volume, candle.Volume, candle.Volume);
        var s = _shortMa.Update(volCandle);
        var l = _longMa.Update(volCandle);
        if (!s.IsValid || !l.IsValid)
            return SingleValueResult.Invalid;
        double vo = l.Value < 1e-20 ? 0 : 100 * (s.Value - l.Value) / l.Value;
        return new SingleValueResult(vo, true);
    }
}
