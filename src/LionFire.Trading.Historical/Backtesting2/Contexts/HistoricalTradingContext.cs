using LionFire.Trading.Indicators;

namespace LionFire.Trading.Backtesting2;

public class HistoricalTradingContext : TradingContext2, ITradingContext2
{
    #region Configuration

    bool IsLive => false;
    public TimeFrame TimeFrame { get; }

    #endregion

    #region Lifecycle

    public HistoricalTradingContext(IServiceProvider serviceProvider, TimeFrame timeFrame) : base(serviceProvider)
    {
        TimeFrame = timeFrame;
    }

    #endregion

    #region State

    public DateTimeOffset SimulatedDateTime { get; protected set; }

    bool ITradingContext2.IsLive => throw new NotImplementedException();

    #endregion

    #region Methods: State

    public void NextBar()
    {
        SimulatedDateTime = TimeFrame.NextBarOpen(SimulatedDateTime);

        //foreach(var indicator in Indicators)
        //{

        //}
        //foreach(var bot in Bots)
        //{

        //}
    }

    #endregion

    public IIndicator2 GetIndicator<TIndicator, TParameters, TInput, TOutput>(TParameters parameters)
         where TIndicator : IIndicator2<TParameters, TInput, TOutput>
    {
        throw new NotImplementedException();

        //// TODO >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

        ////var parameters = new TParameters
        ////{
        ////    //MovingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders,
        ////    MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
        ////    Period = 14
        ////};

        //var h = new HistoricalIndicatorHarness<TIndicator, TParameters, InputSlot, TOutput>(ServiceProvider, new()
        //{
        //    Parameters = parameters,
        //    TimeFrame = TimeFrame,
        //    InputReferences = new[] { new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) } // OPTIMIZE - Aspect: HLC
        //});

        //var result = await h.GetReverseOutput(new DateTimeOffset(2024, 4, 1, 13, 0, 0, TimeSpan.Zero),
        //    new DateTimeOffset(2024, 4, 1, 18, 0, 0, TimeSpan.Zero));

        //return 
    }

}
