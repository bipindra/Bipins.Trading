using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Momentum;

/// <summary>
/// Commodity Channel Index (CCI). (Typical Price - SMA(TP)) / (0.015 * Mean Deviation). Typical Price = (H+L+C)/3.
/// Reference: Donald Lambert; Investopedia.
/// </summary>
public sealed class Cci : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _tpBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Commodity Channel Index.
    /// </summary>
    /// <param name="period">Number of periods (default 20).</param>
    public Cci(int period = 20)
        : base("CCI", "Commodity Channel Index. Measures deviation from average price.", period)
    {
        Period = period;
        _tpBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Number of periods");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _tpBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double tp = (double)(candle.High + candle.Low + candle.Close) / 3;
        _tpBuffer.Add(tp);
        if (!_tpBuffer.IsFull)
            return SingleValueResult.Invalid;
        double sma = _tpBuffer.Sum() / Period;
        double sumDev = 0;
        for (int i = 0; i < Period; i++)
            sumDev += Math.Abs(_tpBuffer[i] - sma);
        double meanDev = sumDev / Period;
        if (meanDev < 1e-20) meanDev = 1e-20;
        double cci = (tp - sma) / (0.015 * meanDev);
        return new SingleValueResult(cci, true);
    }
}
