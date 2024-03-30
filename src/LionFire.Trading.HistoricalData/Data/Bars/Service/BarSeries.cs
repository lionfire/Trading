using LionFire.Trading.Data;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class BarSeries : IHistoricalTimeSeries<IKline>
{

    #region Identity

    public ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; }
    public Type ValueType => typeof(IKline);

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

    public async ValueTask<HistoricalDataResult<IKline>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var bars = await Bars.Get(SymbolBarsRange.FromExchangeSymbolTimeFrame(ExchangeSymbolTimeFrame, start, endExclusive));

        return bars?.Values?.Any() == true ? new(bars.Values) : HistoricalDataResult<IKline>.NoData;
    }

    #endregion

}

