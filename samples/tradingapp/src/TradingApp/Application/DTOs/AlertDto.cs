using TradingApp.Domain;

namespace TradingApp.Application.DTOs;

public record AlertDto(int Id, string Symbol, AlertType AlertType, string? Payload, DateTime CreatedAt);
