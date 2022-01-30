#nullable enable
using LionFire.Assets;
using LionFire.Execution.Jobs;
using LionFire.Reactive;
using LionFire.Reactive.Subjects;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Trading.Workspaces;
using LionFire.Trading.Statistics;
using LionFire.DependencyInjection;
using LionFire.Dependencies;
using System.Threading;

namespace LionFire.Trading.Accounts
{

    public abstract class AccountBase<TTemplate> : FeedBase<TTemplate>, IAccount, ITemplateInstance, IFeed
        where TTemplate : TAccount
    {

        #region Identity

        public string Key => $"{BrokerName}:{AccountName}";

        /// <summary>
        /// Name of account, set and viewed by the user.  (For the Account Id used by the exchange, see AccountId.)
        /// </summary>
        public string? AccountName => Template?.AccountName;

        #endregion

        #region Relationships

        #region Template


        //TFeed IFeed.Template => Template;
        TFeed IFeedCTrader.Template => Template;
        string IAccount.BrokerName => Template?.BrokerName;
        string IAccount.AccountType => Template?.AccountType;

        TAccount IAccount.Template { get { return Template; } }

        #endregion

        public TradingContext? Context
        {
            get
            {
                return tradingContext ?? DependencyContext.Current.GetService<TradingContext>();
            }
            set { tradingContext = value; }
        }
        TradingContext? tradingContext;

        #region Workspace

#if TOPORT
        public TradingWorkspace Workspace
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

        private TradingWorkspace workspace;
#endif
        #endregion


        public Server Server
        {
            get { if (server == null) { server = new Server(this); } return server; }
        }
        private Server server;

        #endregion

        #region Informational Properties

        public string AccountType => "?"; // Netted / hedged, etc.

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

        //private void TWorkspace_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    switch (e.PropertyName)
        //    {
        //        case nameof(Workspace.Template.AllowSubscribeToTicks):
        //            OnAllowSubscribeToTicksChanged();
        //            break;
        //        default:
        //            break;
        //    }
        //}

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
        private bool isTradeApiEnabled = true;

        #endregion
        protected virtual void OnTradeApiEnabledChanging()
        {
        }
        protected virtual void OnTradeApiEnabledChanged()
        {
        }

        #endregion

        #region State

        public virtual Task RefreshState() { return Task.CompletedTask; }

        #region AllowSubscribeToTicks

#if TOPORT
        public override bool AllowSubscribeToTicks
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
#endif

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

        public IPositionsDouble Positions => positions;
        protected Positions positions = new Positions();
        public IPositions Positions2 => positions2;
        protected Positions2 positions2 = new();

        public abstract Task<IPositions> RefreshPositions(CancellationToken cancellationToken = default);


        public abstract double Equity { get; protected set; }
        public abstract double Balance { get; protected set; }
        public abstract decimal EquityDecimal { get; protected set; }
        public abstract decimal BalanceDecimal { get; protected set; }

        public string StatusText { get { return statusText; } protected set { statusText = value; StatusTextChanged?.Invoke(); } }
        private string statusText;
        public event Action StatusTextChanged;


        public double MarginUsed { get; set; } // Rename to margin to be consistent with cTrader?
        public double Margin => MarginUsed;
        public virtual double MarginLevel { get; protected set; }

        #endregion

        public virtual DateTime BacktestEndDate { get { return default(DateTime); } }

        #region Informational Properties

        public abstract bool IsBacktesting { get; }

        public abstract bool IsRealMoney { get; }

        #region From Template (Override to confirm with live server)

        /// <summary>
        /// Override this to confirm at runtime with server
        /// </summary>
        public virtual bool IsDemo => this.Template.IsDemo;

        public virtual bool IsLive => this.Template.IsLive;

        public virtual string AccountId => Template.AccountId;

        public virtual string Currency { get { return Template.Currency; } }

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

        #region Attached AccountParticipants

        protected override bool TryAddParticipant(object child)
        {
            if (base.TryAddParticipant(child)) return true;

            if (child is IAccountParticipant p)
            {
                AddAccountParticipant(p).Wait();
                return true;
            }
            return false;
        }

        public async Task AddAccountParticipant(IAccountParticipant actor)
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

        #region Events

        public virtual TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, double volume, string label = null, double? stopLossPips = default(double?), double? takeProfitPips = default(double?), double? marketRangePips = default(double?), string comment = null) => throw new NotSupportedException();

        public virtual TradeResult ExecuteMarketOrder(TradeType tradeType, string symbolCode, decimal volume, string? label = null, decimal? stopLossPrice = null, decimal? takeProfitPrice = null, decimal? marketRangePrice = null, string comment = null) => throw new NotSupportedException();

        public abstract TradeResult ClosePosition(PositionDouble position);
        public abstract TradeResult ModifyPosition(PositionDouble position, double? stopLoss, double? takeProfit);

        #endregion

        #region Per-Symbol Queries


        public double UnrealizedGrossProfit(string symbol)
        {
            double sum = 0.0;
            foreach (var position in Positions.Where(p => p.Symbol.Code == symbol))
            {
                sum += position.GrossProfit;
            }
            return sum;
        }

        public double UnrealizedNetProfit(string symbol)
        {
            double sum = 0.0;
            foreach (var position in Positions.Where(p => p.Symbol.Code == symbol))
            {
                sum += position.NetProfit;
            }
            return sum;
        }

        

        #endregion

    }
}
