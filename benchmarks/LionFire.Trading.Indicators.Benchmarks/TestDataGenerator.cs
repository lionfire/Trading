using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Trading.Indicators.Benchmarks;

/// <summary>
/// Generates realistic market data for benchmark testing
/// </summary>
public class TestDataGenerator
{
    private readonly Random _random;
    private readonly Faker _faker;

    public TestDataGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _faker = seed.HasValue ? new Faker().UseSeed(seed.Value) : new Faker();
    }

    public class MarketDataPoint
    {
        public DateTime Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }

    /// <summary>
    /// Generates realistic market data with natural price movements
    /// </summary>
    public List<MarketDataPoint> GenerateRealisticData(int count, decimal startPrice = 100m)
    {
        var data = new List<MarketDataPoint>(count);
        var timestamp = DateTime.UtcNow.AddDays(-count / 390); // ~390 minutes in trading day
        var currentPrice = startPrice;

        for (int i = 0; i < count; i++)
        {
            // Generate realistic price movements using multiple factors
            var trend = Math.Sin(i * 0.01) * 0.02m; // Long-term trend
            var shortTermVolatility = (decimal)(_random.NextDouble() - 0.5) * 0.01m; // Short-term noise
            var momentum = i > 20 ? (data[i - 1].Close - data[i - 20].Close) / 20m * 0.1m : 0; // Momentum factor
            
            var changePercent = trend + shortTermVolatility + momentum;
            currentPrice *= (1 + changePercent);
            
            // Ensure price stays positive and reasonable
            currentPrice = Math.Max(currentPrice, startPrice * 0.5m);
            currentPrice = Math.Min(currentPrice, startPrice * 2m);

            // Generate OHLC data
            var open = i > 0 ? data[i - 1].Close : currentPrice;
            var close = currentPrice;
            var intrabarVolatility = (decimal)(_random.NextDouble() * 0.005 + 0.001);
            var high = Math.Max(open, close) * (1 + intrabarVolatility);
            var low = Math.Min(open, close) * (1 - intrabarVolatility);

            // Volume with patterns (higher at open/close, lower mid-day)
            var timeOfDay = i % 390;
            var volumeMultiplier = 1.0m;
            if (timeOfDay < 30 || timeOfDay > 360) volumeMultiplier = 1.5m; // Higher at open/close
            else if (timeOfDay > 150 && timeOfDay < 240) volumeMultiplier = 0.7m; // Lower mid-day
            
            var volume = (decimal)(_random.Next(900000, 1100000)) * volumeMultiplier;

            data.Add(new MarketDataPoint
            {
                Timestamp = timestamp,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = volume
            });

            timestamp = timestamp.AddMinutes(1);
        }

        return data;
    }

    /// <summary>
    /// Generates trending market data (bullish or bearish)
    /// </summary>
    public List<MarketDataPoint> GenerateTrendingData(int count, decimal startPrice = 100m, bool bullish = true)
    {
        var data = new List<MarketDataPoint>(count);
        var timestamp = DateTime.UtcNow.AddDays(-count / 390);
        var currentPrice = startPrice;
        var trendStrength = bullish ? 0.0002m : -0.0002m; // Gradual trend

        for (int i = 0; i < count; i++)
        {
            // Strong trend with occasional pullbacks
            var trendComponent = trendStrength * (1 + (decimal)Math.Sin(i * 0.05) * 0.3m);
            var noise = (decimal)(_random.NextDouble() - 0.5) * 0.003m;
            var pullback = i % 50 == 0 ? -trendStrength * 5 : 0; // Occasional pullbacks
            
            currentPrice *= (1 + trendComponent + noise + pullback);
            currentPrice = Math.Max(currentPrice, 1m); // Ensure positive

            var open = i > 0 ? data[i - 1].Close : currentPrice;
            var close = currentPrice;
            var volatility = (decimal)(_random.NextDouble() * 0.003 + 0.001);
            var high = Math.Max(open, close) * (1 + volatility);
            var low = Math.Min(open, close) * (1 - volatility);
            var volume = (decimal)_random.Next(800000, 1200000);

            // In trending markets, volume often increases with trend
            if ((bullish && close > open) || (!bullish && close < open))
            {
                volume *= 1.2m;
            }

            data.Add(new MarketDataPoint
            {
                Timestamp = timestamp,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = volume
            });

            timestamp = timestamp.AddMinutes(1);
        }

        return data;
    }

    /// <summary>
    /// Generates sideways/ranging market data
    /// </summary>
    public List<MarketDataPoint> GenerateSidewaysData(int count, decimal centerPrice = 100m, decimal rangePercent = 0.05m)
    {
        var data = new List<MarketDataPoint>(count);
        var timestamp = DateTime.UtcNow.AddDays(-count / 390);
        var currentPrice = centerPrice;
        var upperBound = centerPrice * (1 + rangePercent);
        var lowerBound = centerPrice * (1 - rangePercent);

        for (int i = 0; i < count; i++)
        {
            // Oscillate between bounds using sine wave with noise
            var oscillation = (decimal)Math.Sin(i * 0.1) * rangePercent * centerPrice;
            var noise = (decimal)(_random.NextDouble() - 0.5) * 0.002m * centerPrice;
            
            currentPrice = centerPrice + oscillation + noise;
            
            // Mean reversion when approaching bounds
            if (currentPrice > upperBound)
            {
                currentPrice = upperBound - (decimal)_random.NextDouble() * rangePercent * centerPrice * 0.1m;
            }
            else if (currentPrice < lowerBound)
            {
                currentPrice = lowerBound + (decimal)_random.NextDouble() * rangePercent * centerPrice * 0.1m;
            }

            var open = i > 0 ? data[i - 1].Close : currentPrice;
            var close = currentPrice;
            var volatility = (decimal)(_random.NextDouble() * 0.002 + 0.0005);
            var high = Math.Max(open, close) * (1 + volatility);
            var low = Math.Min(open, close) * (1 - volatility);
            
            // Lower volume in ranging markets
            var volume = (decimal)_random.Next(600000, 900000);

            data.Add(new MarketDataPoint
            {
                Timestamp = timestamp,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = volume
            });

            timestamp = timestamp.AddMinutes(1);
        }

        return data;
    }

    /// <summary>
    /// Generates highly volatile market data
    /// </summary>
    public List<MarketDataPoint> GenerateVolatileData(int count, decimal startPrice = 100m)
    {
        var data = new List<MarketDataPoint>(count);
        var timestamp = DateTime.UtcNow.AddDays(-count / 390);
        var currentPrice = startPrice;
        var volatilityRegime = 0.01m; // Start with 1% volatility

        for (int i = 0; i < count; i++)
        {
            // Volatility clustering - periods of high and low volatility
            if (i % 100 == 0)
            {
                volatilityRegime = (decimal)_random.NextDouble() * 0.03m + 0.005m; // 0.5% to 3.5%
            }

            // Large random movements
            var change = (decimal)(_random.NextDouble() - 0.5) * 2 * volatilityRegime;
            
            // Occasional spikes/crashes
            if (_random.NextDouble() < 0.02) // 2% chance of extreme move
            {
                change *= 3;
            }

            currentPrice *= (1 + change);
            currentPrice = Math.Max(currentPrice, startPrice * 0.3m); // Floor at 30% of start
            currentPrice = Math.Min(currentPrice, startPrice * 3m); // Cap at 300% of start

            var open = i > 0 ? data[i - 1].Close : currentPrice;
            var close = currentPrice;
            
            // High intrabar volatility
            var intrabarVol = volatilityRegime * 2;
            var high = Math.Max(open, close) * (1 + intrabarVol);
            var low = Math.Min(open, close) * (1 - intrabarVol);
            
            // High volume during volatile periods
            var volume = (decimal)_random.Next(1000000, 2000000) * (1 + volatilityRegime * 10);

            data.Add(new MarketDataPoint
            {
                Timestamp = timestamp,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = volume
            });

            timestamp = timestamp.AddMinutes(1);
        }

        return data;
    }

    /// <summary>
    /// Generates data with gaps (market opens)
    /// </summary>
    public List<MarketDataPoint> GenerateDataWithGaps(int count, decimal startPrice = 100m, int gapFrequency = 390)
    {
        var data = GenerateRealisticData(count, startPrice);
        
        // Add gaps at regular intervals (simulating daily opens)
        for (int i = gapFrequency; i < data.Count; i += gapFrequency)
        {
            var gapSize = (decimal)(_random.NextDouble() - 0.5) * 0.02m; // -2% to +2% gap
            data[i].Open = data[i - 1].Close * (1 + gapSize);
            
            // Adjust high/low if needed
            if (data[i].Open > data[i].High) data[i].High = data[i].Open;
            if (data[i].Open < data[i].Low) data[i].Low = data[i].Open;
        }

        return data;
    }

    /// <summary>
    /// Generates simple price array (close prices only)
    /// </summary>
    public decimal[] GenerateSimplePriceData(int count, decimal startPrice = 100m)
    {
        return GenerateRealisticData(count, startPrice).Select(d => d.Close).ToArray();
    }

    /// <summary>
    /// Generates data for stress testing with extreme values
    /// </summary>
    public List<MarketDataPoint> GenerateStressTestData(int count)
    {
        var data = new List<MarketDataPoint>(count);
        var timestamp = DateTime.UtcNow.AddDays(-count / 390);

        for (int i = 0; i < count; i++)
        {
            decimal price;
            
            // Mix of extreme cases
            switch (i % 10)
            {
                case 0: price = 0.01m; break; // Very small
                case 1: price = 1000000m; break; // Very large
                case 2: price = decimal.MaxValue / 2; break; // Near max
                case 3: price = decimal.MinValue / 2; break; // Near min (negative)
                default: price = 100m * (decimal)Math.Pow(10, _random.Next(-2, 3)); break; // Various scales
            }

            // Make sure we have valid positive price for OHLC
            price = Math.Abs(price);
            if (price == 0) price = 0.01m;

            var volatility = (decimal)_random.NextDouble() * 0.1m;
            var high = price * (1 + volatility);
            var low = price * (1 - volatility);
            var open = price * (1 + (decimal)(_random.NextDouble() - 0.5) * volatility);
            var close = price * (1 + (decimal)(_random.NextDouble() - 0.5) * volatility);

            data.Add(new MarketDataPoint
            {
                Timestamp = timestamp,
                Open = open,
                High = Math.Max(Math.Max(high, open), close),
                Low = Math.Min(Math.Min(low, open), close),
                Close = close,
                Volume = (decimal)_random.Next(1, 10000000)
            });

            timestamp = timestamp.AddMinutes(1);
        }

        return data;
    }
}