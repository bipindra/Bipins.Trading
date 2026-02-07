using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Volume;

/// <summary>
/// Chaikin Money Flow (CMF). Sum(MFM*Volume) / Sum(Volume) over period. MFM = ((Close-Low)-(High-Close))/(High-Low).
/// Reference: Marc Chaikin; Investopedia.
/// </summary>
public sealed class Cmf : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _mfBuffer;
    private readonly RingBufferDouble _volBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Chaikin Money Flow.
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    public Cmf(int period = 20)
        : base("CMF", "Chaikin Money Flow. Money flow over volume.", period)
    {
        Period = period;
        _mfBuffer = new RingBufferDouble(period);
        _volBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _mfBuffer.Clear();
        _volBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        double v = (double)candle.Volume;
        double range = h - l;
        double mfm = range < 1e-20 ? 0 : ((c - l) - (h - c)) / range;
        _mfBuffer.Add(mfm * v);
        _volBuffer.Add(v);
        if (!_mfBuffer.IsFull)
            return SingleValueResult.Invalid;
        double sumMf = _mfBuffer.Sum();
        double sumV = _volBuffer.Sum();
        if (sumV < 1e-20) return new SingleValueResult(0, true);
        return new SingleValueResult(sumMf / sumV, true);
    }
}
