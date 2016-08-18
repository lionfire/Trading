using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IMarket
    {
        DateTime SimulationTime { get; }
        TimeZoneInfo TimeZone { get; }

        bool IsBacktesting { get; }
        
        bool IsSimulation { get; }
        bool IsRealMoney { get; }

        //IEnumerable<string> SymbolsAvailable { get; }
        //IEnumerable<string> GetSymbolTimeFramesAvailable(string symbol);

        MarketDataProvider Data { get; }

        //MarketSeries GetMarketSeries(string symbol, TimeFrame tf);

    }
}
