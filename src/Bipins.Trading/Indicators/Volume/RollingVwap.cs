using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Volume;

/// <summary>
/// Rolling VWAP. VWAP over the last N bars (Typical Price * Volume sum / Volume sum).
/// Reference: Standard.
/// </summary>
public sealed class RollingVwap : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _tpVBuffer;
    private readonly RingBufferDouble _volBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Rolling VWAP.
    /// </summary>
    /// <param name="period">Lookback period (default 20).</param>
    public RollingVwap(int period = 20)
        : base("Rolling VWAP", "VWAP over last N bars.", period)
    {
        Period = period;
        _tpVBuffer = new RingBufferDouble(period);
        _volBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Lookback period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _tpVBuffer.Clear();
        _volBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double tp = (double)(candle.High + candle.Low + candle.Close) / 3;
        double v = (double)candle.Volume;
        _tpVBuffer.Add(tp * v);
        _volBuffer.Add(v);
        if (!_tpVBuffer.IsFull)
            return SingleValueResult.Invalid;
        double sumTpV = _tpVBuffer.Sum();
        double sumV = _volBuffer.Sum();
        if (sumV < 1e-20) return new SingleValueResult(tp, true);
        return new SingleValueResult(sumTpV / sumV, true);
    }
}
