using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Domain.Events;

public readonly record struct OrderIntentEvent(DateTime Time, OrderIntent Intent) : ITradingEvent
{
    public string Type => "OrderIntent";
}
