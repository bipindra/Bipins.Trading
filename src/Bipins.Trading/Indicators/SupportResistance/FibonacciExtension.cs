using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.SupportResistance;

/// <summary>
/// Fibonacci Extension levels. Based on high-low range; extension levels above high (e.g. 1.272, 1.618).
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class FibonacciExtension : IndicatorBase<MultiValueResult>
{
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Fibonacci Extension (0.618, 1.0, 1.272, 1.618 of range from low).
    /// </summary>
    /// <param name="period">Lookback (default 20).</param>
    public FibonacciExtension(int period = 20)
        : base("Fib Extension", "Fibonacci extension levels.", period)
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
        double e0618 = low + 0.618 * range;
        double e1 = high;
        double e1272 = low + 1.272 * range;
        double e1618 = low + 1.618 * range;
        return new MultiValueResult(e0618, e1, e1272, e1618, true);
    }
}
