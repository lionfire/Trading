using LionFire.Trading.Bots;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Agent
{

    public class AgentConfigItem
    {
        List<string> Instruments { get; set; }

    }

    public class AgentService
    {
        public AgentService()
        {
        }

        SimulatedMarket sim;

        public bool IsLive = false;
        public bool IsSimulation = true;

        public void Run()
        {
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
            //sim.TimeFrame = TimeFrame.m1;

            var tf = TimeFrame.h1;
            sim.Subscribe("XAUUSD", this, tf);
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

            sim.StartDate = new DateTime(2015, 1, 1);
            sim.EndDate = new DateTime(2015, 1, 1, 12,0,0);
            //sim.EndDate = new DateTime(2015, 12, 31);
            sim.TimeFrame = TimeFrame.h1;
            sim.SimulationTimeChanged += Sim_DateTimeChanged;

            var sw = System.Diagnostics.Stopwatch.StartNew();

            var logBot = new LogBot();
            logBot.Market = sim;

            sim.Run();

            Console.WriteLine($"Backtest completed in {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)}");
            
        }

        private void Sim_DateTimeChanged()
        {
            //Console.WriteLine("Sim_DateTimeChanged " + sim.SimulationTime);

        }
    }
}
