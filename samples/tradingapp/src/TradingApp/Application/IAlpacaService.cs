using Bipins.Trading.Domain;
using TradingApp.Application.DTOs;

namespace TradingApp.Application;

public interface IAlpacaService
{
    Task<IReadOnlyList<StockSearchResult>> SearchAssetsAsync(string query, CancellationToken ct = default);
    Task<StockDetailDto?> GetAssetAsync(string symbol, CancellationToken ct = default);
    /// <summary>Gets latest trade price for alert evaluation (uses Alpaca Data API when configured).</summary>
    Task<decimal?> GetLatestPriceAsync(string symbol, CancellationToken ct = default);
    /// <summary>Gets historical bars for indicator-based alerts (Alpaca Data API v2). Timeframe e.g. "1Day", "1Hour".</summary>
    Task<IReadOnlyList<Candle>> GetBarsAsync(string symbol, string timeframe, DateTime start, DateTime end, CancellationToken ct = default);
}
