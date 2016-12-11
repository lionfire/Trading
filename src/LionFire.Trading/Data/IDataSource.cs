using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IDataSource
    {
        string SourceName { get; }
        MarketSeries GetMarketSeries(string symbol, TimeFrame timeFrame, DateTime? startDate = null, DateTime? endDate = null);
        MarketSeries GetMarketSeries(string key, DateTime? startDate = null, DateTime? endDate = null);
    }
    
}
