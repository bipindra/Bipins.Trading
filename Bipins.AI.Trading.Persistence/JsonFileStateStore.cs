using System.Text.Json;
using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Persistence;

public sealed class JsonFileStateStore : IStateStore
{
    private readonly string _basePath;
    private readonly JsonSerializerOptions _options = new() { WriteIndented = false };

    public JsonFileStateStore(string basePath)
    {
        _basePath = basePath;
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    private static string FilePath(string basePath, string strategyId, string symbol)
    {
        var safe = $"{strategyId}_{symbol}".Replace("|", "_").Replace(" ", "_");
        return Path.Combine(basePath, $"{safe}.json");
    }

    public StrategyState? Get(string strategyId, string symbol)
    {
        var path = FilePath(_basePath, strategyId, symbol);
        if (!File.Exists(path)) return null;
        var json = File.ReadAllText(path);
        var dto = JsonSerializer.Deserialize<StateDto>(json);
        return dto == null ? null : new StrategyState { Version = dto.Version, JsonBlob = dto.JsonBlob };
    }

    public void Set(string strategyId, string symbol, StrategyState state)
    {
        var path = FilePath(_basePath, strategyId, symbol);
        var dto = new StateDto { Version = state.Version, JsonBlob = state.JsonBlob };
        File.WriteAllText(path, JsonSerializer.Serialize(dto, _options));
    }

    private class StateDto
    {
        public int Version { get; set; }
        public string? JsonBlob { get; set; }
    }
}
