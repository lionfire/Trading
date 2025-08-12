# Lorentzian Classification Indicator Benchmarks

This document describes the comprehensive benchmark suite for the Lorentzian Classification indicator, a machine learning-based market direction predictor using k-nearest neighbors with Lorentzian distance metric.

## Overview

The Lorentzian Classification benchmarks test performance across multiple dimensions:

- **Initialization Performance**: How quickly the indicator initializes with different parameter configurations
- **Single Update Performance**: Real-time processing latency for streaming data
- **Batch Processing Performance**: Bulk processing efficiency for backtesting scenarios
- **Feature Extraction Overhead**: Cost of extracting technical indicators (RSI, CCI, ADX, etc.)
- **k-NN Search Performance**: Efficiency of finding nearest neighbors with varying K values and lookback periods
- **Memory Usage Patterns**: Memory allocation and deallocation behavior
- **Numeric Type Comparisons**: Performance differences between double, float, and decimal arithmetic
- **Market Condition Adaptability**: Performance consistency across trending, sideways, and volatile markets

## Benchmark Categories

### 1. Initialization Benchmarks

Tests the cost of creating indicator instances with different configurations:

```csharp
[Benchmark]
[BenchmarkCategory("Initialization", "Double")]
public LorentzianClassification_FP<decimal, double> InitializeDouble()
```

**Parameters Tested:**
- K values: 4, 8, 16, 32
- Lookback periods: 50, 100, 500
- Normalization windows: 10, 20, 50
- Numeric types: double, float, decimal

**Key Metrics:**
- Initialization time (nanoseconds)
- Memory allocated during initialization
- Scaling behavior with parameter size

### 2. Single Update Performance

Measures real-time processing latency for streaming scenarios:

```csharp
[Benchmark]
[BenchmarkCategory("SingleUpdate", "Double")]
[OperationsPerInvoke(1)]
public void SingleUpdateDouble()
```

**Critical for:**
- Live trading systems with sub-millisecond requirements
- Real-time signal generation
- Latency-sensitive applications

**Performance Targets:**
- < 1ms per update for real-time trading
- Consistent latency (low standard deviation)
- Linear scaling with K value

### 3. Batch Processing Performance

Tests efficiency for backtesting and historical analysis:

```csharp
[Benchmark]
[BenchmarkCategory("BatchProcessing", "Small")]
[OperationsPerInvoke(1000)]
public (double signal, double confidence) BatchProcessingSmall()
```

**Test Sizes:**
- Small: 1,000 bars
- Medium: 10,000 bars  
- Large: 100,000 bars

**Optimization Focus:**
- Throughput (bars processed per second)
- Memory efficiency with large datasets
- Batch processing advantages vs. single updates

### 4. Feature Extraction Benchmarks

Measures the overhead of technical indicator calculations:

```csharp
[Benchmark]
[BenchmarkCategory("FeatureExtraction", "Overhead")]
public double[] FeatureExtractionOverhead()
```

**Features Measured:**
- RSI (Relative Strength Index)
- CCI (Commodity Channel Index) changes
- ADX (Average Directional Index) approximation
- Price returns and momentum
- Volatility calculations

### 5. k-NN Search Performance

Tests the core machine learning algorithm efficiency:

```csharp
[Benchmark]
[BenchmarkCategory("KNN", "SearchPerformance")]
public (double signal, double confidence) KNNSearchPerformance()
```

**Algorithm Details:**
- Lorentzian distance calculation: L(x,y) = Σ log(1 + |xi - yi|)
- K-nearest neighbor search and voting
- Feature normalization using rolling statistics
- Distance-based confidence scoring

### 6. Memory Usage Analysis

Tracks memory allocation patterns and efficiency:

```csharp
[Benchmark]
[BenchmarkCategory("Memory", "AllocationsPerUpdate")]
public long MemoryAllocationsPerUpdate()
```

**Memory Concerns:**
- Circular buffer efficiency for historical patterns
- Feature normalization window management
- Garbage collection pressure
- Memory leaks in long-running scenarios

### 7. Market Condition Performance

Tests performance consistency across different market types:

```csharp
[Benchmark]
[BenchmarkCategory("MarketCondition", "Trending")]
public (double signal, double confidence) TrendingMarketPerformance()
```

**Market Types:**
- **Trending**: Strong directional movement with momentum
- **Sideways**: Range-bound oscillating markets
- **Volatile**: High volatility with frequent direction changes

### 8. Usage Pattern Comparison

Compares different usage patterns for the indicator:

```csharp
[Benchmark]
[BenchmarkCategory("Usage", "StreamingSimulation")]
public int StreamingSimulation()
```

**Patterns:**
- **Streaming**: Single bar updates (real-time trading)
- **Backtesting**: Batch processing (historical analysis)

## Running the Benchmarks

### Prerequisites

- .NET 8.0 or later
- BenchmarkDotNet NuGet package
- Sufficient memory (recommended 8GB+ for large dataset tests)

### Basic Execution

```bash
cd /src/tp/Trading/benchmarks/LionFire.Trading.Indicators.Benchmarks
dotnet run -c Release -- --filter "*LorentzianClassification*"
```

### Specific Category Tests

```bash
# Test only initialization performance
dotnet run -c Release -- --filter "*LorentzianClassification*" --categories "Initialization"

# Test memory usage patterns
dotnet run -c Release -- --filter "*LorentzianClassification*" --categories "Memory"

# Test specific numeric type
dotnet run -c Release -- --filter "*LorentzianClassification*Double*"
```

### Advanced Options

```bash
# Generate detailed profiling data
dotnet run -c Release -- --filter "*LorentzianClassification*" --profiler ETW

# Export results to multiple formats
dotnet run -c Release -- --filter "*LorentzianClassification*" --exporters html,csv,json

# Custom iteration counts for longer tests
dotnet run -c Release -- --filter "*LorentzianClassification*" --iterationCount 20 --warmupCount 5
```

## Performance Expectations

### Target Performance Characteristics

| Operation | Target Latency | Acceptable Range | Notes |
|-----------|---------------|------------------|--------|
| Single Update | < 500μs | 100μs - 1ms | Real-time trading requirement |
| Initialization | < 10ms | 1ms - 50ms | One-time setup cost |
| Batch 10k bars | < 100ms | 50ms - 500ms | Backtesting efficiency |
| Feature Extraction | < 50μs | 10μs - 200μs | Per-bar technical indicator cost |
| k-NN Search (K=8) | < 100μs | 20μs - 500μs | Core ML algorithm |
| Memory per Pattern | < 1KB | 500B - 2KB | Historical pattern storage |

### Scaling Expectations

- **Linear scaling** with K value (number of neighbors)
- **Constant memory** usage regardless of data stream length (circular buffers)
- **Logarithmic scaling** with lookback period for initialization
- **Consistent performance** across different market conditions

## Interpreting Results

### Key Metrics to Monitor

1. **Mean Execution Time**: Primary performance indicator
2. **Standard Deviation**: Performance consistency (lower is better)
3. **Memory Allocated**: Garbage collection pressure
4. **Throughput**: Bars processed per second
5. **95th Percentile**: Worst-case performance under normal conditions

### Performance Regression Detection

Monitor these metrics for regressions:

```
Metric                    Baseline    Warning    Critical
Single Update Mean        200μs       400μs      800μs
Batch Processing (10k)    50ms        100ms      200ms
Memory per Update         100B        500B       1KB
Initialization Time       2ms         10ms       50ms
```

### Example Results Analysis

```
BenchmarkDotNet=v0.13.7, OS=Ubuntu 22.04.3 LTS
Intel Core i7-9700K CPU 3.60GHz, 1 CPU, 8 logical cores
.NET 8.0.0, X64 RyuJIT AVX2

| Method                    | K  | Lookback | DataSize | Mean      | StdDev    | Allocated |
|-------------------------- |----|----------|----------|-----------|-----------|-----------|
| SingleUpdateDouble        | 8  | 100      | 10000    | 243.2 μs  | 12.8 μs   | 432 B     |
| BatchProcessingMedium     | 8  | 100      | 10000    | 45.2 ms   | 2.1 ms    | 2.4 MB    |
| KNNSearchPerformance      | 16 | 500      | 10000    | 156.7 μs  | 8.9 μs    | 1.2 KB    |
```

## Optimization Guidelines

### For Real-Time Trading

1. **Use smaller K values** (4-8) for lower latency
2. **Prefer double over decimal** for numerical computations
3. **Set reasonable lookback periods** (50-200) to balance accuracy vs. performance
4. **Monitor memory allocation** to minimize GC pressure

### For Backtesting

1. **Use batch processing** for better throughput
2. **Larger K values** (16-32) acceptable for better accuracy
3. **Longer lookback periods** for more historical context
4. **Consider decimal precision** for financial accuracy

### Memory Optimization

1. **Tune normalization window** size based on available memory
2. **Monitor circular buffer efficiency**
3. **Use appropriate numeric types** (float vs. double vs. decimal)
4. **Implement proper disposal** patterns for long-running instances

## Advanced Analysis

### Custom Benchmarks

To add custom benchmarks for specific scenarios:

```csharp
[Benchmark]
[BenchmarkCategory("Custom", "MyScenario")]
public void MyCustomBenchmark()
{
    // Your specific test scenario
    var parameters = new PLorentzianClassification<decimal, double>
    {
        // Custom configuration
    };
    
    var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
    // Measure specific operations
}
```

### Profiling Integration

For detailed performance analysis:

```bash
# CPU profiling
dotnet run -c Release -- --filter "*LorentzianClassification*" --profiler ETW

# Memory profiling  
dotnet-trace collect --providers Microsoft-DotNETCore-SampleProfiler -- dotnet run -c Release

# Custom ETW events
dotnet run -c Release -- --filter "*LorentzianClassification*" --profiler ConcurrencyVisualizer
```

## Troubleshooting

### Common Issues

1. **OutOfMemoryException**: Reduce lookback period or data size
2. **Slow performance**: Check K value and normalization window
3. **Inconsistent results**: Ensure deterministic test data (use fixed seeds)
4. **Permission errors**: Run with appropriate file system permissions

### Diagnostic Commands

```bash
# Check memory usage
dotnet-counters monitor --process-id <pid> --counters System.Runtime

# Profile allocations
dotnet-dump collect -p <pid>

# Analyze GC pressure
dotnet-gcdump collect -p <pid>
```

## Contributing

When adding new benchmarks:

1. Follow the existing naming conventions
2. Add appropriate benchmark categories
3. Include documentation for new scenarios
4. Test with multiple parameter combinations
5. Verify results make sense and are reproducible

## See Also

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [Lorentzian Classification Implementation](/src/LionFire.Trading.Indicators/Native/LorentzianClassification_FP.cs)
- [Performance Test Suite](/tests/LionFire.Trading.Indicators.Tests/LorentzianClassificationPerformanceTests.cs)
- [Trading Indicators Architecture](/docs/architecture/TradingIndicators.md)