using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Statistical;

/// <summary>
/// Beta. Covariance(asset, market) / Variance(market). Simplified: use close as "market" and lagged close as "asset" for single-series beta.
/// For two series, pass market closes; here we use close vs lagged close.
/// Reference: Standard finance; CAPM.
/// </summary>
public sealed class BetaIndicator : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;
    private readonly int _lag;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Beta (close vs lagged close as proxy for market).
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    /// <param name="lag">Lag (default 1).</param>
    public BetaIndicator(int period = 20, int lag = 1)
        : base("Beta", "Beta: Cov/Var of market.", period + lag)
    {
        Period = period;
        _lag = lag;
        _closeBuffer = new RingBufferDouble(period + lag);
        AddParameter("Period", period.ToString(), "Period");
        AddParameter("Lag", lag.ToString(), "Lag");
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
        _closeBuffer.Add((double)candle.Close);
        if (!_closeBuffer.IsFull)
            return SingleValueResult.Invalid;
        double meanX = 0, meanY = 0;
        for (int i = 0; i < Period; i++)
        {
            meanX += _closeBuffer[i];
            meanY += _closeBuffer[i + _lag];
        }
        meanX /= Period;
        meanY /= Period;
        double cov = 0, varY = 0;
        for (int i = 0; i < Period; i++)
        {
            double dy = _closeBuffer[i + _lag] - meanY;
            cov += (_closeBuffer[i] - meanX) * dy;
            varY += dy * dy;
        }
        cov /= Period;
        varY /= Period;
        if (varY < 1e-20) return new SingleValueResult(0, true);
        return new SingleValueResult(cov / varY, true);
    }
}
