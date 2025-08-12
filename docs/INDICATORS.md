# Trading Indicators Library

## Overview

The LionFire Trading Indicators Library provides a comprehensive collection of 35 technical analysis indicators designed for high-performance trading applications. The library offers multiple implementation approaches including first-party optimized implementations (FP), QuantConnect integrations (QC), and specialized optimizations for SIMD and GPU processing.

### Key Features

- **Multiple Implementation Strategies**: Choose from FP (First-Party), QC (QuantConnect), with intelligent auto-selection
- **Type-Safe Generics**: Full generic support for different numeric types (double, float, decimal)
- **High Performance**: Optimized algorithms with circular buffers and efficient memory management
- **Comprehensive Testing**: Extensive benchmark suite measuring performance and accuracy
- **Streaming & Batch Processing**: Support for both real-time streaming and historical batch processing
- **Observable Pattern**: Reactive extensions support for real-time updates
- **Auto-Selection Logic**: Smart implementation selection based on available libraries and performance characteristics

## Complete Indicator List

### Column Legend

- **FP**: First-Party implementation (native LionFire implementation)
- **QC**: QuantConnect implementation (wrapper around QuantConnect indicators)
- **Benchmarks**: Comprehensive performance benchmarks available
- **Auto-Select**: Intelligent implementation selection based on ImplementationHint.Auto or ImplementationHint.Optimized

### Trend Following Indicators

| Indicator | Description | FP | QC | Benchmarks | Auto-Select |
|-----------|-------------|----|----|------------|-------------|
| **SMA** | Simple Moving Average | ✅ | ❌ | ✅ | ✅ |
| **EMA** | Exponential Moving Average | ✅ | ❌ | ✅ | ✅ |
| **TEMA** | Triple Exponential Moving Average | ✅ | ❌ | ✅ | ✅ |
| **HullMovingAverage** | Hull Moving Average | ✅ | ✅ | ✅ | ❌ |
| **VWMA** | Volume Weighted Moving Average | ✅ | ✅ | ✅ | ❌ |
| **LinearRegression** | Linear Regression Indicator | ✅ | ✅ | ✅ | ❌ |

### Momentum Oscillators

| Indicator | Description | FP | QC | Benchmarks | Auto-Select |
|-----------|-------------|----|----|------------|-------------|
| **RSI** | Relative Strength Index | ✅ | ❌ | ✅ | ❌ |
| **MACD** | Moving Average Convergence Divergence | ✅ | ❌ | ✅ | ❌ |
| **Stochastic** | Stochastic Oscillator | ✅ | ❌ | ✅ | ❌ |
| **WilliamsR** | Williams %R | ✅ | ❌ | ✅ | ✅ |
| **ROC** | Rate of Change | ✅ | ❌ | ✅ | ❌ |
| **AwesomeOscillator** | Awesome Oscillator | ✅ | ✅ | ✅ | ❌ |
| **FisherTransform** | Fisher Transform | ✅ | ✅ | ✅ | ❌ |

### Volatility Indicators

| Indicator | Description | FP | QC | Benchmarks | Auto-Select |
|-----------|-------------|----|----|------------|-----------|
| **BollingerBands** | Bollinger Bands | ✅ | ❌ | ✅ | ❌ |
| **ATR** | Average True Range (via benchmarks) | ✅ | ❌ | ✅ | ❌ |
| **StandardDeviation** | Standard Deviation | ✅ | ✅ | ✅ | ❌ |
| **DonchianChannels** | Donchian Channels | ✅ | ❌ | ✅ | ❌ |
| **KeltnerChannels** | Keltner Channels | ✅ | ❌ | ✅ | ❌ |

### Volume Indicators

| Indicator | Description | FP | QC | Benchmarks | Auto-Select |
|-----------|-------------|----|----|------------|-----------|
| **OBV** | On Balance Volume | ✅ | ❌ | ✅ | ❌ |
| **AccumulationDistributionLine** | A/D Line | ✅ | ✅ | ✅ | ❌ |
| **ChaikinMoneyFlow** | Chaikin Money Flow | ✅ | ✅ | ✅ | ❌ |
| **MFI** | Money Flow Index | ✅ | ❌ | ✅ | ❌ |
| **VWAP** | Volume Weighted Average Price | ✅ | ❌ | ✅ | ❌ |
| **KlingerOscillator** | Klinger Oscillator | ✅ | ❌ | ✅ | ❌ |

### Support/Resistance Indicators

| Indicator | Description | FP | QC | Benchmarks | Auto-Select |
|-----------|-------------|----|----|------------|-----------|
| **PivotPoints** | Pivot Points | ✅ | ✅ | ✅ | ❌ |
| **Supertrend** | Supertrend | ✅ | ✅ | ✅ | ✅ |
| **ParabolicSAR** | Parabolic SAR | ✅ | ❌ | ✅ | ❌ |
| **ZigZag** | ZigZag | ✅ | ❌ | ❌ | ❌ |

### Directional Movement Indicators

| Indicator | Description | FP | QC | Benchmarks | Auto-Select |
|-----------|-------------|----|----|------------|-----------|
| **ADX** | Average Directional Index | ✅ | ❌ | ✅ | ❌ |
| **Aroon** | Aroon Indicator | ✅ | ❌ | ✅ | ❌ |

### Specialized Indicators

| Indicator | Description | FP | QC | Benchmarks | Auto-Select |
|-----------|-------------|----|----|------------|-----------|
| **CCI** | Commodity Channel Index | ✅ | ❌ | ✅ | ❌ |
| **IchimokuCloud** | Ichimoku Cloud | ✅ | ❌ | ✅ | ✅ |
| **ChoppinessIndex** | Choppiness Index | ✅ | ❌ | ❌ | ❌ |
| **FibonacciRetracement** | Fibonacci Retracement | ✅ | ❌ | ❌ | ❌ |
| **HeikinAshi** | Heikin Ashi Candles | ✅ | ❌ | ❌ | ❌ |
| **LorentzianClassification** | Lorentzian Classification ML | ✅ | ❌ | ✅ | ❌ |

## Implementation Coverage Summary

- **Total Indicators**: 35
- **FP Implementations**: 35 (100%)
- **QC Implementations**: 10 (29%)
- **With Benchmarks**: 30 (86%)
- **Auto-Selection Logic**: 5 (14% - SMA, EMA, TEMA, WilliamsR, Supertrend, IchimokuCloud)

## Getting Started

### Basic Usage

```csharp
using LionFire.Trading.Indicators;
using LionFire.Trading.Indicators.Parameters;

// Create SMA indicator with 20 period
var smaParams = new PSMA<double, double> { Period = 20 };
var sma = new SMA_FP<double, double>(smaParams);

// Process price data
var prices = new double[] { 100, 101, 99, 102, 98 };
sma.OnBarBatch(prices, null);

// Get current value when ready
if (sma.IsReady)
{
    var currentValue = sma.Value;
    Console.WriteLine($"SMA: {currentValue}");
}
```

### Advanced Usage with Streaming

```csharp
using System.Reactive.Linq;

// Create RSI indicator
var rsiParams = new PRSI<double, double> { Period = 14 };
var rsi = new RSI_FP<double, double>(rsiParams);

// Subscribe to updates
rsi.Subscribe(values => {
    if (values.Count > 0)
    {
        var latestRsi = values[^1];
        if (rsi.IsOverbought)
            Console.WriteLine($"OVERBOUGHT: RSI = {latestRsi}");
        else if (rsi.IsOversold)
            Console.WriteLine($"OVERSOLD: RSI = {latestRsi}");
    }
});

// Stream price updates
foreach (var price in GetLivePrices())
{
    rsi.OnNext(price);
}
```

### Multi-Output Indicators

```csharp
// MACD returns multiple outputs
var macdParams = new PMACD<double, double> { 
    FastPeriod = 12, 
    SlowPeriod = 26, 
    SignalPeriod = 9 
};
var macd = new MACD_FP<double, double>(macdParams);

// Process data and get multiple outputs
macd.OnBarBatch(prices, null);
if (macd.IsReady)
{
    var macdLine = macd.MACD;
    var signalLine = macd.Signal;
    var histogram = macd.Histogram;
    
    Console.WriteLine($"MACD: {macdLine}, Signal: {signalLine}, Histogram: {histogram}");
}
```

### Implementation Selection

```csharp
// Prefer QuantConnect implementation when available
var smaParams = new PSMA<double, double> 
{ 
    Period = 20,
    ImplementationHint = ImplementationHint.QuantConnect
};

// The factory will automatically choose the best available implementation
var sma = IndicatorFactory.Create(smaParams);
```

## Performance Considerations

### Choosing Implementations

1. **FP (First-Party)**: Best for most use cases, optimized for performance
2. **QC (QuantConnect)**: Use when you need compatibility with QuantConnect algorithms
3. **Optimized**: Use for high-frequency scenarios with large datasets

### Memory Management

- Indicators use circular buffers to maintain constant memory usage
- Clear indicators when no longer needed: `indicator.Clear()`
- Prefer batch processing over individual updates for better performance

### Threading

- Indicators are NOT thread-safe by default
- Use separate indicator instances per thread
- Consider using `ConcurrentIndicator<T>` wrapper for thread-safe scenarios

## Configuration Examples

### Typical RSI Setup
```csharp
var rsiConfig = new PRSI<double, double>
{
    Period = 14,              // Standard RSI period
    OverboughtLevel = 70,     // Overbought threshold
    OversoldLevel = 30,       // Oversold threshold
    ImplementationHint = ImplementationHint.FirstParty
};
```

### Bollinger Bands Configuration
```csharp
var bbConfig = new PBollingerBands<double, double>
{
    Period = 20,              // Moving average period
    StandardDeviations = 2.0, // Number of standard deviations
    MovingAverageType = MovingAverageType.Simple
};
```

### MACD Setup
```csharp
var macdConfig = new PMACD<double, double>
{
    FastPeriod = 12,    // Fast EMA period
    SlowPeriod = 26,    // Slow EMA period
    SignalPeriod = 9    // Signal line EMA period
};
```

## Next Steps

- See [INDICATOR_REFERENCE.md](INDICATOR_REFERENCE.md) for detailed specifications of each indicator
- See [BENCHMARKS.md](BENCHMARKS.md) for performance testing information
- Explore the `/Examples` directory for more complex usage scenarios
- Check the `/Benchmarks` directory to run performance tests on your hardware