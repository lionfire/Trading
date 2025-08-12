# Indicator Benchmarks

This directory contains comprehensive benchmark suites for all implemented technical indicators, comparing QuantConnect (QC) and Financial Python (FP) implementations.

## Implemented Benchmarks

### 1. RSI (Relative Strength Index) - `RsiBenchmark.cs`
**Tests:**
- Batch vs Streaming processing
- Different periods (14, 28, 50)
- Convergence speed measurement
- Oversold/Overbought detection (30/70 levels)
- Memory allocation comparison
- Accuracy validation between implementations

**Key Metrics:**
- Throughput (data points/second)
- Memory usage
- Convergence rate
- Signal detection accuracy

### 2. Bollinger Bands - `BollingerBandsBenchmark.cs`
**Tests:**
- Batch vs Streaming processing
- Different standard deviations (2.0, 2.5, 3.0)
- Band calculation speed (Upper, Middle, Lower)
- %B and BandWidth calculations
- Memory allocation comparison
- Three-band accuracy validation

**Key Metrics:**
- Band calculation performance
- Standard deviation computation efficiency
- Memory usage for sliding windows
- Accuracy of band calculations

### 3. Stochastic Oscillator - `StochasticBenchmark.cs`
**Tests:**
- HLC (High-Low-Close) data processing
- %K and %D calculation performance
- Different smoothing periods (Fast vs Slow)
- Crossover detection
- Oversold/Overbought levels (20/80)
- Memory allocation comparison

**Key Metrics:**
- Dual-line calculation efficiency
- Crossover detection speed
- Memory usage with HLC data
- Smoothing performance

### 4. MACD (Placeholder) - `MacdBenchmark.cs`
**Status:** Not yet implemented in the codebase
**Planned Tests:**
- MACD line (Fast EMA - Slow EMA)
- Signal line (EMA of MACD)
- Histogram (MACD - Signal)
- Standard (12,26,9) vs custom parameters
- Crossover detection
- Divergence analysis

## Benchmark Categories

Each benchmark is organized into categories for easy comparison:

- **Main Processing**: Batch vs Streaming performance
- **Convergence**: Speed to reach stable values
- **Signal Detection**: Overbought/Oversold, Crossovers
- **Accuracy**: Validation between implementations
- **Memory**: Allocation and usage patterns
- **Edge Cases**: Performance with volatile/extreme data

## Running Benchmarks

```bash
# Run all benchmarks
dotnet run -c Release

# Run specific indicator benchmarks
dotnet run -c Release -- --filter *RsiBenchmark*
dotnet run -c Release -- --filter *BollingerBandsBenchmark*
dotnet run -c Release -- --filter *StochasticBenchmark*

# Run specific categories
dotnet run -c Release -- --filter *Category=Accuracy*
dotnet run -c Release -- --filter *Category=Memory*
```

## Test Parameters

### Data Sizes
- 1,000 points (small dataset)
- 10,000 points (medium dataset)
- 100,000 points (large dataset)
- 1,000,000 points (stress test)

### Market Conditions
- **Trending**: Simulated trending market data
- **Sideways**: Range-bound market simulation
- **Volatile**: High volatility market conditions
- **Realistic**: Mixed market conditions

### Periods
- Short (14-20): Fast response, more noise
- Medium (28-50): Balanced response
- Long (100-200): Smooth, lagging response

## Performance Expectations

### RSI
- QC implementation: Optimized for accuracy
- FP implementation: Optimized for streaming
- Expected throughput: >1M points/sec for batch processing

### Bollinger Bands
- Three outputs per data point
- Higher memory usage due to sliding window
- Expected throughput: >500K points/sec

### Stochastic
- Two outputs (%K, %D) per data point
- Requires HLC data (3x input size)
- Expected throughput: >300K points/sec

## Memory Considerations

- **RSI**: Minimal memory, uses Wilder's smoothing
- **Bollinger Bands**: Requires sliding window for period
- **Stochastic**: Requires HLC data window
- **MACD**: Three EMA calculations in memory

## Validation Tolerance

All implementations are validated against each other with:
- Default tolerance: 0.01 (1%)
- Precision tolerance: 0.0001 for high-precision comparisons
- Null handling for warm-up periods