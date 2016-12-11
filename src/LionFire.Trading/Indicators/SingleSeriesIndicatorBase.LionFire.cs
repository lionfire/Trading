using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    public abstract partial class SingleSeriesIndicatorBase<TConfig> : IndicatorBase<TConfig>, IHasSingleSeries
        where TConfig : ITIndicator, new()
    {
        public MarketSeries MarketSeries
        {
            get
            {
                if (marketSeries == null && Template != null && Account != null && Template.Symbol != null && Template.TimeFrame != null)
                {
                    marketSeries = (MarketSeries)Account.GetMarketSeries(Template.Symbol, TimeFrame.TryParse(Template.TimeFrame));                    
                }
                return marketSeries;
            }
            protected set { this.marketSeries = value; }
        }
        private MarketSeries marketSeries;

        //IDisposable marketSeriesSubscription;

        partial void OnInitializing_()
        {
            if (TimeFrame == null)
            {
                throw ConfigMissingException(nameof(TimeFrame));
            }
            if (Symbol == null)
            {
                throw ConfigMissingException(nameof(Symbol));
            }

            if (MarketSeries == null) throw new Exception("MarketSeries not resolved at Initialize time.");

            //marketSeriesSubscription = this.MarketSeries.LatestBar.Subscribe(bar => OnBar(MarketSeries, bar));
            MarketSeries.Bar += bar => OnBar(MarketSeries.SymbolCode, MarketSeries.TimeFrame, new TimedBar(bar));
        }


        protected override void OnConfigChanged()
        {
            base.OnConfigChanged();

            if (TimeFrame == null)
            {
                if (Template == null) { throw ConfigMissingException(nameof(TimeFrame)); }
                TimeFrame = Template.TimeFrame;
            }
            if (Symbol == null)
            {
                if (Template == null) { throw ConfigMissingException(nameof(Symbol)); }
                if (Account != null)
                {
                    Symbol = Account.GetSymbol(Template.Symbol);
                }
            }
        }

        protected override void OnAttaching()
        {
            base.OnAttaching();
            if (Account != null)
            {
                Symbol = Account.GetSymbol(Template.Symbol);
                Account.Started.Subscribe(started => { if (started) { OnStarting(); } });
            }
        }
    }
}
