using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Utilities;

namespace Bipins.AI.Trading.Indicators.Trend;

/// <summary>
/// Ichimoku Cloud. Tenkan (9), Kijun (26), Senkou A (lead A), Senkou B (lead B), Chikou (lag). Periods: 9, 26, 52.
/// Tenkan = (highest high + lowest low)/2 over 9; Kijun = same over 26; Senkou A = (Tenkan+Kijun)/2 displaced 26; Senkou B = (highest+lowest)/2 over 52 displaced 26; Chikou = close displaced -26.
/// Reference: Goichi Hosoda; Investopedia.
/// </summary>
public sealed class IchimokuCloud : IndicatorBase<MultiValueResult>
{
    private readonly RingBufferDouble _high9;
    private readonly RingBufferDouble _low9;
    private readonly RingBufferDouble _high26;
    private readonly RingBufferDouble _low26;
    private readonly RingBufferDouble _high52;
    private readonly RingBufferDouble _low52;
    private readonly RingBufferDouble _closeBuffer;
    private readonly RingBufferDouble _senkouABuffer;
    private readonly RingBufferDouble _senkouBBuffer;
    private readonly int _kijunPeriod;

    /// <summary>
    /// Ichimoku Cloud (5 values: Tenkan, Kijun, Senkou A, Senkou B, Chikou).
    /// </summary>
    /// <param name="tenkanPeriod">Tenkan period (default 9).</param>
    /// <param name="kijunPeriod">Kijun period (default 26).</param>
    /// <param name="senkouBPeriod">Senkou B period (default 52).</param>
    public IchimokuCloud(int tenkanPeriod = 9, int kijunPeriod = 26, int senkouBPeriod = 52)
        : base("Ichimoku", "Ichimoku Cloud. Tenkan, Kijun, Senkou A, Senkou B, Chikou.", senkouBPeriod + kijunPeriod)
    {
        _high9 = new RingBufferDouble(tenkanPeriod);
        _low9 = new RingBufferDouble(tenkanPeriod);
        _high26 = new RingBufferDouble(kijunPeriod);
        _low26 = new RingBufferDouble(kijunPeriod);
        _high52 = new RingBufferDouble(senkouBPeriod);
        _low52 = new RingBufferDouble(senkouBPeriod);
        _closeBuffer = new RingBufferDouble(kijunPeriod);
        _senkouABuffer = new RingBufferDouble(kijunPeriod);
        _senkouBBuffer = new RingBufferDouble(kijunPeriod);
        _kijunPeriod = kijunPeriod;
        AddParameter("Tenkan", tenkanPeriod.ToString(), "Tenkan period");
        AddParameter("Kijun", kijunPeriod.ToString(), "Kijun period");
        AddParameter("SenkouB", senkouBPeriod.ToString(), "Senkou B period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _high9.Clear(); _low9.Clear();
        _high26.Clear(); _low26.Clear();
        _high52.Clear(); _low52.Clear();
        _closeBuffer.Clear();
        _senkouABuffer.Clear();
        _senkouBBuffer.Clear();
    }

    /// <inheritdoc />
    protected override MultiValueResult ComputeNext(Candle candle)
    {
        double h = (double)candle.High;
        double l = (double)candle.Low;
        double c = (double)candle.Close;
        _high9.Add(h); _low9.Add(l);
        _high26.Add(h); _low26.Add(l);
        _high52.Add(h); _low52.Add(l);
        _closeBuffer.Add(c);
        if (!_high52.IsFull)
            return MultiValueResult.Invalid();
        double tenkan = 0.5 * (MaxOf(_high9) + MinOf(_low9));
        double kijun = 0.5 * (MaxOf(_high26) + MinOf(_low26));
        double senkouBNow = 0.5 * (MaxOf(_high52) + MinOf(_low52));
        _senkouABuffer.Add(0.5 * (tenkan + kijun));
        _senkouBBuffer.Add(senkouBNow);
        if (!_senkouABuffer.IsFull)
            return MultiValueResult.Invalid();
        double senkouA = _senkouABuffer[0];
        double senkouB = _senkouBBuffer[0];
        double chikou = _closeBuffer[0];
        return new MultiValueResult(tenkan, kijun, senkouA, senkouB, chikou, true);
    }

    private static double MaxOf(RingBufferDouble b)
    {
        double m = double.MinValue;
        for (int i = 0; i < b.Count; i++)
            if (b[i] > m) m = b[i];
        return m;
    }
    private static double MinOf(RingBufferDouble b)
    {
        double m = double.MaxValue;
        for (int i = 0; i < b.Count; i++)
            if (b[i] < m) m = b[i];
        return m;
    }
}
