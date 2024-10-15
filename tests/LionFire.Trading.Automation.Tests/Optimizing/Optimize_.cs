using LionFire.Trading.Automation.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            GranularityStepMultiplier = 4,
            BotParametersType = typeof(PAtrBot<double>),
            Parameters = new List<PParameterOptimization>
            {
                new PParameterOptimization<int> { Name = "ATR.Period", Min = 1, Max = 100, Step = 1 },
                new PParameterOptimization<int> { Name = "OpenThreshold", Min = 1, Max = 100, Step = 1 },
                new PParameterOptimization<int> { Name = "CloseThreshold", Min = 1, Max = 100, Step = 1 },
            },

        };

        #endregion



    }
}
