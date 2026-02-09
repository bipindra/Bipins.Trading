using Bipins.Trading.Domain;
using Bipins.Trading.Domain.Events;
using Bipins.Trading.Engine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingApp.Application;
using TradingApp.Domain;

namespace TradingApp.Web.Services;

/// <summary>
/// Background service that evaluates alerts against current prices and indicator values (Bipins.Trading RSI).
/// Fetches bars from Alpaca Data API for RSI-based alerts; publishes SignalEvent to IEventBus when triggered.
/// </summary>
public sealed class AlertWatchHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlertWatchHostedService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private const string Timeframe = "1Day";
    private static readonly TimeSpan BarsLookback = TimeSpan.FromDays(60);

    public AlertWatchHostedService(IServiceScopeFactory scopeFactory, ILogger<AlertWatchHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EvaluateAlertsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Alert watch cycle failed");
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task EvaluateAlertsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var alertRepo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var alpaca = scope.ServiceProvider.GetRequiredService<IAlpacaService>();
        var eventBus = scope.ServiceProvider.GetService<IEventBus>();

        var alerts = await alertRepo.GetAllAsync(ct);
        if (alerts.Count == 0) return;

        var symbols = alerts.Select(a => a.Symbol).Distinct().ToList();
        var prices = new Dictionary<string, decimal?>(StringComparer.OrdinalIgnoreCase);
        foreach (var symbol in symbols)
        {
            prices[symbol] = await alpaca.GetLatestPriceAsync(symbol, ct);
        }

        var rsiSymbols = alerts.Where(a => a.AlertType == AlertType.RsiOversold || a.AlertType == AlertType.RsiOverbought)
            .Select(a => a.Symbol).Distinct().ToList();
        var candlesBySymbol = new Dictionary<string, IReadOnlyList<Candle>>(StringComparer.OrdinalIgnoreCase);
        var end = DateTime.UtcNow;
        var start = end - BarsLookback;
        foreach (var symbol in rsiSymbols)
        {
            var bars = await alpaca.GetBarsAsync(symbol, Timeframe, start, end, ct);
            if (bars.Count > 0)
                candlesBySymbol[symbol] = bars;
        }

        foreach (var alert in alerts)
        {
            if (!prices.TryGetValue(alert.Symbol, out var price) || price == null)
                continue;

            var candles = (alert.AlertType == AlertType.RsiOversold || alert.AlertType == AlertType.RsiOverbought)
                ? candlesBySymbol.GetValueOrDefault(alert.Symbol)
                : null;

            if (!AlertTriggerEvaluator.ShouldTrigger(alert, price, candles))
                continue;

            var rsiValue = (alert.AlertType == AlertType.RsiOversold || alert.AlertType == AlertType.RsiOverbought) && candles != null
                ? AlertTriggerEvaluator.ComputeRsi(candles)
                : (double?)null;
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

            if (eventBus != null)
            {
                var signalType = alert.AlertType == AlertType.PriceAbove || alert.AlertType == AlertType.RsiOversold
                    ? SignalType.EntryLong
                    : SignalType.EntryShort;
                var signal = new SignalEvent("TradingApp.Alert", alert.Symbol, DateTime.UtcNow, signalType, price, message, null);
                eventBus.Publish(new SignalEventEvent(DateTime.UtcNow, signal));
            }

            var executionEngine = scope.ServiceProvider.GetService<IExecutionEngine>();
            if (executionEngine != null)
            {
                try
                {
                    await executionEngine.ExecuteOnTriggerAsync(alert, price.Value, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Execution on trigger failed for alert {AlertId}", alert.Id);
                }
            }
        }
    }
}
