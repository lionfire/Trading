using LionFire.Applications;
using LionFire.Structures;
using LionFire.Trading.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting
{
    public class BacktestTask : AppTask
    {
        public BacktestTask(Action run = null, Func<bool> tryInitialize = null) : base(run, tryInitialize)
        {
        }

        public override bool TryInitialize()
        {
            //ManualSingleton<IServiceProvider>.Instance.GetService(
            return base.TryInitialize();
        }

        protected override void Run()
        {

            var sim = new BacktestMarket();

            sim.Config = new Backtesting.BacktestConfig()
            {
                
            };

            sim.Add(new LogBot());
            sim.Add(new IndicatorLogBot());

            BacktestAccount account = new BacktestAccount();
            sim.Add(account);




        }
    }
}
