using TradingApp.Domain;

namespace TradingApp.Application.DTOs;

public record AlertRequest(string Symbol, AlertType AlertType, string? Payload = null);
