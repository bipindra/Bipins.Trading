namespace Bipins.AI.Trading.Domain.Events;

public interface ITradingEvent
{
    DateTime Time { get; }
    string Type { get; }
}
