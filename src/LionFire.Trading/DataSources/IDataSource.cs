using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IDataSource
    {
        string SourceName { get; }
        IMarketSeries GetMarketSeries(string symbol, TimeFrame timeFrame, DateTime? startDate = null, DateTime? endDate = null);
        IMarketSeries GetMarketSeries(string key, DateTime? startDate = null, DateTime? endDate = null);
    }
    
}
