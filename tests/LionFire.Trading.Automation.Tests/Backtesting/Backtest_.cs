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
    //        //TimeFrame = exchangeSymbolTimeFrame.TimeFrame,
    //    });
    //}
}

public class BacktestTheoryData : TheoryData<IPBacktestTask2>
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
        {
            var pBot = new PAtrBot<T>(14)
            {
                //ATR = new PAverageTrueRange<T>
                //{
                //    Period = 14,
                //    MovingAverageType = QuantConnect.Indicators.MovingAverageType.Simple,
                //},
                //Points = new PPointsBot
                //{
                //},
                TimeFrame = timeFrame,
                Inputs = [new SymbolValueAspect<T>(Exchange, ExchangeArea, symbol, timeFrame, DataPointAspect.Close)],
            };

            Add(new PBacktestTask2<PAtrBot<T>>
            {
                Bot = pBot,
                Start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndExclusive = new DateTimeOffset(2024, 2, 2, 0, 0, 0, TimeSpan.Zero),
                Features = BotHarnessFeatures.Bars,
            });
        }
    }
}

public class OptimizerOptions
{

}

public class BatchBacktestParameters
{
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset EndExclusive { get; set; }

    public IReadOnlyList<IPBacktestTask2> Backtests { get; } = new();

    public void Add(IPBacktestTask2 backtest)
    {
        if (Start)
        {
        }
        Backtests.Add(backtest);
    }
}

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


public class Backtest_Batch_ : BinanceDataTest
{
    [Fact]
    public async Task _()
    {
        var d = new BacktestTheoryData();

        var pBatch = new BatchBacktestParameters()
        {
            
        };

        var batcher = ServiceProvider.GetService<BacktestBatcher>();


        var p1 = new PBacktestTask2<AtrBot<double>>
             (new PBacktestTask2<PAtrBot<double>>
             {
                 Bot = pBot,
                 Start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
                 EndExclusive = new DateTimeOffset(2024, 2, 2, 0, 0, 0, TimeSpan.Zero),
                 Features = BotHarnessFeatures.Bars,
             });

        var p = new BatchBacktestParameters()
        {

        };

    }
}

public class Backtest_ : BinanceDataTest
{
    [Theory]
    [ClassData(typeof(BacktestTheoryData))]
    public async Task Execute_(IPBacktestTask2 parameters)
    {
        await new BacktestTask2(ServiceProvider, parameters, dateChunker: HistoricalDataChunkRangeProvider).Run();
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