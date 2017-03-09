using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LionFire.Trading.Indicators
{
    public abstract partial class SingleSeriesIndicatorBase<TConfig> : IndicatorBase<TConfig>, IHasSingleSeries, ISingleSeriesIndicator
        where TConfig : ITSingleSeriesIndicator, new()
    {

        public MarketSeries MarketSeries
        {
            get
            {
                if (marketSeries == null && Template != null && Account != null && Template.Symbol != null && Template.TimeFrame != null)
                {
                    var tf = TimeFrame.TryParse(Template.TimeFrame);
                    if (tf == null) { throw new ArgumentException("Failed to parse TimeFrame: " + Template.TimeFrame); }
                    marketSeries = (MarketSeries)Account.GetMarketSeries(Template.Symbol, tf);
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
            MarketSeries.Bar += barHandler;
        }

        private async void barHandler(TimedBar bar)
        {
            try
            {
                OnBar(MarketSeries.SymbolCode, MarketSeries.TimeFrame, bar);
            }
            catch (Exception ex)
            {
                this.FaultException = ex;
                MarketSeries.Bar += barHandler;
                await this.OnFault(ex).ConfigureAwait(false);
            }
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
                Account.Started.Subscribe(async started => { if (started) { await OnStarting().ConfigureAwait(false); } });
            }
        }
    }
}
