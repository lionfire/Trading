using LionFire.Assets;
using LionFire.Reactive;
using LionFire.Reactive.Subjects;
using LionFire.Templating;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Accounts
{
    public abstract class AccountBase<TTemplate> : ITemplateInstance<TTemplate>, IHierarchicalTemplateInstance, IAccount
        where TTemplate : TAccount
    {

        #region Relationships

        #region Template

        public TTemplate Template { get; set; }

        ITemplate ITemplateInstance.Template { get { return Template; } set { Template = (TTemplate)value; } }
        TAccount IAccount.Template { get { return Template; } }

        #endregion

        public ITradingContext Context
        {
            get { return tradingContext ?? Defaults.Get<TradingContext>(); }
            set { tradingContext = value; }
        }
        ITradingContext tradingContext;


        public Server Server
        {
            get { if (server == null) { server = new Server(this); } return server; }
        }
        private Server server;

        #endregion

        #region State

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

        #region Server State

        public abstract DateTime ServerTime { get; protected set; }
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


        public abstract Task<IMarketSeries> CreateMarketSeries(string symbol, TimeFrame timeFrame);

        #region MarketSeries

        protected IMarketSeriesInternal GetMarketSeriesInternal(string symbol, TimeFrame tf) // REVIEW 
        {
            return (IMarketSeriesInternal)GetMarketSeries(symbol, tf);
        }

        public IMarketSeries GetMarketSeries(string symbol, TimeFrame tf)
        {
            return GetSymbol(symbol).GetMarketSeries(tf);
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

        #region Attached MarketParticipants

        void IHierarchicalTemplateInstance.Add(object child)
        { Add((IMarketParticipant)child); }

        public void Add(IMarketParticipant actor)
        {
            if (!participants.Contains(actor))
            {
                participants.Add(actor);
            }
            actor.Account = (IAccount)this;
        }
        public IReadOnlyList<IMarketParticipant> Participants { get { return participants; } }
        List<IMarketParticipant> participants = new List<IMarketParticipant>();

        #endregion

        #region Market Data

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

        #region Misc

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
