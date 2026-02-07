using Bipins.Trading.Domain;

namespace Bipins.Trading.Domain.Events;

public readonly record struct FillEvent(DateTime Time, Fill Fill) : ITradingEvent
{
    public string Type => "Fill";
}
