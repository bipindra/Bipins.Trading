using Bipins.Trading.Domain;

namespace Bipins.Trading.Risk;

public sealed class PercentEquitySizer : IPositionSizer
{
    private readonly decimal _percent; // e.g. 2 = 2%

    public PercentEquitySizer(decimal percent) => _percent = percent;

    public OrderIntent Size(OrderIntent intent, PortfolioState portfolio, decimal currentPrice, object? context = null)
    {
        if (currentPrice <= 0 || portfolio.Equity <= 0) return intent;
        var notional = portfolio.Equity * (_percent / 100m);
        var qty = intent.Quantity ?? (notional / currentPrice);
        return intent with { Quantity = qty };
    }
}
