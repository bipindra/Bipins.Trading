using Bipins.Trading.Domain;
using Bipins.Trading.Domain.Events;

namespace Bipins.Trading.Risk;

public interface IRiskPolicy
{
    RiskDecisionEvent Evaluate(OrderIntent intent, PortfolioState portfolio, object? context = null);
}
