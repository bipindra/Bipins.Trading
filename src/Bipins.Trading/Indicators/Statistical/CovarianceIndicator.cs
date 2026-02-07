using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Statistical;

/// <summary>
/// Covariance between close and lagged close over period.
/// Reference: Standard statistics.
/// </summary>
public sealed class CovarianceIndicator : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;
    private readonly int _lag;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Covariance.
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    /// <param name="lag">Lag (default 1).</param>
    public CovarianceIndicator(int period = 20, int lag = 1)
        : base("Covariance", "Covariance of close with lagged close.", period + lag)
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
        double sumXY = 0;
        for (int i = 0; i < Period; i++)
            sumXY += (_closeBuffer[i] - meanX) * (_closeBuffer[i + _lag] - meanY);
        return new SingleValueResult(sumXY / Period, true);
    }
}
