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
using LionFire.States;
using LionFire.Messaging;

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

                if (index == -1 || index - desiredBars < s.FirstIndex)
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
                        Account.GetSymbol(sub.Symbol).Ticked -= OnTick;
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
                        Account.GetSymbol(sub.Symbol).Ticked += OnTick;
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

        public virtual IEnumerable<IAccountParticipant> Children { get { yield break; } }

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
            foreach (var child in Children)
            {
                child.Account = this.Account;
            }

            

            Account.Started.Subscribe(started => { if (started) { Threading.Tasks.TaskManager.OnNewTask(OnMarketStarted(), Threading.Tasks.TaskFlags.Unowned); } });
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
            var oldState = this.state.Value;
            this.state.OnNext(state);
            new MExecutionStateChanged() { Source = this, OldState = oldState, NewState = state }.Publish();
            OnPropertyChanged(nameof(this.State));
        }

        public ExecutionStateFlags ExecutionStateFlags { get; set; } = ExecutionStateFlags.Autostart;

        #endregion

        [State]
        public ExecutionState DesiredExecutionState
        {
            get
            {
                return desiredExecutionState;
            }
            set
            {
                if (desiredExecutionState == value) return;
                desiredExecutionState = value;

                switch (State.Value)
                {
                    //case ExecutionState.Unspecified:
                    //    break;
                    //case ExecutionState.Uninitialized:
                    //    break;
                    //case ExecutionState.Faulted:
                    //    break;
                    //case ExecutionState.Initializing:
                    //    break;
                    //case ExecutionState.Ready:
                    //    break;
                    //case ExecutionState.Starting:
                    //    break;
                    //case ExecutionState.Started:
                    //    break;
                    //case ExecutionState.Pausing:
                    //    break;
                    //case ExecutionState.Paused:
                    //    break;
                    //case ExecutionState.Unpausing:
                    //    break;
                    //case ExecutionState.Stopping:
                    //    break;
                    //case ExecutionState.Stopped:
                    //    break;
                    //case ExecutionState.Finished:
                    //    break;
                    case ExecutionState.Disposed:
                        if (value != ExecutionState.Disposed)
                        {
                            throw new ObjectDisposedException(this.ToString());
                        }
                        return;
                    default:
                        break;
                }

                switch (desiredExecutionState)
                {
                    //case ExecutionState.Unspecified:
                    //    break;
                    //case ExecutionState.Uninitialized:
                    //    break;
                    //case ExecutionState.Faulted:
                    //    break;
                    //case ExecutionState.Initializing:
                    //    break;
                    //case ExecutionState.Ready:
                    //    break;
                    //case ExecutionState.Starting:
                    //case ExecutionState.Started:
                    //    SetState(desiredExecutionState);
                    //    break;
                    //case ExecutionState.Pausing:
                    //    break;
                    //case ExecutionState.Paused:
                    //    break;
                    //case ExecutionState.Unpausing:
                    //    break;
                    //case ExecutionState.Stopping:
                    //    break;
                    //case ExecutionState.Stopped:
                    //    break;
                    //case ExecutionState.Finished:
                    //    break;
                    //case ExecutionState.Disposed:
                    //    break;
                    default:
                        SetState(desiredExecutionState);
                        break;
                }
            }
        }
        private ExecutionState desiredExecutionState;

        #endregion

        public virtual Task<bool> Initialize()
        {
            this.ValidateDependencies();
            switch (state.Value)
            {
                //case ExecutionState.Unspecified:
                //    break;
                //case ExecutionState.Unconfigured:
                //    break;
                //case ExecutionState.InvalidConfiguration:
                //    break;
                //case ExecutionState.Uninitialized:
                //    break;
                //case ExecutionState.Initializing:
                //    break;
                //case ExecutionState.Ready:
                //    break;
                //case ExecutionState.Starting:
                //    break;
                //case ExecutionState.Started:
                //    break;
                //case ExecutionState.Pausing:
                //    break;
                //case ExecutionState.Paused:
                //    break;
                //case ExecutionState.Unpausing:
                //    break;
                //case ExecutionState.Stopping:
                //    break;
                //case ExecutionState.Stopped:
                //    break;
                //case ExecutionState.Finished:
                //    break;
                case ExecutionState.Disposed:
                    throw new ObjectDisposedException(this.ToString());
                //case ExecutionState.WaitingToStart:
                //    break;
                //case ExecutionState.Faulted:
                //    break;
                default:
                    break;
            }

            state.OnNext(ExecutionState.Initializing);
            foreach (var child in Children.OfType<IAccountParticipant>())
            {
                child.Account = this.Account;
            }
            state.OnNext(ExecutionState.Ready);
            return Task.FromResult(true);
        }

        public async Task Start()
        {
            if (this.IsStarted()) return;

            desiredExecutionState = ExecutionState.Started; // REVIEW - have a single input/output

            StartOnMarketAvailable = true;

            // TODO: 
            //DesiredExecutionState = ExecutionState.Started;

            if (Account != null && Account.Started.Value)
            {
                //await Task.Run(async () => await _Start());
                await _Start().ConfigureAwait(false);
            }
            else
            {
                ExecutionStateFlags |= ExecutionStateFlags.WaitingToStart;
            }
        }

        private async Task _Start()
        {
            int retriesRemaining = 30;
            tryagain:
            switch (State.Value)
            {
                //case ExecutionState.Unconfigured:
                //    break;
                //case ExecutionState.InvalidConfiguration:
                //    break;
                case ExecutionState.Unspecified:
                case ExecutionState.Finished:
                case ExecutionState.Uninitialized:
                case ExecutionState.Stopped:
                    await Initialize();
                    goto tryagain;
                case ExecutionState.Initializing:
                case ExecutionState.Stopping:
                    if (retriesRemaining-- > 0)
                    {
                        await Task.Delay(1000);
                        goto tryagain;
                    }
                    else
                    {
                        throw new Exception("Cannot start, current state is Initializing.");
                    }
                case ExecutionState.Ready:
                    //case ExecutionState.WaitingToStart:
                    break;
                case ExecutionState.Starting:
                case ExecutionState.Started:
                    return;
                //case ExecutionState.Pausing:
                //    break;
                //case ExecutionState.Paused:
                //    break;
                //case ExecutionState.Unpausing:
                //    break;
                case ExecutionState.Disposed:
                    throw new ObjectDisposedException(this.GetType().Name);
                default:
                    throw new Exception("Unsupported state for Start(): " + State.Value);
            }

            SetState(ExecutionState.Starting);
            this.OnEnteringState(LionFire.Execution.ExecutionState.Starting);

            await OnStarting();

            foreach (var child in Children.OfType<IStartable>())
            {
                await child.Start();
            }

            SetState(ExecutionState.Started);
        }

        protected async Task DoStop(StopMode stopMode = StopMode.GracefulShutdown, StopOptions options = StopOptions.StopChildren)
        {
            DesiredExecutionState = ExecutionState.Stopped;

            switch (State.Value)
            {
                case ExecutionState.Unspecified:
                case ExecutionState.Uninitialized:
                case ExecutionState.Faulted:
                case ExecutionState.Stopped:
                case ExecutionState.Finished:
                case ExecutionState.Disposed:
                    return;
                case ExecutionState.Initializing:
                case ExecutionState.Ready:
                case ExecutionState.Starting:
                case ExecutionState.Started:
                case ExecutionState.Pausing:
                case ExecutionState.Paused:
                case ExecutionState.Unpausing:
                    break;
                case ExecutionState.Stopping:
                    break;
                default:
                    throw new Exception();
            }

            SetState(ExecutionState.Stopping);

            OnStopping();

            foreach (var child in Children.OfType<IStoppable>())
            {
                await child.Stop(stopMode, options);
            }

            SetState(ExecutionState.Stopped);

            OnStopped();
        }
        protected virtual void OnStopping()
        {
            logger.LogInformation($"------- STOP {this} -------");
        }
        protected virtual void OnStopped()
        {
            logger.LogInformation($"------- STOP {this} -------");
        }

        public virtual async Task Stop(StopMode stopMode = StopMode.GracefulShutdown, StopOptions options = StopOptions.StopChildren)
        {
            await DoStop();
        }

        protected bool StartOnMarketAvailable = false;

        protected async virtual Task OnMarketStarted()
        {
            if (StartOnMarketAvailable)
            {
                await Start();
            }
        }

        protected virtual async Task OnStarting()
        {
            this.ValidateDependencies();

            // MarketSeriesOfInterest Not used at the moment
            var interested = this as IInterestedInMarketData;
            if (interested != null)
            {
                await EnsureDataAvailable(Account.ExtrapolatedServerTime).ConfigureAwait(false);
            }
        }
        protected virtual Task OnStarted()
        {
            return Task.CompletedTask;
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
            var bar = series.Last;
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

