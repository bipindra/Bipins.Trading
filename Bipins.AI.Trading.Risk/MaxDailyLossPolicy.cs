using Bipins.AI.Trading.Domain;
using Bipins.AI.Trading.Domain.Events;

namespace Bipins.AI.Trading.Risk;

public sealed class MaxDailyLossPolicy : IRiskPolicy
{
    private readonly decimal _maxDailyLossPercent;
    private decimal _dayStartEquity;
    private DateTime _dayDate = DateTime.MinValue;

    public MaxDailyLossPolicy(decimal maxDailyLossPercent) => _maxDailyLossPercent = maxDailyLossPercent;

    public RiskDecisionEvent Evaluate(OrderIntent intent, PortfolioState portfolio, object? context = null)
    {
        var today = portfolio.Equity; // caller can pass day-start equity via context if needed
        if (context is DailyLossContext dl)
        {
            if (dl.Date.Date != _dayDate.Date) { _dayDate = dl.Date.Date; _dayStartEquity = dl.DayStartEquity; }
            var loss = _dayStartEquity - portfolio.Equity;
            var limit = _dayStartEquity * (_maxDailyLossPercent / 100m);
            if (loss >= limit)
                return new RiskDecisionEvent(DateTime.UtcNow, intent, false, $"Daily loss limit ({_maxDailyLossPercent}%) reached.");
        }
        return new RiskDecisionEvent(DateTime.UtcNow, intent, true, null);
    }
}

public sealed class DailyLossContext
{
    public DateTime Date { get; set; }
    public decimal DayStartEquity { get; set; }
}
