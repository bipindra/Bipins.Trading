namespace Bipins.AI.Trading.Domain;

public readonly record struct PortfolioState(
    decimal Equity,
    decimal Cash,
    IReadOnlyDictionary<string, Position> Positions);
