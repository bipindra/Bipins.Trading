using Bipins.Trading.Domain;
using Bipins.Trading.Indicators.Momentum;
using Bipins.Trading.Indicators.Models;
using TradingApp.Domain;
using DomainCandle = Bipins.Trading.Domain.Candle;
using IndicatorCandle = Bipins.Trading.Indicators.Models.Candle;

namespace TradingApp.Application;

/// <summary>
/// Evaluates whether an alert should trigger given current price and/or candles.
/// Uses Bipins.Trading indicators for RSI-based alerts.
/// </summary>
public static class AlertTriggerEvaluator
{
    private const int DefaultRsiPeriod = 14;
    private const double DefaultOversold = 30;
    private const double DefaultOverbought = 70;

    public static bool ShouldTrigger(Alert alert, decimal? currentPrice, IReadOnlyList<DomainCandle>? candles = null)
    {
        if (alert.AlertType == AlertType.RsiOversold || alert.AlertType == AlertType.RsiOverbought)
        {
            if (candles == null || candles.Count == 0) return false;
            var (period, oversold, overbought) = ParseRsiPayload(alert.Payload);
            var rsi = ComputeRsi(candles, period);
            if (!rsi.HasValue) return false;
            return alert.AlertType == AlertType.RsiOversold
                ? rsi.Value <= oversold
                : rsi.Value >= overbought;
        }

        if (currentPrice == null) return false;
        if (alert.AlertType == AlertType.PriceAbove || alert.AlertType == AlertType.PriceBelow)
        {
            if (string.IsNullOrWhiteSpace(alert.Payload)) return false;
            if (!decimal.TryParse(alert.Payload.Trim(), out var threshold)) return false;
            return alert.AlertType == AlertType.PriceAbove
                ? currentPrice >= threshold
                : currentPrice <= threshold;
        }

        return false;
    }

    public static string GetMessage(Alert alert, decimal price, double? rsiValue = null)
    {
        return alert.AlertType switch
        {
            AlertType.PriceAbove => $"{alert.Symbol} reached ${price:F2} (above {alert.Payload})",
            AlertType.PriceBelow => $"{alert.Symbol} reached ${price:F2} (below {alert.Payload})",
            AlertType.RsiOversold => rsiValue.HasValue
                ? $"{alert.Symbol} RSI oversold at {rsiValue.Value:F1} (${price:F2})"
                : $"{alert.Symbol} RSI oversold at ${price:F2}",
            AlertType.RsiOverbought => rsiValue.HasValue
                ? $"{alert.Symbol} RSI overbought at {rsiValue.Value:F1} (${price:F2})"
                : $"{alert.Symbol} RSI overbought at ${price:F2}",
            _ => $"{alert.Symbol} alert triggered at ${price:F2}"
        };
    }

    /// <summary>Compute last RSI value from candles using Bipins.Trading Rsi indicator.</summary>
    public static double? ComputeRsi(IReadOnlyList<DomainCandle> candles, int period = DefaultRsiPeriod)
    {
        if (candles == null || candles.Count == 0) return null;
        var rsi = new Rsi(period);
        SingleValueResult last = default;
        foreach (var c in candles)
        {
            var ind = new IndicatorCandle(c.Time, c.Open, c.High, c.Low, c.Close, c.Volume);
            var r = rsi.Update(ind);
            if (r.IsValid) last = r;
        }
        return last.IsValid ? last.Value : (double?)null;
    }

    private static (int period, double oversold, double overbought) ParseRsiPayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return (DefaultRsiPeriod, DefaultOversold, DefaultOverbought);
        var parts = payload.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);
        var period = parts.Length > 0 && int.TryParse(parts[0].Trim(), out var p) && p > 0 ? p : DefaultRsiPeriod;
        var oversold = parts.Length > 1 && double.TryParse(parts[1].Trim(), out var os) ? os : DefaultOversold;
        var overbought = parts.Length > 2 && double.TryParse(parts[2].Trim(), out var ob) ? ob : DefaultOverbought;
        return (period, oversold, overbought);
    }
}
