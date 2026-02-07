using Bipins.Trading.Indicators.MovingAverages;
using Bipins.Trading.Indicators.Models;
using Xunit;

namespace Bipins.Trading.Indicators.Tests;

public class EmaTests
{
    [Fact]
    public void WarmupPeriod_IsRespected()
    {
        var ema = new Ema(5);
        Assert.Equal(5, ema.WarmupPeriod);
        var candles = TestData.SampleCandles20();
        for (int i = 0; i < 4; i++)
        {
            var r = ema.Update(candles[i]);
            Assert.False(r.IsValid);
        }
        var r5 = ema.Update(candles[4]);
        Assert.True(r5.IsValid);
    }

    [Fact]
    public void BatchAndStreaming_ProduceSameResults()
    {
        var candles = TestData.SampleCandles20();
        var ema = new Ema(5);
        var batchResults = ema.Compute(candles);
        ema.Reset();
        for (int i = 0; i < candles.Count; i++)
        {
            var streamResult = ema.Update(candles[i]);
            Assert.Equal(batchResults[i].IsValid, streamResult.IsValid);
            if (batchResults[i].IsValid)
                Assert.Equal(batchResults[i].Value, streamResult.Value);
        }
    }

    [Fact]
    public void Reset_ClearsState()
    {
        var candles = TestData.SampleCandles20();
        var ema = new Ema(5);
        for (int i = 0; i < 6; i++)
            ema.Update(candles[i]);
        ema.Reset();
        var r = ema.Update(candles[0]);
        Assert.False(r.IsValid);
    }
}
