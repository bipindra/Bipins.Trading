using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Volatility;

/// <summary>
/// Bollinger Bands. Middle = SMA(Close), Upper = Middle + k*StdDev, Lower = Middle - k*StdDev.
/// Reference: John Bollinger; Investopedia.
/// </summary>
public sealed class BollingerBands : IndicatorBase<BandResult>
{
    private readonly RingBufferDouble _closeBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>Standard deviation multiplier.</summary>
    public double StdDevMultiplier { get; }

    /// <summary>
    /// Bollinger Bands.
    /// </summary>
    /// <param name="period">SMA period (default 20).</param>
    /// <param name="stdDevMultiplier">Number of standard deviations (default 2).</param>
    public BollingerBands(int period = 20, double stdDevMultiplier = 2)
        : base("Bollinger Bands", "Bollinger Bands. SMA with volatility bands.", period)
    {
        Period = period;
        StdDevMultiplier = stdDevMultiplier;
        _closeBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "SMA period");
        AddParameter("StdDev", stdDevMultiplier.ToString(), "Standard deviation multiplier");
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
        Span<double> span = stackalloc double[Period];
        for (int i = 0; i < Period; i++)
            span[i] = _closeBuffer[i];
        double middle = SpanMath.Mean(span);
        double sd = SpanMath.StdDev(span);
        double upper = middle + StdDevMultiplier * sd;
        double lower = middle - StdDevMultiplier * sd;
        return new BandResult(upper, middle, lower, true);
    }
}
