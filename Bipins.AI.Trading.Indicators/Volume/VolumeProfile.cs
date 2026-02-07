using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Volume;

/// <summary>
/// Volume Profile (basic). POC = price level with highest volume over period; optional VWAP at POC.
/// Simplified: returns total volume and volume-weighted average price over the period as a single "value" (we return VWAP; full profile would be multi-value).
/// Reference: Standard; used in institutional trading.
/// </summary>
public sealed class VolumeProfile : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;
    private readonly RingBufferDouble _closeBuffer;
    private readonly RingBufferDouble _volumeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Basic Volume Profile (returns period VWAP).
    /// </summary>
    /// <param name="period">Lookback period (default 20).</param>
    public VolumeProfile(int period = 20)
        : base("Volume Profile", "Basic volume profile. Period VWAP.", period)
    {
        Period = period;
        _highBuffer = new RingBufferDouble(period);
        _lowBuffer = new RingBufferDouble(period);
        _closeBuffer = new RingBufferDouble(period);
        _volumeBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Lookback period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _highBuffer.Clear();
        _lowBuffer.Clear();
        _closeBuffer.Clear();
        _volumeBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double tp = (double)(candle.High + candle.Low + candle.Close) / 3;
        double v = (double)candle.Volume;
        _highBuffer.Add((double)candle.High);
        _lowBuffer.Add((double)candle.Low);
        _closeBuffer.Add((double)candle.Close);
        _volumeBuffer.Add(v);
        if (!_volumeBuffer.IsFull)
            return SingleValueResult.Invalid;
        double sumTpV = 0, sumV = 0;
        for (int i = 0; i < Period; i++)
        {
            double t = (_highBuffer[i] + _lowBuffer[i] + _closeBuffer[i]) / 3;
            sumTpV += t * _volumeBuffer[i];
            sumV += _volumeBuffer[i];
        }
        if (sumV < 1e-20) return new SingleValueResult(_closeBuffer[0], true);
        return new SingleValueResult(sumTpV / sumV, true);
    }
}
