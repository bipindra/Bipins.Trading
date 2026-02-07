using Bipins.Trading.Domain;

namespace Bipins.Trading.Domain.Events;

public readonly record struct PositionChangedEvent(DateTime Time, Position Position) : ITradingEvent
{
    public string Type => "PositionChanged";
}
