using Bipins.Trading.Indicators.Models;
using Xunit;

namespace Bipins.Trading.Indicators.Tests;

public class ModelsTests
{
    [Fact]
    public void Candle_CalculatedProperties()
    {
        var c = new Candle(DateTime.UtcNow, 10, 12, 8, 11, 1000);
        Assert.Equal(10m, c.Mid);
        Assert.Equal((12 + 8 + 11) / 3m, c.TypicalPrice);
        Assert.Equal((12 + 8 + 11 + 11) / 4m, c.WeightedClose);
    }

    [Fact]
    public void Candle_TrueRange()
    {
        decimal tr = Candle.TrueRange(110, 90, 100);
        Assert.Equal(20, tr);
    }

    [Fact]
    public void SingleValueResult_Invalid()
    {
        var invalid = SingleValueResult.Invalid;
        Assert.False(invalid.IsValid);
        Assert.Equal(1, invalid.ValueCount);
    }

    [Fact]
    public void BandResult_GetValue()
    {
        var b = new BandResult(100, 50, 0, true);
        Assert.Equal(100, b.GetValue(0));
        Assert.Equal(50, b.GetValue(1));
        Assert.Equal(0, b.GetValue(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => b.GetValue(3));
    }

    [Fact]
    public void MultiValueResult_ThreeValues()
    {
        var m = new MultiValueResult(1, 2, 3, true);
        Assert.True(m.IsValid);
        Assert.Equal(3, m.ValueCount);
        Assert.Equal(1, m.GetValue(0));
        Assert.Equal(2, m.GetValue(1));
        Assert.Equal(3, m.GetValue(2));
    }
}
