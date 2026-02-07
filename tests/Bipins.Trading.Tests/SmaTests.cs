using Bipins.Trading.Indicators.MovingAverages;
using Bipins.Trading.Indicators.Models;
using Xunit;

namespace Bipins.Trading.Indicators.Tests;

public class SmaTests
{
    [Fact]
    public void WarmupPeriod_IsRespected()
    {
        var sma = new Sma(5);
        Assert.Equal(5, sma.WarmupPeriod);
        var candles = TestData.SampleCandles20();
        for (int i = 0; i < 4; i++)
        {
            var r = sma.Update(candles[i]);
            Assert.False(r.IsValid);
        }
        var r5 = sma.Update(candles[4]);
        Assert.True(r5.IsValid);
    }

    [Fact]
    public void BatchAndStreaming_ProduceSameResults()
    {
        var candles = TestData.SampleCandles20();
        var smaStream = new Sma(5);
        var batchResults = smaStream.Compute(candles);
        var smaBatch = new Sma(5);
        for (int i = 0; i < candles.Count; i++)
        {
            var streamResult = smaBatch.Update(candles[i]);
            Assert.Equal(batchResults[i].IsValid, streamResult.IsValid);
            if (batchResults[i].IsValid)
                Assert.Equal(batchResults[i].Value, streamResult.Value);
        }
    }

    [Fact]
    public void Compute_Span_Matches_List()
    {
        var candles = TestData.SampleCandles20Array();
        var sma = new Sma(5);
        var listResults = sma.Compute(candles);
        sma.Reset();
        var spanResults = new SingleValueResult[candles.Length];
        sma.Compute(candles.AsSpan(), spanResults);
        for (int i = 0; i < candles.Length; i++)
        {
            Assert.Equal(listResults[i].IsValid, spanResults[i].IsValid);
            if (listResults[i].IsValid)
                Assert.Equal(listResults[i].Value, spanResults[i].Value);
        }
    }

    [Fact]
    public void Reset_ClearsState()
    {
        var candles = TestData.SampleCandles20();
        var sma = new Sma(5);
        sma.Update(candles[0]);
        sma.Update(candles[1]);
        sma.Reset();
        var r = sma.Update(candles[0]);
        Assert.False(r.IsValid);
    }

    [Fact]
    public void Sma_FirstValid_IsAverageOfFirst5Closes()
    {
        var sma = new Sma(5);
        decimal[] closes = { 10, 11, 12, 13, 14 };
        for (int i = 0; i < 5; i++)
        {
            var c = new Candle(DateTime.UtcNow.AddMinutes(i), closes[i], closes[i] + 1, closes[i] - 1, closes[i], 100);
            var r = sma.Update(c);
            if (i < 4)
                Assert.False(r.IsValid);
            else
            {
                Assert.True(r.IsValid);
                Assert.Equal(12.0, r.Value);
            }
        }
    }
}
