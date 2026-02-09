using TradingApp.Application.DTOs;
using TradingApp.Domain;

namespace TradingApp.Application;

public sealed class AlertService
{
    private readonly IAlertRepository _repository;
    private readonly IActivityLogRepository? _activityLogRepository;

    public AlertService(IAlertRepository repository, IActivityLogRepository? activityLogRepository = null)
    {
        _repository = repository;
        _activityLogRepository = activityLogRepository;
    }

    public async Task<AlertDto> CreateAsync(AlertRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
            throw new ArgumentException("Symbol is required.");
        var alert = new Alert
        {
            Symbol = request.Symbol.Trim().ToUpperInvariant(),
            AlertType = request.AlertType,
            Payload = request.Payload,
            Threshold = request.Threshold,
            ComparisonType = request.ComparisonType,
            Timeframe = request.Timeframe,
            EnableAutoExecute = request.EnableAutoExecute,
            OrderQuantity = request.OrderQuantity,
            OrderType = request.OrderType,
            OrderSideOverride = request.OrderSideOverride,
            OrderLimitPrice = request.OrderLimitPrice,
            OrderStopPrice = request.OrderStopPrice,
            OrderTimeInForce = request.OrderTimeInForce
        };
        var saved = await _repository.AddAsync(alert, ct);
        
        // Log that monitoring has started for this symbol
        if (_activityLogRepository != null)
        {
            try
            {
                await _activityLogRepository.AddAsync(new ActivityLog
                {
                    Level = "Info",
                    Category = "AlertWatch",
                    Symbol = saved.Symbol,
                    AlertId = saved.Id.ToString(),
                    Message = $"Alert created - Monitoring started for {saved.Symbol} ({saved.AlertType})",
                    Details = System.Text.Json.JsonSerializer.Serialize(new { alertType = saved.AlertType.ToString(), payload = saved.Payload })
                }, ct);
            }
            catch
            {
                // Silently fail if logging isn't available (e.g., table doesn't exist yet)
            }
        }
        
        return new AlertDto(saved.Id, saved.Symbol, saved.AlertType, saved.Payload, saved.CreatedAt);
    }

    public async Task<IReadOnlyList<AlertDto>> GetBySymbolAsync(string symbol, CancellationToken ct = default)
    {
        var list = await _repository.GetBySymbolAsync(symbol ?? "", ct);
        return list.Select(a => new AlertDto(a.Id, a.Symbol, a.AlertType, a.Payload, a.CreatedAt)).ToList();
    }

    public async Task<bool> DeleteAsync(int alertId, CancellationToken ct = default)
    {
        return await _repository.DeleteAsync(alertId, ct);
    }
}
