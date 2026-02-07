using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Momentum;

/// <summary>
/// Relative Vigor Index (RVI). Close position relative to range, smoothed. (Close - Open) / (High - Low) then SMA.
/// Reference: John Ehlers; Investopedia.
/// </summary>
public sealed class Rvi : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _rviBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Relative Vigor Index.
    /// </summary>
    /// <param name="period">Smoothing period (default 10).</param>
    public Rvi(int period = 10)
        : base("RVI", "Relative Vigor Index. Close position in range.", period)
    {
        Period = period;
        _rviBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Smoothing period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _rviBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double o = (double)candle.Open;
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        double range = h - l;
        double raw = range < 1e-20 ? 0 : (c - o) / range;
        _rviBuffer.Add(raw);
        if (!_rviBuffer.IsFull)
            return SingleValueResult.Invalid;
        double rvi = _rviBuffer.Sum() / Period;
        return new SingleValueResult(rvi, true);
    }
}
