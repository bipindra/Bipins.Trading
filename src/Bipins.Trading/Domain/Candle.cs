namespace Bipins.Trading.Domain;

public readonly record struct Candle(
    DateTime Time,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    string Symbol,
    string Timeframe)
{
    public decimal Mid => (High + Low) / 2;
    public decimal TypicalPrice => (High + Low + Close) / 3;
}
