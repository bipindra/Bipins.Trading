using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Momentum;

/// <summary>
/// Fisher Transform. Converts price to Gaussian-like distribution. F = 0.5 * ln((1+x)/(1-x)), x = normalized price over period.
/// Reference: J.F. Ehlers; "Rocket Science for Traders".
/// </summary>
public sealed class FisherTransform : IndicatorBase<MultiValueResult>
{
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;
    private double _prevFish;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Fisher Transform.
    /// </summary>
    /// <param name="period">Lookback period (default 9).</param>
    public FisherTransform(int period = 9)
        : base("Fisher Transform", "Fisher Transform. Normalizes price to Gaussian-like.", period)
    {
        Period = period;
        _highBuffer = new RingBufferDouble(period);
        _lowBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Lookback period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _highBuffer.Clear();
        _lowBuffer.Clear();
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        _highBuffer.Add(h);
        _lowBuffer.Add(l);
        if (!_highBuffer.IsFull)
            return MultiValueResult.Invalid();
        double maxH = double.MinValue, minL = double.MaxValue;
        for (int i = 0; i < Period; i++)
        {
            if (_highBuffer[i] > maxH) maxH = _highBuffer[i];
            if (_lowBuffer[i] < minL) minL = _lowBuffer[i];
        }
        double range = maxH - minL;
        double x = range < 1e-20 ? 0 : 2 * (c - minL) / range - 1;
        x = Math.Clamp(x, -0.999, 0.999);
        double fish = 0.5 * Math.Log((1 + x) / (1 - x));
        _prevFish = fish;
        double trigger = _prevFish;
        return new MultiValueResult(fish, trigger, true);
    }
}
