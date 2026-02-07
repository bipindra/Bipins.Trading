namespace Bipins.Trading.Domain;

public readonly record struct SignalEvent(
    string Strategy,
    string Symbol,
    DateTime Time,
    SignalType SignalType,
    decimal? Price,
    string? Reason,
    IReadOnlyDictionary<string, decimal>? Metrics = null);
