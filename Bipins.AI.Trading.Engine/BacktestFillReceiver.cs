using Bipins.AI.Trading.Domain;
using Bipins.AI.Trading.Domain.Events;
using Bipins.AI.Trading.Execution;

namespace Bipins.AI.Trading.Engine;

public sealed class BacktestFillReceiver : IFillReceiver
{
    private readonly IPortfolioService _portfolio;
    private readonly List<Fill> _trades = new();
    private readonly Action<Fill>? _onFill;

    public BacktestFillReceiver(IPortfolioService portfolio, IEventBus? bus = null)
    {
        _portfolio = portfolio;
        _onFill = bus != null ? fill => bus.Publish(new FillEvent(fill.Time, fill)) : null;
    }

    public void OnFill(Fill fill)
    {
        _portfolio.Apply(fill);
        lock (_trades) { _trades.Add(fill); }
        _onFill?.Invoke(fill);
    }

    public IReadOnlyList<Fill> Trades
    {
        get { lock (_trades) return _trades.ToList(); }
    }
}
