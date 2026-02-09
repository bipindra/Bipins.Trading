namespace TradingApp.Application.DTOs;

public record StockDetailDto(string Symbol, string Name, string? Exchange = null, string? Type = null, decimal? LatestPrice = null);
