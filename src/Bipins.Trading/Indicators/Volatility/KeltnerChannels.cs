using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.MovingAverages;
using Bipins.Trading.Indicators.Models;

namespace Bipins.Trading.Indicators.Volatility;

/// <summary>
/// Keltner Channels. Middle = EMA(Close), Upper = Middle + mult*ATR, Lower = Middle - mult*ATR.
/// Reference: Chester Keltner; Investopedia.
/// </summary>
public sealed class KeltnerChannels : IndicatorBase<BandResult>
{
    private readonly Ema _ema;
    private readonly Atr _atr;

    /// <summary>Period.</summary>
    public int Period { get; }

    /// <summary>ATR multiplier.</summary>
    public double Multiplier { get; }

    /// <summary>
    /// Keltner Channels.
    /// </summary>
    /// <param name="period">EMA and ATR period (default 20).</param>
    /// <param name="multiplier">ATR multiplier (default 2).</param>
    public KeltnerChannels(int period = 20, double multiplier = 2)
        : base("Keltner", "Keltner Channels. EMA with ATR bands.", period)
    {
        Period = period;
        Multiplier = multiplier;
        _ema = new Ema(period);
        _atr = new Atr(period);
        AddParameter("Period", period.ToString(), "EMA/ATR period");
        AddParameter("Multiplier", multiplier.ToString(), "ATR multiplier");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _ema.Reset();
        _atr.Reset();
    }

    /// <inheritdoc />
    protected override BandResult ComputeNext(Candle candle)
    {
        var emaR = _ema.Update(candle);
        var atrR = _atr.Update(candle);
        if (!emaR.IsValid || !atrR.IsValid)
            return BandResult.Invalid;
        double middle = emaR.Value;
        double atr = atrR.Value;
        return new BandResult(middle + Multiplier * atr, middle, middle - Multiplier * atr, true);
    }
}
