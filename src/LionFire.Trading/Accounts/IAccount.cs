using LionFire.Reactive;
using LionFire.Instantiating;
using LionFire.Trading.Accounts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Trading.Workspaces;
using LionFire.Trading.Statistics;
using System.Threading;

namespace LionFire.Trading
{
  
    public interface IAccount : IFeed
    //: ITemplateInstance<TAccount>
    {
        #region Identity

        public string Key { get; } // => $"{BrokerName}:{AccountId}";

        #endregion

        AccountStats AccountStats { get; }

        #region Relationships

        new TAccount Template { get; }

        #endregion

        #region From cTrader  - REVIEW

         bool IsLive { get; }

        string AccountId { get; }

        #endregion

        #region State

        double Equity { get; }
        double Balance { get; }
        decimal EquityDecimal { get; }
        decimal BalanceDecimal { get; }
        string Currency { get; }
        double Margin { get; }
        double MarginLevel { get; }

        IPositionsDouble Positions { get; }
        IPositions Positions2 { get; }
        Task<IPositions> RefreshPositions(CancellationToken cancellationToken = default);

        IPendingOrders PendingOrders { get; }

        PositionStats PositionStats { get; }

        string StatusText { get; }
        event Action StatusTextChanged;

        Task RefreshState();

        #endregion


        #region Informational Properties

        double StopOutLevel { get; }

        bool IsDemo { get; }

        bool IsBacktesting { get; }
        DateTime BacktestEndDate { get; }

        bool IsSimulation { get; }
        bool IsRealMoney { get; }

        string BrokerName { get; } // RENAME ExchangeName
        //string Platform { get; }
        //string AccountMode { get; }
        /// <summary>
        /// TODO: What should this be?  What does cTrader return?  Hedged/netting?
        /// </summary>
        string AccountType { get; }

        #endregion

        #region Methods

        TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, double volume, string label = null, double? stopLossPips = null, double? takeProfitPips = null, double? marketRangePips = null, string comment = null);
        TradeResult ExecuteMarketOrder(TradeType tradeType, string symbolCode, decimal volume, string? label = null, decimal? stopLossPrice = null, decimal? takeProfitPrice = null, decimal? marketRangePrice = null, string comment = null);

        TradeResult ClosePosition(PositionDouble position);
        TradeResult ModifyPosition(PositionDouble position, double? stopLoss, double? takeProfit);

        #endregion

#if !cAlgo
        Server Server { get; }
        Task AddAccountParticipant(IAccountParticipant indicator);
        bool IsTradeApiEnabled { get; set; }
#endif


        #region Symbol Query Methods

        double UnrealizedGrossProfit(string symbol);

        double UnrealizedNetProfit(string symbol);

        #endregion

    }

    


}
