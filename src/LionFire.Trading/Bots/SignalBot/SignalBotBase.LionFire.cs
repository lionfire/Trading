using LionFire.Trading.Backtesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Structures;

namespace LionFire.Trading.Bots
{
    public partial class SingleSeriesSignalBotBase<TIndicator, TConfig, TIndicatorConfig>  
    {
        public IMarketSeries MarketSeries { get; set; }

        protected override void OnStarting()
        {
            if (!Market.Started.Value)
            {
                throw new InvalidOperationException("Can't start until Market is started");
            }
            base.OnStarting();

            if (Template.Indicator.Symbol == null) Template.Indicator.Symbol = this.Template.Symbol;
            if (Template.Indicator.TimeFrame == null) Template.Indicator.TimeFrame = this.Template.TimeFrame;

            this.Indicator.Config = this.Template.Indicator;

            this.Market.Add(this.Indicator);

            var sim = Market as ISimulatedMarket;
            if (sim != null)
            {
                sim.SimulationTickFinished += OnSimulationTickFinished;
            }

            this.Symbol = Market.GetSymbol(Template.Symbol);
            this.MarketSeries = Market.GetMarketSeries(this.Template.Symbol, this.Template.TimeFrame /*, Market.IsBacktesting*/);

            UpdateDesiredSubscriptions();
        }

        protected virtual void UpdateDesiredSubscriptions()
        {
            // Disabled for now
            //this.DesiredSubscriptions = new List<MarketDataSubscription>()
            //{
            //    new MarketDataSubscription(this.Template.Symbol, Template.TimeFrame)
            //    //new MarketDataSubscription(this.Config.Symbol, "t1")
            //};
        }

        protected void OnSimulationTickFinished()
        {
            //logger.LogInformation("bot OnSimulationTickFinished " + (Market as BacktestMarket).SimulationTime);
            Evaluate();
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
