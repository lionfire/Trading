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
using LionFire.DependencyInjection;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Diagnostics;
using LionFire.States;
using LionFire.Messaging;
using LionFire.Dependencies;
using LionFire.Structures;
using System.Collections;

namespace LionFire.Trading
{    
    /// <summary>
    /// Represents an entity that participates in the market, either passively (readonly, which will receive events for DesiredSubscriptions via OnBar) and/or actively (by creating orders)
    /// </summary>
    public abstract class AccountParticipant : ExecutableExBase, IAccountParticipant, IExecutableEx, INotifyPropertyChanged
    {

        #region Desired Bars

        public static int DefaultDesiredBars = 100;
        public virtual int GetDesiredBars(string symbolCode, TimeFrame timeFrame)
        {
            return DefaultDesiredBars;
        }
        public virtual TimeSpan GetDesiredTimeSpan(string symbolCode, TimeFrame timeFrame)
        {
            var bars = GetDesiredBars(symbolCode, timeFrame);
            var time = TimeSpan.FromMilliseconds(timeFrame.TimeSpanApproximation.TotalMilliseconds * bars);
            return time;
        }

        public Task EnsureDataAvailable(DateTime startDate, IEnumerable<MarketSeries> marketSeriesOfInterest)
        {
            var tasks = new List<Task>();
            foreach (var s in marketSeriesOfInterest)
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
            get => account;
            set
            {
                if (account == value) return;
                if (account != null)
                {
                    OnDetaching();

                    foreach (var x in marketSubscriptions) x.Dispose();
                    marketSubscriptions.Clear();

                    foreach (var sub in DesiredSubscriptions)
                    {
                        sub.IsActive = false;
                        //sub.Series.BarReceived -= OnBar;
                        sub.Series = null;
                    }

                    var ad = accountDisposer;
                    accountDisposer = null;
                    ad?.Dispose();

                    OnDetached();
                }
                account = value;

                if (account != null)
                {
                    if (account is IAsyncSubscribable s)
                    {
                        accountDisposer = s.SubscribeAsync();
                    }

                    account.AddAccountParticipant(this); // REVIEW

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
        private IDisposable accountDisposer;

        List<IDisposable> marketSubscriptions = new List<IDisposable>();

        protected virtual void OnAttaching() { }
        protected virtual void OnAttached()
        {
            foreach (var child in Children)
            {
                child.Account = this.Account;
            }

            Account.Started.Subscribe(started => { if (started) { LionFire.Threading.Tasks.TaskManager.OnNewTask(OnMarketStarted(), Threading.Tasks.TaskFlags.Unowned); } });
            //Market.Started.Subscribe(async started => { if (started) { await OnMarketStarted(); } });
        }
        
        protected virtual void OnDetaching()
        {
        }

        protected virtual void OnDetached()
        {
        }

        #endregion

        #region State

        #region ExecutionStateEx

        protected void SetState(ExecutionStateEx state)
        {
            var oldState = this.State;
            this.State = state;
            new MExecutionStateChanged() { Source = this, OldState = oldState, NewState = state }.Publish();
            OnPropertyChanged(nameof(this.State));
        }

        public ExecutionStateFlags ExecutionStateFlags { get; set; } = ExecutionStateFlags.Autostart;

        #endregion

        [State]
        public ExecutionStateEx DesiredExecutionState
        {
            get
            {
                return desiredExecutionState;
            }
            set
            {
                if (desiredExecutionState == value) return;
                desiredExecutionState = value;

                switch (State)
                {
                    //case ExecutionStateEx.Unspecified:
                    //    break;
                    //case ExecutionStateEx.Uninitialized:
                    //    break;
                    //case ExecutionStateEx.Faulted:
                    //    break;
                    //case ExecutionStateEx.Initializing:
                    //    break;
                    //case ExecutionStateEx.Ready:
                    //    break;
                    //case ExecutionStateEx.Starting:
                    //    break;
                    //case ExecutionStateEx.Started:
                    //    break;
                    //case ExecutionStateEx.Pausing:
                    //    break;
                    //case ExecutionStateEx.Paused:
                    //    break;
                    //case ExecutionStateEx.Unpausing:
                    //    break;
                    //case ExecutionStateEx.Stopping:
                    //    break;
                    //case ExecutionStateEx.Stopped:
                    //    break;
                    //case ExecutionStateEx.Finished:
                    //    break;
                    case ExecutionStateEx.Disposed:
                        if (value != ExecutionStateEx.Disposed)
                        {
                            throw new ObjectDisposedException(this.ToString());
                        }
                        return;
                    default:
                        break;
                }

                switch (desiredExecutionState)
                {
                    //case ExecutionStateEx.Unspecified:
                    //    break;
                    //case ExecutionStateEx.Uninitialized:
                    //    break;
                    //case ExecutionStateEx.Faulted:
                    //    break;
                    //case ExecutionStateEx.Initializing:
                    //    break;
                    //case ExecutionStateEx.Ready:
                    //    break;
                    //case ExecutionStateEx.Starting:
                    //case ExecutionStateEx.Started:
                    //    SetState(desiredExecutionState);
                    //    break;
                    //case ExecutionStateEx.Pausing:
                    //    break;
                    //case ExecutionStateEx.Paused:
                    //    break;
                    //case ExecutionStateEx.Unpausing:
                    //    break;
                    //case ExecutionStateEx.Stopping:
                    //    break;
                    //case ExecutionStateEx.Stopped:
                    //    break;
                    //case ExecutionStateEx.Finished:
                    //    break;
                    //case ExecutionStateEx.Disposed:
                    //    break;
                    default:
                        SetState(desiredExecutionState);
                        break;
                }
            }
        }
        private ExecutionStateEx desiredExecutionState;

        #endregion

        public virtual async Task<bool> Initialize()
        {
            try
            {
                FaultException = null;
                this.ValidateDependencies();
                switch (State)
                {
                    //case ExecutionStateEx.Unspecified:
                    //    break;
                    //case ExecutionStateEx.Unconfigured:
                    //    break;
                    //case ExecutionStateEx.InvalidConfiguration:
                    //    break;
                    //case ExecutionStateEx.Uninitialized:
                    //    break;
                    //case ExecutionStateEx.Initializing:
                    //    break;
                    //case ExecutionStateEx.Ready:
                    //    break;
                    //case ExecutionStateEx.Starting:
                    //    break;
                    //case ExecutionStateEx.Started:
                    //    break;
                    //case ExecutionStateEx.Pausing:
                    //    break;
                    //case ExecutionStateEx.Paused:
                    //    break;
                    //case ExecutionStateEx.Unpausing:
                    //    break;
                    //case ExecutionStateEx.Stopping:
                    //    break;
                    //case ExecutionStateEx.Stopped:
                    //    break;
                    //case ExecutionStateEx.Finished:
                    //    break;
                    case ExecutionStateEx.Disposed:
                        throw new ObjectDisposedException(this.ToString());
                    //case ExecutionStateEx.WaitingToStart:
                    //    break;
                    //case ExecutionStateEx.Faulted:
                    //    break;
                    default:
                        break;
                }

                State = ExecutionStateEx.Initializing;
                foreach (var child in Children.OfType<IAccountParticipant>())
                {
                    child.Account = this.Account;
                }
                State = ExecutionStateEx.Ready;
                return true;
            }
            catch (Exception ex)
            {
                await OnFault(ex).ConfigureAwait(false);
                throw;
            }
        }

        public virtual bool CanStart
        {
            get { return true; }
        }

        public async Task StartAsync(CancellationToken cancellationToken = default )
        {
            if (this.IsStarted()) return;


            desiredExecutionState = ExecutionStateEx.Started; // REVIEW - have a single input/output

            if (!CanStart)
            {
                throw new Exception("!CanStart");
                //return; // TODO: Error?
            }

            StartOnMarketAvailable = true;

            // TODO: 
            //DesiredExecutionState = ExecutionStateEx.Started;

            bool started = false;
            if (Account != null)
            {
                await Account.AddAccountParticipant(this);

                if (Account.Started.Value)
                {
                    //await Task.Run(async () => await _Start());
                    await _Start().ConfigureAwait(false);
                    started = true;
                }
            }

            if(!started)
            {
                ExecutionStateFlags |= ExecutionStateFlags.WaitingToStart;
            }
        }

        private async Task _Start()
        {
            try
            {
                int retriesRemaining = 30;
                tryagain:
                switch (State)
                {
                    //case ExecutionStateEx.Unconfigured:
                    //    break;
                    //case ExecutionStateEx.InvalidConfiguration:
                    //    break;
                    case ExecutionStateEx.Unspecified:
                    //case ExecutionStateEx.Finished:
                    case ExecutionStateEx.Uninitialized:
                    case ExecutionStateEx.Stopped:
                        await Initialize().ConfigureAwait(false);
                        goto tryagain;
                    case ExecutionStateEx.Initializing:
                    case ExecutionStateEx.Stopping:
                        if (retriesRemaining-- > 0)
                        {
                            await Task.Delay(1000).ConfigureAwait(false);
                            goto tryagain;
                        }
                        else
                        {
                            throw new Exception("Cannot start, current state is Initializing.");
                        }
                    case ExecutionStateEx.Ready:
                        //case ExecutionStateEx.WaitingToStart:
                        break;
                    case ExecutionStateEx.Starting:
                    case ExecutionStateEx.Started:
                        return;
                    //case ExecutionStateEx.Pausing:
                    //    break;
                    //case ExecutionStateEx.Paused:
                    //    break;
                    //case ExecutionStateEx.Unpausing:
                    //    break;
                    case ExecutionStateEx.Disposed:
                        throw new ObjectDisposedException(this.GetType().Name);
                    default:
                        throw new Exception("Unsupported state for Start(): " + State);
                }

                SetState(ExecutionStateEx.Starting);
                this.OnEnteringState(LionFire.Execution.ExecutionStateEx.Starting);

                await OnStarting().ConfigureAwait(false);

                foreach (var child in Children.OfType<IStartable>())
                {
                    await child.StartAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                await OnFault(ex).ConfigureAwait(false);
                return;
            }

            SetState(ExecutionStateEx.Started);
        }

        protected async Task OnFault(Exception ex)
        {
            FaultException = ex;
            SetState(ExecutionStateEx.Faulted);

            try
            {
                // FUTURE: StoppingContext
                //var stopContext = new StoppingContext
                //{
                //    StopMode = StopMode.CriticalFailure,
                //    StopOptions = StopOptions.StopChildren,
                //};
                foreach (var child in Children.OfType<IStoppable>())
                {
                    //await child.Stop(StopMode.CriticalFailure, StopOptions.StopChildren).ConfigureAwait(false);

                    await child.StopAsync().ConfigureAwait(false);
                }
            }
            catch
            {
                // TODO EMPTYCATCH
            }
        }
        //public class StoppingContext
        //{
        //    public StopMode StopMode { get; set; }
        //    public StopOptions Stopoptions { get; set; }
        //}

        public Exception FaultException { get; set; }

        protected async Task DoStop(StopMode stopMode = StopMode.GracefulShutdown, StopOptions options = StopOptions.StopChildren)
        {
            DesiredExecutionState = ExecutionStateEx.Stopped;

            switch (State)
            {
                case ExecutionStateEx.Unspecified:
                case ExecutionStateEx.Uninitialized:
                case ExecutionStateEx.Faulted:
                case ExecutionStateEx.Stopped:
                //case ExecutionStateEx.Finished:
                case ExecutionStateEx.Disposed:
                    return;
                case ExecutionStateEx.Initializing:
                case ExecutionStateEx.Ready:
                case ExecutionStateEx.Starting:
                case ExecutionStateEx.Started:
                case ExecutionStateEx.Pausing:
                case ExecutionStateEx.Paused:
                case ExecutionStateEx.Unpausing:
                    break;
                case ExecutionStateEx.Stopping:
                    break;
                default:
                    throw new Exception();
            }

            SetState(ExecutionStateEx.Stopping);

            OnStopping();

            foreach (var child in Children.OfType<IStoppableEx>())
            {
                await child.Stop(stopMode, options).ConfigureAwait(false);
            }
            foreach (var child in Children.OfType<IStoppable>())
            {
                await child.StopAsync().ConfigureAwait(false);
            }

            SetState(ExecutionStateEx.Stopped);

            OnStopped();
        }
        protected virtual void OnStopping()
        {
            logger.LogInformation($"------- STOPPING {this} -------");
        }
        protected virtual void OnStopped()
        {
            logger.LogInformation($"------- STOP {this} -------");
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken = default)
        {
            await DoStop().ConfigureAwait(false);
        }

        protected bool StartOnMarketAvailable = false;

        protected async virtual Task OnMarketStarted()
        {
            if (StartOnMarketAvailable)
            {
                await StartAsync().ConfigureAwait(false);
            }
        }

        protected virtual Task OnStarting()
        {
            this.ValidateDependencies();
            return Task.CompletedTask;
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

        public ILogger Logger { get { return logger; } }
        protected ILogger logger;

        #endregion

    }



}

