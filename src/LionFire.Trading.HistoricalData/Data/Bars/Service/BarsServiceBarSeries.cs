using LionFire.Trading.Data;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class BarsServiceBarSeries : IHistoricalTimeSeries<IKline>
{

    #region Identity

    public ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; }

    #endregion

    #region Dependencies

    public BarsService BarsService { get; }

    #endregion

    #region Lifecycle

    public BarsServiceBarSeries(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, BarsService barsService)
    {
        ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame;
        BarsService = barsService;
    }

    #endregion

    #region Methods

    public async ValueTask<HistoricalDataResult<IKline>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var bars = await BarsService.Bars(SymbolBarsRange.FromExchangeSymbolTimeFrame(ExchangeSymbolTimeFrame, start, endExclusive));

        return bars.Any() ? (HistoricalDataResult<IKline>)new(bars) : HistoricalDataResult<IKline>.NoData;
    }

    #endregion

}

