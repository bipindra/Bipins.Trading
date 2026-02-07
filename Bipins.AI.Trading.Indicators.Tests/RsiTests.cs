using Bipins.AI.Trading.Indicators.Momentum;
using Bipins.AI.Trading.Indicators.Models;
using Xunit;

namespace Bipins.AI.Trading.Indicators.Tests;

public class RsiTests
{
    [Fact]
    public void WarmupPeriod_IsRespected()
    {
        var rsi = new Rsi(14);
        Assert.True(rsi.WarmupPeriod >= 14);
        var candles = TestData.SampleCandles20();
        for (int i = 0; i < 14; i++)
        {
            var r = rsi.Update(candles[i]);
            if (i < 14)
                Assert.False(r.IsValid);
        }
        var r15 = rsi.Update(candles[14]);
        Assert.True(r15.IsValid);
    }

    [Fact]
    public void Rsi_Values_InRange_0_100()
    {
        var rsi = new Rsi(14);
        var candles = TestData.SampleCandles20();
        foreach (var c in candles)
        {
            var r = rsi.Update(c);
            if (r.IsValid)
            {
                Assert.True(r.Value >= 0 && r.Value <= 100, $"RSI should be 0-100, got {r.Value}");
            }
        }
    }

    [Fact]
    public void BatchAndStreaming_ProduceSameResults()
    {
        var candles = TestData.SampleCandles20();
        var rsi = new Rsi(14);
        var batchResults = rsi.Compute(candles);
        rsi.Reset();
        for (int i = 0; i < candles.Count; i++)
        {
            var streamResult = rsi.Update(candles[i]);
            Assert.Equal(batchResults[i].IsValid, streamResult.IsValid);
            if (batchResults[i].IsValid)
                Assert.Equal(batchResults[i].Value, streamResult.Value);
        }
    }

    [Fact]
    public void Reset_ClearsState()
    {
        var candles = TestData.SampleCandles20();
        var rsi = new Rsi(14);
        for (int i = 0; i < 16; i++)
            rsi.Update(candles[i]);
        rsi.Reset();
        var r = rsi.Update(candles[0]);
        Assert.False(r.IsValid);
    }
}
