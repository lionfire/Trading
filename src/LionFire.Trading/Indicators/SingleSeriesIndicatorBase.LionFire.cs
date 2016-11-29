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
        public MarketSeries MarketSeries { get; protected set; }
        IDisposable marketSeriesSubscription;

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
            this.MarketSeries = Account.Data.GetMarketSeries(this.Symbol.Code, this.TimeFrame);

            //marketSeriesSubscription = this.MarketSeries.LatestBar.Subscribe(bar => OnBar(MarketSeries, bar));
                this.MarketSeries.Bar += bar => OnBar(MarketSeries.SymbolCode, MarketSeries.TimeFrame, new TimedBar(bar));
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
                if (Account != null)
                {
                    Symbol = Account.GetSymbol(Config.Symbol);
                }
            }
        }

        protected override void OnAttaching()
        {
            base.OnAttaching();
            if (Account != null)
            {
                Symbol = Account.GetSymbol(Config.Symbol);
                Account.Started.Subscribe(started => { if (started) { OnStarting(); } });
            }
        }
    }
}
