namespace TradingApp.Application.DTOs;

public record AlpacaSettingsDto(string? ApiKey, string? ApiSecretMasked, string? BaseUrl);
