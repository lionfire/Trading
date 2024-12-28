using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.Serialization.Csv;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Automation.Bots.Parameters;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Journal;
using Microsoft.Extensions.DependencyInjection;

namespace Optimizing_;

public class Optimize_ : BinanceDataTest
{
    [Fact]
    public async Task _()
    {
        #region Input

        var ExchangeSymbol = new ExchangeSymbol("Binance", "futures", "BTCUSDT");

        var c = new PMultiBacktestContext()
        {
            CommonBacktestParameters = new PBacktestBatchTask2
            {
                PBotType = typeof(PAtrBot<double>),
                Start = new DateTimeOffset(2021, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EndExclusive = new DateTimeOffset(2021, 3, 1, 0, 0, 0, TimeSpan.Zero),
                Features = BotHarnessFeatures.Bars,
                TimeFrame = TimeFrame.h1,
                ExchangeSymbol = ExchangeSymbol,
                //StartingBalance = 10000,
            },
        };

        var p = c.POptimization;

        //MaxBatchSize = 100_000,
        //MaxBatchSize = 20_048,
        //MaxBatchSize = 4_096,
        p.MaxBatchSize = 2_048;
        //MaxBatchSize = 1_024,
        //MaxBatchSize = 10,

        //MaxBacktests = 1_000,
        //MaxBacktests = 8_192,
        //MaxBacktests = 10_000,
        //MaxBacktests = 100_000,
        p.MaxBacktests = 1_000_000;
        //MaxBacktests = 15_000,

        p.MinParameterPriority = -10;

        p.TradeJournalOptions = new()
        {
            Enabled = false,
        };

        p.GranularityStepMultiplier = 4;

        var parameters = BotParameterPropertiesInfo.Get(typeof(PAtrBot<double>))!.PathDictionary;

        p.ParameterOptimizationOptions = new Dictionary<string, IParameterOptimizationOptions>();

        var period = ParameterOptimizationOptions.Create<int>(parameters["ATR.Period"]);
        period.MaxCount = 20;
        period.MinCount = 20;
        period.MinValue = 2;
        period.MaxValue = 40;
        period.Step = 3;

        //{
        //    ["Period"] = new ParameterOptimizationOptions<int>
        //    {
        //        MaxCount = 20,
        //        MinCount = 20,
        //        MinValue = 2,
        //        MaxValue = 40,
        //        Step = 3
        //    },
        //    //["OpenThreshold"] = new ParameterOptimizationOptions<int> { OptimizePriority = 2 },
        //    //["CloseThreshold"] = new ParameterOptimizationOptions<int> { OptimizePriority = 3 },
        //};


        //var p = new POptimization(c)
        //{
        //    //MaxBatchSize = 100_000,
        //    //MaxBatchSize = 20_048,
        //    //MaxBatchSize = 4_096,
        //    MaxBatchSize = 2_048,
        //    //MaxBatchSize = 1_024,
        //    //MaxBatchSize = 10,

        //    //MaxBacktests = 1_000,
        //    //MaxBacktests = 8_192,
        //    //MaxBacktests = 10_000,
        //    //MaxBacktests = 100_000,
        //    MaxBacktests = 1_000_000,
        //    //MaxBacktests = 15_000,

        //    MinParameterPriority = -10,

        //    TradeJournalOptions = new()
        //    {
        //        Enabled = false,
        //    },

        //    GranularityStepMultiplier = 4,
        //    ParameterOptimizationOptions = new Dictionary<string, IParameterOptimizationOptions>
        //    {
        //        ["Period"] = new ParameterOptimizationOptions<int>
        //        {
        //            MaxProbes = 20,
        //            MinProbes = 20,
        //            MinValue = 2,
        //            MaxValue = 40,
        //            OptimizationStep = 3
        //        },
        //        //["OpenThreshold"] = new ParameterOptimizationOptions<int> { OptimizePriority = 2 },
        //        //["CloseThreshold"] = new ParameterOptimizationOptions<int> { OptimizePriority = 3 },
        //    },

        //    //ParameterRanges = new List<IPParameterOptimization>
        //    //{
        //    //    new PParameterOptimization<uint> { Name = "ATR.Period", Min = 10, Max = 20, Step = 2 },
        //    //    new PParameterOptimization<int> { Name = "OpenThreshold", Min = 3, Max = 30, Step = 1 },
        //    //    new PParameterOptimization<int> { Name = "CloseThreshold", Min = 1, Max = 20, Step = 1 },
        //    //},

        //};

        #endregion

        var bq = ServiceProvider.GetRequiredService<BacktestQueue>();
        await bq.StartAsync(default);

        await new OptimizationTask(ServiceProvider, c).Run();
        //await bq.StopAsync(default);
    }
}
