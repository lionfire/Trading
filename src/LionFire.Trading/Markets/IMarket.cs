using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IMarket
    {
        List<IAccount> Accounts { get; }

        DateTime SimulationTime { get; }
        TimeZoneInfo TimeZone { get; }

        bool IsBacktesting { get; }
        
        bool IsSimulation { get; }
        bool IsRealMoney { get; }

        Symbol GetSymbol(string symbolCode);

        //MarketSeries GetSeries(Symbol symbol, TimeFrame timeFrame);

        //MarketSeries GetMarketSeries(string symbol, TimeFrame tf);
        //IEnumerable<string> SymbolsAvailable { get; }
        //IEnumerable<string> GetSymbolTimeFramesAvailable(string symbol);

        IObservable<bool> Started { get; }

        MarketData MarketData { get; set; }
        MarketDataProvider Data { get; }

        void Add(IMarketParticipant indicator);
    }
}
