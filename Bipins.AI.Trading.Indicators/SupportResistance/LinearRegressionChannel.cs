using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Trend;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.SupportResistance;

/// <summary>
/// Linear Regression Channel. Upper and lower band as linear regression line +/- offset (e.g. standard deviation).
/// Reference: Standard.
/// </summary>
public sealed class LinearRegressionChannel : IndicatorBase<BandResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>Standard deviations for channel width.</summary>
    public double StdDevMultiplier { get; }

    /// <summary>
    /// Linear Regression Channel.
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    /// <param name="stdDevMultiplier">Channel width in std devs (default 2).</param>
    public LinearRegressionChannel(int period = 20, double stdDevMultiplier = 2)
        : base("LinReg Channel", "Linear regression with upper/lower band.", period)
    {
        Period = period;
        StdDevMultiplier = stdDevMultiplier;
        _closeBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Period");
        AddParameter("StdDev", stdDevMultiplier.ToString(), "Channel width");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _closeBuffer.Clear();
    }

    /// <inheritdoc />
    protected override BandResult ComputeNext(Candle candle)
    {
        _closeBuffer.Add((double)candle.Close);
        if (!_closeBuffer.IsFull)
            return BandResult.Invalid;
        Span<double> y = stackalloc double[Period];
        for (int i = 0; i < Period; i++)
            y[i] = _closeBuffer[i];
        var (slope, intercept) = SpanMath.LinearRegression(y);
        double middle = slope * (Period - 1) + intercept;
        double sd = SpanMath.StdDev(y, sample: true);
        return new BandResult(middle + StdDevMultiplier * sd, middle, middle - StdDevMultiplier * sd, true);
    }
}
