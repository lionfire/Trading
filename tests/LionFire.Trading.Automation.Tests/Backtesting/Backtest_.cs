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
using Microsoft.CodeAnalysis.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Execution;
using System.Diagnostics;
using System.Threading.Tasks;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Journal;
using System.Numerics;

namespace Backtesting_;

public static class BacktestX
{
    //public static BacktestTask2 Backtest(this PAtrBot<decimal> bot, IServiceProvider serviceProvider, DateTimeOffset start, DateTimeOffset endExclusive, ExchangeSymbolTimeFrame exchangeSymbolTimeFrame)
    //{
    //    return new BacktestTask2(serviceProvider, new PBacktestTask2<PAtrBot<decimal>>
    //    {
    //        Bot = bot,
    //        Start = start,
    //        EndExclusive = endExclusive,
    //        ExchangeSymbol = exchangeSymbolTimeFrame,
    //        //DefaultTimeFrame = exchangeSymbolTimeFrame.DefaultTimeFrame,
    //    });
    //}
}

#if TODO //fix after refactor - need MultiBacktest now?
public class BacktestTheoryData : TheoryData<PBacktestTask2>
{
    public IEnumerable<TimeFrame> TimeFrames
    {
        get
        {
            yield return TimeFrame.h1;
            yield return TimeFrame.m1;
            // TODO: More timeframes
            //yield return TimeFrame.m5;
            //yield return TimeFrame.m15;
            //yield return TimeFrame.t1;
        }
    }
    public IEnumerable<string> Symbols { get; set; } = new[] {
        "BTCUSDT",
        "ETHUSDT"
        // FUTURE: Add forex pairs (that close on weekends)
    };

    public string Exchange = "Binance";
    public string ExchangeArea = "futures";

    public BacktestTheoryData()
    {
        foreach (var timeFrame in TimeFrames)
        {
            foreach (var symbol in Symbols)
            {
                createBotParameters<decimal>(timeFrame, symbol);
                createBotParameters<double>(timeFrame, symbol);
                createBotParameters<float>(timeFrame, symbol);
            }
        }
        void createBotParameters<T>(TimeFrame timeFrame, string symbol)
            where T : struct, INumber<T>
        {
            var pBot = new PAtrBot<T>(new ExchangeSymbolTimeFrame(Exchange, ExchangeArea, symbol, timeFrame), 14)
            {
                //SlowATR = new PAverageTrueRange<T>
                //{
                //    Period = 14,
                //    MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
                //},
                //Points = new PPointsBot
                //{
                //},
                //TimeFrame = timeFrame,
                //Inputs = [new SymbolValueAspect<T>(Exchange, ExchangeArea, symbol, timeFrame, DataPointAspect.Close)],
                //Bars = new SymbolValueAspect<T>(Exchange, ExchangeArea, symbol, timeFrame, DataPointAspect.Close),
            };

            Add(new PBacktestTask2<PAtrBot<T>>
            {
                PBot = pBot,
                TimeFrame = timeFrame,
                Start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndExclusive = new DateTimeOffset(2024, 2, 2, 0, 0, 0, TimeSpan.Zero),
                Features = SimFeatures.Bars,
            });
        }
    }
}
#endif

//public class BatchBacktestParameters
//{
//    public DateTimeOffset Start { get; set; }
//    public DateTimeOffset EndExclusive { get; set; }

//    public IReadOnlyList<IPBacktestTask2> BacktestBatches { get; } 

//    public void Add(IPBacktestTask2 backtest)
//    {
//        if (Start)
//        {
//        }
//        BacktestBatches.Add(backtest);
//    }
//}

#if TODO // Restore after refactor - maybe switch to be centered around MultiBacktest since we more rarely do just one

public class Backtest_Batch_ : BinanceDataTest
{
    [Fact]
    public async Task _()
    {
        //var d = new BacktestTheoryData();

        var Start = new DateTimeOffset(2023, 4, 1, 0, 0, 0, TimeSpan.Zero);
        var EndExclusive = new DateTimeOffset(2023, 7, 1, 0, 0, 0, TimeSpan.Zero);

        var batchQueue = ServiceProvider.GetRequiredService<BacktestQueue>();


        List<string> symbols = [
                "BTCUSDT",
                    //"ETHUSDT",
                    //"LTCUSDT"
                ];

        foreach (var symbol in symbols)
        {
            var exchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame("Binance", "futures", symbol, TimeFrame.m1);

            var job = await batchQueue.EnqueueJob(await MultiBacktestContext<double>.Create(ServiceProvider,
                new PMultiBacktest(typeof(PAtrBot<double>), exchangeSymbolTimeFrame,
               Start,
               EndExclusive
                ))
            ,
            batch =>
            {
                PBacktestTask2<PAtrBot<double>> createBacktest(string symbol, uint atrPeriod)
                {
                    return new PBacktestTask2<PAtrBot<double>>
                    {
                        PBot = new PAtrBot<double>(exchangeSymbolTimeFrame, atrPeriod, QuantConnect.Indicators.MovingAverageType.Simple)
                        {
                            //Bars = new SymbolValueAspect<double>("Binance", "futures", symbol, TimeFrame.m1, DataPointAspect.Close),
                            //Inputs = [new SymbolValueAspect<double>("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close)],

                            Points = new PPointsBot
                            {
                                OpenThreshold = 15,
                                CloseThreshold = 3,
                            }
                        },

                        //Start = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero),
                        Start = Start,
                        EndExclusive = EndExclusive,
                        //EndExclusive = new DateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.Zero),
                        //Start = new DateTimeOffset(2024, 7, 22, 0, 0, 0, TimeSpan.Zero),
                        //EndExclusive = new DateTimeOffset(2024, 7, 23, 0, 0, 0, TimeSpan.Zero),
                        Features = SimFeatures.Bars,

                    };
                }

                var list = new List<PBacktestTask2>();

                for (uint i = 14; i <= 17; i++) { list.Add(createBacktest(symbol, i)); }

                batch.BacktestBatches = [
                        list
                        //[
                        //createBacktest("BTCUSDT", 14),
                        //createBacktest("BTCUSDT", 15),
                        //createBacktest("ETHUSDT", 14),
                        //createBacktest("ETHUSDT", 15)
                        //new PBacktestTask2<PAtrBot<double>>
                        // {
                        //     Bot = new PAtrBot<double>(14)
                        //        {
                        //            TimeFrame = TimeFrame.m1,
                        //            Bars = new SymbolValueAspect<double>("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close),
                        //            //Inputs = [new SymbolValueAspect<double>("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close)],
                        //        },
                        //     Start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        //     EndExclusive = new DateTimeOffset(2024, 2, 2, 0, 0, 0, TimeSpan.Zero),
                        //     Features = BotHarnessFeatures.Bars,
                        // },
                        //new PBacktestTask2<PAtrBot<double>>
                        // {
                        //     Bot = new PAtrBot<double>(15)
                        //        {
                        //            TimeFrame = TimeFrame.m1,
                        //            Bars = new SymbolValueAspect<double>("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close),
                        //            //Inputs = [new SymbolValueAspect<double>("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close)],
                        //        },
                        //     Start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                        //     EndExclusive = new DateTimeOffset(2024, 2, 2, 0, 0, 0, TimeSpan.Zero),
                        //     Features = BotHarnessFeatures.Bars,
                        // },
                        //]
                        ];
            });

            await job.Task;
        }


    }

    [Fact]
    public void fail_DifferentDateRanges()
    {
        var batchQueue = ServiceProvider.GetRequiredService<BacktestQueue>();

        var exchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.m1);
        var Start = new DateTimeOffset(2020, 9, 9, 9, 9, 9, TimeSpan.Zero);
        var EndExclusive = new DateTimeOffset(2020, 11, 11, 11, 11, 11, TimeSpan.Zero);

        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            var job = batchQueue.EnqueueJob(await MultiBacktestContext.Create(ServiceProvider, new(
                new PMultiBacktest(typeof(PAtrBot<double>), exchangeSymbolTimeFrame, Start, EndExclusive)
                )), batch =>
            {
                batch.BacktestBatches = [[
                new PBacktestTask2<PAtrBot<double>>
                 {
                     PBot = new PAtrBot<double>(exchangeSymbolTimeFrame, 14)
                        {
                            //TimeFrame = TimeFrame.m1,
                            //Bars = new SymbolValueAspect<double>("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close),
                            //Inputs = [new SymbolValueAspect<double>("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close)],
                        },
                     Start = Start,
                     EndExclusive = EndExclusive,
                     Features = SimFeatures.Bars,
                 },
                new PBacktestTask2<PAtrBot<double>>
                 {
                     PBot = new PAtrBot<double>(exchangeSymbolTimeFrame, 15)
                        {
                            //TimeFrame = TimeFrame.m1,
                            //Bars = new SymbolValueAspect<double>("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close),
                            //Inputs = [new SymbolValueAspect<double>("Binance", "futures", "BTCUSDT", TimeFrame.m1, DataPointAspect.Close)],
                        },
                     // Invalid: different range
                     Start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                     // Invalid: different range
                     EndExclusive = new DateTimeOffset(2024, 2, 2, 0, 0, 0, TimeSpan.Zero),
                     Features = SimFeatures.Bars,
                 },
                    ]];
            });
        });

    }
}

#endif
public class Backtest_Optimize_ : BinanceDataTest
{
#if TODO
    [Fact]
    public async Task _()
    {
        var pOptimization = new POptimization
        {
            Parameters = new(),  // TODO
            BotParametersType = typeof(PAtrBot<double>),
            IsComprehensive = true,
        };

        var optimization = new OptimizationTask(ServiceProvider, pOptimization);
        
        await optimization.Run();
    }
#endif
}

#if TODO //fix after refactor - need MultiBacktest now?
public class Backtest_ : BinanceDataTest
{
    // TODO - FIXME
    [Theory]
    [ClassData(typeof(BacktestTheoryData))]
    public async Task Execute_(PBacktestTask2 parameters)
    {
        await (await MultiBacktestHarness<double>.Create(ServiceProvider, new PSimContext<double>()
        {
            DefaultExchangeArea = parameters.ExchangeSymbol,
            DefaultMarketSymbol = parameters.ExchangeSymbol?.Symbol,
        }, [parameters], dateChunker: HistoricalDataChunkRangeProvider)).Run();
    }

#if RunViaExtensionMethod

    [Fact]
    public async void _()
    {

        var backtest = new PAtrBot<decimal>(14)
        {
            TimeFrame = TimeFrame.m1,

        }.Backtest(ServiceProvider,
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
#endif
}

#endif





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