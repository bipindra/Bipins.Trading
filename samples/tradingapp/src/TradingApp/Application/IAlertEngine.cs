using TradingApp.Application.DTOs;

namespace TradingApp.Application;

/// <summary>
/// Future library adapter for symbol alerts and strategies.
/// No implementation in this repo; alerts are only persisted.
/// </summary>
public interface IAlertEngine
{
    Task<bool> ExecuteAsync(AlertRequest request, CancellationToken ct = default);
}
