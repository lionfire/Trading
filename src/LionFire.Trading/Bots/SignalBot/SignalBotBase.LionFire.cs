using LionFire.Trading.Backtesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public partial class SingleSeriesSignalBotBase<TIndicator, TConfig, TIndicatorConfig> 
    {
        public MarketSeries MarketSeries { get; set; } 


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

            this.MarketSeries = Market.Data.GetMarketSeries(this.Config.Symbol, this.Config.TimeFrame, Market.IsBacktesting);

            this.Symbol = Market.GetSymbol(Config.Symbol);

            UpdateDesiredSubscriptions();
        }

        protected virtual void UpdateDesiredSubscriptions()
        {
            this.DesiredSubscriptions = new List<MarketDataSubscription>()
            {
                new MarketDataSubscription(this.Config.Symbol, Config.TimeFrame)
            };
        }

        protected void OnSimulationTickFinished()
        {
            //logger.LogInformation("bot OnSimulationTickFinished " + (Market as BacktestMarket).SimulationTime);
            Evaluate();
        }
    }
}
