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

    // Store previous RSI values for crossover detection
    private static readonly Dictionary<int, double?> PreviousRsiValues = new();

    public static bool ShouldTrigger(Alert alert, decimal? currentPrice, IReadOnlyList<DomainCandle>? candles = null)
    {
        if (alert.AlertType == AlertType.RsiOversold || alert.AlertType == AlertType.RsiOverbought)
        {
            if (candles == null || candles.Count == 0) return false;
            var (period, oversold, overbought) = ParseRsiPayload(alert.Payload);
            var rsi = ComputeRsi(candles, period);
            if (!rsi.HasValue) return false;

            // Use threshold from alert if provided, otherwise use parsed defaults
            var threshold = alert.Threshold.HasValue ? (double)alert.Threshold.Value : 
                (alert.AlertType == AlertType.RsiOversold ? oversold : overbought);

            // Get previous RSI value for crossover detection
            var previousRsi = PreviousRsiValues.GetValueOrDefault(alert.Id);

            // Determine comparison logic
            if (alert.ComparisonType.HasValue)
            {
                return alert.ComparisonType.Value switch
                {
                    ComparisonType.Above => rsi.Value > threshold,
                    ComparisonType.Below => rsi.Value < threshold,
                    ComparisonType.CrossesOver => previousRsi.HasValue && previousRsi.Value <= threshold && rsi.Value > threshold,
                    ComparisonType.CrossesBelow => previousRsi.HasValue && previousRsi.Value >= threshold && rsi.Value < threshold,
                    _ => false
                };
            }

            // Default behavior (backward compatible)
            return alert.AlertType == AlertType.RsiOversold
                ? rsi.Value <= threshold
                : rsi.Value >= threshold;
        }

        if (currentPrice == null) return false;
        if (alert.AlertType == AlertType.PriceAbove || alert.AlertType == AlertType.PriceBelow)
        {
            // Use threshold from alert if provided, otherwise parse from payload
            var threshold = alert.Threshold ?? 
                (string.IsNullOrWhiteSpace(alert.Payload) || !decimal.TryParse(alert.Payload.Trim(), out var parsed) 
                    ? 0m : parsed);
            
            if (threshold == 0m) return false;

            // Support comparison type for price alerts too
            if (alert.ComparisonType.HasValue)
            {
                return alert.ComparisonType.Value switch
                {
                    ComparisonType.Above => currentPrice > threshold,
                    ComparisonType.Below => currentPrice < threshold,
                    ComparisonType.CrossesOver => currentPrice >= threshold, // Simplified for price
                    ComparisonType.CrossesBelow => currentPrice <= threshold, // Simplified for price
                    _ => false
                };
            }

            // Default behavior (backward compatible)
            return alert.AlertType == AlertType.PriceAbove
                ? currentPrice >= threshold
                : currentPrice <= threshold;
        }

        return false;
    }

    // Call this after evaluating to store RSI for next evaluation
    public static void StoreRsiValue(Alert alert, double? rsiValue)
    {
        if (rsiValue.HasValue)
            PreviousRsiValues[alert.Id] = rsiValue.Value;
    }

    public static string GetMessage(Alert alert, decimal price, double? rsiValue = null)
    {
        var thresholdStr = alert.Threshold.HasValue 
            ? (alert.AlertType == AlertType.RsiOversold || alert.AlertType == AlertType.RsiOverbought 
                ? alert.Threshold.Value.ToString("F1") 
                : alert.Threshold.Value.ToString("F2"))
            : alert.Payload ?? "threshold";
        
        var comparisonStr = alert.ComparisonType.HasValue
            ? alert.ComparisonType.Value switch
            {
                ComparisonType.Above => "above",
                ComparisonType.Below => "below",
                ComparisonType.CrossesOver => "crossed over",
                ComparisonType.CrossesBelow => "crossed below",
                _ => ""
            }
            : "";
        
        var timeframeStr = !string.IsNullOrEmpty(alert.Timeframe) ? $" ({alert.Timeframe})" : "";
        
        return alert.AlertType switch
        {
            AlertType.PriceAbove => $"{alert.Symbol} reached ${price:F2} {comparisonStr} {thresholdStr}",
            AlertType.PriceBelow => $"{alert.Symbol} reached ${price:F2} {comparisonStr} {thresholdStr}",
            AlertType.RsiOversold => rsiValue.HasValue
                ? $"{alert.Symbol} RSI {comparisonStr} {thresholdStr} at {rsiValue.Value:F1}{timeframeStr} (${price:F2})"
                : $"{alert.Symbol} RSI oversold at ${price:F2}{timeframeStr}",
            AlertType.RsiOverbought => rsiValue.HasValue
                ? $"{alert.Symbol} RSI {comparisonStr} {thresholdStr} at {rsiValue.Value:F1}{timeframeStr} (${price:F2})"
                : $"{alert.Symbol} RSI overbought at ${price:F2}{timeframeStr}",
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

    public static (int period, double oversold, double overbought) ParseRsiPayload(string? payload)
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
