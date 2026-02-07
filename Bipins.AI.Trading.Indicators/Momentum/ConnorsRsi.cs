using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Momentum;

/// <summary>
/// Connors RSI (CRSI). Average of RSI(3), streak RSI (up/down days), and rank of percent change. (RSI + StreakRSI + Rank) / 3.
/// Simplified: RSI(3) and rank of 1-period return over 100 bars. Streak = consecutive up/down.
/// Reference: Connors Research; "Short Term Trading Strategies".
/// </summary>
public sealed class ConnorsRsi : IndicatorBase<SingleValueResult>
{
    private readonly Rsi _rsi;
    private readonly RingBufferDouble _returnBuffer;
    private int _streak;
    private double _prevClose;
    private bool _havePrev;
    private readonly RingBufferDouble _streakBuffer;

    /// <summary>RSI period.</summary>
    public int RsiPeriod { get; }

    /// <summary>Streak period.</summary>
    public int StreakPeriod { get; }

    /// <summary>Rank lookback.</summary>
    public int RankPeriod { get; }

    /// <summary>
    /// Connors RSI.
    /// </summary>
    /// <param name="rsiPeriod">RSI period (default 3).</param>
    /// <param name="streakPeriod">Streak RSI period (default 2).</param>
    /// <param name="rankPeriod">Rank lookback (default 100).</param>
    public ConnorsRsi(int rsiPeriod = 3, int streakPeriod = 2, int rankPeriod = 100)
        : base("Connors RSI", "Connors RSI. RSI + Streak + Rank.", rankPeriod + rsiPeriod)
    {
        RsiPeriod = rsiPeriod;
        StreakPeriod = streakPeriod;
        RankPeriod = rankPeriod;
        _rsi = new Rsi(rsiPeriod);
        _returnBuffer = new RingBufferDouble(rankPeriod);
        _streakBuffer = new RingBufferDouble(streakPeriod + 2);
        AddParameter("RsiPeriod", rsiPeriod.ToString(), "RSI period");
        AddParameter("StreakPeriod", streakPeriod.ToString(), "Streak period");
        AddParameter("RankPeriod", rankPeriod.ToString(), "Rank lookback");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _rsi.Reset();
        _returnBuffer.Clear();
        _streakBuffer.Clear();
        _streak = 0;
        _havePrev = false;
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        double c = (double)candle.Close;
        double ret = _havePrev && Math.Abs(_prevClose) > 1e-20 ? (c - _prevClose) / _prevClose * 100 : 0;
        _havePrev = true;
        _prevClose = c;
        _returnBuffer.Add(ret);
        if (_returnBuffer.Count > 1)
        {
            double prevRet = _returnBuffer.Count >= 2 ? _returnBuffer[1] - _returnBuffer[0] : 0;
            if (ret > 0) _streak = prevRet > 0 ? _streak + 1 : 1;
            else if (ret < 0) _streak = prevRet < 0 ? _streak - 1 : -1;
            else _streak = 0;
        }
        _streakBuffer.Add(_streak);
        var rsiVal = _rsi.Update(candle);
        if (!rsiVal.IsValid || !_returnBuffer.IsFull || !_streakBuffer.IsFull)
            return SingleValueResult.Invalid;
        double streakMin = double.MaxValue, streakMax = double.MinValue;
        for (int i = 0; i < _streakBuffer.Count; i++)
        {
            if (_streakBuffer[i] < streakMin) streakMin = _streakBuffer[i];
            if (_streakBuffer[i] > streakMax) streakMax = _streakBuffer[i];
        }
        double streakRange = streakMax - streakMin;
        double streakRsi = streakRange < 1e-20 ? 50 : 100 * (_streak - streakMin) / streakRange;
        int rank = 0;
        double currentRet = _returnBuffer[RankPeriod - 1];
        for (int i = 0; i < RankPeriod; i++)
            if (_returnBuffer[i] < currentRet) rank++;
        double rankPct = 100.0 * rank / RankPeriod;
        double crsi = (rsiVal.Value + streakRsi + rankPct) / 3;
        return new SingleValueResult(Math.Clamp(crsi, 0, 100), true);
    }
}
