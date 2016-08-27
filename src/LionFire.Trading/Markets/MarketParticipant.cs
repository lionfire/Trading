using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    //public interface IMarketDataSubscriber
    //{
    //    void OnBar(SymbolBar bar);
    //    void OnTick(SymbolBar bar); // TODO: Tick
    //}


    /// <summary>
    /// Represents an entity that participates in the market, either passively (readonly) or actively (by creating orders)
    /// </summary>
    public class MarketParticipant
    //: IMarketDataSubscriber
    {


        #region Subscriptions

        public IEnumerable<MarketDataSubscription> DesiredSubscriptions {
            get {
                return desiredSubscriptions;
            }
            set {
                if (desiredSubscriptions == value) return;
                var oldValue = desiredSubscriptions;
                desiredSubscriptions = value;
                DesiredSubscriptionsChangedFrom?.Invoke(oldValue);
            }
        }
        private IEnumerable<MarketDataSubscription> desiredSubscriptions;

        #endregion


        public event Action<IEnumerable<MarketDataSubscription>> DesiredSubscriptionsChangedFrom;

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
                        sub.Series.BarReceived -= OnBar;
                        sub.Series = null;
                    }
                }
                market = value;

                if (market != null && DesiredSubscriptions != null)
                {
                    foreach (var sub in DesiredSubscriptions)
                    {
                        sub.Series = market.Data.GetMarketSeries(sub.Symbol, sub.TimeFrame);
                        if (sub.Series == null)
                        {
                            sub.IsActive = false;
                            if (!sub.IsOptional)
                            {
                                throw new MarketDataUnavailableException("Market data not available: " + sub);
                            }
                            else
                            {
                                Console.WriteLine("Market data not available: " + sub); // TOLOG
                            }
                            continue;
                        }
                        else
                        {
                            sub.IsActive = true;
                            sub.Series.BarReceived += OnBar;
                        }
                    }
                }
                OnAttached();
            }
        }
        private IMarket market;

        List<IDisposable> marketSubscriptions = new List<IDisposable>();

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


        public void OnBar(MarketSeries series)
        {
            var bar = series.LastBar;
            OnBar(series.SymbolCode, series.TimeFrame, bar);
        }

        #region IMarketDataSubscriber - REVIWE
        
        public virtual void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {

        }

        public virtual void OnBarFinished(string symbolCode, TimeFrame timeFrame)
        {
        }

        public virtual void OnBar(SymbolBar bar)
        {
        }

        public void OnBars(IEnumerable<SymbolBar> bars)
        {
            //foreach (var bar in bars)
            //{
            //    OnBar(bar);
            //}
        }

        public void OnTick(SymbolBar bar)
        {
        }

        #endregion

    }



}
