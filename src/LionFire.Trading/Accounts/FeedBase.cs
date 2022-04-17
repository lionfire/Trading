#nullable enable
using LionFire.Collections.Concurrent;
using LionFire.Execution.Jobs;
using LionFire.Instantiating;
using LionFire.Reactive;
using LionFire.Reactive.Subjects;
using LionFire.Structures;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Accounts
{
    public abstract class FeedBase<TTemplate> : ITemplateInstance<TTemplate>, IHierarchicalTemplateInstance, INotifyPropertyChanged, IFeed
        , IAsyncSubscribable
        where TTemplate : TFeed
    {

        #region Construction

        public FeedBase(ILogger logger)
        {
            this.Logger = logger;
        }

        #endregion


        #region State

        public bool IsStarted { get; private set; }

        #endregion

        public virtual bool AllowSubscribeToTicks { get { return true; } set { } }

        public string BrokerName { get { return Template.BrokerName; } }

        #region Template

        //TFeed IFeed.Template => Template;
        TFeed IFeedCTrader.Template => Template;

        public TTemplate Template { get; set; }
        
        #endregion

        #region Children


        void IHierarchicalTemplateInstance.Add(object child)
        {
            if (TryAddParticipant(child)) return;

            if (child is IFeedParticipant p)
            {
                Add(p).Wait();
            }
        }
        protected virtual bool TryAddParticipant(object child)
        {
            return false;
        }

        public Task Add(IFeedParticipant actor)
        {
            actor.Account = this;
            if (!feedParticipants.Contains(actor))
            {
                feedParticipants.Add(actor);
                //var interested = actor as IInterestedInMarketData; // OLD
                //if (interested != null)
                //{
                //    await interested.EnsureDataAvailable(ExtrapolatedServerTime).ConfigureAwait(false);
                //}
            }
            return Task.CompletedTask;
        }
        public IReadOnlyList<IFeedParticipant> FeedParticipants { get { return feedParticipants; } }
        List<IFeedParticipant> feedParticipants = new List<IFeedParticipant>();

        #endregion

        public virtual IHistoricalDataProvider HistoricalDataProvider { get { return null; } }


        public abstract bool IsSimulation { get; }

        public virtual bool TicksAvailable { get { return true; } }

        #region Lifecycle

        protected virtual Task OnStarting()
        {
            //await EnsureParticipantsHaveDesiredData().ConfigureAwait(false); // OLD
            return Task.CompletedTask;
        }

        //protected async virtual Task EnsureParticipantsHaveDesiredData() // OLD
        //{
        //    var time = ExtrapolatedServerTime;
        //    if (time == default(DateTime))
        //    {
        //        time = DateTime.UtcNow;
        //    }

        //    //foreach (var p in feedParticipants.OfType<IInterestedInMarketData>()) // OLD
        //    //{
        //    //    await p.EnsureDataAvailable(time).ConfigureAwait(false);
        //    //}
        //}

        #endregion

        #region Market Data

        public JobQueue LoadDataJobQueue = new JobQueue() { MaxConcurrentJobs = 4 };

        public MarketDataProvider Data { get { if (data == null) { data = new MarketDataProvider(this); } return data; } set { data = value; } }
        private MarketDataProvider data;

        public MarketData MarketData
        {
            get
            {
                if (marketData == null)
                {
                    marketData = new MarketData { Account = (IAccount)this };
                }
                return marketData;
            }
            set { marketData = value; }
        }
        private MarketData marketData;

        #endregion

        #region Series

        protected ConcurrentDictionary<KeyValuePair<string, string>, MarketSeries> marketSeries = new ConcurrentDictionary<KeyValuePair<string, string>, MarketSeries>();
        public virtual MarketSeries GetSeries(Symbol symbol, TimeFrame timeFrame)
        {
            return (MarketSeries)GetMarketSeries(symbol.Code, timeFrame);
        }

        public abstract MarketSeries CreateMarketSeries(string symbol, TimeFrame timeFrame);

        #region MarketSeries

        protected IMarketSeriesInternal GetMarketSeriesInternal(string symbol, TimeFrame tf) // REVIEW 
        {
            return (IMarketSeriesInternal)GetMarketSeries(symbol, tf);
        }

        public MarketSeriesBase GetMarketSeriesBase(string symbol, TimeFrame tf)
        {
            if (tf == TimeFrame.t1)
            {
                return GetSymbol(symbol).MarketTickSeries;
            }
            else
            {
                return GetMarketSeries(symbol, tf);
            }
        }

        public MarketSeries GetMarketSeries(string symbol, TimeFrame tf)
        {
            if (symbol == null) return null;
            if (tf == TimeFrame.t1)
            {
                throw new ArgumentException("Use Symbol.MarketTickSeries for t1");
            }
            else
            {
                return GetSymbol(symbol).GetMarketSeries(tf);
            }
        }

        #endregion

        #endregion

        #region Symbols

        public abstract IEnumerable<string> SymbolsAvailable { get; }

        protected ConcurrentDictionary<string, Symbol> symbols = new ConcurrentDictionary<string, Symbol>();

        public Symbol GetSymbol(string symbolCode)
        {
            return symbols.GetOrAdd(symbolCode, code => CreateSymbol(code));
        }

        protected abstract Symbol CreateSymbol(string symbolCode);

        #region Symbol Subscriptions

        /// <summary>
        /// Subscribe to a symbol at a particular timeframe.  Failure to subscribe will result in a NotSubscribedException
        /// </summary>
        /// <param name="symbolCode"></param>
        /// <param name="timeFrame"></param>
        /// <returns></returns>
        public IDisposable Subscribe(string symbolCode, TimeFrame timeFrame)
        {
            var key = symbolCode + ";" + timeFrame.Name;
            var newValue = subscriptions.AddOrUpdate(key, 1, (_, val) => val + 1);
            OnSubscriptionChanged(key, newValue);
            return new SubscriptionDecrementer(symbolCode, this);
        }

        protected virtual Task Subscribe(string symbolCode, string timeFrame)
        {
            return Task.CompletedTask;
        }
        protected virtual void Unsubscribe(string symbolCode, string timeFrame)
        {
        }

        #region Private

        ConcurrentDictionary<string, int> subscriptions = new ConcurrentDictionary<string, int>();
        private object subscriptionsLock = new object();

        private void OnSubscriptionChanged(string key, int newValue)
        {
            var split = key.Split(';');
            if (newValue == 1)
            {
                Subscribe(split[0], split[1]);
            }
            else if (newValue == 0)
            {
                Unsubscribe(split[0], split[1]);
            }
        }

        private class SubscriptionDecrementer : IDisposable
        {
            FeedBase<TTemplate> market;
            private ConcurrentDictionary<string, int> Dict { get { return market.subscriptions; } }
            private string Key;
            public SubscriptionDecrementer(string key, FeedBase<TTemplate> market)
            {
                this.Key = key;
                this.market = market;
            }
            public void Dispose()
            {
                var newValue = Dict.AddOrUpdate(Key, 0, (key, val) => Math.Max(0, val - 1));
                this.market.OnSubscriptionChanged(Key, newValue);
            }
        }

        #endregion

        #endregion

        #endregion

        #region Time

        public abstract DateTime ExtrapolatedServerTime { get; }
        public virtual TimeZoneInfo TimeZone
        {
            get
            {
                return TimeZoneInfo.Utc;
            }
        }

        public abstract DateTime ServerTime { get; protected set; }

        #endregion


        #region Events

        public IBehaviorObservable<bool> Started { get { return started; } }
        protected BehaviorObservable<bool> started = new BehaviorObservable<bool>(false);

        public event Action Ticked
        {
            add
            {
                if (ticked == null)
                {
                    tickedSubscriber = this.SubscribeAsync();
                }
                ticked += value;
            }
            remove
            {
                ticked -= value;
                if (ticked == null)
                {
                    tickedSubscriber.Dispose();
                }
            }
        }
        private event Action ticked;
        IDisposable tickedSubscriber;

        /// <summary>
        /// Backtesting: called once for each time step, if there was at least one tick or bar
        /// </summary>
        protected virtual void RaiseTicked() { ticked?.Invoke(); }

        #endregion

        #region Start / Stop / Subscribe

        #region Subscribe

        /// <summary>
        /// As long as someone is subscribed, this will be Started
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IDisposable> SubscribeAsync(CancellationToken cancellationToken = default)
        {
            if (!IsStarted)
            {
                Logger.LogInformation("First subscriber subscribed -- starting.");
                await OnStartAsync(cancellationToken);
            }
            Disposable disposable = new Disposable();
            concurrentHashSet.Add(disposable.Guid);
            disposable.Disposed += Disposable_Disposed;
            return disposable;
        }
        

        private void Disposable_Disposed(Disposable obj)
        {
            concurrentHashSet.Remove(obj.Guid);
            if (concurrentHashSet.Count == 0)
            {
                Logger.LogInformation("Last subscriber unsubscribed -- stopping.");
                OnStopAsync();
            }
        }

        private ConcurrentHashSet<Guid> concurrentHashSet = new();

        #endregion

        #region Start/Stop wrapper for subscribe

        protected IDisposable? StartAsyncSubscription { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (StartAsyncSubscription == null)
            {
                StartAsyncSubscription = SubscribeAsync();
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            StartAsyncSubscription?.Dispose();
            StartAsyncSubscription = null;
            return Task.CompletedTask;
        }

        #endregion

        #region Start / Stop implementation

        protected virtual Task OnStartAsync(CancellationToken cancellationToken = default)
        {
            IsStarted = true;
            return Task.CompletedTask;
        }

        protected virtual Task OnStopAsync(CancellationToken cancellationToken = default)
        {
            IsStarted = false;
            return Task.CompletedTask;
        }

        #endregion

        #endregion

        #region Misc

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Logger

        public ILogger Logger { get; }
        
        #endregion

        #endregion
    }

    public class Disposable : IDisposable
    {
        public Guid Guid { get; } = Guid.NewGuid();
        public void Dispose() => Disposed?.Invoke(this);
        public event Action<Disposable>? Disposed;
    }
}
