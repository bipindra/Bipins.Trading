using Bipins.Trading.Domain;

namespace Bipins.Trading.Charting;

public interface IChartSink
{
    void Publish(SignalEvent signal);
    void Flush();
}
