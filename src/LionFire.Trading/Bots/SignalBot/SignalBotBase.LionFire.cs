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
            if (!Account.Started.Value)
            {
                throw new InvalidOperationException("Can't start until Market is started");
            }
            base.OnStarting();

            if (Template.Indicator.Symbol == null) Template.Indicator.Symbol = this.Template.Symbol;
            if (Template.Indicator.TimeFrame == null) Template.Indicator.TimeFrame = this.Template.TimeFrame;

            this.Indicator.Config = this.Template.Indicator;

            this.Indicator.Account = Account;

            this.Symbol = Account.GetSymbol(Template.Symbol);
            this.MarketSeries = Account.GetMarketSeries(this.Template.Symbol, this.Template.TimeFrame /*, Market.IsBacktesting*/);

            UpdateDesiredSubscriptions();

            var sim = Account as ISimulatedAccount;
            if (Account as ISimulatedAccount != null)
            {
                Account.Ticked += Market_Ticked;
            }
            else
            {
                this.MarketSeries.Bar += MarketSeries_Bar;
            }
        }

        private void Market_Ticked()
        {
            // Unlikely OPTIMIZE: convey which symbols/tf's ticked and only evaluate here if relevant?  Backtests are usually focused on one bot though.
            Evaluate();
        }

        private void MarketSeries_Bar(SymbolBar obj)
        {
            //Console.WriteLine("[bar] " + obj);
            Evaluate();
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
