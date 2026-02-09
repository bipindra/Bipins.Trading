using TradingApp.Domain;

namespace TradingApp.Application;

/// <summary>
/// Optional engine to execute a trade when an alert triggers (e.g. paper or live via Bipins.Trading).
/// When registered, the alert watcher calls this after creating the notification.
/// </summary>
public interface IExecutionEngine
{
    Task ExecuteOnTriggerAsync(Alert alert, decimal triggerPrice, CancellationToken ct = default);
}
