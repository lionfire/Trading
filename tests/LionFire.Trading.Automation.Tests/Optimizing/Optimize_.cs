using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Bots.Parameters;
using LionFire.Trading.Automation.Optimization;
using Microsoft.Extensions.DependencyInjection;

namespace Optimizing_;

public class Optimize_ : BinanceDataTest
{
    [Fact]
    public async Task _()
    {
        #region Input

        var p = new POptimization()
        {
            //MaxBatchSize = 100_000,
            //MaxBatchSize = 20_048,
            MaxBatchSize = 4_096,
            //MaxBatchSize = 1_024,
            //MaxBatchSize = 10,
            //MaxBacktests = 1_000,
            //MaxBacktests = 10_000,
            //MaxBacktests = 100_000,
            MaxBacktests = 1_000_000,
            //MaxBacktests = 15_000,

            MaxDetailedJournals = 10,
            CommonBacktestParameters = new PBacktestBatchTask2
            {
                PBotType = typeof(PAtrBot<double>),
                Start = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndExclusive = new DateTimeOffset(2024, 2, 20, 0, 0, 0, TimeSpan.Zero),
                Features = BotHarnessFeatures.Bars,
                TimeFrame = TimeFrame.h1,
                ExchangeSymbol = new ExchangeSymbol("Binance", "futures", "BTCUSDT"),
                //StartingBalance = 10000,

            },
            GranularityStepMultiplier = 4,
            BotParametersType = typeof(PAtrBot<double>),
            ParameterRanges = new List<IPParameterOptimization>
            {
                new PParameterOptimization<uint> { Name = "ATR.Period", Min = 10, Max = 20, Step = 2 },
                new PParameterOptimization<int> { Name = "OpenThreshold", Min = 3, Max = 30, Step = 1 },
                new PParameterOptimization<int> { Name = "CloseThreshold", Min = 1, Max = 20, Step = 1 },
            },

        };

        #endregion

        var bq = ServiceProvider.GetRequiredService<BacktestQueue>();
        await bq.StartAsync(default);

        await new OptimizationTask(ServiceProvider, p).Run();
        //await bq.StopAsync(default);
    }
}
