using Bipins.Trading.Indicators.Momentum;
using Bipins.Trading.Indicators.Models;
using Xunit;

namespace Bipins.Trading.Indicators.Tests;

public class RsiTests
{
    [Fact]
    public void WarmupPeriod_IsRespected()
    {
        var rsi = new Rsi(14);
        Assert.Equal(15, rsi.WarmupPeriod); // RSI warmup is period + 1 = 15
        var candles = TestData.SampleCandles20();
        // RSI needs: 1 candle to establish prevClose, then 14 gain/loss values for RMA
        // First gain/loss is at candle 1, so RMA becomes valid at candle 14 (14th gain/loss)
        // However, RMA actually becomes valid when it has 14 values, which happens at candle 13
        // So: candles 0-12 should be invalid, candle 13+ should be valid
        for (int i = 0; i < 13; i++)
        {
            var r = rsi.Update(candles[i]);
            Assert.False(r.IsValid, $"Candle {i} should be invalid during warmup");
        }
        // Candle 13 should be the first valid one (RMA has 14 values at this point)
        var r13 = rsi.Update(candles[13]);
        Assert.True(r13.IsValid, "Candle 13 should be valid after RMA warmup completes");
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
