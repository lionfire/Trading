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
  
    public interface IAccount
    //: ITemplateInstance<TAccount>
    {
        bool AllowSubscribeToTicks { get; set; }
        AccountStats AccountStats { get; }

        #region Relationships

        TAccount Template { get; }

        #endregion

        #region State

        double Equity { get; }
        double Balance { get; }
        string Currency { get; }

        IPositions Positions { get; }

        IPendingOrders PendingOrders { get; }

        PositionStats PositionStats { get; }

        string StatusText { get; }
        event Action StatusTextChanged;

        #region Server State

        DateTime ServerTime { get; }
        DateTime ExtrapolatedServerTime { get; }
        TimeZoneInfo TimeZone { get; }

        #endregion

        #endregion


        #region Informational Properties

        double StopOutLevel { get; }

        bool IsDemo { get; }



        bool IsBacktesting { get; }
        DateTime BacktestEndDate { get; }

        bool IsSimulation { get; }
        bool IsRealMoney { get; }

        bool TicksAvailable { get; }


        #endregion

        #region Market Series

        MarketSeries CreateMarketSeries(string symbol, TimeFrame timeFrame);

        #endregion

        #region Methods

        TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, long volume, string label = null, double? stopLossPips = null, double? takeProfitPips = null, double? marketRangePips = null, string comment = null);


        TradeResult ClosePosition(Position position);
        TradeResult ModifyPosition(Position position, double? stopLoss, double? takeProfit);

        #endregion

        #region Events

        event Action Ticked;

        IBehaviorObservable<bool> Started { get; }

        #endregion

#if !cAlgo
        Symbol GetSymbol(string symbolCode);
        void TryAdd(Session session);
        MarketSeries GetMarketSeries(string symbol, TimeFrame tf);
        MarketData MarketData { get; set; }
        MarketDataProvider Data { get; }
        Server Server { get; }

        Task Add(IAccountParticipant indicator);


        IHistoricalDataProvider HistoricalDataProvider { get; }

        bool IsTradeApiEnabled { get; set; }
#endif

        //MarketSeries GetSeries(Symbol symbol, TimeFrame timeFrame);
        IEnumerable<string> SymbolsAvailable { get; }
        //IEnumerable<string> GetSymbolTimeFramesAvailable(string symbol);

        #region Misc

        // TODO: Move to internal interface and use friend assemblies
        ILogger Logger { get; }

        #endregion

    }

    


}
