using Bipins.Trading.Indicators.Advanced;
using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Statistical;

/// <summary>
/// Z-Score of close. Wrapper of Advanced.ZScoreIndicator.
/// Reference: Standard statistics.
/// </summary>
public sealed class ZScore : IndicatorBase<SingleValueResult>
{
    private readonly ZScoreIndicator _inner;

    /// <summary>
    /// Z-Score.
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    public ZScore(int period = 20)
        : base("Z-Score", "Z-Score of close vs period mean.", period)
    {
        _inner = new ZScoreIndicator(period);
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _inner.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        return _inner.Update(candle);
    }
}
