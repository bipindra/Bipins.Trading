using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.MovingAverages;

/// <summary>
/// Hull Moving Average (HMA). Reduces lag: HMA = WMA(2*WMA(n/2) - WMA(n), sqrt(n)).
/// Reference: Alan Hull; TradingView.
/// </summary>
public sealed class Hma : IndicatorBase<SingleValueResult>
{
    private readonly Wma _wmaFull;
    private readonly Wma _wmaHalf;
    private readonly int _sqrtPeriod;
    private readonly RingBufferDouble _rawHmaBuffer;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Hull Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Hma(int period = 14)
        : base("HMA", "Hull Moving Average. Low-lag smoothed average.", period + Math.Max(1, (int)Math.Round(Math.Sqrt(period))))
    {
        Period = period;
        int half = Math.Max(1, period / 2);
        _sqrtPeriod = Math.Max(1, (int)Math.Round(Math.Sqrt(period)));
        _wmaFull = new Wma(period);
        _wmaHalf = new Wma(half);
        _rawHmaBuffer = new RingBufferDouble(_sqrtPeriod);
        AddParameter("Period", period.ToString(), "Number of periods");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _wmaFull.Reset();
        _wmaHalf.Reset();
        _rawHmaBuffer.Clear();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var rFull = _wmaFull.Update(candle);
        var rHalf = _wmaHalf.Update(candle);
        if (!rFull.IsValid || !rHalf.IsValid)
            return SingleValueResult.Invalid;
        double raw = 2 * rHalf.Value - rFull.Value;
        _rawHmaBuffer.Add(raw);
        if (!_rawHmaBuffer.IsFull)
            return SingleValueResult.Invalid;
        double sum = 0, weightSum = 0;
        for (int i = 0; i < _sqrtPeriod; i++)
        {
            double w = i + 1;
            sum += w * _rawHmaBuffer[i];
            weightSum += w;
        }
        return new SingleValueResult(sum / weightSum, true);
    }
}
