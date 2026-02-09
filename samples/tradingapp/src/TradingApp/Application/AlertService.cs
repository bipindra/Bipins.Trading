using TradingApp.Application.DTOs;
using TradingApp.Domain;

namespace TradingApp.Application;

public sealed class AlertService
{
    private readonly IAlertRepository _repository;

    public AlertService(IAlertRepository repository)
    {
        _repository = repository;
    }

    public async Task<AlertDto> CreateAsync(AlertRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
            throw new ArgumentException("Symbol is required.");
        var alert = new Alert
        {
            Symbol = request.Symbol.Trim().ToUpperInvariant(),
            AlertType = request.AlertType,
            Payload = request.Payload
        };
        var saved = await _repository.AddAsync(alert, ct);
        return new AlertDto(saved.Id, saved.Symbol, saved.AlertType, saved.Payload, saved.CreatedAt);
    }

    public async Task<IReadOnlyList<AlertDto>> GetBySymbolAsync(string symbol, CancellationToken ct = default)
    {
        var list = await _repository.GetBySymbolAsync(symbol ?? "", ct);
        return list.Select(a => new AlertDto(a.Id, a.Symbol, a.AlertType, a.Payload, a.CreatedAt)).ToList();
    }
}
