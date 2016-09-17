#define Proprietary
using LionFire.Applications;
using LionFire.Applications.Hosting;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Proprietary.Bots;
using LionFire.Trading.Proprietary.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Agent.Program
{
    public static class AgentProgramExtensions
    {
        public static IAppHost AddBacktest(this IAppHost app, BacktestConfig config = null)
        {
            var backtestConfig = config ?? new BacktestConfig()
            {
                BrokerName = "IC Markets",
                StartDate = new DateTime(2016, 1, 1),
                EndDate = new DateTime(2016, 4, 1),
                TimeFrame = TimeFrame.h1,

                //Bots = new List<Type> { // UNUSED
                //    typeof(LionTrender)
                //},
                //Symbols = new List<string> { "XAUUSD", "EURUSD" }, // UNUSED

            };

            var backtest = app.Add(new BacktestTask(backtestConfig));

#if Proprietary

#if Indicator
                var lionTrending = new LionTrendingBase(new LionTrendingConfig("XAUUSD", "h1")
                {
                    Log = true,
                    OpenWindowPeriods = 55,
                    CloseWindowPeriods = 34,
                    PointsToOpenLong = 3.0,
                    PointsToOpenShort = 3.0,
                    PointsToCloseLong = 2.0,
                    PointsToCloseShort = 2.0,
                });
                sim.Add(lionTrending);
#endif

            app.AddInit(_ =>
            {
                var lionTrender = new LionTrender();
                lionTrender.Config = new TLionTrender("XAUUSD", "h1")
                {
                    Indicator = new LionTrendingConfig
                    {
                        OpenWindowPeriods = 15,
                        CloseWindowPeriods = 15,
                        PointsToOpenLong = 1,
                        PointsToOpenShort = 1,
                        Symbol = "XAUUSD",
                        TimeFrame = "h1"
                    }
                };
                backtest.Market.Add(lionTrender);
            });
#else
#error No bot for backtest
#endif

            return app;
        }
    }
}
