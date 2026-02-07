using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.MovingAverages;

/// <summary>
/// Variable Moving Average (VMA). Alpha varies with volatility (e.g. based on CVI or standard deviation).
/// Common form: alpha = 2/(V+1) where V is a volatility measure; we use period-adjusted volatility ratio.
/// Reference: Tushar Chande; Variable Index Dynamic Average.
/// </summary>
public sealed class Vma : IndicatorBase<SingleValueResult>
{
    private readonly RingBufferDouble _closeBuffer;
    private readonly int _volatilityPeriod;
    private double _vma;
    private bool _initialized;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Variable Moving Average.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    /// <param name="volatilityPeriod">Period for volatility calculation (default same as period).</param>
    public Vma(int period = 14, int? volatilityPeriod = null)
        : base("VMA", "Variable Moving Average. Adapts smoothing to volatility.", period + (volatilityPeriod ?? period))
    {
        Period = period;
        _volatilityPeriod = volatilityPeriod ?? period;
        _closeBuffer = new RingBufferDouble(Math.Max(period, _volatilityPeriod) + 1);
        AddParameter("Period", period.ToString(), "Number of periods");
        AddParameter("VolatilityPeriod", _volatilityPeriod.ToString(), "Period for volatility");
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
        if (!_initialized)
        {
            _vma = c;
            _initialized = true;
            return new SingleValueResult(_vma, true);
        }
        double vol = 0;
        for (int i = 0; i < _volatilityPeriod && (i + 1) < _closeBuffer.Count; i++)
            vol += Math.Abs(_closeBuffer[i] - _closeBuffer[i + 1]);
        if (vol < 1e-20) vol = 1e-20;
        double v = Math.Min(vol * 0.2, 30);
        double alpha = 2.0 / (v + 1);
        _vma = alpha * c + (1 - alpha) * _vma;
        return new SingleValueResult(_vma, true);
    }
}
