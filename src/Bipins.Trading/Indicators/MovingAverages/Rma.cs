using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.MovingAverages;

/// <summary>
/// Running Moving Average (RMA). Used in RSI and other indicators. Alpha = 1/period.
/// Formula: RMA = (RMA_prev*(period-1) + Close)/period. First RMA = SMA of first period.
/// Reference: TradingView (RMA); same as SMMA.
/// </summary>
public sealed class Rma : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _warmup;
    private double _rma;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Running Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Rma(int period = 14)
        : base("RMA", "Running Moving Average. Alpha = 1/period.", period)
    {
        Period = period;
        _warmup = new RingBufferDouble(period);
        AddParameter("Period", period.ToString(), "Number of periods");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _warmup.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        if (!_warmup.IsFull)
        {
            _warmup.Add(c);
            if (!_warmup.IsFull)
                return SingleValueResult.Invalid;
            _rma = _warmup.Sum() / Period;
            return new SingleValueResult(_rma, true);
        }
        _rma = (_rma * (Period - 1) + c) / Period;
        return new SingleValueResult(_rma, true);
    }
}
