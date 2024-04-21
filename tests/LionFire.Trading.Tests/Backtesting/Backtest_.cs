//namespace LionFire.Trading.Indicators.Harnesses.Tests;

using Backtesting_;
using LionFire.Collections;
using LionFire.Instantiating;
using LionFire.Structures;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Bots;
using LionFire.Trading.Indicators;
using LionFire.Trading.Indicators.Harnesses;
using LionFire.Trading.Indicators.QuantConnect_;
using Microsoft.Extensions.DependencyInjection;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Bots;

namespace Backtesting_;

public class Backtest_ : BinanceDataTest
{
    [Fact]
    public async void _()
    {

        var backtestTask = new BacktestTask2<PAtrBot>(ServiceProvider, new PAtrBot
        {
            ATR = new PAverageTrueRange
            {
                MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
                Period = 8,
            }
        })
        {
        };

        //var h = new HistoricalIndicatorHarness<TIndicator, TParameters, IKline, decimal>(ServiceProvider, new()
        //{
        //    Parameters = new TParameters
        //    {
        //        //MovingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders,
        //        MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
        //        Period = 14
        //    },
        //    TimeFrame = TimeFrame.h1,
        //    InputReferences = new[] { new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1) } // OPTIMIZE - Aspect: HLC
        //});

    }
}






#if NEXT

// TODO
//public class LiveTradingContext : ITradingContext
//{
//    bool IsLive => true;
//}
public class BotHarness
{

}
public class HistoricalBotHarness
{
    public HistoricalBotHarness(IBot bot)
    {

    }
}


#endif