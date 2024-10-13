using LionFire.Trading.Automation.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Trading.Automation.Bots.Parameters; 

namespace Optimizing_;

public class OptimizationParameters
{

    public double GranularityStepMultiplier { get; set; }
    //public long MaxBacktests { get; set; }  // FUTURE ENH
}



public class Optimize_ : BinanceDataTest
{
    [Fact]
    public async Task _()
    {
        #region Input

        // Normally user will focus on one bot type, but do here just to prove we can optimize using the high level optimization parameters without tweaking individual bot parameters
        List<Type> botTypes = [
                typeof(PAtrBot<double>),
                typeof(PDualAtrBot<double>),
            ];

        var p = new OptimizationParameters()
        {
            GranularityStepMultiplier = 4,
        };

        #endregion



    }
}
