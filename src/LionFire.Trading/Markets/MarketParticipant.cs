using LionFire.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.Execution.Executables;
using LionFire.Reactive;
using LionFire.Reactive.Subjects;
using System.Threading;
using System.Collections.Concurrent;
using LionFire.Dependencies;

namespace LionFire.Trading
{
    /// <summary>
    /// Represents an entity that participates in the market, either passively (readonly, which will receive events for DesiredSubscriptions via OnBar) and/or actively (by creating orders)
    /// </summary>
    public abstract class MarketParticipant : IMarketParticipant, IExecutable
    {

        #region Construction

        public MarketParticipant()
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
                            logger.LogWarning("Market data not available: " + sub);
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

        [Dependency]
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
            Market.Started.Subscribe(async started => { if (started) { await OnMarketStarted(); } });
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

        #region State

        #region ExecutionState

        public IBehaviorObservable<ExecutionState> ExecutionState {
            get {
                return executionState;
            }
        }
        protected BehaviorObservable<ExecutionState> executionState = new BehaviorObservable<ExecutionState>();

        #endregion
        
        #endregion

        public async Task Start()
        {
            this.ValidateDependencies();

            await Task.Run(() => this.OnStarting());
        }

        protected async virtual Task OnMarketStarted()
        {
            await Start();
        }

        protected virtual void OnStarting() 
        {
            this.OnEnteringState(LionFire.Execution.ExecutionState.Starting);
        }

        #region Market Event Handling

        public void OnBar(IMarketSeries series, TimedBar bar)
        {
            OnBar(series.SymbolCode, series.TimeFrame, bar);
        }

        public void OnBar(MarketSeries series)
        {
            var bar = series.LastBar;
            OnBar(series.SymbolCode, series.TimeFrame, bar);
        }

        #endregion


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
        
        #region Misc

        private ILogger logger;

        #endregion

    }



}

