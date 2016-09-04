using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    public abstract partial class SingleSeriesIndicatorBase<TConfig> : IndicatorBase<TConfig>, IHasSingleSeries
        where TConfig : IIndicatorConfig
    {
        public MarketSeries MarketSeries { get; protected set; }


        partial void _InitPartial()
        {
            if (TimeFrame == null)
            {
                throw ConfigMissingException(nameof(TimeFrame));
            }
            if (Symbol == null)
            {
                throw ConfigMissingException(nameof(Symbol));
            }
            this.MarketSeries = Market.Data.GetMarketSeries(this.Symbol.Code, this.TimeFrame);


        }

        protected override void OnConfigChanged()
        {
            base.OnConfigChanged();

            if (TimeFrame == null)
            {
                if (Config == null) { throw ConfigMissingException(nameof(TimeFrame)); }
                TimeFrame = Config.TimeFrame;
            }
            if (Symbol == null)
            {
                if (Config == null) { throw ConfigMissingException(nameof(Symbol)); }
                if (Market != null)
                {
                    Symbol = Market.GetSymbol(Config.Symbol);
                }
            }
        }

        protected override void OnAttaching()
        {
            base.OnAttaching();
            if (Market != null)
            {
                Symbol = Market.GetSymbol(Config.Symbol);
            }
        }

    }

}
