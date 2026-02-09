using System.Text.Json;
using Bipins.Trading.Domain;
using Bipins.Trading.Domain.Events;
using Bipins.Trading.Engine;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingApp.Application;
using TradingApp.Domain;
using TradingApp.Web.Hubs;

namespace TradingApp.Web.Services;

/// <summary>
/// Background service that evaluates alerts against current prices and indicator values (Bipins.Trading RSI).
/// Fetches bars from Alpaca Data API for RSI-based alerts; publishes SignalEvent to IEventBus when triggered.
/// </summary>
public sealed class AlertWatchHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlertWatchHostedService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    private const string DefaultTimeframe = "1Day";
    private static readonly TimeSpan DefaultBarsLookback = TimeSpan.FromDays(60);
    
    // Helper to determine lookback period based on timeframe
    private static TimeSpan GetBarsLookback(string timeframe)
    {
        return timeframe switch
        {
            "1Min" => TimeSpan.FromDays(1),
            "3Min" => TimeSpan.FromDays(3),
            "5Min" => TimeSpan.FromDays(5),
            "15Min" => TimeSpan.FromDays(7),
            "1Hour" => TimeSpan.FromDays(30),
            "1Day" => TimeSpan.FromDays(60),
            _ => DefaultBarsLookback
        };
    }

    public AlertWatchHostedService(IServiceScopeFactory scopeFactory, ILogger<AlertWatchHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AlertWatchHostedService started - will monitor alerts and quotes every {Interval} seconds", Interval.TotalSeconds);
        
        // Start immediately, then wait for interval
        try
        {
            await EvaluateAlertsAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initial alert evaluation failed");
        }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, stoppingToken);
                await EvaluateAlertsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alert watch cycle failed");
            }
        }
    }

    private async Task EvaluateAlertsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var alertRepo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var alpaca = scope.ServiceProvider.GetRequiredService<IAlpacaService>();
        var eventBus = scope.ServiceProvider.GetService<IEventBus>();
        var activityLogRepo = scope.ServiceProvider.GetRequiredService<IActivityLogRepository>();
        var watchlistRepo = scope.ServiceProvider.GetRequiredService<IWatchlistRepository>();
        var hubContext = scope.ServiceProvider.GetService<IHubContext<ActivityLogHub>>();

        await LogActivityAsync(activityLogRepo, hubContext, "Info", "AlertWatch", null, null, "Starting alert evaluation cycle", null, ct);

        var alerts = await alertRepo.GetAllAsync(ct);
        
        if (alerts.Count > 0)
        {
            await LogActivityAsync(activityLogRepo, hubContext, "Info", "AlertWatch", null, null, $"Found {alerts.Count} active alert(s) to evaluate", null, ct);

        var symbols = alerts.Select(a => a.Symbol).Distinct().ToList();
        await LogActivityAsync(activityLogRepo, hubContext, "Info", "AlertWatch", null, null, $"Fetching latest prices for {symbols.Count} unique symbol(s): {string.Join(", ", symbols)}", null, ct);

        var prices = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);
        foreach (var symbol in symbols)
        {
            try
            {
                _logger.LogInformation("Fetching quote for {Symbol}", symbol);
                var price = await alpaca.GetLatestPriceAsync(symbol, ct);
                prices[symbol] = price;
                // Log every quote received - this is the key requirement
                if (price.HasValue)
                {
                    _logger.LogInformation("Quote received for {Symbol}: ${Price:F2} - Logging to activity log", symbol, price.Value);
                    await LogActivityAsync(activityLogRepo, hubContext, "Info", "QuoteIngestion", symbol, null, $"Quote received: ${price.Value:F2}", JsonSerializer.Serialize(new { price = price.Value, timestamp = DateTime.UtcNow }), ct);
                }
                else
                {
                    _logger.LogWarning("Quote received for {Symbol} but price is null", symbol);
                    await LogActivityAsync(activityLogRepo, hubContext, "Warning", "QuoteIngestion", symbol, null, "Quote received but price is null", null, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch price for {Symbol}", symbol);
                await LogActivityAsync(activityLogRepo, hubContext, "Error", "QuoteIngestion", symbol, null, $"Failed to fetch price: {ex.Message}", JsonSerializer.Serialize(new { error = ex.Message }), ct);
            }
        }

        // Group RSI alerts by symbol and timeframe
        var rsiAlerts = alerts.Where(a => a.AlertType == AlertType.RsiOversold || a.AlertType == AlertType.RsiOverbought).ToList();
        var rsiSymbolsByTimeframe = rsiAlerts
            .GroupBy(a => new { a.Symbol, Timeframe = a.Timeframe ?? DefaultTimeframe })
            .ToList();
        
        if (rsiSymbolsByTimeframe.Count > 0)
        {
            await LogActivityAsync(activityLogRepo, hubContext, "Info", "AlertWatch", null, null, $"Fetching historical bars for {rsiAlerts.Count} RSI-based alert(s) across {rsiSymbolsByTimeframe.Count} symbol/timeframe combination(s)", null, ct);
        }

        var candlesBySymbolAndTimeframe = new Dictionary<string, IReadOnlyList<Candle>>(StringComparer.OrdinalIgnoreCase);
        var end = DateTime.UtcNow;
        
        foreach (var group in rsiSymbolsByTimeframe)
        {
            var symbol = group.Key.Symbol;
            var timeframe = group.Key.Timeframe;
            var lookback = GetBarsLookback(timeframe);
            var start = end - lookback;
            var key = $"{symbol}:{timeframe}";
            
            try
            {
                var bars = await alpaca.GetBarsAsync(symbol, timeframe, start, end, ct);
                if (bars.Count > 0)
                {
                    candlesBySymbolAndTimeframe[key] = bars;
                    await LogActivityAsync(activityLogRepo, hubContext, "Info", "AlertWatch", symbol, null, $"Fetched {bars.Count} historical bar(s) for RSI calculation ({timeframe})", JsonSerializer.Serialize(new { barCount = bars.Count, timeframe }), ct);
                }
                else
                {
                    await LogActivityAsync(activityLogRepo, hubContext, "Warning", "AlertWatch", symbol, null, $"No historical bars returned for RSI calculation ({timeframe})", null, ct);
                }
            }
            catch (Exception ex)
            {
                await LogActivityAsync(activityLogRepo, hubContext, "Error", "AlertWatch", symbol, null, $"Failed to fetch historical bars ({timeframe}): {ex.Message}", null, ct);
                _logger.LogError(ex, "Failed to fetch bars for {Symbol} ({Timeframe})", symbol, timeframe);
            }
        }

        foreach (var alert in alerts)
        {
            await LogActivityAsync(activityLogRepo, hubContext, "Debug", "AlertWatch", alert.Symbol, alert.Id.ToString(), 
                $"Evaluating alert: {alert.AlertType} for {alert.Symbol} (AutoExecute: {alert.EnableAutoExecute}, Threshold: {alert.Threshold}, Comparison: {alert.ComparisonType})", 
                JsonSerializer.Serialize(new { alertType = alert.AlertType.ToString(), payload = alert.Payload, threshold = alert.Threshold, comparisonType = alert.ComparisonType, enableAutoExecute = alert.EnableAutoExecute }), ct);

            if (!prices.TryGetValue(alert.Symbol, out var price) || price == null)
            {
                await LogActivityAsync(activityLogRepo, hubContext, "Warning", "AlertWatch", alert.Symbol, alert.Id.ToString(), "No price available for alert evaluation", null, ct);
                continue;
            }

            // Get candles for this alert's symbol and timeframe
            var alertTimeframe = alert.Timeframe ?? DefaultTimeframe;
            var candleKey = $"{alert.Symbol}:{alertTimeframe}";
            var candles = (alert.AlertType == AlertType.RsiOversold || alert.AlertType == AlertType.RsiOverbought)
                ? candlesBySymbolAndTimeframe.GetValueOrDefault(candleKey)
                : null;

            double? rsiValue = null;
            if (candles != null && (alert.AlertType == AlertType.RsiOversold || alert.AlertType == AlertType.RsiOverbought))
            {
                var (period, _, _) = AlertTriggerEvaluator.ParseRsiPayload(alert.Payload);
                rsiValue = AlertTriggerEvaluator.ComputeRsi(candles, period);
                await LogActivityAsync(activityLogRepo, hubContext, "Debug", "AlertWatch", alert.Symbol, alert.Id.ToString(), $"RSI calculated: {rsiValue:F2} ({alertTimeframe})", JsonSerializer.Serialize(new { rsi = rsiValue, candleCount = candles.Count, timeframe = alertTimeframe }), ct);
            }

            var shouldTrigger = AlertTriggerEvaluator.ShouldTrigger(alert, price, candles);
            await LogActivityAsync(activityLogRepo, hubContext, "Debug", "AlertWatch", alert.Symbol, alert.Id.ToString(), $"Alert trigger check: {(shouldTrigger ? "TRIGGERED" : "Not triggered")} - Price: ${price:F2}", null, ct);

            // Store RSI value for crossover detection on next evaluation
            if (rsiValue.HasValue)
            {
                AlertTriggerEvaluator.StoreRsiValue(alert, rsiValue.Value);
            }

            if (!shouldTrigger)
                continue;

            // Recalculate RSI if not already calculated
            if (rsiValue == null && (alert.AlertType == AlertType.RsiOversold || alert.AlertType == AlertType.RsiOverbought) && candles != null)
            {
                var (period, _, _) = AlertTriggerEvaluator.ParseRsiPayload(alert.Payload);
                rsiValue = AlertTriggerEvaluator.ComputeRsi(candles, period);
            }
            var message = AlertTriggerEvaluator.GetMessage(alert, price.Value, rsiValue);

            var notification = new Notification
            {
                AlertId = alert.Id,
                Symbol = alert.Symbol,
                Message = message
            };
            await notificationRepo.AddAsync(notification, ct);
            await alertRepo.SetTriggeredAtAsync(alert.Id, DateTime.UtcNow, ct);
            _logger.LogInformation("Alert {AlertId} triggered: {Message}", alert.Id, message);
            
            await LogActivityAsync(activityLogRepo, hubContext, "Info", "AlertWatch", alert.Symbol, alert.Id.ToString(), $"ALERT TRIGGERED: {message}", JsonSerializer.Serialize(new { price, rsiValue }), ct);

            if (eventBus != null)
            {
                var signalType = alert.AlertType == AlertType.PriceAbove || alert.AlertType == AlertType.RsiOversold
                    ? SignalType.EntryLong
                    : SignalType.EntryShort;
                var signal = new SignalEvent("TradingApp.Alert", alert.Symbol, DateTime.UtcNow, signalType, price, message, null);
                eventBus.Publish(new SignalEventEvent(DateTime.UtcNow, signal));
                await LogActivityAsync(activityLogRepo, hubContext, "Info", "SignalGeneration", alert.Symbol, alert.Id.ToString(), $"Signal published: {signalType}", JsonSerializer.Serialize(new { signalType = signalType.ToString(), price }), ct);
            }

            // Execute order if auto-execute is enabled
            if (alert.EnableAutoExecute)
            {
                var executionEngine = scope.ServiceProvider.GetService<IExecutionEngine>();
                if (executionEngine != null)
                {
                    try
                    {
                        await LogActivityAsync(activityLogRepo, hubContext, "Info", "AlertWatch", alert.Symbol, alert.Id.ToString(), 
                            $"Executing order on alert trigger (auto-execute enabled) - Side: {alert.OrderSideOverride}, Type: {alert.OrderType}, Qty: {alert.OrderQuantity}", 
                            JsonSerializer.Serialize(new { orderSide = alert.OrderSideOverride, orderType = alert.OrderType, quantity = alert.OrderQuantity, limitPrice = alert.OrderLimitPrice, stopPrice = alert.OrderStopPrice }), ct);
                        await executionEngine.ExecuteOnTriggerAsync(alert, price.Value, ct);
                        await LogActivityAsync(activityLogRepo, hubContext, "Info", "AlertWatch", alert.Symbol, alert.Id.ToString(), "Order execution completed successfully", null, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Execution on trigger failed for alert {AlertId}", alert.Id);
                        await LogActivityAsync(activityLogRepo, hubContext, "Error", "AlertWatch", alert.Symbol, alert.Id.ToString(), 
                            $"Order execution failed: {ex.Message}", 
                            JsonSerializer.Serialize(new { error = ex.Message, stackTrace = ex.StackTrace }), ct);
                    }
                }
                else
                {
                    _logger.LogWarning("IExecutionEngine not registered but alert {AlertId} has EnableAutoExecute=true", alert.Id);
                    await LogActivityAsync(activityLogRepo, hubContext, "Error", "AlertWatch", alert.Symbol, alert.Id.ToString(), 
                        "Auto-execute enabled but IExecutionEngine not registered. Check configuration.", null, ct);
                }
            }
            else
            {
                await LogActivityAsync(activityLogRepo, hubContext, "Debug", "AlertWatch", alert.Symbol, alert.Id.ToString(), 
                    "Alert triggered but auto-execute is disabled (EnableAutoExecute=false)", null, ct);
            }
        }

            await LogActivityAsync(activityLogRepo, hubContext, "Info", "AlertWatch", null, null, "Alert evaluation cycle completed", null, ct);
        }
        else
        {
            await LogActivityAsync(activityLogRepo, hubContext, "Debug", "AlertWatch", null, null, "No active alerts to evaluate", null, ct);
        }
        
        // Always monitor watchlist symbols for quote logging, regardless of alerts
        await MonitorWatchlistSymbolsAsync(alpaca, activityLogRepo, hubContext, watchlistRepo, ct);
    }

    private async Task MonitorWatchlistSymbolsAsync(
        IAlpacaService alpaca,
        IActivityLogRepository activityLogRepo,
        IHubContext<ActivityLogHub>? hubContext,
        IWatchlistRepository watchlistRepo,
        CancellationToken ct)
    {
        try
        {
            var watchlistItems = await watchlistRepo.GetAllAsync(ct);
            if (watchlistItems.Count == 0) return;

            var watchlistSymbols = watchlistItems.Select(w => w.Symbol).Distinct().ToList();
            await LogActivityAsync(activityLogRepo, hubContext, "Info", "QuoteIngestion", null, null, $"Monitoring {watchlistSymbols.Count} watchlist symbol(s) for quotes", null, ct);

            foreach (var symbol in watchlistSymbols)
            {
                try
                {
                    var price = await alpaca.GetLatestPriceAsync(symbol, ct);
                    if (price.HasValue)
                    {
                        await LogActivityAsync(activityLogRepo, hubContext, "Info", "QuoteIngestion", symbol, null, $"Quote received: ${price.Value:F2}", JsonSerializer.Serialize(new { price = price.Value, timestamp = DateTime.UtcNow }), ct);
                    }
                    else
                    {
                        await LogActivityAsync(activityLogRepo, hubContext, "Warning", "QuoteIngestion", symbol, null, "No price available", null, ct);
                    }
                }
                catch (Exception ex)
                {
                    await LogActivityAsync(activityLogRepo, hubContext, "Error", "QuoteIngestion", symbol, null, $"Failed to fetch price: {ex.Message}", null, ct);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to monitor watchlist symbols");
        }
    }

    private async Task LogActivityAsync(
        IActivityLogRepository repo,
        IHubContext<ActivityLogHub>? hubContext,
        string level,
        string category,
        string? symbol,
        string? alertId,
        string message,
        string? details,
        CancellationToken ct)
    {
        // Always log to console/logger first for visibility
        var logMessage = $"[{category}] {message}";
        if (!string.IsNullOrEmpty(symbol)) logMessage = $"[{category}] [{symbol}] {message}";
        
        switch (level.ToUpperInvariant())
        {
            case "ERROR":
                _logger.LogError(logMessage);
                break;
            case "WARNING":
                _logger.LogWarning(logMessage);
                break;
            case "DEBUG":
                _logger.LogDebug(logMessage);
                break;
            default:
                _logger.LogInformation(logMessage);
                break;
        }
        
        // Create the log entry
        var logEntry = new ActivityLog
        {
            Level = level,
            Category = category,
            Symbol = symbol,
            AlertId = alertId,
            Message = message,
            Details = details,
            Timestamp = DateTime.UtcNow
        };
        
        // Try to save to database
        try
        {
            await repo.AddAsync(logEntry, ct);
        }
        catch (Exception ex)
        {
            // Log to logger so user can see if ActivityLogs table doesn't exist
            _logger.LogError(ex, "Failed to save activity log to database ({Category}): {Message}. Error: {Error}. This may indicate the ActivityLogs table needs to be created via migration.", category, message, ex.Message);
        }
        
        // Broadcast via SignalR if hub context is available
        if (hubContext != null)
        {
            try
            {
                var logDto = new
                {
                    id = logEntry.Id,
                    timestamp = logEntry.Timestamp,
                    level = logEntry.Level,
                    category = logEntry.Category,
                    symbol = logEntry.Symbol,
                    alertId = logEntry.AlertId,
                    message = logEntry.Message,
                    details = logEntry.Details
                };
                await hubContext.Clients.All.SendAsync("ActivityLog", logDto, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast activity log via SignalR");
            }
        }
    }
}
