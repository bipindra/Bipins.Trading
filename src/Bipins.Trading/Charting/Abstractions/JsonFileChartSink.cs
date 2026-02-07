using System.Text.Json;
using Bipins.Trading.Domain;

namespace Bipins.Trading.Charting;

public sealed class JsonFileChartSink : IChartSink
{
    private readonly string _path;
    private readonly List<SignalEvent> _buffer = new();
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public JsonFileChartSink(string path)
    {
        _path = path;
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    public void Publish(SignalEvent signal) => _buffer.Add(signal);

    public void Flush()
    {
        var dto = _buffer.Select(s => new
        {
            s.Strategy,
            s.Symbol,
            s.Time,
            SignalType = s.SignalType.ToString(),
            s.Price,
            s.Reason
        }).ToList();
        File.WriteAllText(_path, JsonSerializer.Serialize(dto, _options));
    }
}
