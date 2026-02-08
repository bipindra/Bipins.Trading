using Bipins.Trading.Domain;
using Bipins.Trading.Domain.Events;
using Bipins.Trading.Engine;
using Bipins.Trading.Execution;
using Microsoft.Extensions.Logging;

namespace Bipins.Trading.Samples.Alpaca;

/// <summary>
/// Fill receiver for live trading that updates portfolio and publishes events.
/// </summary>
public sealed class LiveFillReceiver : IFillReceiver
{
    private readonly IPortfolioService _portfolio;
    private readonly IEventBus? _eventBus;
    private readonly ILogger<LiveFillReceiver>? _logger;

    public LiveFillReceiver(
        IPortfolioService portfolio,
        IEventBus? eventBus = null,
        ILogger<LiveFillReceiver>? logger = null)
    {
        _portfolio = portfolio ?? throw new ArgumentNullException(nameof(portfolio));
        _eventBus = eventBus;
        _logger = logger;
    }

    public void OnFill(Fill fill)
    {
        try
        {
            _logger?.LogInformation("Fill received: {Symbol} {Side} {Quantity} @ ${Price:F2}",
                fill.Symbol, fill.Side, fill.Quantity, fill.Price);

            // Update portfolio
            _portfolio.Apply(fill);

            // Publish event
            _eventBus?.Publish(new FillEvent(fill.Time, fill));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error processing fill: {Fill}", fill);
        }
    }
}
