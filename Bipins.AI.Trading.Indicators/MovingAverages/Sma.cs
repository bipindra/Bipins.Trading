using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.MovingAverages;

/// <summary>
/// Simple Moving Average (SMA). Average of the last N closing prices.
/// Formula: SMA = (C1 + C2 + ... + Cn) / N
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class Sma : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _buffer;

    /// <summary>Period (number of bars).</summary>
    public int Period { get; }

    /// <summary>
    /// Simple Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Sma(int period = 14)
        : base("SMA", "Simple Moving Average. Average of the last N closing prices.", period)
    {
        Period = period;
        _buffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Number of periods");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _buffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        _buffer.Add((double)candle.Close);
        if (!_buffer.IsFull)
            return SingleValueResult.Invalid;
        double sum = _buffer.Sum();
        double value = sum / Period;
        return new SingleValueResult(value, true);
    }
}
