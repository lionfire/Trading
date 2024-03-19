#if OLD
using LionFire.Trading.Data;
using MorseCode.ITask;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class BarsServiceAspectSeries_OLD<TValue> : IHistoricalTimeSeries<TValue>
{
    #region Identity

    public ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; }
    public DataPointAspect Aspect { get; }

    #endregion

    #region Dependencies

    public IBars Bars { get; }

    #endregion

    #region Lifecycle

    public BarsServiceAspectSeries_OLD(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, IBars barsService, DataPointAspect aspect)
    {
        ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame;
        Aspect = aspect;
        Bars = barsService;
    }


    #endregion

    #region Methods

    public async ValueTask<HistoricalDataResult<TValue>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var bars = await Bars.Bars(SymbolBarsRange.FromExchangeSymbolTimeFrame(ExchangeSymbolTimeFrame, start, endExclusive));

        return bars.Any() ? (HistoricalDataResult<TValue>)new(bars.Cast<TValue>()) : HistoricalDataResult<TValue>.NoData;
    }


    //async ValueTask<IHistoricalDataResult<IEnumerable<object>>> IHistoricalTimeSeries.TryGetValueChunks(DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    var result = (IHistoricalDataResult<IEnumerable<object>>)(IHistoricalDataResult)await TryGetValues(start, endExclusive).ConfigureAwait(false);
    //    return result;
    //}

    #endregion
}


#endif