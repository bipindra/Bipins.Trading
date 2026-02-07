using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Domain.Events;

public readonly record struct PositionChangedEvent(DateTime Time, Position Position) : ITradingEvent
{
    public string Type => "PositionChanged";
}
