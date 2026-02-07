using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.PriceTransform;

/// <summary>
/// Weighted Close Price. (High + Low + 2*Close) / 4.
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class WeightedClosePrice : IndicatorBase<SingleValueResult>
{
    /// <summary>
    /// Weighted Close.
    /// </summary>
    public WeightedClosePrice()
        : base("Weighted Close", "Weighted Close (H+L+2*C)/4.", 1)
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
        double wc = (double)(candle.High + candle.Low + candle.Close + candle.Close) / 4;
        return new SingleValueResult(wc, true);
    }
}
