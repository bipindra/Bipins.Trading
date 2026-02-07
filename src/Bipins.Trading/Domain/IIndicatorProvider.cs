namespace Bipins.Trading.Domain;

public interface IIndicatorProvider
{
    double? Get(IndicatorKey key, Func<double?> computeFn);
    IReadOnlyList<double>? GetMulti(IndicatorKey key, Func<IReadOnlyList<double>?> computeFn);
}
