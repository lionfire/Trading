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
using LionFire.Trading;
using Xunit;

namespace Backtesting_;

public static class BacktestX
{
    public static BacktestTask2 Backtest(this PAtrBot<decimal> bot, IServiceProvider serviceProvider, DateTimeOffset start, DateTimeOffset end, ExchangeSymbolTimeFrame exchangeSymbolTimeFrame)
    {
        return new BacktestTask2(serviceProvider, new PBacktestTask2<PAtrBot<decimal>>
        {
            Bot = bot,
            Start = start,
            End = end,
            ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame,
        });
    }
}

public class Backtest_ : BinanceDataTest
{
    [Fact]
    public async void _()
    {

        var backtest = new PAtrBot<decimal>(14).Backtest(ServiceProvider,
            new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero),
            TimeFrame.m1,
            new ExchangeSymbol("Binance", "futures", "BTCUSDT")
            );

        await backtest.Run();


        //var inputSignal = new InputSignal<decimal>();
        //var backtestTask = new BacktestTask2(ServiceProvider, new PAtrBot

        //var backtestTask = new BacktestTask2<PBacktestTask2<PAtrBot, PAtrBot>>(ServiceProvider, new PAtrBot
        //{
        //    TimeFrame = TimeFrame.m1,
        //    Standard = new PStandardBot
        //    {

        //    },
        //    Input = new object(),
        //    ATR = new PAverageTrueRange<decimal>
        //    {
        //        MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
        //        Period = 8,
        //    }
        //})
        //{
        //};

        //var h = new HistoricalIndicatorHarness<TIndicator, TValue, IKline, decimal>(ServiceProvider, new()
        //{
        //    Parameters = new TValue
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