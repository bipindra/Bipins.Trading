using Bipins.Trading.Domain;

namespace Bipins.Trading.Domain.Events;

public readonly record struct OrderIntentEvent(DateTime Time, OrderIntent Intent) : ITradingEvent
{
    public string Type => "OrderIntent";
}
