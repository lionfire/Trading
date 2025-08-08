# CachedMarketDataResolver Enhancement

## Overview
Smart memory-managed caching layer for IMarketDataResolver to prevent memory exhaustion and improve performance.

## Phase 1: Smart Memory-Managed Cache for IMarketDataResolver

### 1. Cached Market Data Resolver Interface
```csharp
public interface ICachedMarketDataResolver : IMarketDataResolver
{
    // Cache management
    void ConfigureCachePolicy(MarketDataCachePolicy policy);
    MarketDataCacheStatistics GetCacheStatistics();
    void ClearCache(string? pattern = null);
}

public class CachedMarketDataResolver : IMarketDataResolver, ICachedMarketDataResolver
{
    private readonly IMarketDataResolver _inner;
    private readonly IMemoryCache _cache;
    private readonly MemoryPressureMonitor _memoryMonitor;
    
    public IHistoricalTimeSeries? TryResolve(object reference, SlotInfo? slotInfo = null, Type? valueType = null)
    {
        var cacheKey = GenerateCacheKey(reference, slotInfo, valueType);
        
        if (_cache.TryGetValue<IHistoricalTimeSeries>(cacheKey, out var cached))
        {
            return cached;
        }
        
        var result = _inner.TryResolve(reference, slotInfo, valueType);
        
        if (result != null)
        {
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = _policy.DefaultTTL,
                Size = EstimateSize(result),
                Priority = DeterminePriority(reference)
            };
            
            _cache.Set(cacheKey, result, options);
        }
        
        return result;
    }
}
```

### 2. Advanced Cache Eviction Policies
```csharp
public class MarketDataCachePolicy
{
    public TimeSpan DefaultTTL { get; set; } = TimeSpan.FromMinutes(10);
    public int MaxCacheEntries { get; set; } = 1000;
    public long MaxMemoryBytes { get; set; } = 500_000_000; // 500MB
    public EvictionStrategy Strategy { get; set; } = EvictionStrategy.LRUWithMemoryPressure;
    
    // Per-data-type TTL overrides
    public Dictionary<Type, TimeSpan> TypeSpecificTTL { get; set; } = new()
    {
        [typeof(IKline<>)] = TimeSpan.FromMinutes(15),
        [typeof(Tick<>)] = TimeSpan.FromMinutes(5),
        [typeof(OrderBook<>)] = TimeSpan.FromMinutes(1)
    };
}

public enum EvictionStrategy
{
    LRU,                    // Least Recently Used
    LFU,                    // Least Frequently Used  
    TTLBased,               // Time To Live
    MemoryPressure,         // GC pressure based
    LRUWithMemoryPressure   // Hybrid approach
}
```

### 3. Memory Pressure Monitoring
```csharp
public class MemoryPressureMonitor
{
    private readonly Timer _monitorTimer;
    private MemoryPressureLevel _currentLevel = MemoryPressureLevel.Low;
    
    public event Action<MemoryPressureLevel> MemoryPressureChanged;
    
    public MemoryPressureMonitor()
    {
        _monitorTimer = new Timer(CheckMemoryPressure, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }
    
    private void CheckMemoryPressure(object state)
    {
        var info = GC.GetMemoryInfo();
        var usedMemoryRatio = (double)info.HeapSizeBytes / info.TotalAvailableMemoryBytes;
        
        var newLevel = usedMemoryRatio switch
        {
            < 0.5 => MemoryPressureLevel.Low,
            < 0.7 => MemoryPressureLevel.Medium,
            < 0.85 => MemoryPressureLevel.High,
            _ => MemoryPressureLevel.Critical
        };
        
        if (newLevel != _currentLevel)
        {
            _currentLevel = newLevel;
            MemoryPressureChanged?.Invoke(newLevel);
        }
    }
}

public enum MemoryPressureLevel { Low, Medium, High, Critical }
```

### 4. Cache Statistics and Monitoring
```csharp
public class MarketDataCacheStatistics
{
    public long TotalRequests { get; set; }
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    public double HitRate => TotalRequests > 0 ? (double)CacheHits / TotalRequests : 0;
    
    public long CurrentEntryCount { get; set; }
    public long EstimatedMemoryBytes { get; set; }
    public long EvictionCount { get; set; }
    
    public Dictionary<string, long> HitsByDataType { get; set; } = new();
    public Dictionary<MemoryPressureLevel, long> EvictionsByPressureLevel { get; set; } = new();
}
```

## Implementation Details

### Memory Size Estimation
```csharp
private long EstimateSize(IHistoricalTimeSeries series)
{
    // Base object overhead
    long size = 24; 
    
    if (series is IHistoricalTimeSeries<IKline<decimal>> klineSeries)
    {
        // Estimate based on typical bar size and lookback
        // Each bar: ~8 fields * 8 bytes = 64 bytes
        var estimatedBars = 1000; // Default estimate
        size += estimatedBars * 64;
    }
    else if (series is IHistoricalTimeSeries<Tick<decimal>> tickSeries)
    {
        // Ticks are smaller but more numerous
        size += 10000 * 24; // 10k ticks * 24 bytes
    }
    
    return size;
}
```

### Eviction Strategy Implementation
```csharp
private void ConfigureEvictionCallbacks()
{
    _memoryMonitor.MemoryPressureChanged += level =>
    {
        switch (level)
        {
            case MemoryPressureLevel.High:
                // Remove 25% of least recently used entries
                EvictPercentage(0.25, EvictionReason.MemoryPressure);
                break;
                
            case MemoryPressureLevel.Critical:
                // Aggressive eviction - keep only most recent 10%
                EvictPercentage(0.90, EvictionReason.CriticalMemoryPressure);
                break;
        }
    };
}
```

## Integration with BatchHarness

Replace the existing IMarketDataResolver injection with ICachedMarketDataResolver:

```csharp
// In BatchHarness constructor or initialization
var innerResolver = ServiceProvider.GetRequiredService<IMarketDataResolver>();
var cachePolicy = new MarketDataCachePolicy
{
    MaxMemoryBytes = 1_000_000_000, // 1GB for backtesting
    Strategy = EvictionStrategy.LRUWithMemoryPressure
};

var cachedResolver = new CachedMarketDataResolver(innerResolver, cachePolicy);
// Use cachedResolver instead of direct IMarketDataResolver
```

## Benefits

1. **Memory Protection**: Prevents out-of-memory errors during large backtests
2. **Performance**: Reduces redundant data loading for frequently accessed symbols
3. **Flexibility**: Configurable policies per deployment scenario
4. **Observability**: Built-in statistics for monitoring cache effectiveness
5. **Graceful Degradation**: Handles memory pressure without crashing