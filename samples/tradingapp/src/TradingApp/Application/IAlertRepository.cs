using TradingApp.Domain;

namespace TradingApp.Application;

public interface IAlertRepository
{
    Task<Alert> AddAsync(Alert alert, CancellationToken ct = default);
    Task<IReadOnlyList<Alert>> GetBySymbolAsync(string symbol, CancellationToken ct = default);
    Task<IReadOnlyList<Alert>> GetAllAsync(CancellationToken ct = default);
    Task SetTriggeredAtAsync(int alertId, DateTime triggeredAt, CancellationToken ct = default);
}
