using LionFire.Trading.Backtesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Structures;

namespace LionFire.Trading.Bots
{
    public partial class SingleSeriesSignalBotBase<TIndicator, TConfig, TIndicatorConfig>  : IHandler<SymbolTick>
    {
        public IMarketSeries MarketSeries { get; set; } 


        protected override void OnStarting()
        {
            base.OnStarting();

            if (Config.Indicator.Symbol == null) Config.Indicator.Symbol = this.Config.Symbol;
            if (Config.Indicator.TimeFrame == null) Config.Indicator.TimeFrame = this.Config.TimeFrame;

            this.Indicator.Config = this.Config.Indicator;

            this.Market.Add(this.Indicator);

            var sim = Market as ISimulatedMarket;
            if (sim != null)
            {
                sim.SimulationTickFinished += OnSimulationTickFinished;
            }

            this.Symbol = Market.GetSymbol(Config.Symbol);
            this.MarketSeries = Market.GetMarketSeries(this.Config.Symbol, this.Config.TimeFrame /*, Market.IsBacktesting*/);

            UpdateDesiredSubscriptions();
        }

        protected virtual void UpdateDesiredSubscriptions()
        {
            this.DesiredSubscriptions = new List<MarketDataSubscription>()
            {
                new MarketDataSubscription(this.Config.Symbol, Config.TimeFrame)
                //new MarketDataSubscription(this.Config.Symbol, "t1")
            };
        }

        protected void OnSimulationTickFinished()
        {
            //logger.LogInformation("bot OnSimulationTickFinished " + (Market as BacktestMarket).SimulationTime);
            Evaluate();
        }

        void IHandler<SymbolTick>.Handle(SymbolTick tick)
        {
            Console.WriteLine("OnTick: " + tick);
        }

        //long i = 0;
        public override void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
            if (bar != null)
            {
                Console.WriteLine("OnBar: " + bar);
                Evaluate();
            }
            //if (i++ % 48 == 0)
            //{
            //    Console.WriteLine($"{this.GetType().Name} [{timeFrame.Name}] {symbolCode} {bar}");
            //}
        }

    }
}
