using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Risk;

public sealed class AtrRiskSizer : IPositionSizer
{
    private readonly decimal _riskPercentPerTrade;
    private readonly Func<object?, decimal?> _getAtrOrStopDistance;

    public AtrRiskSizer(decimal riskPercentPerTrade, Func<object?, decimal?> getAtrOrStopDistance)
    {
        _riskPercentPerTrade = riskPercentPerTrade;
        _getAtrOrStopDistance = getAtrOrStopDistance;
    }

    public OrderIntent Size(OrderIntent intent, PortfolioState portfolio, decimal currentPrice, object? context = null)
    {
        var stopDist = _getAtrOrStopDistance(context);
        if (!stopDist.HasValue || stopDist.Value <= 0 || portfolio.Equity <= 0) return intent;
        var riskAmount = portfolio.Equity * (_riskPercentPerTrade / 100m);
        var qty = intent.Quantity ?? (riskAmount / stopDist.Value);
        return intent with { Quantity = qty };
    }
}
