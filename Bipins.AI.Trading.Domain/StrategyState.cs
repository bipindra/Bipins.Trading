namespace Bipins.AI.Trading.Domain;

public sealed class StrategyState
{
    public int Version { get; set; }
    public string? JsonBlob { get; set; }
}
