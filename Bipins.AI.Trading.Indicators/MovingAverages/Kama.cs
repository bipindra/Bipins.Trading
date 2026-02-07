using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.MovingAverages;

/// <summary>
/// Kaufman Adaptive Moving Average (KAMA). Adapts smoothing based on efficiency ratio.
/// Formula: ER = |Close - Close_n| / Sum(|Close_i - Close_{i-1}|), alpha = (ER*(fast-slow)+slow)^2, KAMA = alpha*Close + (1-alpha)*KAMA_prev.
/// Reference: Perry Kaufman; "Trading Systems and Methods".
/// </summary>
public sealed class Kama : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;
    private double _kama;
    private bool _initialized;
    private readonly int _fast, _slow;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Kaufman Adaptive Moving Average.
    /// </summary>
    /// <param name="period">Efficiency ratio period (default 10).</param>
    /// <param name="fast">Fast EMA period (default 2).</param>
    /// <param name="slow">Slow EMA period (default 30).</param>
    public Kama(int period = 10, int fast = 2, int slow = 30)
        : base("KAMA", "Kaufman Adaptive Moving Average. Adapts to market efficiency.", period)
    {
        Period = period;
        _fast = fast;
        _slow = slow;
        _closeBuffer = new RingBufferDouble(period + 1);
        AddParameter("Period", period.ToString(), "Efficiency ratio period");
        AddParameter("Fast", fast.ToString(), "Fast period");
        AddParameter("Slow", slow.ToString(), "Slow period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _closeBuffer.Clear();
        _initialized = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        _closeBuffer.Add(c);
        if (!_closeBuffer.IsFull)
            return SingleValueResult.Invalid;
        double change = Math.Abs(c - _closeBuffer[0]);
        double volatility = 0;
        for (int i = 0; i < Period && (i + 1) < _closeBuffer.Count; i++)
            volatility += Math.Abs(_closeBuffer[i] - _closeBuffer[i + 1]);
        if (volatility < 1e-20) volatility = 1e-20;
        double er = change / volatility;
        double fastAlpha = 2.0 / (_fast + 1);
        double slowAlpha = 2.0 / (_slow + 1);
        double alpha = er * (fastAlpha - slowAlpha) + slowAlpha;
        alpha = alpha * alpha;
        if (!_initialized)
        {
            _kama = c;
            _initialized = true;
        }
        else
        {
            _kama = alpha * c + (1 - alpha) * _kama;
        }
        return new SingleValueResult(_kama, true);
    }
}
