namespace TradingApp.Application.DTOs;

public record WatchlistItemDto(string Symbol, string? Name, DateTime AddedAt, decimal? LatestPrice = null);
