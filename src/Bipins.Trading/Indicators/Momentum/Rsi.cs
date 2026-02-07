using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.MovingAverages;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Momentum;

/// <summary>
/// Relative Strength Index (RSI). Momentum oscillator 0-100. RSI = 100 - 100/(1+RS), RS = avg gain / avg loss (RMA/SMMA).
/// Reference: J. Welles Wilder; "New Concepts in Technical Trading Systems".
/// </summary>
public sealed class Rsi : IndicatorBase<SingleValueResult>
{
    private readonly Rma _rmaGain;
    private readonly Rma _rmaLoss;
    private double _prevClose;
    private bool _havePrev;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>
    /// Relative Strength Index.
    /// </summary>
    /// <param name="period">Number of periods (default 14).</param>
    public Rsi(int period = 14)
        : base("RSI", "Relative Strength Index. Momentum oscillator 0-100.", period + 1)
    {
        Period = period;
        _rmaGain = new Rma(period);
        _rmaLoss = new Rma(period);
        AddParameter("Period", period.ToString(), "Number of periods");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _rmaGain.Reset();
        _rmaLoss.Reset();
        _havePrev = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        double gain = 0, loss = 0;
        if (_havePrev)
        {
            double diff = c - _prevClose;
            gain = diff > 0 ? diff : 0;
            loss = diff < 0 ? -diff : 0;
        }
        _havePrev = true;
        _prevClose = c;
        var rG = _rmaGain.Update(new Candle(candle.Time, (decimal)gain, (decimal)gain, (decimal)gain, (decimal)gain, candle.Volume));
        var rL = _rmaLoss.Update(new Candle(candle.Time, (decimal)loss, (decimal)loss, (decimal)loss, (decimal)loss, candle.Volume));
        if (!rG.IsValid || !rL.IsValid)
            return SingleValueResult.Invalid;
        if (rL.Value < 1e-20)
            return new SingleValueResult(100, true);
        double rs = rG.Value / rL.Value;
        double rsi = 100 - 100 / (1 + rs);
        return new SingleValueResult(rsi, true);
    }
}
