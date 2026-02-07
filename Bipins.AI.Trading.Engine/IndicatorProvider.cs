using System.Collections.Concurrent;
using Bipins.AI.Trading.Domain;
using Bipins.AI.Trading.Indicators.Abstractions;
using Bipins.AI.Trading.Indicators.Momentum;
using Bipins.AI.Trading.Indicators.MovingAverages;
using Bipins.AI.Trading.Indicators.Trend;
using Bipins.AI.Trading.Indicators.Volatility;
using Bipins.AI.Trading.Indicators.Models;

namespace Bipins.AI.Trading.Engine;

public sealed class IndicatorProvider : IIndicatorProvider
{
    private readonly ConcurrentDictionary<string, double> _cache = new();
    private readonly ConcurrentDictionary<string, IReadOnlyList<double>> _multiCache = new();

    public void ClearCache()
    {
        _cache.Clear();
        _multiCache.Clear();
    }

    public double? Get(IndicatorKey key, Func<double?> computeFn)
    {
        var k = key.Key;
        if (_cache.TryGetValue(k, out var v)) return v;
        var result = computeFn();
        if (result.HasValue) _cache[k] = result.Value;
        return result;
    }

    public IReadOnlyList<double>? GetMulti(IndicatorKey key, Func<IReadOnlyList<double>?> computeFn)
    {
        var k = key.Key;
        if (_multiCache.TryGetValue(k, out var v)) return v;
        var result = computeFn();
        if (result != null && result.Count > 0) _multiCache[k] = result;
        return result;
    }

    public static double? ComputeSingle(IndicatorKey key, IReadOnlyList<Domain.Candle> domainCandles)
    {
        var indCandles = CandleMapper.ToIndicatorCandles(domainCandles).ToList();
        if (indCandles.Count == 0) return null;

        var keyStr = key.Key;
        if (keyStr.StartsWith("RSI(", StringComparison.OrdinalIgnoreCase))
        {
            var period = ParseOneParam(keyStr, "RSI");
            if (period == null) return null;
            var rsi = new Rsi(period.Value);
            double? last = null;
            foreach (var c in indCandles) { var r = rsi.Update(c); if (r.IsValid) last = r.Value; }
            return last;
        }
        if (keyStr.StartsWith("EMA(", StringComparison.OrdinalIgnoreCase))
        {
            var period = ParseOneParam(keyStr, "EMA");
            if (period == null) return null;
            var ema = new Ema(period.Value);
            double? last = null;
            foreach (var c in indCandles) { var r = ema.Update(c); if (r.IsValid) last = r.Value; }
            return last;
        }
        if (keyStr.StartsWith("SMA(", StringComparison.OrdinalIgnoreCase))
        {
            var period = ParseOneParam(keyStr, "SMA");
            if (period == null) return null;
            var sma = new Sma(period.Value);
            double? last = null;
            foreach (var c in indCandles) { var r = sma.Update(c); if (r.IsValid) last = r.Value; }
            return last;
        }
        if (keyStr.StartsWith("ATR(", StringComparison.OrdinalIgnoreCase))
        {
            var period = ParseOneParam(keyStr, "ATR");
            if (period == null) return null;
            var atr = new Atr(period.Value);
            double? last = null;
            foreach (var c in indCandles) { var r = atr.Update(c); if (r.IsValid) last = r.Value; }
            return last;
        }
        if (keyStr.StartsWith("Donchian(", StringComparison.OrdinalIgnoreCase))
        {
            var period = ParseOneParam(keyStr, "Donchian");
            if (period == null) return null;
            var donch = new DonchianChannels(period.Value);
            double? last = null;
            foreach (var c in indCandles)
            {
                var r = donch.Update(c);
                if (r.IsValid) last = r.Middle;
            }
            return last;
        }
        return null;
    }

    public static IReadOnlyList<double>? ComputeMulti(IndicatorKey key, IReadOnlyList<Domain.Candle> domainCandles)
    {
        var indCandles = CandleMapper.ToIndicatorCandles(domainCandles).ToList();
        if (indCandles.Count == 0) return null;

        var keyStr = key.Key;
        if (keyStr.StartsWith("MACD(", StringComparison.OrdinalIgnoreCase))
        {
            var (f, s, sig) = ParseThreeParam(keyStr, "MACD");
            if (f == null) return null;
            var macd = new Macd(f.Value, s ?? 26, sig ?? 9);
            double[]? last = null;
            foreach (var c in indCandles)
            {
                var r = macd.Update(c);
                if (r.IsValid) last = new[] { r.GetValue(0), r.GetValue(1), r.GetValue(2) };
            }
            return last;
        }
        if (keyStr.StartsWith("Donchian(", StringComparison.OrdinalIgnoreCase))
        {
            var period = ParseOneParam(keyStr, "Donchian");
            if (period == null) return null;
            var donch = new DonchianChannels(period.Value);
            double[]? last = null;
            foreach (var c in indCandles)
            {
                var r = donch.Update(c);
                if (r.IsValid) last = new[] { r.Upper, r.Middle, r.Lower };
            }
            return last;
        }
        return null;
    }

    private static int? ParseOneParam(string keyStr, string prefix)
    {
        var open = keyStr.IndexOf('(');
        var close = keyStr.IndexOf(')');
        if (open < 0 || close <= open + 1) return null;
        var inner = keyStr[(open + 1)..close];
        var pipe = inner.IndexOf('|');
        if (pipe >= 0) inner = inner[..pipe].Trim();
        return int.TryParse(inner.Trim(), out var p) ? p : null;
    }

    private static (int?, int?, int?) ParseThreeParam(string keyStr, string prefix)
    {
        var open = keyStr.IndexOf('(');
        var close = keyStr.IndexOf(')');
        if (open < 0 || close <= open + 1) return (null, null, null);
        var inner = keyStr[(open + 1)..close];
        var pipe = inner.IndexOf('|');
        if (pipe >= 0) inner = inner[..pipe].Trim();
        var parts = inner.Split(',');
        if (parts.Length < 1) return (null, null, null);
        int? a = int.TryParse(parts[0].Trim(), out var a0) ? a0 : null;
        int? b = parts.Length >= 2 && int.TryParse(parts[1].Trim(), out var b0) ? b0 : null;
        int? c = parts.Length >= 3 && int.TryParse(parts[2].Trim(), out var c0) ? c0 : null;
        return (a, b, c);
    }
}
