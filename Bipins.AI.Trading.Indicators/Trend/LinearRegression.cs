using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Trend;

/// <summary>
/// Linear Regression. Fits y = slope*x + intercept over the last N closes (x = 0..N-1). Returns the value at current x = N-1 (forecast).
/// Reference: Standard; least squares fit.
/// </summary>
public sealed class LinearRegression : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Linear Regression (value at end of period).
    /// </summary>
    /// <param name="period">Period (default 14).</param>
    public LinearRegression(int period = 14)
        : base("Linear Regression", "Least squares linear regression over period.", period)
    {
        Period = period;
        _closeBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Period");
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
        Span<double> y = stackalloc double[Period];
        for (int i = 0; i < Period; i++)
            y[i] = _closeBuffer[i];
        var (slope, intercept) = SpanMath.LinearRegression(y);
        double value = slope * (Period - 1) + intercept;
        return new SingleValueResult(value, true);
    }
}
