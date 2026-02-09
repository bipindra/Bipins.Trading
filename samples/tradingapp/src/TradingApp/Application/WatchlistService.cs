using TradingApp.Application.DTOs;

namespace TradingApp.Application;

public sealed class WatchlistService
{
    private readonly IWatchlistRepository _repository;
    private readonly IAlpacaService _alpacaService;

    public WatchlistService(IWatchlistRepository repository, IAlpacaService alpacaService)
    {
        _repository = repository;
        _alpacaService = alpacaService;
    }

    public async Task<IReadOnlyList<WatchlistItemDto>> GetWatchlistAsync(CancellationToken ct = default)
    {
        var items = await _repository.GetAllAsync(ct);
        var dtos = new List<WatchlistItemDto>();
        foreach (var item in items)
        {
            var detail = await _alpacaService.GetAssetAsync(item.Symbol, ct);
            var price = await _alpacaService.GetLatestPriceAsync(item.Symbol, ct);
            dtos.Add(new WatchlistItemDto(item.Symbol, detail?.Name, item.AddedAt, price));
        }
        return dtos;
    }

    public async Task<WatchlistItemDto?> AddAsync(string symbol, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(symbol)) return null;
        var item = await _repository.AddAsync(symbol, ct);
        var detail = await _alpacaService.GetAssetAsync(item.Symbol, ct);
        var price = await _alpacaService.GetLatestPriceAsync(item.Symbol, ct);
        return new WatchlistItemDto(item.Symbol, detail?.Name, item.AddedAt, price);
    }

    public async Task<bool> RemoveAsync(string symbol, CancellationToken ct = default)
    {
        return await _repository.RemoveAsync(symbol ?? "", ct);
    }
}
