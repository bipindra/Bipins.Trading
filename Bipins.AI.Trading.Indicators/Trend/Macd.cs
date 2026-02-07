using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.MovingAverages;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Indicators.Trend;

/// <summary>
/// Moving Average Convergence Divergence (MACD). MACD = EMA(Close, fast) - EMA(Close, slow), Signal = EMA(MACD, signal), Histogram = MACD - Signal.
/// Reference: Gerald Appel; Investopedia.
/// </summary>
public sealed class Macd : IndicatorBase<MultiValueResult>
{
    private readonly Ema _emaFast;
    private readonly Ema _emaSlow;
    private readonly Ema _emaSignal;

    /// <summary>Fast period.</summary>
    public int FastPeriod { get; }

    /// <summary>Slow period.</summary>
    public int SlowPeriod { get; }

    /// <summary>Signal period.</summary>
    public int SignalPeriod { get; }

    /// <summary>
    /// MACD.
    /// </summary>
    /// <param name="fastPeriod">Fast EMA period (default 12).</param>
    /// <param name="slowPeriod">Slow EMA period (default 26).</param>
    /// <param name="signalPeriod">Signal line period (default 9).</param>
    public Macd(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        : base("MACD", "Moving Average Convergence Divergence. MACD, Signal, Histogram.", slowPeriod + signalPeriod)
    {
        FastPeriod = fastPeriod;
        SlowPeriod = slowPeriod;
        SignalPeriod = signalPeriod;
        _emaFast = new Ema(fastPeriod);
        _emaSlow = new Ema(slowPeriod);
        _emaSignal = new Ema(signalPeriod);
        AddParameter("Fast", fastPeriod.ToString(), "Fast EMA period");
        AddParameter("Slow", slowPeriod.ToString(), "Slow EMA period");
        AddParameter("Signal", signalPeriod.ToString(), "Signal EMA period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _emaFast.Reset();
        _emaSlow.Reset();
        _emaSignal.Reset();
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        var fast = _emaFast.Update(candle);
        var slow = _emaSlow.Update(candle);
        if (!fast.IsValid || !slow.IsValid)
            return MultiValueResult.Invalid();
        double macd = fast.Value - slow.Value;
        var signalResult = _emaSignal.Update(new Candle(candle.Time, (decimal)macd, (decimal)macd, (decimal)macd, (decimal)macd, candle.Volume));
        if (!signalResult.IsValid)
            return MultiValueResult.Invalid();
        double signal = signalResult.Value;
        double histogram = macd - signal;
        return new MultiValueResult(macd, signal, histogram, true);
    }
}
