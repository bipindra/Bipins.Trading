using Bipins.AI.Trading.Domain;
using Bipins.AI.Trading.Domain.Events;

namespace Bipins.AI.Trading.Risk;

public sealed class CompositeRiskManager : IRiskManager
{
    private readonly IReadOnlyList<IRiskPolicy> _policies;

    public CompositeRiskManager(IReadOnlyList<IRiskPolicy> policies) => _policies = policies;

    public RiskDecisionEvent Approve(OrderIntent intent, PortfolioState portfolio, object? context = null)
    {
        foreach (var p in _policies)
        {
            var decision = p.Evaluate(intent, portfolio, context);
            if (!decision.Approved)
                return decision;
        }
        return new RiskDecisionEvent(DateTime.UtcNow, intent, true, null);
    }
}
