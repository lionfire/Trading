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

namespace LionFire.Trading
{
  
    public interface IAccount : IFeed
    //: ITemplateInstance<TAccount>
    {
        AccountStats AccountStats { get; }

        #region Relationships

        new TAccount Template { get; }

        #endregion

        #region State

        double Equity { get; }
        double Balance { get; }
        string Currency { get; }
        double Margin { get; }
        double MarginLevel { get; }

        IPositions Positions { get; }

        IPendingOrders PendingOrders { get; }

        PositionStats PositionStats { get; }

        string StatusText { get; }
        event Action StatusTextChanged;

        #endregion


        #region Informational Properties

        double StopOutLevel { get; }

        bool IsDemo { get; }

        bool IsBacktesting { get; }
        DateTime BacktestEndDate { get; }

        bool IsSimulation { get; }
        bool IsRealMoney { get; }

        string BrokerName { get; }
        //string Platform { get; }
        //string AccountMode { get; }
        /// <summary>
        /// TODO: What should this be?  What does cTrader return?  Hedged/netting?
        /// </summary>
        string AccountType { get; }

        #endregion

        #region Methods

        TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, long volume, string label = null, double? stopLossPips = null, double? takeProfitPips = null, double? marketRangePips = null, string comment = null);

        TradeResult ClosePosition(Position position);
        TradeResult ModifyPosition(Position position, double? stopLoss, double? takeProfit);

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
