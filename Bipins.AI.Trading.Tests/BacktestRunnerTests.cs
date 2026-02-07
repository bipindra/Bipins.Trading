using Bipins.AI.Trading.Domain;
using Bipins.AI.Trading.Engine;
using Bipins.AI.Trading.Execution;
using Bipins.AI.Trading.Risk;
using Bipins.AI.Trading.Strategies;
using Xunit;

namespace Bipins.AI.Trading.Tests;

public class BacktestRunnerTests
{
    [Fact]
    public void Run_with_synthetic_candles_produces_equity_curve()
    {
        var candles = new List<Candle>();
        var start = new DateTime(2020, 1, 1);
        var rnd = new Random(12345);
        var price = 100m;
        for (var i = 0; i < 200; i++)
        {
            var t = start.AddDays(i);
            price = Math.Max(1, price + (decimal)(rnd.NextDouble() - 0.48) * 2);
            var o = price;
            var c = price + (decimal)(rnd.NextDouble() - 0.5);
            candles.Add(new Candle(t, o, Math.Max(o, c) + 1, Math.Min(o, c) - 1, c, 1000, "S", "1d"));
        }
        var feed = new HistoricalCandleFeed((s, tf, st, end) => candles.Where(c => c.Time >= st && c.Time <= end));
        var bus = new InMemoryEventBus();
        var portfolio = new PortfolioService();
        var fillReceiver = new BacktestFillReceiver(portfolio, bus);
        var execution = new PaperExecutionAdapter(fillReceiver);
        var risk = new CompositeRiskManager(new IRiskPolicy[] { new MaxPositionsPolicy(2) });
        var sizer = new FixedQtySizer(10);
        var strategies = new IStrategy[] { new EmaCrossoverStrategy(12, 26) };
        var runner = new BacktestRunner(feed, bus, portfolio, risk, sizer, execution, strategies);
        var config = new BacktestConfig
        {
            Symbols = new[] { "S" },
            Timeframes = new[] { "1d" },
            Start = start,
            End = start.AddDays(199),
            InitialCash = 100000m,
            WarmupBars = 50
        };
        var result = runner.Run(config, fillReceiver);
        Assert.NotNull(result.EquityCurve);
        Assert.True(result.EquityCurve.Count > 0);
        Assert.Equal(100000m, result.EquityCurve[0].Equity);
    }

    [Fact]
    public void Run_twice_same_config_deterministic()
    {
        var candles = new List<Candle>();
        var start = new DateTime(2020, 1, 1);
        var rnd = new Random(999);
        var price = 100m;
        for (var i = 0; i < 150; i++)
        {
            var t = start.AddDays(i);
            price = Math.Max(1, price + (decimal)(rnd.NextDouble() - 0.5));
            candles.Add(new Candle(t, price, price + 1, price - 1, price, 1000, "X", "1d"));
        }
        var feed = new HistoricalCandleFeed((s, tf, st, end) => candles.Where(c => c.Time >= st && c.Time <= end));
        decimal RunOnce()
        {
            var bus = new InMemoryEventBus();
            var portfolio = new PortfolioService();
            var fillReceiver = new BacktestFillReceiver(portfolio, bus);
            var execution = new PaperExecutionAdapter(fillReceiver);
            var risk = new CompositeRiskManager(Array.Empty<IRiskPolicy>());
            var sizer = new FixedQtySizer(1);
            var runner = new BacktestRunner(feed, bus, portfolio, risk, sizer, execution, new IStrategy[] { new RsiMeanReversionStrategy(14, 30, 70) });
            var result = runner.Run(new BacktestConfig { Symbols = new[] { "X" }, Timeframes = new[] { "1d" }, Start = start, End = start.AddDays(149), WarmupBars = 20 }, fillReceiver);
            return result.FinalEquity;
        }
        var a = RunOnce();
        var b = RunOnce();
        Assert.Equal(a, b);
    }
}
