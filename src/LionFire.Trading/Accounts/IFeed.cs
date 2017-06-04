using LionFire.Reactive;
using LionFire.Trading.Accounts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace LionFire.Trading
{
    //public class TPaperAccount
    //{
    //}
    //public class PaperAccount : BacktestAccount

    public interface IFeed
    //: ITemplateInstance<TFeed>
    {
        bool AllowSubscribeToTicks { get; set; }

        TFeed Template { get; }

        #region Server State

        DateTime ServerTime { get; }
        DateTime ExtrapolatedServerTime { get; }
        TimeZoneInfo TimeZone { get; }

        #endregion

        bool TicksAvailable { get; }



        #region Events

        event Action Ticked;

        IBehaviorObservable<bool> Started { get; }

        #endregion


#if !cAlgo
        #region Market Series

        MarketSeries CreateMarketSeries(string symbol, TimeFrame timeFrame);

        #endregion

        Symbol GetSymbol(string symbolCode);
        //void TryAdd(Session session);
        MarketSeries GetMarketSeries(string symbol, TimeFrame tf);
        MarketData MarketData { get; set; }
        MarketDataProvider Data { get; }
        

        IHistoricalDataProvider HistoricalDataProvider { get; }
        
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
