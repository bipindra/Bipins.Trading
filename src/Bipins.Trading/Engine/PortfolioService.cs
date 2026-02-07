using System.Collections.Concurrent;
using Bipins.Trading.Domain;

namespace Bipins.Trading.Engine;

public sealed class PortfolioService : IPortfolioService
{
    private decimal _cash;
    private readonly ConcurrentDictionary<string, Position> _positions = new();

    public void Reset(decimal initialCash)
    {
        _cash = initialCash;
        _positions.Clear();
    }

    public void Apply(Fill fill)
    {
        _positions.AddOrUpdate(fill.Symbol,
            _ => NewPosition(fill),
            (_, existing) => ApplyToPosition(existing, fill));

        // Update cash: sell adds, buy subtracts
        var notional = fill.Price * fill.Quantity + fill.Fees;
        _cash -= fill.Side == OrderSide.Buy ? notional : -notional;
    }

    private static Position NewPosition(Fill fill)
    {
        var side = fill.Side == OrderSide.Buy ? PositionSide.Long : PositionSide.Short;
        return new Position(fill.Symbol, side, fill.Quantity, fill.Price, 0, -fill.Fees);
    }

    private Position ApplyToPosition(Position existing, Fill fill)
    {
        if (existing.Side == PositionSide.Flat)
            return NewPosition(fill);

        var sameSide = (existing.Side == PositionSide.Long && fill.Side == OrderSide.Buy) ||
                       (existing.Side == PositionSide.Short && fill.Side == OrderSide.Sell);

        if (sameSide)
        {
            var qty = existing.Quantity + fill.Quantity;
            var avg = (existing.AvgPrice * existing.Quantity + fill.Price * fill.Quantity) / qty;
            return existing with { Quantity = qty, AvgPrice = avg, RealizedPnl = existing.RealizedPnl - fill.Fees };
        }
        else
        {
            var closeQty = Math.Min(existing.Quantity, fill.Quantity);
            var openQty = fill.Quantity - closeQty;
            var realized = closeQty * (existing.Side == PositionSide.Long ? fill.Price - existing.AvgPrice : existing.AvgPrice - fill.Price) - fill.Fees;
            var newQty = existing.Quantity - closeQty + (existing.Side == PositionSide.Short ? openQty : -openQty);
            decimal newAvg = existing.AvgPrice;
            var newSide = newQty > 0 ? PositionSide.Long : newQty < 0 ? PositionSide.Short : PositionSide.Flat;
            if (openQty > 0) newAvg = fill.Price;
            return new Position(fill.Symbol, newSide, Math.Abs(newQty), newAvg, 0, existing.RealizedPnl + realized);
        }
    }

    public PortfolioState GetState()
    {
        var equity = _cash;
        foreach (var p in _positions.Values)
            if (p.Quantity != 0)
                equity += p.Quantity * p.AvgPrice + p.UnrealizedPnl;
        return new PortfolioState(equity, _cash, new Dictionary<string, Position>(_positions));
    }
}
