using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.PriceTransform;

/// <summary>
/// Typical Price. (High + Low + Close) / 3.
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class TypicalPrice : IndicatorBase<SingleValueResult>
{
    /// <summary>
    /// Typical Price.
    /// </summary>
    public TypicalPrice()
        : base("Typical Price", "Typical Price (H+L+C)/3.", 1)
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
        double tp = (double)(candle.High + candle.Low + candle.Close) / 3;
        return new SingleValueResult(tp, true);
    }
}
