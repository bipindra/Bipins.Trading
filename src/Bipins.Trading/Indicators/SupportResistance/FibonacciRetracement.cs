using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.SupportResistance;

/// <summary>
/// Fibonacci Retracement levels. Based on high-low range over period. Returns level at given ratio (e.g. 0.382, 0.618).
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class FibonacciRetracement : IndicatorBase<MultiValueResult>
{
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Fibonacci Retracement (High, Low, 0.382, 0.5, 0.618 levels).
    /// </summary>
    /// <param name="period">Lookback for swing high/low (default 20).</param>
    public FibonacciRetracement(int period = 20)
        : base("Fib Retracement", "Fibonacci retracement levels.", period)
    {
        Period = period;
        _highBuffer = new RingBufferDouble(period);
        _lowBuffer = new RingBufferDouble(period);
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
        double high = double.MinValue, low = double.MaxValue;
        for (int i = 0; i < Period; i++)
        {
            if (_highBuffer[i] > high) high = _highBuffer[i];
            if (_lowBuffer[i] < low) low = _lowBuffer[i];
        }
        double range = high - low;
        double l382 = high - 0.382 * range;
        double l5 = high - 0.5 * range;
        double l618 = high - 0.618 * range;
        return new MultiValueResult(high, low, l382, l5, l618, true);
    }
}
