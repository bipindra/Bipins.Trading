namespace Bipins.Trading.Domain.Events;

public readonly record struct RiskDecisionEvent(DateTime Time, OrderIntent Intent, bool Approved, string? Reason) : ITradingEvent
{
    public string Type => "RiskDecision";
}
