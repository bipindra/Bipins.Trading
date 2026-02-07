using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.MovingAverages;

/// <summary>
/// Fractal Adaptive Moving Average (FRAMA). Uses fractal dimension to adapt smoothing.
/// Formula: FC = (H - L) / period; D = (log(N1+N2) - log(N3)) / log(2); alpha = exp(-4.6*(D-1)); FRAMA = alpha*Close + (1-alpha)*FRAMA_prev.
/// Simplified: uses high-low range and exponential smoothing with adaptive alpha.
/// Reference: John Ehlers; "Rocket Science for Traders".
/// </summary>
public sealed class Frama : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _highBuffer;
    private readonly RingBufferDouble _lowBuffer;
    private readonly RingBufferDouble _closeBuffer;
    private double _frama;
    private bool _initialized;
    private readonly int _window;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Fractal Adaptive Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Frama(int period = 14)
        : base("FRAMA", "Fractal Adaptive Moving Average. Adapts to fractal dimension.", period)
    {
        Period = period;
        _window = Math.Max(2, period / 2);
        _highBuffer = new RingBufferDouble(period);
        _lowBuffer = new RingBufferDouble(period);
        _closeBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Number of periods");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _highBuffer.Clear();
        _lowBuffer.Clear();
        _closeBuffer.Clear();
        _initialized = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        _highBuffer.Add(h);
        _lowBuffer.Add(l);
        _closeBuffer.Add(c);
        if (!_closeBuffer.IsFull)
            return SingleValueResult.Invalid;
        double n1 = 0, n2 = 0, n3 = 0;
        for (int i = 0; i < _window && (i + _window) < Period; i++)
        {
            n1 += Math.Abs(_highBuffer[i] - _lowBuffer[i]);
            n2 += Math.Abs(_highBuffer[i + _window] - _lowBuffer[i + _window]);
            n3 += Math.Abs(_highBuffer[i + _window / 2] - _lowBuffer[i + _window / 2]);
        }
        if (n1 < 1e-20) n1 = 1e-20;
        if (n2 < 1e-20) n2 = 1e-20;
        if (n3 < 1e-20) n3 = 1e-20;
        double d = (Math.Log(n1 + n2) - Math.Log(n3)) / Math.Log(2);
        if (double.IsNaN(d) || d < 0.01) d = 0.01;
        if (d > 2) d = 2;
        double alpha = Math.Exp(-4.6 * (d - 1));
        if (alpha > 1) alpha = 1;
        if (alpha < 0.01) alpha = 0.01;
        if (!_initialized)
        {
            _frama = _closeBuffer.Sum() / Period;
            _initialized = true;
        }
        else
        {
            _frama = alpha * c + (1 - alpha) * _frama;
        }
        return new SingleValueResult(_frama, true);
    }
}
