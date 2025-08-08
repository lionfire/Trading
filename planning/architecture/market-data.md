# Market Data Architecture - Phase 3 Advanced Features

## Overview
Advanced features for the market data system, to be implemented after core caching and participant framework.

## Phase 3: Registry and Advanced Features

### 1. Market Data Input Registry
```csharp
public interface IMarketDataInputRegistry<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    // Registration of custom input providers
    void RegisterInputProvider<TData>(string inputKey, Func<ExchangeSymbolTimeFrame, int, IHistoricalTimeSeries<TData>> provider);
    
    // Batch operations for efficiency
    void HydrateMarketParticipant(IMarketParticipant<TPrecision> participant);
    Task PreloadDataAsync(IEnumerable<ExchangeSymbolTimeFrame> symbols, CancellationToken cancellation = default);
    
    // Discovery
    IEnumerable<string> GetRegisteredInputKeys();
    bool IsInputKeyRegistered(string inputKey);
}

public class MarketDataInputRegistry<TPrecision> : IMarketDataInputRegistry<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    private readonly Dictionary<string, object> _providers = new();
    private readonly ICachedMarketDataResolver _resolver;
    
    public void RegisterInputProvider<TData>(string inputKey, 
        Func<ExchangeSymbolTimeFrame, int, IHistoricalTimeSeries<TData>> provider)
    {
        _providers[inputKey] = provider;
    }
    
    public async Task PreloadDataAsync(IEnumerable<ExchangeSymbolTimeFrame> symbols, 
        CancellationToken cancellation = default)
    {
        // Parallel preloading with cancellation support
        var tasks = symbols.Select(async symbol =>
        {
            foreach (var provider in _providers.Values)
            {
                cancellation.ThrowIfCancellationRequested();
                // Trigger cache population
                await Task.Run(() => provider.DynamicInvoke(symbol, 1), cancellation);
            }
        });
        
        await Task.WhenAll(tasks);
    }
}
```

### 2. Extended IMarketDataResolver with Additional Data Types
```csharp
public static class IMarketDataResolverExtensions
{
    // Convenience method for tick data
    public static IHistoricalTimeSeries<Tick<TPrecision>> ResolveTicks<TPrecision>(
        this IMarketDataResolver resolver, 
        ExchangeSymbol symbol, 
        TimeSpan lookback)
        where TPrecision : struct, INumber<TPrecision>
    {
        var tickReference = new TickDataReference<TPrecision>(symbol, lookback);
        return resolver.Resolve<Tick<TPrecision>>(tickReference);
    }
    
    // Convenience method for order book data
    public static IHistoricalTimeSeries<OrderBook<TPrecision>> ResolveOrderBook<TPrecision>(
        this IMarketDataResolver resolver, 
        ExchangeSymbol symbol,
        int depth = 10)
        where TPrecision : struct, INumber<TPrecision>
    {
        var orderBookReference = new OrderBookReference<TPrecision>(symbol, depth);
        return resolver.Resolve<OrderBook<TPrecision>>(orderBookReference);
    }
    
    // Convenience method for aggregated trade data
    public static IHistoricalTimeSeries<AggregatedTrades<TPrecision>> ResolveAggTrades<TPrecision>(
        this IMarketDataResolver resolver,
        ExchangeSymbol symbol,
        TimeSpan aggregationPeriod)
        where TPrecision : struct, INumber<TPrecision>
    {
        var aggTradesReference = new AggregatedTradesReference<TPrecision>(symbol, aggregationPeriod);
        return resolver.Resolve<AggregatedTrades<TPrecision>>(aggTradesReference);
    }
}
```

### 3. Streaming Data Support
```csharp
public interface IStreamingMarketDataResolver : IMarketDataResolver
{
    IAsyncEnumerable<TValue> StreamAsync<TValue>(
        object reference,
        CancellationToken cancellation = default);
}

public static class StreamingMarketDataExtensions
{
    public static async IAsyncEnumerable<HLC<TPrecision>> StreamBarsAsync<TPrecision>(
        this IStreamingMarketDataResolver resolver,
        ExchangeSymbolTimeFrame symbolTimeFrame,
        [EnumeratorCancellation] CancellationToken cancellation = default)
        where TPrecision : struct, INumber<TPrecision>
    {
        await foreach (var bar in resolver.StreamAsync<HLC<TPrecision>>(symbolTimeFrame, cancellation))
        {
            yield return bar;
        }
    }
}
```

### 4. Market Data References for New Types
```csharp
public record TickDataReference<TPrecision>(
    ExchangeSymbol Symbol,
    TimeSpan Lookback) : IPInput
    where TPrecision : struct, INumber<TPrecision>
{
    public string Key => $"{Symbol.Key}/ticks#{Lookback.TotalSeconds}s";
    public Type ValueType => typeof(Tick<TPrecision>);
}

public record OrderBookReference<TPrecision>(
    ExchangeSymbol Symbol,
    int Depth = 10) : IPInput
    where TPrecision : struct, INumber<TPrecision>
{
    public string Key => $"{Symbol.Key}/orderbook#{Depth}";
    public Type ValueType => typeof(OrderBook<TPrecision>);
}

public record AggregatedTradesReference<TPrecision>(
    ExchangeSymbol Symbol,
    TimeSpan AggregationPeriod) : IPInput
    where TPrecision : struct, INumber<TPrecision>
{
    public string Key => $"{Symbol.Key}/aggtrades#{AggregationPeriod.TotalSeconds}s";
    public Type ValueType => typeof(AggregatedTrades<TPrecision>);
}
```

### 5. Market Data Transformations
```csharp
public interface IMarketDataTransformer<TInput, TOutput>
{
    IHistoricalTimeSeries<TOutput> Transform(IHistoricalTimeSeries<TInput> input);
}

public class VolumeProfileTransformer<TPrecision> 
    : IMarketDataTransformer<IKline<TPrecision>, VolumeProfile<TPrecision>>
    where TPrecision : struct, INumber<TPrecision>
{
    private readonly int _priceLevels;
    
    public VolumeProfileTransformer(int priceLevels = 50)
    {
        _priceLevels = priceLevels;
    }
    
    public IHistoricalTimeSeries<VolumeProfile<TPrecision>> Transform(
        IHistoricalTimeSeries<IKline<TPrecision>> input)
    {
        // Implementation that creates volume profile from bars
        return new TransformedTimeSeries<IKline<TPrecision>, VolumeProfile<TPrecision>>(
            input, 
            bars => CalculateVolumeProfile(bars, _priceLevels));
    }
}
```

### 6. Market Data Quality and Validation
```csharp
public interface IMarketDataValidator<TValue>
{
    ValidationResult Validate(TValue data);
    IEnumerable<TValue> CleanData(IEnumerable<TValue> data);
}

public class BarDataValidator<TPrecision> : IMarketDataValidator<IKline<TPrecision>>
    where TPrecision : struct, INumber<TPrecision>
{
    public ValidationResult Validate(IKline<TPrecision> bar)
    {
        var errors = new List<string>();
        
        if (bar.High.CompareTo(bar.Low) < 0)
            errors.Add("High is less than Low");
            
        if (bar.Open.CompareTo(bar.High) > 0 || bar.Open.CompareTo(bar.Low) < 0)
            errors.Add("Open is outside High/Low range");
            
        if (bar.Close.CompareTo(bar.High) > 0 || bar.Close.CompareTo(bar.Low) < 0)
            errors.Add("Close is outside High/Low range");
            
        return new ValidationResult(errors.Count == 0, errors);
    }
}
```

## Future Considerations

### Performance Optimizations
- Column-oriented storage for time series data
- SIMD operations for bulk calculations
- Memory-mapped files for large datasets
- GPU acceleration for complex transformations

### Integration Points
- gRPC streaming for real-time data
- Apache Arrow for efficient data interchange
- Parquet files for historical data storage
- Redis for distributed caching

### Advanced Analytics
- Built-in technical indicators as data transformations
- Market microstructure analytics
- Cross-asset correlation calculations
- Real-time anomaly detection