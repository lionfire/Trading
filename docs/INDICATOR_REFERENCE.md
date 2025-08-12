# Trading Indicators - Detailed Reference

This document provides comprehensive technical specifications for all 35 indicators in the LionFire Trading Indicators Library.

## Table of Contents

- [Trend Following Indicators](#trend-following-indicators)
- [Momentum Oscillators](#momentum-oscillators)
- [Volatility Indicators](#volatility-indicators)
- [Volume Indicators](#volume-indicators)
- [Support/Resistance Indicators](#supportresistance-indicators)
- [Directional Movement Indicators](#directional-movement-indicators)
- [Specialized Indicators](#specialized-indicators)

---

## Trend Following Indicators

### SMA (Simple Moving Average)

**Purpose**: Smooths price data by creating a constantly updated average price over a specified period.

**Formula**:
```
SMA = (P₁ + P₂ + ... + Pₙ) / n
```
Where P = Price, n = Period

**Parameters**:
- `Period` (int): Number of periods (default: 20, range: 1-65536)
- `ImplementationHint` (enum): Implementation preference (Auto/QuantConnect/FirstParty/Optimized)

**Outputs**:
- `Value` (TOutput): Current SMA value

**Use Cases**:
- Trend identification
- Support/resistance levels
- Entry/exit signals when price crosses SMA
- Base for other indicators

**Recommended Settings**:
- Short-term: 10-20 periods
- Medium-term: 50 periods
- Long-term: 200 periods

---

### EMA (Exponential Moving Average)

**Purpose**: Gives more weight to recent prices, making it more responsive than SMA to price changes.

**Formula**:
```
EMA = (Price × Multiplier) + (Previous EMA × (1 - Multiplier))
Multiplier = 2 / (Period + 1)
```

**Parameters**:
- `Period` (int): Number of periods (default: 12, range: 1-65536)
- `ImplementationHint` (enum): Implementation preference

**Outputs**:
- `Value` (TOutput): Current EMA value

**Use Cases**:
- Faster trend following than SMA
- MACD calculation component
- Dynamic support/resistance
- Crossover strategies

**Recommended Settings**:
- Fast EMA: 12 periods
- Slow EMA: 26 periods
- Signal line: 9 periods

---

### TEMA (Triple Exponential Moving Average)

**Purpose**: Reduces lag further than EMA by applying exponential smoothing three times.

**Formula**:
```
EMA₁ = EMA(Price, Period)
EMA₂ = EMA(EMA₁, Period)
EMA₃ = EMA(EMA₂, Period)
TEMA = (3 × EMA₁) - (3 × EMA₂) + EMA₃
```

**Parameters**:
- `Period` (int): Number of periods (default: 14, range: 1-65536)

**Outputs**:
- `Value` (TOutput): Current TEMA value

**Use Cases**:
- Ultra-responsive trend following
- Reduced whipsaws in volatile markets
- Short-term trading strategies

---

### Hull Moving Average

**Purpose**: Combines weighted moving average smoothness with low lag characteristics.

**Formula**:
```
HMA = WMA(2 × WMA(Price, Period/2) - WMA(Price, Period), √Period)
```
Where WMA = Weighted Moving Average

**Parameters**:
- `Period` (int): Number of periods (default: 16, range: 1-65536)

**Outputs**:
- `Value` (TOutput): Current HMA value

**Use Cases**:
- Low-lag trend identification
- Smooth trend following
- Signal line in trading systems

---

### VWMA (Volume Weighted Moving Average)

**Purpose**: Weights price by volume, giving more importance to high-volume periods.

**Formula**:
```
VWMA = Σ(Price × Volume) / Σ(Volume)
```

**Parameters**:
- `Period` (int): Number of periods (default: 20, range: 1-65536)

**Inputs**:
- Price (TPrice): Price data
- Volume (TPrice): Volume data

**Outputs**:
- `Value` (TOutput): Current VWMA value

**Use Cases**:
- Volume-adjusted trend analysis
- Institutional money flow tracking
- Support/resistance with volume confirmation

---

### Linear Regression

**Purpose**: Fits a linear trend line through price data to identify trend direction and strength.

**Formula**:
```
y = mx + b
Where: m = slope, b = y-intercept
```

**Parameters**:
- `Period` (int): Number of periods (default: 14, range: 1-65536)

**Outputs**:
- `Value` (TOutput): Current linear regression value
- `Slope` (TOutput): Trend slope
- `Intercept` (TOutput): Y-intercept

**Use Cases**:
- Trend strength measurement
- Price channel identification
- Momentum confirmation

---

## Momentum Oscillators

### RSI (Relative Strength Index)

**Purpose**: Measures the speed and magnitude of price changes, oscillating between 0-100.

**Formula**:
```
RS = Average Gain / Average Loss
RSI = 100 - (100 / (1 + RS))
```

**Parameters**:
- `Period` (int): Number of periods (default: 14, range: 2-65536)
- `OverboughtLevel` (TOutput): Overbought threshold (default: 70, range: 50-100)
- `OversoldLevel` (TOutput): Oversold threshold (default: 30, range: 0-50)

**Outputs**:
- `CurrentValue` (TOutput): Current RSI value
- `IsOverbought` (bool): True when RSI > OverboughtLevel
- `IsOversold` (bool): True when RSI < OversoldLevel

**Use Cases**:
- Overbought/oversold conditions
- Divergence analysis
- Momentum confirmation
- Reversal signals

**Recommended Settings**:
- Standard: 14 periods
- Short-term: 7 periods
- Long-term: 21 periods

---

### MACD (Moving Average Convergence Divergence)

**Purpose**: Shows relationship between two moving averages, indicating trend changes.

**Formula**:
```
MACD Line = EMA(Fast) - EMA(Slow)
Signal Line = EMA(MACD Line, Signal Period)
Histogram = MACD Line - Signal Line
```

**Parameters**:
- `FastPeriod` (int): Fast EMA period (default: 12, range: 1-65536)
- `SlowPeriod` (int): Slow EMA period (default: 26, range: 1-65536)
- `SignalPeriod` (int): Signal EMA period (default: 9, range: 1-65536)

**Outputs**:
- `MACD` (TOutput): MACD line value
- `Signal` (TOutput): Signal line value
- `Histogram` (TOutput): MACD histogram value

**Use Cases**:
- Trend change identification
- Momentum shifts
- Buy/sell signals on crossovers
- Divergence analysis

---

### Stochastic Oscillator

**Purpose**: Compares closing price to price range over a given period, oscillating 0-100.

**Formula**:
```
%K = 100 × (Close - LowestLow) / (HighestHigh - LowestLow)
%D = SMA(%K, SmoothingPeriod)
```

**Parameters**:
- `KPeriod` (int): %K period (default: 14, range: 1-65536)
- `DPeriod` (int): %D smoothing period (default: 3, range: 1-65536)
- `Smoothing` (int): Additional smoothing (default: 3, range: 1-65536)

**Outputs**:
- `PercentK` (TOutput): %K line value
- `PercentD` (TOutput): %D line value

**Use Cases**:
- Overbought/oversold identification
- Momentum confirmation
- Crossover signals
- Divergence analysis

---

### Williams %R

**Purpose**: Momentum oscillator measuring overbought/oversold conditions, inverse of Stochastic %K.

**Formula**:
```
%R = -100 × (HighestHigh - Close) / (HighestHigh - LowestLow)
```

**Parameters**:
- `Period` (int): Lookback period (default: 14, range: 1-65536)

**Outputs**:
- `Value` (TOutput): Current %R value (-100 to 0)

**Use Cases**:
- Overbought (-20 to 0) / Oversold (-100 to -80) signals
- Short-term reversal signals
- Momentum confirmation

---

### ROC (Rate of Change)

**Purpose**: Measures percentage change in price from n periods ago.

**Formula**:
```
ROC = ((Current Price - Price n periods ago) / Price n periods ago) × 100
```

**Parameters**:
- `Period` (int): Number of periods (default: 12, range: 1-65536)

**Outputs**:
- `Value` (TOutput): Current ROC percentage

**Use Cases**:
- Momentum measurement
- Trend strength analysis
- Divergence identification
- Overbought/oversold conditions

---

### Awesome Oscillator

**Purpose**: Measures market momentum using the difference between 5-period and 34-period SMAs of median prices.

**Formula**:
```
AO = SMA(Median Price, 5) - SMA(Median Price, 34)
Median Price = (High + Low) / 2
```

**Parameters**:
- `FastPeriod` (int): Fast SMA period (default: 5, range: 1-65536)
- `SlowPeriod` (int): Slow SMA period (default: 34, range: 1-65536)

**Outputs**:
- `Value` (TOutput): Current AO value

**Use Cases**:
- Momentum shifts
- Zero-line crossovers
- Twin peaks divergence
- Saucer signals

---

### Fisher Transform

**Purpose**: Transforms prices into a Gaussian normal distribution to identify turning points.

**Formula**:
```
Value1 = 0.33 × 2 × ((Price - MinPrice) / (MaxPrice - MinPrice) - 0.5) + 0.67 × Previous Value1
Fisher = 0.5 × ln((1 + Value1) / (1 - Value1)) + 0.5 × Previous Fisher
```

**Parameters**:
- `Period` (int): Lookback period (default: 10, range: 1-65536)

**Outputs**:
- `Value` (TOutput): Fisher Transform value
- `Trigger` (TOutput): Previous Fisher value (trigger line)

**Use Cases**:
- Turning point identification
- Crossover signals
- Trend reversal detection

---

## Volatility Indicators

### Bollinger Bands

**Purpose**: Creates volatility bands around a moving average using standard deviation.

**Formula**:
```
Middle Band = SMA(Price, Period)
Upper Band = Middle Band + (StandardDeviation × Multiplier)
Lower Band = Middle Band - (StandardDeviation × Multiplier)
```

**Parameters**:
- `Period` (int): SMA period (default: 20, range: 1-65536)
- `StandardDeviations` (double): Band width multiplier (default: 2.0, range: 0.1-10.0)
- `MovingAverageType` (enum): Type of moving average (Simple/Exponential/etc.)

**Outputs**:
- `UpperBand` (TOutput): Upper band value
- `MiddleBand` (TOutput): Middle band (SMA) value
- `LowerBand` (TOutput): Lower band value
- `PercentB` (TOutput): Position within bands (0-1)
- `BandWidth` (TOutput): Distance between bands

**Use Cases**:
- Volatility measurement
- Overbought/oversold identification
- Breakout signals
- Mean reversion strategies

---

### ATR (Average True Range)

**Purpose**: Measures market volatility using the average of true ranges over a specified period.

**Formula**:
```
True Range = Max(High - Low, |High - Previous Close|, |Low - Previous Close|)
ATR = Average(True Range, Period)
```

**Parameters**:
- `Period` (int): Averaging period (default: 14, range: 1-65536)

**Inputs**:
- High, Low, Close prices

**Outputs**:
- `Value` (TOutput): Current ATR value

**Use Cases**:
- Stop-loss positioning
- Position sizing
- Volatility filtering
- Breakout confirmation

---

### Standard Deviation

**Purpose**: Measures price dispersion from the mean, indicating volatility.

**Formula**:
```
StdDev = √(Σ(Price - Mean)² / n)
```

**Parameters**:
- `Period` (int): Calculation period (default: 20, range: 1-65536)

**Outputs**:
- `Value` (TOutput): Current standard deviation

**Use Cases**:
- Volatility measurement
- Bollinger Bands calculation
- Risk assessment
- Market regime identification

---

### Donchian Channels

**Purpose**: Identifies the highest high and lowest low over a specified period.

**Formula**:
```
Upper Channel = Highest High over Period
Lower Channel = Lowest Low over Period
Middle Channel = (Upper Channel + Lower Channel) / 2
```

**Parameters**:
- `Period` (int): Lookback period (default: 20, range: 1-65536)

**Outputs**:
- `UpperBand` (TOutput): Highest high
- `LowerBand` (TOutput): Lowest low
- `MiddleBand` (TOutput): Channel midpoint

**Use Cases**:
- Breakout identification
- Support/resistance levels
- Volatility measurement
- Trend following

---

### Keltner Channels

**Purpose**: Volatility-based channels using ATR around an exponential moving average.

**Formula**:
```
Middle Line = EMA(Typical Price, Period)
Upper Band = Middle Line + (ATR × Multiplier)
Lower Band = Middle Line - (ATR × Multiplier)
Typical Price = (High + Low + Close) / 3
```

**Parameters**:
- `Period` (int): EMA period (default: 20, range: 1-65536)
- `ATRPeriod` (int): ATR calculation period (default: 10, range: 1-65536)
- `Multiplier` (double): ATR multiplier (default: 2.0, range: 0.1-10.0)

**Outputs**:
- `UpperBand` (TOutput): Upper channel
- `MiddleLine` (TOutput): EMA centerline
- `LowerBand` (TOutput): Lower channel

**Use Cases**:
- Trend identification
- Breakout signals
- Volatility measurement
- Mean reversion

---

## Volume Indicators

### OBV (On Balance Volume)

**Purpose**: Relates volume to price changes, cumulative indicator showing buying/selling pressure.

**Formula**:
```
If Close > Previous Close: OBV = Previous OBV + Volume
If Close < Previous Close: OBV = Previous OBV - Volume
If Close = Previous Close: OBV = Previous OBV
```

**Parameters**:
- No configurable parameters

**Inputs**:
- Close price and Volume

**Outputs**:
- `Value` (TOutput): Current OBV value

**Use Cases**:
- Volume trend confirmation
- Divergence analysis
- Accumulation/distribution detection
- Trend strength measurement

---

### Accumulation/Distribution Line

**Purpose**: Combines price and volume to show cumulative flow of money into/out of security.

**Formula**:
```
CLV = ((Close - Low) - (High - Close)) / (High - Low)
A/D Line = Previous A/D Line + (CLV × Volume)
```

**Parameters**:
- No configurable parameters

**Inputs**:
- High, Low, Close prices and Volume

**Outputs**:
- `Value` (TOutput): Current A/D Line value

**Use Cases**:
- Money flow analysis
- Accumulation/distribution phases
- Divergence identification
- Volume confirmation

---

### Chaikin Money Flow

**Purpose**: Measures money flow volume over a specific period.

**Formula**:
```
Money Flow Multiplier = ((Close - Low) - (High - Close)) / (High - Low)
Money Flow Volume = Money Flow Multiplier × Volume
CMF = Sum(Money Flow Volume, Period) / Sum(Volume, Period)
```

**Parameters**:
- `Period` (int): Calculation period (default: 21, range: 1-65536)

**Inputs**:
- High, Low, Close prices and Volume

**Outputs**:
- `Value` (TOutput): Current CMF value (-1 to +1)

**Use Cases**:
- Buying/selling pressure measurement
- Overbought/oversold confirmation
- Divergence analysis
- Trend validation

---

### MFI (Money Flow Index)

**Purpose**: Volume-weighted RSI, oscillating between 0-100.

**Formula**:
```
Typical Price = (High + Low + Close) / 3
Raw Money Flow = Typical Price × Volume
Positive Money Flow = Sum of positive Raw Money Flow
Negative Money Flow = Sum of negative Raw Money Flow
Money Flow Ratio = Positive Money Flow / Negative Money Flow
MFI = 100 - (100 / (1 + Money Flow Ratio))
```

**Parameters**:
- `Period` (int): Calculation period (default: 14, range: 1-65536)

**Inputs**:
- High, Low, Close prices and Volume

**Outputs**:
- `Value` (TOutput): Current MFI value (0-100)

**Use Cases**:
- Volume-adjusted overbought/oversold
- Divergence analysis
- Market strength measurement
- Reversal confirmation

---

### VWAP (Volume Weighted Average Price)

**Purpose**: Shows average price weighted by volume, important institutional benchmark.

**Formula**:
```
VWAP = Σ(Typical Price × Volume) / Σ(Volume)
Typical Price = (High + Low + Close) / 3
```

**Parameters**:
- `Period` (int): Calculation period (default: 0 for session-based)

**Inputs**:
- High, Low, Close prices and Volume

**Outputs**:
- `Value` (TOutput): Current VWAP value

**Use Cases**:
- Institutional price benchmark
- Fair value reference
- Support/resistance identification
- Algorithm execution benchmark

---

### Klinger Oscillator

**Purpose**: Combines price and volume to identify long-term money flow trends.

**Formula**:
```
Volume Force = Volume × Trend × dm
KO = EMA(Volume Force, 34) - EMA(Volume Force, 55)
Signal = EMA(KO, 13)
```
Where Trend and dm are complex calculations involving price movement

**Parameters**:
- `FastPeriod` (int): Fast EMA period (default: 34, range: 1-65536)
- `SlowPeriod` (int): Slow EMA period (default: 55, range: 1-65536)
- `SignalPeriod` (int): Signal line period (default: 13, range: 1-65536)

**Inputs**:
- High, Low, Close prices and Volume

**Outputs**:
- `Value` (TOutput): Klinger Oscillator value
- `Signal` (TOutput): Signal line value

**Use Cases**:
- Long-term trend identification
- Volume flow analysis
- Divergence detection
- Trend reversal signals

---

## Support/Resistance Indicators

### Pivot Points

**Purpose**: Calculates support and resistance levels based on previous period's high, low, close.

**Formula**:
```
Pivot Point = (High + Low + Close) / 3
R1 = 2 × PP - Low
R2 = PP + (High - Low)
R3 = High + 2 × (PP - Low)
S1 = 2 × PP - High
S2 = PP - (High - Low)
S3 = Low - 2 × (High - PP)
```

**Parameters**:
- `PivotType` (enum): Standard, Fibonacci, Woodie, Camarilla

**Inputs**:
- High, Low, Close prices

**Outputs**:
- `PivotPoint` (TOutput): Central pivot point
- `R1`, `R2`, `R3` (TOutput): Resistance levels
- `S1`, `S2`, `S3` (TOutput): Support levels

**Use Cases**:
- Support/resistance identification
- Target setting
- Entry/exit levels
- Day trading reference points

---

### Supertrend

**Purpose**: Trend following indicator using ATR to create dynamic support/resistance levels.

**Formula**:
```
Basic Upperband = (High + Low) / 2 + (Multiplier × ATR)
Basic Lowerband = (High + Low) / 2 - (Multiplier × ATR)
Final Upperband = Basic Upperband < Previous Final Upperband OR Previous Close > Previous Final Upperband ? Basic Upperband : Previous Final Upperband
Final Lowerband = Basic Lowerband > Previous Final Lowerband OR Previous Close < Previous Final Lowerband ? Basic Lowerband : Previous Final Lowerband
Supertrend = Close > Final Upperband ? Final Lowerband : Final Upperband
```

**Parameters**:
- `Period` (int): ATR period (default: 10, range: 1-65536)
- `Multiplier` (double): ATR multiplier (default: 3.0, range: 0.1-10.0)

**Inputs**:
- High, Low, Close prices

**Outputs**:
- `Value` (TOutput): Supertrend line value
- `UpperBand` (TOutput): Upper band value
- `LowerBand` (TOutput): Lower band value
- `IsUpTrend` (bool): True when in uptrend

**Use Cases**:
- Trend identification
- Stop-loss positioning
- Entry/exit signals
- Trend reversal detection

---

### Parabolic SAR

**Purpose**: Time and price-based trailing stop system.

**Formula**:
```
SAR = Previous SAR + AF × (EP - Previous SAR)
AF = Acceleration Factor (starts at 0.02, increases by 0.02 up to 0.20)
EP = Extreme Point (highest high in uptrend, lowest low in downtrend)
```

**Parameters**:
- `Acceleration` (double): Initial acceleration factor (default: 0.02, range: 0.01-0.1)
- `MaximumAcceleration` (double): Maximum AF (default: 0.20, range: 0.1-1.0)

**Inputs**:
- High, Low, Close prices

**Outputs**:
- `Value` (TOutput): Current SAR value
- `IsLong` (bool): True when in long position
- `AF` (TOutput): Current acceleration factor

**Use Cases**:
- Trailing stop placement
- Trend following
- Position reversal signals
- Entry/exit timing

---

### ZigZag

**Purpose**: Identifies significant price swings by filtering out minor fluctuations.

**Formula**:
```
Identifies peaks and troughs based on percentage or absolute price changes
```

**Parameters**:
- `Percentage` (double): Minimum percentage change (default: 5.0, range: 0.1-50.0)
- `UsePercentage` (bool): Use percentage vs absolute change

**Inputs**:
- High, Low, Close prices

**Outputs**:
- `Value` (TOutput): ZigZag line value
- `LastPeak` (TOutput): Last identified peak
- `LastTrough` (TOutput): Last identified trough

**Use Cases**:
- Pattern recognition
- Support/resistance identification
- Elliott Wave analysis
- Trend structure analysis

---

## Directional Movement Indicators

### ADX (Average Directional Index)

**Purpose**: Measures trend strength regardless of direction, ranges 0-100.

**Formula**:
```
+DI = 100 × (Smoothed +DM) / ATR
-DI = 100 × (Smoothed -DM) / ATR
DX = 100 × |+DI - -DI| / (+DI + -DI)
ADX = Smoothed Average of DX
```

**Parameters**:
- `Period` (int): Calculation period (default: 14, range: 1-65536)

**Inputs**:
- High, Low, Close prices

**Outputs**:
- `Value` (TOutput): ADX value (0-100)
- `PlusDI` (TOutput): +DI value
- `MinusDI` (TOutput): -DI value

**Use Cases**:
- Trend strength measurement
- Strong trend: ADX > 25
- Weak trend: ADX < 20
- Trend direction from +DI/-DI crossovers

---

### Aroon

**Purpose**: Identifies when trends are likely to change by measuring time since highest highs and lowest lows.

**Formula**:
```
Aroon Up = ((Period - Periods Since Highest High) / Period) × 100
Aroon Down = ((Period - Periods Since Lowest Low) / Period) × 100
Aroon Oscillator = Aroon Up - Aroon Down
```

**Parameters**:
- `Period` (int): Lookback period (default: 25, range: 1-65536)

**Inputs**:
- High, Low prices

**Outputs**:
- `AroonUp` (TOutput): Aroon Up value (0-100)
- `AroonDown` (TOutput): Aroon Down value (0-100)
- `AroonOscillator` (TOutput): Difference between Up and Down

**Use Cases**:
- Trend strength identification
- Trend change detection
- Consolidation vs trending markets
- Breakout confirmation

---

## Specialized Indicators

### CCI (Commodity Channel Index)

**Purpose**: Measures current price level relative to average price level over a given period.

**Formula**:
```
Typical Price = (High + Low + Close) / 3
CCI = (Typical Price - SMA of Typical Price) / (0.015 × Mean Deviation)
```

**Parameters**:
- `Period` (int): Calculation period (default: 20, range: 1-65536)

**Inputs**:
- High, Low, Close prices

**Outputs**:
- `Value` (TOutput): Current CCI value (typically -200 to +200)

**Use Cases**:
- Overbought (>+100) / Oversold (<-100) identification
- Trend direction confirmation
- Divergence analysis
- Cyclical turning points

---

### Ichimoku Cloud

**Purpose**: Comprehensive indicator providing support/resistance, trend direction, and momentum information.

**Formula**:
```
Tenkan-sen = (Highest High + Lowest Low) / 2 over 9 periods
Kijun-sen = (Highest High + Lowest Low) / 2 over 26 periods
Senkou Span A = (Tenkan-sen + Kijun-sen) / 2, plotted 26 periods ahead
Senkou Span B = (Highest High + Lowest Low) / 2 over 52 periods, plotted 26 periods ahead
Chikou Span = Close plotted 26 periods behind
```

**Parameters**:
- `TenkanPeriod` (int): Tenkan-sen period (default: 9)
- `KijunPeriod` (int): Kijun-sen period (default: 26)
- `SenkouBPeriod` (int): Senkou Span B period (default: 52)
- `Displacement` (int): Forward/backward displacement (default: 26)

**Inputs**:
- High, Low, Close prices

**Outputs**:
- `TenkanSen` (TOutput): Tenkan-sen (Conversion Line)
- `KijunSen` (TOutput): Kijun-sen (Base Line)
- `SenkouSpanA` (TOutput): Leading Span A
- `SenkouSpanB` (TOutput): Leading Span B
- `ChikouSpan` (TOutput): Lagging Span

**Use Cases**:
- Comprehensive trend analysis
- Support/resistance identification
- Momentum measurement
- Cloud breakout strategies

---

### Choppiness Index

**Purpose**: Determines whether market is choppy/sideways or trending.

**Formula**:
```
CI = 100 × log10(Sum(TrueRange, n) / (MaxHigh - MinLow)) / log10(n)
```

**Parameters**:
- `Period` (int): Calculation period (default: 14, range: 1-65536)

**Inputs**:
- High, Low, Close prices

**Outputs**:
- `Value` (TOutput): Choppiness Index value (0-100)

**Use Cases**:
- Market regime identification
- High values (>61.8): Choppy/sideways market
- Low values (<38.2): Trending market
- Filter for trend-following strategies

---

### Fibonacci Retracement

**Purpose**: Identifies potential support and resistance levels based on Fibonacci ratios.

**Formula**:
```
Level = Low + (High - Low) × Fibonacci Ratio
Common ratios: 0.236, 0.382, 0.500, 0.618, 0.786
```

**Parameters**:
- `HighValue` (TOutput): Swing high price
- `LowValue` (TOutput): Swing low price
- `Levels` (double[]): Fibonacci levels to calculate

**Outputs**:
- `Level236` (TOutput): 23.6% retracement
- `Level382` (TOutput): 38.2% retracement  
- `Level500` (TOutput): 50% retracement
- `Level618` (TOutput): 61.8% retracement
- `Level786` (TOutput): 78.6% retracement

**Use Cases**:
- Support/resistance identification
- Target setting
- Entry/exit levels
- Pullback analysis

---

### Heikin Ashi

**Purpose**: Modified candlestick chart that smooths price action to better identify trends.

**Formula**:
```
HA Close = (Open + High + Low + Close) / 4
HA Open = (Previous HA Open + Previous HA Close) / 2
HA High = Max(High, HA Open, HA Close)
HA Low = Min(Low, HA Open, HA Close)
```

**Parameters**:
- No configurable parameters

**Inputs**:
- Open, High, Low, Close prices

**Outputs**:
- `Open` (TOutput): Heikin Ashi Open
- `High` (TOutput): Heikin Ashi High
- `Low` (TOutput): Heikin Ashi Low
- `Close` (TOutput): Heikin Ashi Close

**Use Cases**:
- Trend identification
- Noise reduction
- Entry/exit signal smoothing
- Trend strength visualization

---

### Lorentzian Classification

**Purpose**: Machine learning-based classification system for market prediction using Lorentzian distance.

**Formula**:
```
Uses machine learning algorithms with Lorentzian distance metrics to classify market conditions
```

**Parameters**:
- `LookbackWindow` (int): Historical data window (default: 8)
- `RelativeWeighting` (double): Feature weighting (default: 0.85)
- `StartLongTrades` (bool): Enable long predictions
- `StartShortTrades` (bool): Enable short predictions
- Various ML-specific parameters

**Inputs**:
- Open, High, Low, Close, Volume

**Outputs**:
- `Signal` (int): Classification signal (-1, 0, 1)
- `Probability` (double): Prediction confidence
- `IsLong` (bool): Long signal indicator
- `IsShort` (bool): Short signal indicator

**Use Cases**:
- Machine learning-based market prediction
- Pattern recognition
- Regime classification
- Advanced signal generation

**Note**: This is an experimental ML indicator requiring careful parameter tuning and validation.

---

This completes the detailed reference for all 35 trading indicators. Each indicator includes mathematical formulas, parameter specifications, typical use cases, and recommended settings based on common trading practices.