namespace TradingApp.Application.DTOs;

public record NotificationDto(int Id, int AlertId, string Symbol, string Message, DateTime TriggeredAt, DateTime? ReadAt);
