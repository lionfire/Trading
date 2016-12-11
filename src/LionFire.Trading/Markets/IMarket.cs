//using LionFire.Reactive;
//using LionFire.Templating;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace LionFire.Trading
//{
//    // REVIEW: Move these to account

//    public interface IAccount : IHierarchicalTemplateInstance
//    {
//#if !cAlgo
//        IAccount Account { get;  }
//        //List<IAccount> Accounts { get; }
//        Symbol GetSymbol(string symbolCode);
//        IMarketSeries GetMarketSeries(string symbol, TimeFrame tf);
//        MarketData MarketData { get; set; }
//        MarketDataProvider Data { get; }
//        Server Server { get; }

//        void Add(IAccountParticipant indicator);

//        event Action Ticked;

//#endif

//        DateTime SimulationTime { get; }
//        TimeZoneInfo TimeZone { get; }

//        bool IsBacktesting { get; }
        
//        bool IsSimulation { get; }
//        bool IsRealMoney { get; }


//        bool TicksAvailable { get; }

//        //MarketSeries GetSeries(Symbol symbol, TimeFrame timeFrame);


//        //IEnumerable<string> SymbolsAvailable { get; }
//        //IEnumerable<string> GetSymbolTimeFramesAvailable(string symbol);

//        IBehaviorObservable<bool> Started { get; }


    
        
//    }
//}
