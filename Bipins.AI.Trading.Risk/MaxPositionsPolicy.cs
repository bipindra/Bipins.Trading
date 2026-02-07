using Bipins.AI.Trading.Domain;
using Bipins.AI.Trading.Domain.Events;

namespace Bipins.AI.Trading.Risk;

public sealed class MaxPositionsPolicy : IRiskPolicy
{
    private readonly int _maxPositions;

    public MaxPositionsPolicy(int maxPositions) => _maxPositions = maxPositions;

    public RiskDecisionEvent Evaluate(OrderIntent intent, PortfolioState portfolio, object? context = null)
    {
        var openCount = portfolio.Positions.Values.Count(p => p.Side != PositionSide.Flat && p.Quantity != 0);
        if (intent.Quantity.HasValue && intent.Quantity.Value != 0)
        {
            var hasPosition = portfolio.Positions.TryGetValue(intent.Symbol, out var pos) && pos.Quantity != 0;
            var newPosition = !hasPosition;
            if (newPosition && openCount >= _maxPositions)
                return new RiskDecisionEvent(DateTime.UtcNow, intent, false, $"Max positions ({_maxPositions}) reached.");
        }
        return new RiskDecisionEvent(DateTime.UtcNow, intent, true, null);
    }
}
