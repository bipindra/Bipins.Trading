using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.MovingAverages;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Volatility;

/// <summary>
/// Mass Index. Sum of single EMA(High-Low) / double EMA(High-Low) over period. Used to spot reversals.
/// Reference: Donald Dorsey; Investopedia.
/// </summary>
public sealed class MassIndex : IndicatorBase<SingleValueResult>
{
    private readonly MovingAverages.Ema _ema1;
    private readonly MovingAverages.Ema _ema2;
    private readonly RingBufferDouble _ratioBuffer;

    /// <summary>Single EMA period.</summary>
    public int Period { get; }

    /// <summary>Sum period.</summary>
    public int SumPeriod { get; }

    /// <summary>
    /// Mass Index.
    /// </summary>
    /// <param name="period">EMA period (default 9).</param>
    /// <param name="sumPeriod">Sum period (default 25).</param>
    public MassIndex(int period = 9, int sumPeriod = 25)
        : base("Mass Index", "Mass Index. High-low range ratio sum.", period + sumPeriod)
    {
        Period = period;
        SumPeriod = sumPeriod;
        _ema1 = new Ema(period);
        _ema2 = new Ema(period);
        _ratioBuffer = new RingBufferDouble(sumPeriod);
        AddParameter("Period", period.ToString(), "EMA period");
        AddParameter("SumPeriod", sumPeriod.ToString(), "Sum period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _ema1.Reset();
        _ema2.Reset();
        _ratioBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double hl = (double)(candle.High - candle.Low);
        var e1 = _ema1.Update(new Candle(candle.Time, (decimal)hl, (decimal)hl, (decimal)hl, (decimal)hl, candle.Volume));
        if (!e1.IsValid) return SingleValueResult.Invalid;
        var e2 = _ema2.Update(new Candle(candle.Time, (decimal)e1.Value, (decimal)e1.Value, (decimal)e1.Value, (decimal)e1.Value, candle.Volume));
        if (!e2.IsValid) return SingleValueResult.Invalid;
        double ratio = e2.Value < 1e-20 ? 1 : e1.Value / e2.Value;
        _ratioBuffer.Add(ratio);
        if (!_ratioBuffer.IsFull)
            return SingleValueResult.Invalid;
        double sum = _ratioBuffer.Sum();
        return new SingleValueResult(sum, true);
    }
}
