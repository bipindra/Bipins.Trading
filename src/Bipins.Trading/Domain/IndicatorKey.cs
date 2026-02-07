namespace Bipins.Trading.Domain;

public readonly record struct IndicatorKey(string Key)
{
    public static IndicatorKey Rsi(int period, string? timeframe = null) =>
        new($"RSI({period})" + (string.IsNullOrEmpty(timeframe) ? "" : $"|{timeframe}"));

    public static IndicatorKey Ema(int period, string? timeframe = null) =>
        new($"EMA({period})" + (string.IsNullOrEmpty(timeframe) ? "" : $"|{timeframe}"));

    public static IndicatorKey Sma(int period, string? timeframe = null) =>
        new($"SMA({period})" + (string.IsNullOrEmpty(timeframe) ? "" : $"|{timeframe}"));

    public static IndicatorKey Macd(int fast, int slow, int signal, string? timeframe = null) =>
        new($"MACD({fast},{slow},{signal})" + (string.IsNullOrEmpty(timeframe) ? "" : $"|{timeframe}"));

    public static IndicatorKey Atr(int period, string? timeframe = null) =>
        new($"ATR({period})" + (string.IsNullOrEmpty(timeframe) ? "" : $"|{timeframe}"));

    public static IndicatorKey Donchian(int period, string? timeframe = null) =>
        new($"Donchian({period})" + (string.IsNullOrEmpty(timeframe) ? "" : $"|{timeframe}"));

    public IndicatorKey PreviousBar() => new(Key + "_prev");

    public override string ToString() => Key;
}
