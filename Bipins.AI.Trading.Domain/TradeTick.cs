namespace Bipins.AI.Trading.Domain;

public readonly record struct TradeTick(
    DateTime Time,
    decimal Price,
    decimal Size,
    string? Symbol = null);
