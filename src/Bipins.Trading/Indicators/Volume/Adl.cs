using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Volume;

/// <summary>
/// Accumulation Distribution Line (ADL). Cumulative MFM * Volume; MFM = ((Close-Low)-(High-Close))/(High-Low).
/// Reference: Marc Chaikin; Investopedia.
/// </summary>
public sealed class Adl : IndicatorBase<SingleValueResult>
{
    private double _adl;

    /// <summary>
    /// Accumulation Distribution Line.
    /// </summary>
    public Adl()
        : base("ADL", "Accumulation Distribution Line. Money flow cumulative.", 1)
    {
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _adl = 0;
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
        _adl += mfm * v;
        return new SingleValueResult(_adl, true);
    }
}
