using LionFire.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    #if false

    public class TradingExecutable : ExecutableBase, IStartable, IInitializable, 
    {
        #region Relationships


        protected List<IDisposable> marketSubscriptions = new List<IDisposable>();



        
        public IAccount Market {
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
        private IAccount market;

        protected virtual void OnAttaching() { }
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


        #region Construction

        public TradingExecutable()
        {
            logger = this.GetLogger();
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

        public Task<bool> Initialize()
        {
            missingDependencies.Clear();
            if (Market == null)
            {
                missingDependencies.Add("Market");
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

        public Task Start()
        {
            throw new NotImplementedException();
        }

        #region Misc

        protected ILogger logger;

        #endregion
    }
#endif

}
