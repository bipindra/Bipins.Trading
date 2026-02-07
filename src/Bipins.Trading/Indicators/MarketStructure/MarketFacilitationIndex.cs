using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.MarketStructure;

/// <summary>
/// Market Facilitation Index (MFI). (High - Low) / Volume. Measures efficiency of price movement per unit volume.
/// Reference: Bill Williams; Investopedia.
/// </summary>
public sealed class MarketFacilitationIndex : IndicatorBase<SingleValueResult>
{
    /// <summary>
    /// Market Facilitation Index.
    /// </summary>
    public MarketFacilitationIndex()
        : base("MFI (Market Facilitation)", "Market Facilitation Index. Range/Volume.", 1)
    {
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double range = (double)(candle.High - candle.Low);
        double v = (double)candle.Volume;
        if (v < 1e-20) v = 1e-20;
        double mfi = range / v;
        return new SingleValueResult(mfi, true);
    }
}
