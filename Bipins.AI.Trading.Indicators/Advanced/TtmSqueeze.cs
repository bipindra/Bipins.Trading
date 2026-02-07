using Bipins.AI.Trading.Indicators.Base;
using Bipins.AI.Trading.Indicators.Models;
using Bipins.AI.Trading.Indicators.Volatility;

namespace Bipins.AI.Trading.Indicators.Advanced;

/// <summary>
/// TTM Squeeze. Detects when Bollinger Bands are inside Keltner Channels (squeeze). Returns 0 when squeezed, 1 when not; optional momentum.
/// Reference: John Carter; Investopedia.
/// </summary>
public sealed class TtmSqueeze : IndicatorBase<SingleValueResult>
{
    private readonly BollingerBands _bb;
    private readonly KeltnerChannels _kc;

    /// <summary>
    /// TTM Squeeze.
    /// </summary>
    /// <param name="period">Period (default 20).</param>
    /// <param name="bbMult">Bollinger multiplier (default 2).</param>
    /// <param name="kcMult">Keltner multiplier (default 2).</param>
    public TtmSqueeze(int period = 20, double bbMult = 2, double kcMult = 2)
        : base("TTM Squeeze", "Squeeze: BB inside Keltner. 0=squeeze, 1=no squeeze.", period)
    {
        _bb = new BollingerBands(period, bbMult);
        _kc = new KeltnerChannels(period, kcMult);
        AddParameter("Period", period.ToString(), "Period");
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();
        _bb.Reset();
        _kc.Reset();
    }

    /// <inheritdoc />
    protected override SingleValueResult ComputeNext(Candle candle)
    {
        var bbR = _bb.Update(candle);
        var kcR = _kc.Update(candle);
        if (!bbR.IsValid || !kcR.IsValid)
            return SingleValueResult.Invalid;
        bool squeeze = bbR.Upper < kcR.Upper && bbR.Lower > kcR.Lower;
        return new SingleValueResult(squeeze ? 0 : 1, true);
    }
}
