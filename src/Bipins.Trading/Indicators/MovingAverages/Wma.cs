using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.MovingAverages;

/// <summary>
/// Weighted Moving Average (WMA). Linear weights 1,2,...,N (newest weighted highest).
/// Formula: WMA = (1*C1 + 2*C2 + ... + N*Cn) / (1+2+...+N)
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class Wma : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _buffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Weighted Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Wma(int period = 14)
        : base("WMA", "Weighted Moving Average. Linear weights with newest highest.", period)
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
        double sum = 0, weightSum = 0;
        for (int i = 0; i < Period; i++)
        {
            double w = i + 1;
            sum += w * _buffer[i];
            weightSum += w;
        }
        return new SingleValueResult(sum / weightSum, true);
    }
}
