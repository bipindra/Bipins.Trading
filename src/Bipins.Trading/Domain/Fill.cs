namespace Bipins.Trading.Domain;

public readonly record struct Fill(
    string Symbol,
    DateTime Time,
    OrderSide Side,
    decimal Quantity,
    decimal Price,
    decimal Fees,
    string? ClientOrderId = null);
