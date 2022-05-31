﻿#define Proprietary
using LionFire.Applications;
using LionFire.Applications.Hosting;
using LionFire.Trading.Backtesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Instantiating;
using LionFire.Applications.Trading;
using LionFire.Trading.Proprietary.Bots;
using LionFire.Trading.Proprietary.Indicators;

namespace LionFire.Trading.Agent.Program
{
    public static class BacktestAppExtensions
    {

        public static TBacktestAccount DefaultConfig {
            get {
                return new TBacktestAccount()
                {
                    
                    SimulateAccount = @"cTrader\IC Markets.Live.Manual",
                    Exchange = "IC Markets",
                    StartDate = new DateTime(2003, 5, 5),
                    EndDate = new DateTime(2016, 9, 15),
                    TimeFrame = TimeFrame.h1,

                    //Symbols = new List<string> { "XAUUSD", "EURUSD" }, // UNUSED

                    Children = new InstantiationCollection
                {
#if Proprietary         
                    new TLionTrender("XAUUSD", "h1")
                    {
                        //Log=false,
                        //LogBacktest = true,
                        MinPositionSize = 1,
                        Indicator = new TLionTrending
                        {
                            Log = false,
                            OpenWindowMinutes = 55*60,
                            CloseWindowMinutes = 34*60,
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

        public static IAppHost AddBacktest(this IAppHost app, TBacktestAccount config = null)
        {
            app.Add(new BacktestTask(config ?? DefaultConfig));
            return app;
        }
    }
}
