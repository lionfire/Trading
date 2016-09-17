using LionFire.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public interface IMarketParticipant
    {
        IMarket Market { get; set; }

        //void Init();
    }

    /// <summary>
    /// Represents an entity that participates in the market, either passively (readonly, which will receive events for DesiredSubscriptions via OnBar) and/or actively (by creating orders)
    /// </summary>
    public abstract class MarketParticipant : IMarketParticipant
    {

        #region Construction

        public MarketParticipant()
        {
            l = this.GetLogger();
        }

        #endregion

        #region Subscriptions

        public IEnumerable<MarketDataSubscription> DesiredSubscriptions {
            get {
                return desiredSubscriptions;
            }
            set {
                if (desiredSubscriptions == value) return;
                var oldValue = desiredSubscriptions;
                desiredSubscriptions = value;
                //DesiredSubscriptionsChangedFrom?.Invoke(oldValue);
                OnDesiredSubscriptionsChanged(oldValue, value);
            }
        }
        private IEnumerable<MarketDataSubscription> desiredSubscriptions;

        protected virtual void OnDesiredSubscriptionsChanged(IEnumerable<MarketDataSubscription> oldValue, IEnumerable<MarketDataSubscription> newValue)
        {
            if (oldValue != null)
            {
                foreach (var old in oldValue)
                {
                    old.Observable?.Dispose();
                }
            }
            if (market != null && newValue != null)
            {
                foreach (var sub in newValue)
                {
                    if (sub.Series == null)
                    {
                        sub.Series = market.Data.GetMarketSeries(sub.Symbol, sub.TimeFrame);
                    }
                    if (sub.Series == null)
                    {
                        sub.IsActive = false;
                        if (!sub.IsOptional)
                        {
                            throw new MarketDataUnavailableException("Market data not available: " + sub);
                        }
                        else
                        {
                            l.LogWarning("Market data not available: " + sub);
                        }
                        continue;
                    }

                    sub.IsActive = true;
                    //sub.Series.BarReceived += OnBar;
                    sub.Observable = sub.Series.LatestBar.Subscribe(timedBar => OnBar(sub.Series, timedBar));
                }
            }
        }

        #endregion

        #region Relationships

        public IMarket Market {
            get {
                return market;
            }
            set {
                if (market == value) return;
                if (market != null)
                {
                    OnDetaching();
                    foreach (var sub in DesiredSubscriptions)
                    {
                        sub.IsActive = false;
                        //sub.Series.BarReceived -= OnBar;
                        sub.Series = null;
                    }
                }
                market = value;

                if (market != null && DesiredSubscriptions != null)
                {
                    OnDesiredSubscriptionsChanged(null, DesiredSubscriptions);
                }
                OnAttaching();
                OnAttached();
            }
        }
        private IMarket market;

        List<IDisposable> marketSubscriptions = new List<IDisposable>();

        protected virtual void OnAttaching()
        {
        }
        protected virtual void OnAttached()
        {
            Market.Started.Subscribe(started => { if (started) { OnStarting(); } });
        }

        /// <summary>
        /// Throws NotImplementedException by default.  Override to support detaching.
        /// </summary>
        protected virtual void OnDetaching()
        {
            foreach (var x in marketSubscriptions) x.Dispose();
            marketSubscriptions.Clear();
        }

        #endregion

        protected virtual void OnStarting()
        {
        }


        public void OnBar(IMarketSeries series, TimedBar bar)
        {
            OnBar(series.SymbolCode, series.TimeFrame, bar);
        }

        public void OnBar(MarketSeries series)
        {
            var bar = series.LastBar;
            OnBar(series.SymbolCode, series.TimeFrame, bar);
        }

        #region IMarketDataSubscriber - REVIEW

        public virtual void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
        }

        public virtual void OnBarFinished(string symbolCode, TimeFrame timeFrame)
        {
        }

        public virtual void OnBar(SymbolBar bar)
        {
        }

        //public void OnBars(IEnumerable<SymbolBar> bars)
        //{
        //}

        public virtual void OnTick(SymbolBar bar)
        {
        }

        #endregion


        private ILogger l;


    }



}
