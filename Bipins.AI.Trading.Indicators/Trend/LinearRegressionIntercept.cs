using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Trend;

/// <summary>
/// Linear Regression Intercept. Intercept of least-squares line over the last N closes.
/// Reference: Standard.
/// </summary>
public sealed class LinearRegressionIntercept : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Linear Regression Intercept.
    /// </summary>
    /// <param name="period">Period (default 14).</param>
    public LinearRegressionIntercept(int period = 14)
        : base("LinReg Intercept", "Intercept of linear regression over period.", period)
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
        var (_, intercept) = SpanMath.LinearRegression(y);
        return new SingleValueResult(intercept, true);
    }
}
