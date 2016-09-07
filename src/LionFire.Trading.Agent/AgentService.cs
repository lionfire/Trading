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
using NLog.Extensions.Logging;
using LionFire.Trading.Indicators;
using LionFire.Trading.Bots;

namespace LionFire.Trading.Agent
{

    public class AgentConfigItem
    {
        List<string> Instruments { get; set; }
    }

    public class AgentService
    {
        ILogger logger;

        public AgentService()
        {
            var sc = Singleton<ServiceCollection>.Instance;

            sc.AddLogging();
            
                var sp = Singleton<ServiceCollection>.Instance.BuildServiceProvider();
            ManualSingleton<IServiceProvider>.Instance = sp;

            sp.GetService<ILoggerFactory>()
                .AddConsole()
                .AddNLog();

            NLogConfig.Init();

            logger = this.GetLogger();
        }

        SimulatedMarket sim;

        public bool IsLive = false;
        public bool IsSimulation = true;

        public void Run()
        {
            logger.LogInformation("----- AgentService starting -----");
            if (!IsLive)
            {
                RunBacktest();
            }
            else
            {
                if (IsSimulation)
                {
                    RunLiveSimulation();
                }
                else
                {
                    RunLive();
                }
            }
            logger.LogInformation("----- AgentService finished -----");
        }

        public bool ExitRequested = false;
        private void RunLive()
        {
            var liveMarket = new LiveMarket();

            // TODO: Subscribe to prices of interest
            while (!ExitRequested)
            {
            }
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
            Console.ReadKey();
        }

        private void RunBacktest()
        {
            sim = new SimulatedMarket();

            sim.StartDate = new DateTime(2010, 1, 1);
            //sim.StartDate = new DateTime(2015, 1, 1);

            //sim.EndDate = new DateTime(2015, 1, 1, 12,0,0);
            //sim.EndDate = new DateTime(2015, 1, 3);
            //sim.EndDate = new DateTime(2015, 2, 7);
            sim.EndDate = new DateTime(2016, 1, 1);

            sim.TimeFrame = TimeFrame.h1;
            sim.SimulationTimeChanged += Sim_DateTimeChanged;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            //sim.Add(new LogBot());
            //sim.Add(new IndicatorLogBot());

            //BacktestAccount account = new BacktestAccount();
            //sim.Add(account);

#if Proprietary

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

            //sim.Add(new LionTrender() { Account = account });
            //var lionTrender = new LionTrender();
            //sim.Add(lionTrender);
#endif

            sim.Run();

            logger.LogInformation($"Backtest completed in {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)}");

        }

        private void Sim_DateTimeChanged()
        {
            //Console.WriteLine("Sim_DateTimeChanged " + sim.SimulationTime);

        }
    }
}
