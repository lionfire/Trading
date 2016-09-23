#define Proprietary
#if Proprietary
using LionFire.Trading.Proprietary.Bots;
using LionFire.Trading.Proprietary.Indicators;
#endif
using LionFire.Structures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LionFire.Extensions.Logging;
using LionFire.Trading.Indicators;
using LionFire.Trading.Bots;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Threading.Tasks;
using LionFire.Applications.Hosting;
using LionFire.Trading.Backtesting;

namespace LionFire.Trading.Agent
{
    public enum TradingAgentMode
    {
        Unspecified = 0,
        Backtest = 1,
        Simulation = 2,
        Live = 4,
    }

    public class TTradingAgentApp
    {
        public TradingAgentMode Mode { get; set; }
    }

    public class TradingAgentApp
    {

        #region Configuration

        public TTradingAgentApp Config { get; set; } = new TTradingAgentApp();

        public bool IsLive { get { return Config.Mode == TradingAgentMode.Live; } }
        public bool IsSimulation { get { return Config.Mode == TradingAgentMode.Simulation; } }
        public bool IsBacktest { get { return Config.Mode == TradingAgentMode.Backtest; } }

        #endregion

        #region Construction and Initialization

        //public TradingAgentApp()
        //{
        //    logger = this.GetLogger();

        //}

        #endregion


        public void Run()
        {
            logger.LogInformation($"----- {this} starting -----");

//            switch (Config.Mode)
//            {
//                case TradingAgentMode.Backtest:
//                    {
//                        var bt = new BacktestTask();
                        
//                        var sim = bt.Market;

//                        sim.Add(new LogBot());
//                        sim.Add(new IndicatorLogBot());

//                        BacktestAccount account = new BacktestAccount();
//                        sim.Add(account);

//#if Proprietary

//                        var lionTrending = new LionTrendingBase(new LionTrendingConfig("XAUUSD", "h1")
//                        {
//                            Log = true,
//                            OpenWindowPeriods = 55,
//                            CloseWindowPeriods = 34,
//                            PointsToOpenLong = 3.0,
//                            PointsToOpenShort = 3.0,
//                            PointsToCloseLong = 2.0,
//                            PointsToCloseShort = 2.0,
//                        });
//                        sim.Add(lionTrending);
                        
//                        var lionTrender = new LionTrender();
//                        lionTrender.Config = new LionTrenderConfig("XAUUSD", "h1")
//                        {
//                            Indicator = new LionTrendingConfig
//                            {
//                                OpenWindowPeriods = 15,
//                                CloseWindowPeriods = 15,
//                                PointsToOpenLong = 1,
//                                PointsToOpenShort = 1,
//                                Symbol = "XAUUSD",
//                                TimeFrame = "h1"
//                            }
//                        };
//                        bt.Market.Add(lionTrender);
//#endif

//                        bt.Run();
//                    }
//                    break;
//                case TradingAgentMode.Simulation:
//                    break;
//                case TradingAgentMode.Live:
//                    break;
//                default:
//                case TradingAgentMode.Unspecified:
//                    break;
//            }


//            if (!IsLive)
//            {
                
//            }
//            else
//            {
//                if (IsSimulation)
//                {
//                    RunLiveSimulation();
//                }
//                else
//                {
//                    RunLive();
//                }
//            }
            logger.LogInformation($"----- {this} finished -----");
        }

        public bool ExitRequested = false;



        private void RunLive()
        {
            var liveMarket = new LiveMarket();

            // TODO: Subscribe to prices of interest
            //while (!ExitRequested)
            //{
            //}

        }

        private void RunLiveSimulation()
        {
            int maxBars = 50;
            int barCount = 0;
            var sim = new LiveMarketSimulation();
            sim.TimeFrame = TimeFrame.h1;

            var tf = TimeFrame.h1;
            sim.Subscribe("XAUUSD", this, tf);
            sim.Subscribe("USDJPY", this, tf);
            sim.SimulationTime = new DateTime(2012, 01, 01);

            var dir = @"E:\st\Projects\Investing\Historical Data\dukascopy\XAUUSD\m1\";
            var file = @"2005.01.01-2016.08.10_cAlgo_XAUUSD_m1.csv";
            var path = Path.Combine(dir, file);
            using (var sr = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                string line;
                while (!ExitRequested && (line = sr.ReadLine()) != null)
                {
                    var cells = line.Split(',');
                    var bar = new SymbolBar
                    {
                        Code = "XAUUSD",
                        Time = DateTime.Parse(cells[0] + " " + cells[1]),

                        Bar = new Bar
                        {
                            Open = Convert.ToDouble(cells[2]),
                            High = Convert.ToDouble(cells[3]),
                            Low = Convert.ToDouble(cells[4]),
                            Close = Convert.ToDouble(cells[5]),
                            Volume = Convert.ToDouble(cells[6]),
                        }
                    };

                    sim.SimulateBar(bar);

                    if (++barCount > maxBars) break;
                    Thread.Sleep(300);
                }
            }
        }


        #region Misc

        ILogger logger;

        #endregion
    }
}
