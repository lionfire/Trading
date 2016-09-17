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
        public MarketSeries MarketSeries { get; set; } // TODO - Single series bot?


        protected override void OnStarting()
        {
            base.OnStarting();
            this.Indicator.Config = this.Config.Indicator;
            this.Market.Add(this.Indicator);

            var sim = Market as ISimulatedMarket;
            if (sim != null)
            {
                sim.SimulationTickFinished += OnSimulationTickFinished;
            }

            this.MarketSeries = Market.Data.GetMarketSeries(this.Config.SymbolCode, this.Config.TimeFrameCode, Market.IsBacktesting);
            this.Symbol = Market.GetSymbol(Config.SymbolCode);
        }

        protected void OnSimulationTickFinished()
        {
            logger.LogInformation("bot OnSimulationTickFinished " + (Market as BacktestMarket).SimulationTime);
            Evaluate();
        }
    }
}
