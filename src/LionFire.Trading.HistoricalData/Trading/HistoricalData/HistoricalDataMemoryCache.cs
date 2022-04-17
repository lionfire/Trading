using Swordfish.NET.Collections;

namespace LionFire.Trading.HistoricalData;

public class HistoricalDataMemoryCache<T>
{
    // TODO: control expiry time?
    public WeakDictionary<HistoricalDataMemoryCacheKey, T[]> Dict = new();
}

