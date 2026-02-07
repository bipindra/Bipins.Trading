using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Statistical;

/// <summary>
/// Correlation between two price series (e.g. close and another series). Uses last N bars of close vs same-period prior close (auto-correlation style) or pass second series.
/// Simplified: correlation of close with lagged close (period bars ago) over period.
/// Reference: Standard statistics.
/// </summary>
public sealed class CorrelationIndicator : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;
    private readonly int _lag;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Correlation (close vs lagged close).
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    /// <param name="lag">Lag for second series (default 1).</param>
    public CorrelationIndicator(int period = 20, int lag = 1)
        : base("Correlation", "Correlation of close with lagged close.", period + lag)
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
        double sumXY = 0, sumX2 = 0, sumY2 = 0;
        for (int i = 0; i < Period; i++)
        {
            double dx = _closeBuffer[i] - meanX;
            double dy = _closeBuffer[i + _lag] - meanY;
            sumXY += dx * dy;
            sumX2 += dx * dx;
            sumY2 += dy * dy;
        }
        double denom = Math.Sqrt(sumX2 * sumY2);
        if (denom < 1e-20) return new SingleValueResult(0, true);
        return new SingleValueResult(sumXY / denom, true);
    }
}
