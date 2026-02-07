using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.MovingAverages;

/// <summary>
/// Smoothed Moving Average (SMMA). First value = SMA of first N; then SMMA = (SMMA_prev*(N-1) + Close)/N.
/// Reference: TradingView, MetaStock.
/// </summary>
public sealed class Smma : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _warmup;
    private double _smma;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Smoothed Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Smma(int period = 14)
        : base("SMMA", "Smoothed Moving Average. Smoothed cumulative average.", period)
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
            _smma = _warmup.Sum() / Period;
            return new SingleValueResult(_smma, true);
        }
        _smma = (_smma * (Period - 1) + c) / Period;
        return new SingleValueResult(_smma, true);
    }
}
