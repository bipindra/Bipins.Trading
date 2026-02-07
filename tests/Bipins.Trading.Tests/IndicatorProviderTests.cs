using Bipins.Trading.Domain;
using Bipins.Trading.Engine;
using Xunit;

namespace Bipins.Trading.Tests;

public class IndicatorProviderTests
{
    [Fact]
    public void Get_caches_result_per_key()
    {
        var provider = new IndicatorProvider();
        var key = IndicatorKey.Rsi(14);
        var candles = new List<Candle>();
        for (var i = 0; i < 50; i++)
            candles.Add(new Candle(DateTime.UtcNow.AddDays(i), 100 + i, 101 + i, 99 + i, 100 + i, 1000, "S", "1d"));
        var callCount = 0;
        double? Compute() { callCount++; return 45.0; }
        var a = provider.Get(key, Compute);
        var b = provider.Get(key, Compute);
        Assert.True(a.HasValue);
        Assert.Equal(45.0, a!.Value);
        Assert.Equal(b, a);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void ClearCache_resets_cache()
    {
        var provider = new IndicatorProvider();
        var key = IndicatorKey.Ema(10);
        provider.Get(key, () => 99.0);
        provider.ClearCache();
        var callCount = 0;
        provider.Get(key, () => { callCount++; return 88.0; });
        Assert.Equal(1, callCount);
    }
}
