using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.PriceTransform;

/// <summary>
/// Median Price. (High + Low) / 2.
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class MedianPrice : IndicatorBase<SingleValueResult>
{
    /// <summary>
    /// Median Price.
    /// </summary>
    public MedianPrice()
        : base("Median Price", "Median Price (H+L)/2.", 1)
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
        double mid = (double)(candle.High + candle.Low) / 2;
        return new SingleValueResult(mid, true);
    }
}
