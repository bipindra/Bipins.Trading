using System.Text.Json;
using Bipins.Trading.Domain;
using Bipins.Trading.Charting;
using Bipins.Trading.Engine;
using Bipins.Trading.Execution;
using Bipins.Trading.Risk;
using Bipins.Trading.Strategies;

if (args.Length > 0 && args[0] == "backtest")
{
    var configPath = args.Length > 1 ? args[1] : "config.json";
    if (!File.Exists(configPath))
    {
        await Console.Out.WriteLineAsync($"Config not found: {configPath}. Create a config with Symbols, Timeframes, Start, End.");
        return 1;
    }
    var json = await File.ReadAllTextAsync(configPath);
    var config = JsonSerializer.Deserialize<EngineConfig>(json);
    if (config?.Symbols == null || config.Symbols.Count == 0)
    {
        await Console.Out.WriteLineAsync("Config must have Symbols and Timeframes.");
        return 1;
    }

    var candles = LoadCandles(config);
    var feed = new HistoricalCandleFeed((s, tf, start, end) =>
        candles.Where(c => c.Symbol == s && c.Timeframe == tf && c.Time >= start && c.Time <= end));

    var bus = new InMemoryEventBus();
    var portfolio = new PortfolioService();
    var fillReceiver = new BacktestFillReceiver(portfolio, bus);
    var execution = new PaperExecutionAdapter(fillReceiver);
    var risk = new CompositeRiskManager(new IRiskPolicy[] { new MaxPositionsPolicy(5) });
    var sizer = new FixedQtySizer(100);
    var strategies = new IStrategy[] { new EmaCrossoverStrategy(12, 26), new RsiMeanReversionStrategy(14, 30, 70) };
    var chartPath = config.ChartExportPath ?? "signals.json";
    var chartSink = new JsonFileChartSink(chartPath);

    var runner = new BacktestRunner(feed, bus, portfolio, risk, sizer, execution, strategies, chartSink);
    var backtestConfig = new BacktestConfig
    {
        Symbols = config.Symbols,
        Timeframes = config.Timeframes,
        Start = config.Start,
        End = config.End,
        InitialCash = config.InitialCash,
        WarmupBars = config.WarmupBars
    };
    var result = runner.Run(backtestConfig, fillReceiver);

    await Console.Out.WriteLineAsync($"Backtest done. Final equity: {result.FinalEquity}, Trades: {result.Trades.Count}");
    await Console.Out.WriteLineAsync($"Signals exported to {chartPath}");
    return 0;
}

await Console.Out.WriteLineAsync("Usage: backtest [config.json]");
return 0;

static IReadOnlyList<Candle> LoadCandles(EngineConfig config)
{
    if (config.DataPath != null && File.Exists(config.DataPath))
    {
        var lines = File.ReadAllLines(config.DataPath).Skip(1).ToList();
        var list = new List<Candle>();
        var symbol = config.Symbols.Count > 0 ? config.Symbols[0] : "SYMBOL";
        var tf = config.Timeframes.Count > 0 ? config.Timeframes[0] : "1d";
        foreach (var line in lines)
        {
            var p = line.Split(',');
            if (p.Length >= 6 && DateTime.TryParse(p[0], out var time) && decimal.TryParse(p[1], out var o) && decimal.TryParse(p[2], out var h) && decimal.TryParse(p[3], out var l) && decimal.TryParse(p[4], out var c) && decimal.TryParse(p[5], out var v))
                list.Add(new Candle(time, o, h, l, c, v, symbol, tf));
        }
        return list;
    }
    var gen = new List<Candle>();
    var t = config.Start;
    var rnd = new Random(42);
    var price = 100m;
    while (t <= config.End)
    {
        var change = (decimal)(rnd.NextDouble() - 0.48) * 2;
        price = Math.Max(1, price + change);
        var o = price;
        var c = price + (decimal)(rnd.NextDouble() - 0.5);
        var h = Math.Max(o, c) + (decimal)rnd.NextDouble();
        var low = Math.Min(o, c) - (decimal)rnd.NextDouble();
        gen.Add(new Candle(t, o, h, low, c, 1000, config.Symbols[0], config.Timeframes[0]));
        t = t.AddDays(1);
    }
    return gen;
}

internal class EngineConfig
{
    public List<string> Symbols { get; set; } = new();
    public List<string> Timeframes { get; set; } = new() { "1d" };
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public decimal InitialCash { get; set; } = 100000m;
    public int WarmupBars { get; set; } = 100;
    public string? DataPath { get; set; }
    public string? ChartExportPath { get; set; }
}
