namespace Bipins.Trading.Domain;

public readonly record struct Quote(
    DateTime Time,
    decimal Bid,
    decimal Ask,
    string? Symbol = null);
