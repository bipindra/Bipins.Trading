namespace TradingApp.Application.DTOs;

public record StockSearchResult(string Symbol, string Name, string? Exchange = null, string? Type = null);
