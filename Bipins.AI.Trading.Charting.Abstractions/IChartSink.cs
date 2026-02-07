using Bipins.AI.Trading.Domain;

namespace Bipins.AI.Trading.Charting;

public interface IChartSink
{
    void Publish(SignalEvent signal);
    void Flush();
}
