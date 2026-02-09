using Bipins.Trading.Domain;
using Bipins.Trading.Domain.Events;
using Bipins.Trading.Engine;
using Bipins.Trading.Execution;
using Microsoft.Extensions.Logging;

namespace TradingApp.Web.Services;

/// <summary>
/// Receives paper fills from Bipins.Trading PaperExecutionAdapter, logs them, and publishes FillEvent to IEventBus.
/// </summary>
public sealed class TradingAppFillReceiver : IFillReceiver
{
    private readonly IEventBus? _eventBus;
    private readonly ILogger<TradingAppFillReceiver> _logger;

    public TradingAppFillReceiver(IEventBus eventBus, ILogger<TradingAppFillReceiver> logger)
    {
        _eventBus = eventBus;
        _logger = logger;
    }

    public void OnFill(Fill fill)
    {
        _logger.LogInformation(
            "Paper fill: {Side} {Quantity} {Symbol} @ {Price:F2} (fees: {Fees:F2})",
            fill.Side, fill.Quantity, fill.Symbol, fill.Price, fill.Fees);

        _eventBus?.Publish(new FillEvent(fill.Time, fill));
    }
}
