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
using System.Reactive.Linq;

namespace LionFire.Trading
{
    //public abstract class StateDependency
    //{
    //    public abstract ExecutionState State { get; }


    //}

    //public class MarketDependency : StateDependency
    //{
    //    public override ExecutionState State { get { return ExecutionState.Started; } }

    //}

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

        public IEnumerable<MarketDataSubscription> DesiredSubscriptions
        {
            get
            {
                return desiredSubscriptions;
            }
            set
            {
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
                foreach (var sub in oldValue)
                {
                    if (sub.TimeFrame.Name == "t1")
                    {
                        Account.GetSymbol(sub.Symbol).Tick -= OnTick;
                    }
                    else
                    {
                        sub.Observable?.Dispose();
                    }
                }
            }
            if (account != null && newValue != null)
            {
                foreach (var sub in newValue)
                {
                    if (sub.Series == null)
                    {
                        sub.Series = account.GetMarketSeries(sub.Symbol, sub.TimeFrame);
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
                    if (sub.TimeFrame.Name == "t1")
                    {
                        //sub.Observable = Observable.FromEvent(add => 
                        Account.GetSymbol(sub.Symbol).Tick += OnTick;
                    }
                    else
                    {
                        sub.Observable = sub.Series.LatestBar.Subscribe(timedBar => OnBar(sub.Series, timedBar));
                    }
                }
            }
        }

        #endregion

        #region Relationships

        [Dependency]
        public IAccount Account
        {
            get
            {
                return account;
            }
            set
            {
                if (account == value) return;
                if (account != null)
                {
                    OnDetaching();
                    foreach (var sub in DesiredSubscriptions)
                    {
                        sub.IsActive = false;
                        //sub.Series.BarReceived -= OnBar;
                        sub.Series = null;
                    }
                }
                account = value;

                if (account != null)
                {
                    account.Add(this); // REVIEW

                    if (DesiredSubscriptions != null)
                    {
                        OnDesiredSubscriptionsChanged(null, DesiredSubscriptions);
                    }
                }
                OnAttaching();
                OnAttached();
            }
        }
        private IAccount account;

        List<IDisposable> marketSubscriptions = new List<IDisposable>();

        protected virtual void OnAttaching()
        {
        }
        protected virtual void OnAttached()
        {
            Account.Started.Subscribe(started => { if (started) { OnMarketStarted().Wait(); } });
            //Market.Started.Subscribe(async started => { if (started) { await OnMarketStarted(); } });
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

        public IBehaviorObservable<ExecutionState> ExecutionState
        {
            get
            {
                return executionState;
            }
        }
        protected BehaviorObservable<ExecutionState> executionState = new BehaviorObservable<ExecutionState>();

        #endregion

        #endregion

        public async Task Start()
        {
            this.ValidateDependencies();

            StartOnMarketAvailable = true;

            if (Account.Started.Value)
            {
                await Task.Run(() => this.OnStarting());
            }
        }

        protected bool StartOnMarketAvailable = false;

        protected async virtual Task OnMarketStarted()
        {
            if (StartOnMarketAvailable)
            {
                await Start();
            }
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
        public void OnBar(string symbolCode, SymbolBar bar, TimeFrame timeFrame)
        {
            OnBar(symbolCode, timeFrame, new TimedBar(bar));
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

        public virtual void OnTick(SymbolTick tick)
        {
            Console.WriteLine("MarketParticipant.OnTick: " + tick.ToString());
        }

        #endregion

        #region Misc

        public ILogger Logger { get { return logger; } }
        protected ILogger logger;

        #endregion

    }



}

