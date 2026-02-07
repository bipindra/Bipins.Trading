using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Volatility;
using Xunit;

namespace Bipins.AI.Trading.Indicators.Tests;

public class BollingerBandsTests
{
    [Fact]
    public void BollingerBands_UpperGreaterThanMiddleGreaterThanLower()
    {
        var bb = new BollingerBands(20, 2);
        var candles = TestData.SampleCandles20();
        foreach (var c in candles)
        {
            var r = bb.Update(c);
            if (r.IsValid)
            {
                Assert.True(r.Upper >= r.Middle);
                Assert.True(r.Middle >= r.Lower);
            }
        }
    }

    [Fact]
    public void BatchAndStreaming_ProduceSameResults()
    {
        var candles = TestData.SampleCandles20();
        var bb = new BollingerBands(20, 2);
        var batchResults = bb.Compute(candles);
        bb.Reset();
        for (int i = 0; i < candles.Count; i++)
        {
            var streamResult = bb.Update(candles[i]);
            Assert.Equal(batchResults[i].IsValid, streamResult.IsValid);
            if (batchResults[i].IsValid)
            {
                Assert.Equal(batchResults[i].Upper, streamResult.Upper);
                Assert.Equal(batchResults[i].Middle, streamResult.Middle);
                Assert.Equal(batchResults[i].Lower, streamResult.Lower);
            }
        }
    }
}
