using Bipins.Trading.Domain;
using Bipins.Trading.Risk;
using Xunit;

namespace Bipins.Trading.Tests;

public class RiskTests
{
    [Fact]
    public void MaxPositionsPolicy_rejects_when_at_limit()
    {
        var policy = new MaxPositionsPolicy(2);
        var portfolio = new PortfolioState(100000m, 50000m, new Dictionary<string, Position>
        {
            ["A"] = new("A", PositionSide.Long, 100, 10, 0, 0),
            ["B"] = new("B", PositionSide.Long, 100, 20, 0, 0)
        });
        var order = new OrderIntent("S", "C", DateTime.UtcNow, OrderSide.Buy, OrderType.Market, TimeInForce.GTC, 10, null, null, null, null, null, null);
        var evt = policy.Evaluate(order, portfolio, null);
        Assert.False(evt.Approved);
        Assert.NotNull(evt.Reason);
    }

    [Fact]
    public void MaxPositionsPolicy_approves_when_under_limit()
    {
        var policy = new MaxPositionsPolicy(3);
        var portfolio = new PortfolioState(100000m, 50000m, new Dictionary<string, Position>
        {
            ["A"] = new("A", PositionSide.Long, 100, 10, 0, 0)
        });
        var order = new OrderIntent("S", "B", DateTime.UtcNow, OrderSide.Buy, OrderType.Market, TimeInForce.GTC, 10, null, null, null, null, null, null);
        var evt = policy.Evaluate(order, portfolio, null);
        Assert.True(evt.Approved);
    }

    [Fact]
    public void FixedQtySizer_sets_quantity()
    {
        var sizer = new FixedQtySizer(50);
        var portfolio = new PortfolioState(100000m, 100000m, new Dictionary<string, Position>());
        var order = new OrderIntent("S", "X", DateTime.UtcNow, OrderSide.Buy, OrderType.Market, TimeInForce.GTC, null, null, null, null, null, null, null);
        var sized = sizer.Size(order, portfolio, 100m, null);
        Assert.True(sized.Quantity.HasValue);
        Assert.Equal(50, sized.Quantity!.Value);
    }

    [Fact]
    public void PercentEquitySizer_sizes_by_equity_percent()
    {
        var sizer = new PercentEquitySizer(10m);
        var portfolio = new PortfolioState(100000m, 100000m, new Dictionary<string, Position>());
        var order = new OrderIntent("S", "X", DateTime.UtcNow, OrderSide.Buy, OrderType.Market, TimeInForce.GTC, null, null, null, null, null, null, null);
        var sized = sizer.Size(order, portfolio, 50m, null);
        Assert.True(sized.Quantity.HasValue);
        Assert.Equal(10000m / 50m, sized.Quantity!.Value);
    }
}
