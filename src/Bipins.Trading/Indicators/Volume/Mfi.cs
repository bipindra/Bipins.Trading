using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Volume;

/// <summary>
/// Money Flow Index (MFI). Volume-weighted RSI using typical price. 100 - 100/(1 + Money Ratio). Money Ratio = Positive MF / Negative MF.
/// Reference: Gene Quong, Avrum Soudack; Investopedia.
/// </summary>
public sealed class Mfi : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _positiveMf;
    private readonly RingBufferDouble _negativeMf;
    private double _prevTp;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Money Flow Index.
    /// </summary>
    /// <param name="period">Period (default 14).</param>
    public Mfi(int period = 14)
        : base("MFI", "Money Flow Index. Volume-weighted RSI.", period + 1)
    {
        Period = period;
        _positiveMf = new RingBufferDouble(period);
        _negativeMf = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _positiveMf.Clear();
        _negativeMf.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double tp = (double)(candle.High + candle.Low + candle.Close) / 3;
        double v = (double)candle.Volume;
        double rawMf = tp * v;
        if (UpdateCount > 0)
        {
            if (tp > _prevTp) { _positiveMf.Add(rawMf); _negativeMf.Add(0); }
            else if (tp < _prevTp) { _positiveMf.Add(0); _negativeMf.Add(rawMf); }
            else { _positiveMf.Add(0); _negativeMf.Add(0); }
        }
        _prevTp = tp;
        if (!_positiveMf.IsFull)
            return SingleValueResult.Invalid;
        double posSum = _positiveMf.Sum();
        double negSum = _negativeMf.Sum();
        if (negSum < 1e-20) return new SingleValueResult(100, true);
        double mr = posSum / negSum;
        double mfi = 100 - 100 / (1 + mr);
        return new SingleValueResult(mfi, true);
    }
}
