namespace Bipins.AI.Trading.Domain.Events;

public readonly record struct DiagnosticEvent(DateTime Time, string Message, IReadOnlyDictionary<string, object>? Data = null) : ITradingEvent
{
    public string Type => "Diagnostic";
}
