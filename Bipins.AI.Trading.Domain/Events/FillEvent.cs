using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Domain.Events;

public readonly record struct FillEvent(DateTime Time, Fill Fill) : ITradingEvent
{
    public string Type => "Fill";
}
