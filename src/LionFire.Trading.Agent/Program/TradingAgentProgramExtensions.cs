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
using LionFire.Templating;
using LionFire.Applications.Trading;

namespace LionFire.Trading.Agent.Program
{
    public static class BacktestAppExtensions
    {

        public static TBacktestMarket DefaultConfig {
            get {
                return new TBacktestMarket()
                {
                    
                    BrokerName = "IC Markets",
                    StartDate = new DateTime(2003, 5, 5),
                    EndDate = new DateTime(2016, 9, 15),
                    TimeFrame = TimeFrame.h1,

                    //Symbols = new List<string> { "XAUUSD", "EURUSD" }, // UNUSED

                    Children = new List<ITemplate>
                {
#if Proprietary         
                    new TLionTrender("XAUUSD", "h1")
                    {
                        Log=false,
                        LogBacktest = true,
                        MinPositionSize = 1,
                        Indicator = new TLionTrending
                        {
                            Log = false,
                            OpenWindowPeriods = 55,
                            CloseWindowPeriods = 34,
                            PointsToOpenLong = 3.0,
                            PointsToOpenShort = 3.0,
                            PointsToCloseLong = 2.0,
                            PointsToCloseShort = 2.0,
                        }
                    }
#endif
                }
                };
            }
        }

        public static IAppHost AddBacktest(this IAppHost app, TBacktestMarket config = null)
        {
            app.Add(new BacktestTask(config ?? DefaultConfig));
            return app;
        }
    }
}
