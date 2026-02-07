using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Advanced;

/// <summary>
/// Choppiness Index. 100 * Log10(Sum(ATR(1)) / (Highest High - Lowest Low)) / Log10(period). Measures choppiness 0-100.
/// Reference: E.W. Dreiss; Investopedia.
/// </summary>
public sealed class ChoppinessIndex : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _atrBuffer;
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;
    private double _prevClose;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Choppiness Index.
    /// </summary>
    /// <param name="period">Period (default 14).</param>
    public ChoppinessIndex(int period = 14)
        : base("Choppiness Index", "Choppiness Index 0-100.", period)
    {
        Period = period;
        _atrBuffer = new RingBufferDouble(period);
        _highBuffer = new RingBufferDouble(period);
        _lowBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _atrBuffer.Clear();
        _highBuffer.Clear();
        _lowBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        double tr = UpdateCount > 0 ? (double)Candle.TrueRange((decimal)h, (decimal)l, (decimal)_prevClose) : h - l;
        _prevClose = c;
        _atrBuffer.Add(tr);
        _highBuffer.Add(h);
        _lowBuffer.Add(l);
        if (!_atrBuffer.IsFull)
            return SingleValueResult.Invalid;
        double sumAtr = _atrBuffer.Sum();
        double highest = double.MinValue, lowest = double.MaxValue;
        for (int i = 0; i < Period; i++)
        {
            if (_highBuffer[i] > highest) highest = _highBuffer[i];
            if (_lowBuffer[i] < lowest) lowest = _lowBuffer[i];
        }
        double range = highest - lowest;
        if (range < 1e-20) return new SingleValueResult(100, true);
        double ci = 100 * Math.Log10(sumAtr / range) / Math.Log10(Period);
        return new SingleValueResult(Math.Clamp(ci, 0, 100), true);
    }
}
