using LionFire.Trading.Backtesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Structures;
using LionFire.Validation;
using LionFire.Templating;

namespace LionFire.Trading.Bots
{
    public partial class SingleSeriesSignalBotBase<IndicatorType, TSingleSeriesSignalBotBase, TIndicator>
    {
        public IMarketSeries MarketSeries { get; set; }

        protected override void OnStarting()
        {
            this.Validate()
            .PropertyNonDefault(nameof(Template.Symbol), Template.Symbol)
            .PropertyNonDefault(nameof(Template.TimeFrame), Template.TimeFrame)
            .EnsureValid();

            if (!Account.Started.Value)
            {
                throw new InvalidOperationException("Can't start until Account is started");
            }
            base.OnStarting();

            if (Template.Indicator.Symbol == null) Template.Indicator.Symbol = this.Template.Symbol;
            if (Template.Indicator.TimeFrame == null) Template.Indicator.TimeFrame = this.Template.TimeFrame;

            this.Symbol = Account.GetSymbol(Template.Symbol);
            this.MarketSeries = Account.GetMarketSeries(this.Template.Symbol, this.Template.TimeFrame /*, Market.IsBacktesting*/);

            CreateIndicator();

            //this.Indicator.Template = this.Template.Indicator;

            this.Indicator.Account = Account; // 

            Indicator.Start();

            //UpdateDesiredSubscriptions();

            var sim = Account as ISimulatedAccount;
            if (Account as ISimulatedAccount != null)
            {
                Account.Ticked += Market_Ticked;
            }
            else
            {
                this.MarketSeries.Bar += MarketSeries_Bar;
            }
            //this.Symbol.GetLastBar(TimeFrame.m1).Wait(); // Gets server time

            this.Symbol.Ticked += Symbol_Tick;

            Evaluate();

        }

        private void Market_Ticked()
        {
            // Unlikely OPTIMIZE: convey which symbols/tf's ticked and only evaluate here if relevant?  Backtests are usually focused on one bot though.
            Evaluate();
        }

        public DateTime End = default(DateTime);

        private void Symbol_Tick(SymbolTick obj) // MOVE unsubscribe logic  to Base
        {
            if (End == default(DateTime))  // REFACTOR to base
            {
                End = DateTime.Now + TimeSpan.FromSeconds(150);
            }
            //Console.WriteLine($"[liontrender] [tick] {z} {obj}    (Test time remaining: {(End-DateTime.Now).TotalSeconds.ToString("N")} seconds)");

#if !cAlgo
            if (End <= DateTime.Now)
            {
                Console.WriteLine("[test] Test complete.  Unsubscribing from tick events.");
                this.Symbol.Ticked -= Symbol_Tick;
            }
#endif
        }

        private void MarketSeries_Bar(SymbolBar obj)
        {
            //Console.WriteLine("[bar] " + obj);
            Evaluate();
        }

        //protected virtual void UpdateDesiredSubscriptions()
        //{
        //    // Disabled for now
        //    //this.DesiredSubscriptions = new List<MarketDataSubscription>()
        //    //{
        //    //    new MarketDataSubscription(this.Template.Symbol, Template.TimeFrame)
        //    //    //new MarketDataSubscription(this.Config.Symbol, "t1")
        //    //};
        //}

        //protected void OnSimulationTickFinished()
        //{
        //    //logger.LogInformation("bot OnSimulationTickFinished " + (Market as BacktestMarket).SimulationTime);
        //    Evaluate();
        //}


        //long i = 0;
        public override void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
            if (bar.IsValid)
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
