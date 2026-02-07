using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Trend;

/// <summary>
/// Aroon Indicator. Aroon Up = 100 * (period - periods since high) / period; Aroon Down = 100 * (period - periods since low) / period.
/// Reference: Tushar Chande; Investopedia.
/// </summary>
public sealed class Aroon : IndicatorBase<MultiValueResult>
{
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Aroon (Up and Down).
    /// </summary>
    /// <param name="period">Lookback period (default 25).</param>
    public Aroon(int period = 25)
        : base("Aroon", "Aroon Up and Down. Measures time since high/low.", period + 1)
    {
        Period = period;
        _highBuffer = new RingBufferDouble(period + 1);
        _lowBuffer = new RingBufferDouble(period + 1);
        AddParameter("Period", period.ToString(), "Lookback period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _highBuffer.Clear();
        _lowBuffer.Clear();
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        _highBuffer.Add((double)candle.High);
        _lowBuffer.Add((double)candle.Low);
        if (!_highBuffer.IsFull)
            return MultiValueResult.Invalid();
        int periodsSinceHigh = 0, periodsSinceLow = 0;
        double maxH = double.MinValue, minL = double.MaxValue;
        for (int i = 0; i <= Period; i++)
        {
            if (_highBuffer[i] > maxH) { maxH = _highBuffer[i]; periodsSinceHigh = i; }
            if (_lowBuffer[i] < minL) { minL = _lowBuffer[i]; periodsSinceLow = i; }
        }
        double aroonUp = 100 * (Period - periodsSinceHigh) / (double)Period;
        double aroonDown = 100 * (Period - periodsSinceLow) / (double)Period;
        return new MultiValueResult(aroonUp, aroonDown, true);
    }
}
