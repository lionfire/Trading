using LionFire.Trading.Data;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class BarSeries : BarSeries<IKline<decimal>>, IPrecision<decimal>
{
    public BarSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, IBars barsService) : base(exchangeSymbolTimeFrame, barsService)
    {
    }
}
public class BarSeries<TValue> : IHistoricalTimeSeries<TValue>
{

    #region Identity

    public ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; }
    public Type ValueType => typeof(TValue);

    public TimeFrame TimeFrame => ExchangeSymbolTimeFrame.TimeFrame;

    #endregion

    #region Dependencies

    public IBars Bars { get; }

    #endregion

    #region Lifecycle

    public BarSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, IBars barsService)
    {
        ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame;
        Bars = barsService;
    }

    #endregion

    #region Methods

    public async ValueTask<HistoricalDataResult<TValue>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var bars = await Bars.Get<TValue>(SymbolBarsRange.FromExchangeSymbolTimeFrame(ExchangeSymbolTimeFrame, start, endExclusive));

        return bars?.Values?.Any() == true ? new(bars.Values) : HistoricalDataResult<TValue>.NoData;
    }

    #endregion

}

