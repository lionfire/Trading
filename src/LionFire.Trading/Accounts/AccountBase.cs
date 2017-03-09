using LionFire.Assets;
using LionFire.Execution.Jobs;
using LionFire.Reactive;
using LionFire.Reactive.Subjects;
using LionFire.Instantiating;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Trading.Workspaces;
using LionFire.Trading.Statistics;
using LionFire.DependencyInjection;

namespace LionFire.Trading.Accounts
{
    //public class TPaperAccount
    //{
    //}
    //public class PaperAccount : BacktestAccount

    public abstract class AccountBase<TTemplate> : ITemplateInstance<TTemplate>, IHierarchicalTemplateInstance, IAccount, INotifyPropertyChanged
        where TTemplate : TAccount
    {

        #region Relationships

        #region Template

        public TTemplate Template { get; set; }

        ITemplate ITemplateInstance.Template { get { return Template; } set { Template = (TTemplate)value; } }
        TAccount IAccount.Template { get { return Template; } }

        #endregion

        public TradingContext Context
        {
            get
            {
                return InjectionContext.Current.GetService<TradingContext>();
                //return tradingContext ?? Defaults.Get<TradingContext>();
            }
            set { tradingContext = value; }
        }
        TradingContext tradingContext;


        #region Workspace

        public Workspace Workspace
        {
            get { return workspace; }
            set
            {
                if (workspace == value) return;
                workspace = value;
                if (workspace != null)
                {
                    if (workspace.Template == null)
                        {
                        throw new ArgumentException("Workspace must have Template");
                    }
                    workspace.Template.PropertyChanged += TWorkspace_PropertyChanged;
                }
            }
        }

        private Workspace workspace;

        #endregion


        public Server Server
        {
            get { if (server == null) { server = new Server(this); } return server; }
        }
        private Server server;

        #endregion

        #region Lifecycle

        public AccountBase()
        {
            //if (Context == null)
            //{
            //    throw new Exception("Cannot create AccountBase before TradingContext is available.");
            //}
            //Context.Options.PropertyChanged += Options_PropertyChanged;
        }

        #endregion

        private void TWorkspace_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Workspace.Template.AllowSubscribeToTicks):
                    OnAllowSubscribeToTicksChanged();
                    break;
                default:
                    break;
            }
        }
        
        #region Lifecycle State

        #region IsTradeApiEnabled

        /// <summary>
        /// Get live updates
        /// </summary>
        public bool IsTradeApiEnabled
        {
            get { return isTradeApiEnabled; }
            set
            {
                if (isTradeApiEnabled == value) return;
                OnTradeApiEnabledChanging();
                isTradeApiEnabled = value;
                OnTradeApiEnabledChanged();
                OnPropertyChanged(nameof(IsTradeApiEnabled));
            }
        }
        private bool isTradeApiEnabled = false;

        #endregion
        protected virtual void OnTradeApiEnabledChanging()
        {
        }
        protected virtual void OnTradeApiEnabledChanged()
        {
        }

        #endregion

        #region State

        #region AllowSubscribeToTicks

        public virtual bool AllowSubscribeToTicks
        {
            get
            {
                if (Workspace?.Template != null && !Workspace.Template.AllowSubscribeToTicks) return false;
                return allowSubscribeToTicks;
            }
            set
            {
                if (allowSubscribeToTicks == value) return;
                allowSubscribeToTicks = value;
                OnAllowSubscribeToTicksChanged();
            }
        }
        private bool allowSubscribeToTicks;
        protected virtual void OnAllowSubscribeToTicksChanged() { }

        #endregion

        public AccountStats AccountStats { get; } = new AccountStats();
        public PositionStats PositionStats
        {
            get
            {
                if (positionStats == null)
                {
                    positionStats = new PositionStats(this);
                }
                return positionStats;
            }
            protected set
            {
                positionStats = value;
            }
        }
        private PositionStats positionStats;

        public IPendingOrders PendingOrders
        {
            get
            {
                if (pendingOrders == null)
                {
                    pendingOrders = new PendingOrders();
                }
                return pendingOrders;
            }
            protected set
            {
                pendingOrders = value;
            }
        }
        private IPendingOrders pendingOrders;

        public IPositions Positions
        {
            get { return positions; }
        }
        protected Positions positions = new Positions();

        public virtual bool TicksAvailable { get { return true; } }

        public abstract double Equity { get; protected set; }
        public abstract double Balance { get; protected set; }

        public string StatusText { get { return statusText; } protected set { statusText = value; StatusTextChanged?.Invoke(); } }
        private string statusText;
        public event Action StatusTextChanged;


        public double MarginUsed { get; set; }

        #endregion

        public virtual DateTime BacktestEndDate { get { return default(DateTime); } }

        #region Server State

        public abstract DateTime ServerTime { get; protected set; }
        public abstract DateTime ExtrapolatedServerTime { get; }
        public virtual TimeZoneInfo TimeZone
        {
            get
            {
                return TimeZoneInfo.Utc;
            }
        }

        #endregion

        #region Informational Properties

        public abstract bool IsBacktesting { get; }
        public abstract bool IsSimulation { get; }
        public abstract bool IsRealMoney { get; }

        #region From Template

        /// <summary>
        /// Override this to confirm at runtime with server
        /// </summary>
        public virtual bool IsDemo { get { return Template.IsDemo; } }

        public virtual string Currency { get { return Template.Currency; } }
        public string BrokerName { get { return Template.BrokerName; } }

        public double StopOutLevel
        {
            get
            {
                if (double.IsNaN(stopOutLevel)) { stopOutLevel = Template.StopOutLevel; }
                return stopOutLevel;
            }
            protected set { stopOutLevel = value; }
        }
        protected double stopOutLevel = double.NaN;

        #endregion

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

        protected ConcurrentDictionary<string, Symbol> symbols = new ConcurrentDictionary<string, Symbol>();

        public Symbol GetSymbol(string symbolCode)
        {
            return symbols.GetOrAdd(symbolCode, code => CreateSymbol(code));
        }

        protected abstract Symbol CreateSymbol(string symbolCode);

        #endregion

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

        protected virtual void Subscribe(string symbolCode, string timeFrame)
        {
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
            AccountBase<TTemplate> market;
            private ConcurrentDictionary<string, int> Dict { get { return market.subscriptions; } }
            private string Key;
            public SubscriptionDecrementer(string key, AccountBase<TTemplate> market)
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

        #region Attached AccountParticipants

        void IHierarchicalTemplateInstance.Add(object child)
        {
            Add((IAccountParticipant)child).Wait();
        }

        public async Task Add(IAccountParticipant actor)
        {
            actor.Account = (IAccount)this;
            if (!participants.Contains(actor))
            {
                participants.Add(actor);
                var interested = actor as IInterestedInMarketData;
                if (interested != null)
                {
                    await interested.EnsureDataAvailable(ExtrapolatedServerTime).ConfigureAwait(false);
                }
            }
        }
        public IReadOnlyList<IAccountParticipant> Participants { get { return participants; } }
        List<IAccountParticipant> participants = new List<IAccountParticipant>();

        #endregion

        public virtual IHistoricalDataProvider HistoricalDataProvider { get { return null; } }

        protected virtual async Task OnStarting()
        {
            await EnsureParticipantsHaveDesiredData().ConfigureAwait(false);
        }

        protected async virtual Task EnsureParticipantsHaveDesiredData()
        {
            var time = ExtrapolatedServerTime;
            if (time == default(DateTime))
            {
                time = DateTime.UtcNow;
            }

            foreach (var p in participants.OfType<IInterestedInMarketData>())
            {
                await p.EnsureDataAvailable(time).ConfigureAwait(false);
            }
        }

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

        #region Events

        public IBehaviorObservable<bool> Started { get { return started; } }
        protected BehaviorObservable<bool> started = new BehaviorObservable<bool>(false);

        public event Action Ticked;

        /// <summary>
        /// Backtesting: called once for each time step, if there was at least one tick or bar
        /// </summary>
        protected virtual void RaiseTicked() { Ticked?.Invoke(); }

        public abstract TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, long volume, string label = null, double? stopLossPips = default(double?), double? takeProfitPips = default(double?), double? marketRangePips = default(double?), string comment = null);
        public abstract TradeResult ClosePosition(Position position);
        public abstract TradeResult ModifyPosition(Position position, double? stopLoss, double? takeProfit);

        #endregion

        public abstract IEnumerable<string> SymbolsAvailable { get; }

        public void TryAdd(Session session)
        {

        }


        #region Misc


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        #region Logger

        public ILogger Logger
        {
            get { return logger; }
        }


        protected ILogger logger;

        #endregion

        #endregion
    }
}
