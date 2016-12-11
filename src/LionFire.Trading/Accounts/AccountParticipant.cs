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
using System.ComponentModel;
using System.Diagnostics;

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

    public interface IInterestedInMarketData
    {
        IEnumerable<MarketSeries> MarketSeriesOfInterest { get; }
        Task EnsureDataAvailable(DateTime upToDate);
    }
    /// <summary>
    /// Represents an entity that participates in the market, either passively (readonly, which will receive events for DesiredSubscriptions via OnBar) and/or actively (by creating orders)
    /// </summary>
    public abstract class AccountParticipant : IAccountParticipant, IExecutable, INotifyPropertyChanged, IInterestedInMarketData
    {


        #region Desired Bars

        public virtual IEnumerable<MarketSeries> MarketSeriesOfInterest
        {
            get { yield break; }
        }

        public static int DefaultDesiredBars = 100;
        public virtual int GetDesiredBars(string symbolCode, TimeFrame timeFrame)
        {
            return DefaultDesiredBars;
        }
        public virtual TimeSpan GetDesiredTimeSpan(string symbolCode, TimeFrame timeFrame)
        {
            var bars = GetDesiredBars(symbolCode, timeFrame);
            var time = TimeSpan.FromMilliseconds(timeFrame.TimeSpan.TotalMilliseconds * bars);
            return time;
        }

        public Task EnsureDataAvailable(DateTime startDate)
        {
            var tasks = new List<Task>();
            foreach (var s in MarketSeriesOfInterest)
            {
                var desiredBars = GetDesiredBars(s.SymbolCode, s.TimeFrame);
                var index = s.FindIndex(Account.ExtrapolatedServerTime);

                if (index == -1 || index - desiredBars < s.MinIndex)
                {
                    var task = Account.Data.EnsureDataAvailable(s, null, startDate /* yes, supplying startDate as endDate */, desiredBars); // TODO: Reorder  parameters to move startdate to end and default to null
                    if (task != null)
                    {
                        tasks.Add(task);
                    }
                    else
                    {
                        Debug.WriteLine("task==null");
                    }
                }
            }
            if (tasks.Count == 1)
            {
                return tasks[0];
            }
            else if (tasks.Count == 0)
            {
                return Task.CompletedTask;
            }
            else
            {
                return Task.Factory.StartNew(() => Task.WaitAll(tasks.ToArray()));
            }
        }

        #endregion


        #region Construction

        public AccountParticipant()
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
                    OnDetached();
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

        protected virtual void OnDetached()
        {
        }
        #endregion

        #region State

        #region ExecutionState

        public IBehaviorObservable<ExecutionState> State
        {
            get
            {
                return state;
            }
        }
        protected BehaviorObservable<ExecutionState> state = new BehaviorObservable<ExecutionState>();
        protected void SetState(ExecutionState state)
        {
            this.state.OnNext(state);
            OnPropertyChanged(nameof(this.State));
        }


        #endregion

        #endregion

        public async Task Start()
        {
            if (State.Value == ExecutionState.Started || State.Value == ExecutionState.Starting) return;

            this.ValidateDependencies();

            StartOnMarketAvailable = true;

            if (Account.Started.Value)
            {
                OnStarting();
            }
            else
            {
                state.OnNext(ExecutionState.WaitingToStart);
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

            var interested = this as IInterestedInMarketData;
            if (interested != null)
            {
                EnsureDataAvailable(Account.ExtrapolatedServerTime).Wait();
            }
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
            Console.WriteLine("AccountParticipant.OnTick: " + tick.ToString());
        }

        #endregion

        #region Misc


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        public ILogger Logger { get { return logger; } }
        protected ILogger logger;

        #endregion

    }



}

