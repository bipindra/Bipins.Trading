namespace TradingApp.Domain;

public enum AlertType
{
    PriceAbove = 0,
    PriceBelow = 1,
    PercentChange = 2,
    RsiOversold = 3,
    RsiOverbought = 4,
}
