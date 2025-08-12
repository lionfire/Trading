using Xunit;
using FluentAssertions;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LionFire.Trading.Indicators.Tests;

/// <summary>
/// Performance validation tests for the Lorentzian Classification indicator.
/// These tests ensure the indicator meets specific performance characteristics
/// and validate behavior under various conditions.
/// </summary>
public class LorentzianClassificationPerformanceTests
{
    #region Constants

    private const int SMALL_DATASET = 1_000;
    private const int MEDIUM_DATASET = 10_000;
    private const int LARGE_DATASET = 100_000;
    private const double ACCEPTABLE_LATENCY_MS = 1.0; // 1ms per update for real-time trading
    private const double BATCH_PROCESSING_THRESHOLD_MS = 100.0; // 100ms for 10k bars

    #endregion

    #region Initialization Performance Tests

    [Fact]
    public void InitializationShouldBeFastForSmallK()
    {
        // Arrange
        var parameters = CreateDefaultParameters(neighborsCount: 8);
        
        // Act & Assert
        var stopwatch = Stopwatch.StartNew();
        var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
        stopwatch.Stop();
        
        // Should initialize in less than 10ms
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10);
        indicator.Should().NotBeNull();
        indicator.IsReady.Should().BeFalse();
    }

    [Fact]
    public void InitializationShouldScaleWithLookbackPeriod()
    {
        // Arrange & Act
        var timings = new List<(int lookback, long ms)>();
        
        foreach (var lookback in new[] { 50, 100, 500, 1000 })
        {
            var parameters = CreateDefaultParameters(lookbackPeriod: lookback);
            
            var stopwatch = Stopwatch.StartNew();
            var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
            stopwatch.Stop();
            
            timings.Add((lookback, stopwatch.ElapsedMilliseconds));
        }
        
        // Assert - initialization time should not grow significantly with lookback period
        // (it should be mostly constant since we're just allocating arrays)
        timings.All(t => t.ms < 20).Should().BeTrue("initialization should be fast regardless of lookback period");
    }

    #endregion

    #region Single Update Performance Tests

    [Fact]
    public void SingleUpdateShouldMeetRealTimeLatencyRequirements()
    {
        // Arrange
        var parameters = CreateDefaultParameters();
        var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
        var testData = GenerateTestData(100); // Warm up data
        
        // Warm up the indicator
        indicator.OnBarBatch(testData.Take(50).ToList(), null);
        
        // Act & Assert
        var latencies = new List<double>();
        var remainingData = testData.Skip(50).ToList();
        
        foreach (var bar in remainingData)
        {
            var stopwatch = Stopwatch.StartNew();
            indicator.OnBarBatch([bar], null);
            stopwatch.Stop();
            
            latencies.Add(stopwatch.Elapsed.TotalMilliseconds);
        }
        
        // Assert
        var averageLatency = latencies.Average();
        var maxLatency = latencies.Max();
        
        averageLatency.Should().BeLessThan(ACCEPTABLE_LATENCY_MS, 
            "average update latency should be suitable for real-time trading");
        maxLatency.Should().BeLessThan(ACCEPTABLE_LATENCY_MS * 3, 
            "maximum update latency should not have excessive spikes");
    }

    [Theory]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    public void SingleUpdateLatencyShouldScaleReasonablyWithK(int kValue)
    {
        // Arrange
        var parameters = CreateDefaultParameters(neighborsCount: kValue);
        var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
        var testData = GenerateTestData(200);
        
        // Warm up with enough data to fill historical patterns
        indicator.OnBarBatch(testData.Take(150).ToList(), null);
        
        // Act
        var latencies = new List<double>();
        var testBars = testData.Skip(150).Take(20).ToList();
        
        foreach (var bar in testBars)
        {
            var stopwatch = Stopwatch.StartNew();
            indicator.OnBarBatch([bar], null);
            stopwatch.Stop();
            
            latencies.Add(stopwatch.Elapsed.TotalMilliseconds);
        }
        
        // Assert
        var averageLatency = latencies.Average();
        
        // Latency should scale roughly linearly with K, but still be reasonable
        var expectedMaxLatency = ACCEPTABLE_LATENCY_MS * (1 + Math.Log(kValue) / Math.Log(8));
        
        averageLatency.Should().BeLessThan(expectedMaxLatency, 
            $"average latency should scale reasonably with K={kValue}");
    }

    #endregion

    #region Batch Processing Performance Tests

    [Fact]
    public void BatchProcessingShouldBeEfficientForMediumDataset()
    {
        // Arrange
        var parameters = CreateDefaultParameters();
        var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
        var testData = GenerateTestData(MEDIUM_DATASET);
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        indicator.OnBarBatch(testData, null);
        stopwatch.Stop();
        
        // Assert
        var totalTime = stopwatch.Elapsed.TotalMilliseconds;
        var timePerBar = totalTime / MEDIUM_DATASET;
        
        totalTime.Should().BeLessThan(BATCH_PROCESSING_THRESHOLD_MS * 100, // 10 seconds for 10k bars
            "batch processing should be efficient for medium datasets");
        
        timePerBar.Should().BeLessThan(1.0, 
            "time per bar in batch processing should be under 1ms");
        
        // Verify the indicator is working correctly
        indicator.IsReady.Should().BeTrue();
        indicator.HistoricalPatternsCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BatchProcessingShouldHandleLargeDatasetWithoutMemoryIssues()
    {
        // Arrange
        var parameters = CreateDefaultParameters(lookbackPeriod: 1000); // Larger lookback for large dataset
        var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
        
        // Monitor memory before
        GC.Collect();
        GC.WaitForPendingFinalizers();
        var memoryBefore = GC.GetTotalMemory(false);
        
        // Act
        var batchSize = 10_000;
        var totalBars = LARGE_DATASET;
        
        for (int i = 0; i < totalBars; i += batchSize)
        {
            var remainingBars = Math.Min(batchSize, totalBars - i);
            var batch = GenerateTestData(remainingBars);
            indicator.OnBarBatch(batch, null);
            
            // Periodic memory checks
            if (i % 50_000 == 0)
            {
                var currentMemory = GC.GetTotalMemory(false);
                var memoryGrowth = currentMemory - memoryBefore;
                
                // Memory growth should be bounded (not linear with data size)
                memoryGrowth.Should().BeLessThan(500_000_000, // 500MB limit
                    $"memory growth should be bounded at iteration {i}");
            }
        }
        
        // Assert
        indicator.IsReady.Should().BeTrue();
        indicator.HistoricalPatternsCount.Should().Be(parameters.LookbackPeriod, 
            "should maintain fixed-size historical pattern buffer");
        
        // Final memory check
        GC.Collect();
        var memoryAfter = GC.GetTotalMemory(true);
        var totalMemoryGrowth = memoryAfter - memoryBefore;
        
        totalMemoryGrowth.Should().BeLessThan(100_000_000, // 100MB limit
            "total memory growth should be reasonable for large dataset processing");
    }

    #endregion

    #region Feature Extraction Performance Tests

    [Fact]
    public void FeatureExtractionOverheadShouldBeMinimal()
    {
        // Arrange
        var parameters = CreateDefaultParameters();
        var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
        var testData = GenerateTestData(100);
        
        // Warm up
        indicator.OnBarBatch(testData.Take(50).ToList(), null);
        
        // Act & Assert
        var overheadTimes = new List<double>();
        var remainingBars = testData.Skip(50).Take(20).ToList();
        
        foreach (var bar in remainingBars)
        {
            var stopwatch = Stopwatch.StartNew();
            indicator.OnBarBatch([bar], null);
            var features = indicator.CurrentFeatures; // This should be cached, not recalculated
            stopwatch.Stop();
            
            overheadTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
            
            // Verify features are extracted
            features.Should().NotBeNull();
            features.Length.Should().BeGreaterThan(0);
        }
        
        var averageOverhead = overheadTimes.Average();
        averageOverhead.Should().BeLessThan(0.5, 
            "feature extraction overhead should be minimal");
    }

    #endregion

    #region Memory Usage Tests

    [Fact]
    public void MemoryUsageShouldBeBoundedWithLongRunning()
    {
        // Arrange
        var parameters = CreateDefaultParameters(lookbackPeriod: 500);
        var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
        
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(false);
        
        // Act - simulate long-running operation
        for (int cycle = 0; cycle < 10; cycle++)
        {
            var testData = GenerateTestData(1000);
            indicator.OnBarBatch(testData, null);
            
            // Check memory growth
            if (cycle % 3 == 0)
            {
                GC.Collect();
                var currentMemory = GC.GetTotalMemory(false);
                var growth = currentMemory - initialMemory;
                
                growth.Should().BeLessThan(50_000_000, // 50MB limit
                    $"memory growth should be bounded in cycle {cycle}");
            }
        }
        
        // Assert
        indicator.HistoricalPatternsCount.Should().Be(parameters.LookbackPeriod,
            "should maintain fixed historical pattern count");
    }

    [Fact]
    public void ClearOperationShouldReleaseMemoryEffectively()
    {
        // Arrange
        var parameters = CreateDefaultParameters();
        var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
        var testData = GenerateTestData(5000);
        
        // Fill up the indicator
        indicator.OnBarBatch(testData, null);
        
        GC.Collect();
        var memoryAfterFilling = GC.GetTotalMemory(false);
        
        // Act
        indicator.Clear();
        GC.Collect();
        var memoryAfterClear = GC.GetTotalMemory(true);
        
        // Assert
        var memoryReleased = memoryAfterFilling - memoryAfterClear;
        
        memoryReleased.Should().BeGreaterThan(0, "Clear() should release memory");
        
        // Verify state is reset
        indicator.IsReady.Should().BeFalse();
        indicator.HistoricalPatternsCount.Should().Be(0);
        indicator.CurrentFeatures.All(f => f == 0.0).Should().BeTrue();
    }

    #endregion

    #region Market Condition Performance Tests

    [Theory]
    [InlineData(MarketCondition.Trending)]
    [InlineData(MarketCondition.Sideways)]
    [InlineData(MarketCondition.Volatile)]
    public void PerformanceShouldBeConsistentAcrossMarketConditions(MarketCondition condition)
    {
        // Arrange
        var parameters = CreateDefaultParameters();
        var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
        var testData = GenerateTestDataForCondition(condition, 2000);
        
        // Act
        var latencies = new List<double>();
        
        foreach (var bar in testData)
        {
            var stopwatch = Stopwatch.StartNew();
            indicator.OnBarBatch([bar], null);
            stopwatch.Stop();
            
            latencies.Add(stopwatch.Elapsed.TotalMilliseconds);
        }
        
        // Assert
        var averageLatency = latencies.Average();
        var maxLatency = latencies.Max();
        var latencyStdDev = CalculateStandardDeviation(latencies);
        
        averageLatency.Should().BeLessThan(ACCEPTABLE_LATENCY_MS * 2, 
            $"average latency should be acceptable for {condition} market");
        
        maxLatency.Should().BeLessThan(ACCEPTABLE_LATENCY_MS * 10, 
            $"maximum latency should not be excessive for {condition} market");
        
        latencyStdDev.Should().BeLessThan(averageLatency * 2, 
            $"latency should be consistent for {condition} market");
    }

    #endregion

    #region Stress Tests

    [Fact]
    public void ShouldHandleExtremeParameterValues()
    {
        // Arrange - Test with extreme but valid parameter combinations
        var extremeParameters = new PLorentzianClassification<decimal, double>
        {
            NeighborsCount = 100, // High K
            LookbackPeriod = 5000, // Large lookback
            NormalizationWindow = 200, // Large normalization window
            RSIPeriod = 50,
            CCIPeriod = 50,
            ADXPeriod = 50,
            MinConfidence = 0.95, // Very high confidence threshold
            LabelLookahead = 20,
            LabelThreshold = 0.001 // Very sensitive threshold
        };
        
        // Act & Assert
        var indicator = new LorentzianClassification_FP<decimal, double>(extremeParameters);
        var testData = GenerateTestData(1000);
        
        Action act = () => indicator.OnBarBatch(testData, null);
        act.Should().NotThrow("should handle extreme parameter values gracefully");
        
        // Verify it still works
        indicator.IsReady.Should().BeTrue();
        indicator.HistoricalPatternsCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShouldMaintainAccuracyUnderStress()
    {
        // Arrange
        var parameters = CreateDefaultParameters();
        var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
        
        // Generate consistent test data for accuracy verification
        var testData = GenerateTestData(1000, seed: 42);
        
        // Act - Process the same data multiple times to check consistency
        var results1 = ProcessDataAndGetResults(indicator, testData);
        
        indicator.Clear();
        var results2 = ProcessDataAndGetResults(indicator, testData);
        
        // Assert - Results should be identical for same input data
        results1.signal.Should().BeApproximately(results2.signal, 1e-10, 
            "signal should be consistent across runs");
        results1.confidence.Should().BeApproximately(results2.confidence, 1e-10, 
            "confidence should be consistent across runs");
    }

    #endregion

    #region Helper Methods

    private static PLorentzianClassification<decimal, double> CreateDefaultParameters(
        int neighborsCount = 8,
        int lookbackPeriod = 100,
        int normalizationWindow = 20)
    {
        return new PLorentzianClassification<decimal, double>
        {
            NeighborsCount = neighborsCount,
            LookbackPeriod = lookbackPeriod,
            NormalizationWindow = normalizationWindow,
            RSIPeriod = 14,
            CCIPeriod = 20,
            ADXPeriod = 14,
            MinConfidence = 0.6,
            LabelLookahead = 5,
            LabelThreshold = 0.01
        };
    }

    private static List<OHLC<decimal>> GenerateTestData(int count, int? seed = null)
    {
        var generator = seed.HasValue ? 
            new TestDataGenerator(seed.Value) : 
            new TestDataGenerator();
            
        var marketData = generator.GenerateRealisticData(count);
        
        return marketData.Select(d => new OHLC<decimal>
        {
            Open = d.Open,
            High = d.High,
            Low = d.Low,
            Close = d.Close
        }).ToList();
    }

    private static List<OHLC<decimal>> GenerateTestDataForCondition(MarketCondition condition, int count)
    {
        var generator = new TestDataGenerator(42); // Fixed seed for consistency
        
        var marketData = condition switch
        {
            MarketCondition.Trending => generator.GenerateTrendingData(count, bullish: true),
            MarketCondition.Sideways => generator.GenerateSidewaysData(count),
            MarketCondition.Volatile => generator.GenerateVolatileData(count),
            _ => generator.GenerateRealisticData(count)
        };
        
        return marketData.Select(d => new OHLC<decimal>
        {
            Open = d.Open,
            High = d.High,
            Low = d.Low,
            Close = d.Close
        }).ToList();
    }

    private static (double signal, double confidence) ProcessDataAndGetResults(
        LorentzianClassification_FP<decimal, double> indicator,
        List<OHLC<decimal>> testData)
    {
        indicator.OnBarBatch(testData, null);
        return (indicator.Signal, indicator.Confidence);
    }

    private static double CalculateStandardDeviation(List<double> values)
    {
        var mean = values.Average();
        var sumOfSquares = values.Sum(x => Math.Pow(x - mean, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }

    #endregion
}

/// <summary>
/// Market condition enumeration for test data generation
/// </summary>
public enum MarketCondition
{
    Trending,
    Sideways,
    Volatile
}

/// <summary>
/// Simple test data generator for performance tests
/// </summary>
public class TestDataGenerator
{
    private readonly Random _random;

    public TestDataGenerator(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public class MarketDataPoint
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }

    public List<MarketDataPoint> GenerateRealisticData(int count, decimal startPrice = 100m)
    {
        var data = new List<MarketDataPoint>(count);
        var currentPrice = startPrice;

        for (int i = 0; i < count; i++)
        {
            var change = (decimal)(_random.NextDouble() - 0.5) * 0.02m;
            currentPrice *= (1 + change);
            
            var open = i > 0 ? data[i - 1].Close : currentPrice;
            var close = currentPrice;
            var volatility = (decimal)(_random.NextDouble() * 0.01 + 0.005);
            var high = Math.Max(open, close) * (1 + volatility);
            var low = Math.Min(open, close) * (1 - volatility);

            data.Add(new MarketDataPoint
            {
                Open = open,
                High = high,
                Low = low,
                Close = close
            });
        }

        return data;
    }

    public List<MarketDataPoint> GenerateTrendingData(int count, decimal startPrice = 100m, bool bullish = true)
    {
        var data = new List<MarketDataPoint>(count);
        var currentPrice = startPrice;
        var trendStrength = bullish ? 0.001m : -0.001m;

        for (int i = 0; i < count; i++)
        {
            var trendComponent = trendStrength * (1 + (decimal)Math.Sin(i * 0.1) * 0.5m);
            var noise = (decimal)(_random.NextDouble() - 0.5) * 0.01m;
            
            currentPrice *= (1 + trendComponent + noise);
            
            var open = i > 0 ? data[i - 1].Close : currentPrice;
            var close = currentPrice;
            var volatility = (decimal)(_random.NextDouble() * 0.005 + 0.002);
            var high = Math.Max(open, close) * (1 + volatility);
            var low = Math.Min(open, close) * (1 - volatility);

            data.Add(new MarketDataPoint
            {
                Open = open,
                High = high,
                Low = low,
                Close = close
            });
        }

        return data;
    }

    public List<MarketDataPoint> GenerateSidewaysData(int count, decimal centerPrice = 100m)
    {
        var data = new List<MarketDataPoint>(count);
        var rangePercent = 0.05m;

        for (int i = 0; i < count; i++)
        {
            var oscillation = (decimal)Math.Sin(i * 0.2) * rangePercent * centerPrice;
            var noise = (decimal)(_random.NextDouble() - 0.5) * 0.01m * centerPrice;
            var currentPrice = centerPrice + oscillation + noise;
            
            var open = i > 0 ? data[i - 1].Close : currentPrice;
            var close = currentPrice;
            var volatility = (decimal)(_random.NextDouble() * 0.005 + 0.002);
            var high = Math.Max(open, close) * (1 + volatility);
            var low = Math.Min(open, close) * (1 - volatility);

            data.Add(new MarketDataPoint
            {
                Open = open,
                High = high,
                Low = low,
                Close = close
            });
        }

        return data;
    }

    public List<MarketDataPoint> GenerateVolatileData(int count, decimal startPrice = 100m)
    {
        var data = new List<MarketDataPoint>(count);
        var currentPrice = startPrice;

        for (int i = 0; i < count; i++)
        {
            var volatility = (decimal)(_random.NextDouble() * 0.05 + 0.01);
            var change = (decimal)(_random.NextDouble() - 0.5) * 2 * volatility;
            
            currentPrice *= (1 + change);
            
            var open = i > 0 ? data[i - 1].Close : currentPrice;
            var close = currentPrice;
            var intrabarVol = volatility * 2;
            var high = Math.Max(open, close) * (1 + intrabarVol);
            var low = Math.Min(open, close) * (1 - intrabarVol);

            data.Add(new MarketDataPoint
            {
                Open = open,
                High = high,
                Low = low,
                Close = close
            });
        }

        return data;
    }
}