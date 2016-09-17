#define Proprietary
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LionFire.Extensions.Logging;
using LionFire.Trading.Backtesting;
using LionFire.Applications;

namespace LionFire.Trading.Agent
{

    public interface IMarketTask
    {
        IMarket Market { get; }
    }

    public class BacktestTask : AppTask, IMarketTask
    {
        IMarket IMarketTask.Market { get { return this.Market; } }

        public BacktestMarket Market { get; set; }


        BacktestConfig config;
        public BacktestTask(BacktestConfig config)
        {
            this.config = config;
            //var config = new BacktestConfig()
            //{
            //    BrokerName = "IC Markets",
            //    StartDate = new DateTime(2016, 1, 1),
            //    EndDate = new DateTime(2016, 4, 1),
            //    TimeFrame = TimeFrame.h1,
            //};
        }

        public override bool TryInitialize()
        {
            logger = this.GetLogger();

            Market = new BacktestMarket(config);

            return base.TryInitialize();
        }

        protected override void Run()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Market.Run();
            logger.LogInformation($"Backtest task completed in {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)}");
        }

        #region Misc

        ILogger logger;

        #endregion
    }
}
