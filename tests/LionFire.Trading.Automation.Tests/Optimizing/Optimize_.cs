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

namespace Optimizing_;

public class Optimize_ : BinanceDataTest
{
    [Fact]
    public async Task _()
    {
        #region Input

        var p = new POptimization()
        {
            CommonBacktestParameters = new PBacktestBatchTask2
            {
                PBotType = typeof(PAtrBot<double>),
                Start             = new DateTimeOffset(2021, 1, 1,0,0,0, TimeSpan.Zero),
                EndExclusive = new DateTimeOffset(2021, 1, 20, 0, 0, 0, TimeSpan.Zero),
                Features = BotHarnessFeatures.Bars,
                TimeFrame = TimeFrame.h1,
                ExchangeSymbol = new ExchangeSymbol("Binance", "futures", "BTCUSDT"),
                //StartingBalance = 10000,

            },
            GranularityStepMultiplier = 4,
            BotParametersType = typeof(PAtrBot<double>),
            ParameterRanges = new List<IPParameterOptimization>
            {
                new PParameterOptimization<uint> { Name = "ATR.Period", Min = 10, Max = 20, Step = 1 },
                new PParameterOptimization<int> { Name = "OpenThreshold", Min = 3, Max = 30, Step = 1 },
                new PParameterOptimization<int> { Name = "CloseThreshold", Min = 1, Max = 20, Step = 1 },
            },

        };

        #endregion

        await new OptimizationTask(ServiceProvider, p).Run();
    }
}
