using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Momentum;

/// <summary>
/// Momentum Indicator. Close - Close_n. Raw price change over N periods.
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class MomentumIndicator : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Momentum (price difference).
    /// </summary>
    /// <param name="period">Lookback period (default 10).</param>
    public MomentumIndicator(int period = 10)
        : base("Momentum", "Momentum. Close - Close_n.", period + 1)
    {
        Period = period;
        _closeBuffer = new RingBufferDouble(period + 1);
        AddParameter("Period", period.ToString(), "Lookback period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _closeBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        _closeBuffer.Add(c);
        if (!_closeBuffer.IsFull)
            return SingleValueResult.Invalid;
        double mom = c - _closeBuffer[0];
        return new SingleValueResult(mom, true);
    }
}
