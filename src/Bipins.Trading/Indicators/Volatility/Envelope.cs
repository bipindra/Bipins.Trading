using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.MovingAverages;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Volatility;

/// <summary>
/// Envelope. Middle = SMA(Close), Upper = Middle * (1 + percent), Lower = Middle * (1 - percent).
/// Reference: Standard; Investopedia.
/// </summary>
public sealed class Envelope : IndicatorBase<BandResult>
{
    private readonly Sma _sma;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>Percent offset (e.g. 0.025 = 2.5%).</summary>
    public double Percent { get; }

    /// <summary>
    /// Envelope.
    /// </summary>
    /// <param name="period">SMA period (default 20).</param>
    /// <param name="percent">Band offset as decimal (default 0.025).</param>
    public Envelope(int period = 20, double percent = 0.025)
        : base("Envelope", "SMA with percentage bands.", period)
    {
        Period = period;
        Percent = percent;
        _sma = new Sma(period);
        AddParameter("Period", period.ToString(), "SMA period");
        AddParameter("Percent", percent.ToString(), "Band percent");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _sma.Reset();
    }

    /// <inheritdoc />
    protected override BandResult ComputeNext(Candle candle)
    {
        var r = _sma.Update(candle);
        if (!r.IsValid)
            return BandResult.Invalid;
        double m = r.Value;
        return new BandResult(m * (1 + Percent), m, m * (1 - Percent), true);
    }
}
