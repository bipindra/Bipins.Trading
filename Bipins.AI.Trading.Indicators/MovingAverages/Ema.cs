using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.MovingAverages;

/// <summary>
/// Exponential Moving Average (EMA). Gives more weight to recent prices.
/// Formula: alpha = 2/(period+1), EMA = alpha*Close + (1-alpha)*EMA_prev. First EMA = SMA of first period.
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class Ema : IndicatorBase<SingleValueResult>
{
    private double _ema;
    private bool _initialized;
    private readonly double _alpha;
    private readonly RingBufferDouble _warmupBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Exponential Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Ema(int period = 14)
        : base("EMA", "Exponential Moving Average. Weights recent prices more.", period)
    {
        Period = period;
        _alpha = 2.0 / (period + 1);
        _warmupBuffer = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Number of periods");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _initialized = false;
        _warmupBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        if (!_initialized)
        {
            _warmupBuffer.Add(c);
            if (!_warmupBuffer.IsFull)
                return SingleValueResult.Invalid;
            _ema = _warmupBuffer.Sum() / Period;
            _initialized = true;
            return new SingleValueResult(_ema, true);
        }
        _ema = _alpha * c + (1 - _alpha) * _ema;
        return new SingleValueResult(_ema, true);
    }
}
