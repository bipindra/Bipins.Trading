namespace Bipins.AI.Trading.Domain;

public readonly record struct Position(
    string Symbol,
    PositionSide Side,
    decimal Quantity,
    decimal AvgPrice,
    decimal UnrealizedPnl,
    decimal RealizedPnl);
