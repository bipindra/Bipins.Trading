using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Volume;

/// <summary>
/// Ease of Movement (EMV). (Distance moved) / (Volume ratio). MidPoint move / (Volume / (High-Low)).
/// Reference: Richard Arms; Investopedia.
/// </summary>
public sealed class Emv : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _emvBuffer;
    private double _prevMid;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Ease of Movement.
    /// </summary>
    /// <param name="period">Smoothing period (default 14).</param>
    public Emv(int period = 14)
        : base("EMV", "Ease of Movement. Price move per volume unit.", period)
    {
        Period = period;
        _emvBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Smoothing period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _emvBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double v = (double)candle.Volume;
        double mid = (h + l) / 2;
        double dist = UpdateCount > 0 ? mid - _prevMid : 0;
        _prevMid = mid;
        double range = h - l;
        double br = range < 1e-20 ? 1e-20 : v / range;
        if (br < 1e-20) br = 1e-20;
        double emv = dist / br;
        _emvBuffer.Add(emv);
        if (!_emvBuffer.IsFull)
            return SingleValueResult.Invalid;
        return new SingleValueResult(_emvBuffer.Sum() / Period, true);
    }
}
