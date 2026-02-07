using Bipins.Trading.Indicators.Base;
using Bipins.Trading.Indicators.MovingAverages;
using Bipins.Trading.Indicators.Models;
using Bipins.Trading.Indicators.Utilities;

namespace Bipins.Trading.Indicators.Momentum;

/// <summary>
/// Stochastic RSI. Applies Stochastic formula to RSI values. StochRSI = (RSI - RSI_min) / (RSI_max - RSI_min) over period.
/// Reference: Tushar Chande, Stanley Kroll; Investopedia.
/// </summary>
public sealed class StochRsi : IndicatorBase<MultiValueResult>
{
    private readonly Rsi _rsi;
    private readonly RingBufferDouble _rsiBuffer;
    private readonly int _smoothK;
    private readonly RingBufferDouble _kBuffer;

    /// <summary>RSI period.</summary>
    public int Period { get; }

    /// <summary>
    /// Stochastic RSI.
    /// </summary>
    /// <param name="rsiPeriod">RSI period (default 14).</param>
    /// <param name="stochPeriod">Stochastic period (default 14).</param>
    /// <param name="smoothK">%K smoothing (default 3).</param>
    public StochRsi(int rsiPeriod = 14, int stochPeriod = 14, int smoothK = 3)
        : base("StochRSI", "Stochastic of RSI. Oscillator 0-100.", rsiPeriod + stochPeriod + smoothK)
    {
        Period = stochPeriod;
        _rsi = new Rsi(rsiPeriod);
        _rsiBuffer = new RingBufferDouble(stochPeriod);
        _smoothK = smoothK;
        _kBuffer = new RingBufferDouble(smoothK);
        AddParameter("RsiPeriod", rsiPeriod.ToString(), "RSI period");
        AddParameter("StochPeriod", stochPeriod.ToString(), "Stochastic lookback");
        AddParameter("SmoothK", smoothK.ToString(), "%K smoothing");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _rsi.Reset();
        _rsiBuffer.Clear();
        _kBuffer.Clear();
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        var r = _rsi.Update(candle);
        if (!r.IsValid)
            return MultiValueResult.Invalid();
        _rsiBuffer.Add(r.Value);
        if (!_rsiBuffer.IsFull)
            return MultiValueResult.Invalid();
        double rsiMin = double.MaxValue, rsiMax = double.MinValue;
        for (int i = 0; i < Period; i++)
        {
            double v = _rsiBuffer[i];
            if (v < rsiMin) rsiMin = v;
            if (v > rsiMax) rsiMax = v;
        }
        double denom = rsiMax - rsiMin;
        double k = denom < 1e-20 ? 50 : 100 * (r.Value - rsiMin) / denom;
        k = Math.Clamp(k, 0, 100);
        _kBuffer.Add(k);
        if (!_kBuffer.IsFull)
            return MultiValueResult.Invalid();
        double d = _kBuffer.Sum() / _smoothK;
        return new MultiValueResult(k, d, true);
    }
}
