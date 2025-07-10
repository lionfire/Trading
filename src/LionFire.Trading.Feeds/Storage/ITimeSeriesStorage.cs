using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LionFire.Trading.Feeds.Models;

namespace LionFire.Trading.Feeds.Storage;

public interface ITimeSeriesStorage : IDisposable
{
    Task AppendAsync(MarketDataSnapshot snapshot);
    Task<IEnumerable<MarketDataSnapshot>> ReadRangeAsync(
        string symbol, 
        DateTime startTime, 
        DateTime endTime);
    Task FlushAsync();
    Task<long> GetSizeAsync();
}