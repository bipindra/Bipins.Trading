using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.MovingAverages;

/// <summary>
/// Volume Weighted Moving Average (VWMA). Average of close weighted by volume.
/// Formula: VWMA = Sum(Close_i * Volume_i) / Sum(Volume_i) over period.
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class Vwma : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;
    private readonly RingBufferDouble _volumeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Volume Weighted Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Vwma(int period = 14)
        : base("VWMA", "Volume Weighted Moving Average. Weights prices by volume.", period)
    {
        Period = period;
        _closeBuffer = new RingBufferDouble(period);
        _volumeBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Number of periods");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _closeBuffer.Clear();
        _volumeBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        double v = (double)candle.Volume;
        _closeBuffer.Add(c);
        _volumeBuffer.Add(v);
        if (!_closeBuffer.IsFull)
            return SingleValueResult.Invalid;
        double sumPv = 0, sumV = 0;
        for (int i = 0; i < Period; i++)
        {
            sumPv += _closeBuffer[i] * _volumeBuffer[i];
            sumV += _volumeBuffer[i];
        }
        if (sumV < 1e-20) sumV = 1e-20;
        return new SingleValueResult(sumPv / sumV, true);
    }
}
