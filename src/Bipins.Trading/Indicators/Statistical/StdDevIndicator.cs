using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Volatility;

namespace Bipins.Trading.Indicators.Statistical;

/// <summary>
/// Standard Deviation of close. Wrapper of Volatility.StandardDeviation.
/// Reference: Standard statistics.
/// </summary>
public sealed class StdDevIndicator : IndicatorBase<SingleValueResult>
{
    private readonly StandardDeviation _inner;

    /// <summary>
    /// Standard Deviation.
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    public StdDevIndicator(int period = 20)
        : base("StdDev", "Standard Deviation of close.", period)
    {
        _inner = new StandardDeviation(period, sample: true);
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
