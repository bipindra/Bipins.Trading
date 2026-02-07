using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Trend;
using Xunit;

namespace Bipins.AI.Trading.Indicators.Tests;

public class MacdTests
{
    [Fact]
    public void Macd_ReturnsThreeValues()
    {
        var macd = new Macd(12, 26, 9);
        var candles = TestData.SampleCandles20();
        foreach (var c in candles)
        {
            var r = macd.Update(c);
            if (r.IsValid)
            {
                Assert.Equal(3, r.ValueCount);
                double hist = r.GetValue(2);
                Assert.Equal(r.GetValue(0) - r.GetValue(1), hist);
            }
        }
    }

    [Fact]
    public void BatchAndStreaming_ProduceSameResults()
    {
        var candles = TestData.SampleCandles20();
        var macd = new Macd(12, 26, 9);
        var batchResults = macd.Compute(candles);
        macd.Reset();
        for (int i = 0; i < candles.Count; i++)
        {
            var streamResult = macd.Update(candles[i]);
            Assert.Equal(batchResults[i].IsValid, streamResult.IsValid);
            if (batchResults[i].IsValid)
            {
                Assert.Equal(batchResults[i].GetValue(0), streamResult.GetValue(0));
                Assert.Equal(batchResults[i].GetValue(1), streamResult.GetValue(1));
                Assert.Equal(batchResults[i].GetValue(2), streamResult.GetValue(2));
            }
        }
    }
}
