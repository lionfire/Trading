using LionFire.Trading.Indicators.Defaults;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading;
using System;
using System.Linq;

namespace LionFire.Trading.Indicators.Examples;

/// <summary>
/// Example demonstrating how to use the ZigZag indicator.
/// This indicator identifies significant price swing highs and lows by filtering out minor price movements.
/// </summary>
public static class ZigZagExample
{
    /// <summary>
    /// Basic usage example with default parameters
    /// </summary>
    public static void BasicUsage()
    {
        // Create ZigZag indicator with default parameters (5% deviation, 12 bars depth)
        var zigzag = ZigZagCommon.Create();
        
        // Subscribe to ZigZag results
        zigzag.Subscribe(results =>
        {
            if (zigzag.IsReady)
            {
                Console.WriteLine($"ZigZag Value: {results[0]:F2}, Direction: {GetDirectionText(zigzag.Direction)}");
            }
        });

        // Example market data with clear swings
        var marketData = new[]
        {
            new HLC<decimal> { High = 102.0m, Low = 99.0m, Close = 101.0m },
            new HLC<decimal> { High = 103.0m, Low = 100.0m, Close = 102.5m },
            new HLC<decimal> { High = 104.0m, Low = 101.0m, Close = 103.0m },
            new HLC<decimal> { High = 105.5m, Low = 102.0m, Close = 105.0m }, // Potential high
            new HLC<decimal> { High = 104.0m, Low = 100.0m, Close = 102.0m },
            new HLC<decimal> { High = 103.0m, Low = 98.0m, Close = 100.0m },
            new HLC<decimal> { High = 101.0m, Low = 95.0m, Close = 97.5m },  // Potential low
            new HLC<decimal> { High = 102.0m, Low = 97.0m, Close = 100.0m },
            new HLC<decimal> { High = 105.0m, Low = 99.0m, Close = 103.0m },
            // ... more data
        };

        // Process market data
        zigzag.OnNext(marketData);

        // Check current values
        Console.WriteLine($"Current ZigZag Value: {zigzag.CurrentValue:F2}");
        Console.WriteLine($"Last Pivot High: {zigzag.LastPivotHigh:F2}");
        Console.WriteLine($"Last Pivot Low: {zigzag.LastPivotLow:F2}");
        Console.WriteLine($"Direction: {GetDirectionText(zigzag.Direction)}");
        Console.WriteLine($"Is Ready: {zigzag.IsReady}");
        
        // Display recent pivot points
        if (zigzag.RecentPivots != null && zigzag.RecentPivots.Count > 0)
        {
            Console.WriteLine("\nRecent Pivot Points:");
            foreach (var pivot in zigzag.RecentPivots.TakeLast(5))
            {
                Console.WriteLine($"  {pivot}");
            }
        }
    }

    /// <summary>
    /// Advanced usage example with custom parameters for different sensitivities
    /// </summary>
    public static void AdvancedUsage()
    {
        Console.WriteLine("=== ZigZag Sensitivity Comparison ===\n");

        // Create ZigZag indicators with different sensitivities
        var sensitiveZZ = ZigZagCommon.Create(deviation: 2.0m, depth: 5);    // More sensitive
        var standardZZ = ZigZagCommon.Create(deviation: 5.0m, depth: 12);    // Standard
        var conservativeZZ = ZigZagCommon.Create(deviation: 10.0m, depth: 20); // Less sensitive

        var indicators = new[]
        {
            ("Sensitive (2%, 5)", sensitiveZZ),
            ("Standard (5%, 12)", standardZZ),
            ("Conservative (10%, 20)", conservativeZZ)
        };

        // Subscribe to all indicators
        foreach (var (name, indicator) in indicators)
        {
            indicator.Subscribe(results =>
            {
                if (indicator.IsReady && indicator.RecentPivots?.Count > 0)
                {
                    var lastPivot = indicator.RecentPivots.Last();
                    Console.WriteLine($"{name}: New Pivot - {lastPivot}");
                }
            });
        }

        // Generate test data with various swing sizes
        var testData = GenerateTestData();
        
        // Process data through all indicators
        foreach (var data in testData)
        {
            foreach (var (_, indicator) in indicators)
            {
                indicator.OnNext(data);
            }
        }

        // Compare results
        Console.WriteLine("\n=== Comparison Results ===");
        foreach (var (name, indicator) in indicators)
        {
            var pivotCount = indicator.RecentPivots?.Count ?? 0;
            Console.WriteLine($"{name}: {pivotCount} pivot points detected");
            Console.WriteLine($"  Current Value: {indicator.CurrentValue:F2}");
            Console.WriteLine($"  Direction: {GetDirectionText(indicator.Direction)}");
        }
    }

    /// <summary>
    /// Example for trend analysis using ZigZag
    /// </summary>
    public static void TrendAnalysisExample()
    {
        Console.WriteLine("=== ZigZag Trend Analysis ===\n");

        var zigzag = ZigZagCommon.Create(deviation: 5.0m, depth: 10);
        
        zigzag.Subscribe(results =>
        {
            if (zigzag.IsReady && zigzag.RecentPivots != null)
            {
                AnalyzeTrend(zigzag.RecentPivots);
            }
        });

        // Generate trending data
        var trendingData = GenerateTrendingData();
        
        // Process the data
        zigzag.OnNext(trendingData);

        Console.WriteLine($"Final Analysis - Current ZigZag: {zigzag.CurrentValue:F2}");
        Console.WriteLine($"Trend Direction: {GetDirectionText(zigzag.Direction)}");
    }

    /// <summary>
    /// Example for support and resistance level identification
    /// </summary>
    public static void SupportResistanceExample()
    {
        Console.WriteLine("=== Support & Resistance Identification ===\n");

        var zigzag = ZigZagCommon.Create(deviation: 4.0m, depth: 8);
        var supportLevels = new List<decimal>();
        var resistanceLevels = new List<decimal>();

        zigzag.Subscribe(results =>
        {
            if (zigzag.IsReady && zigzag.RecentPivots != null)
            {
                foreach (var pivot in zigzag.RecentPivots.Where(p => p.IsConfirmed))
                {
                    if (pivot.IsHigh)
                    {
                        resistanceLevels.Add(pivot.Price);
                    }
                    else
                    {
                        supportLevels.Add(pivot.Price);
                    }
                }
            }
        });

        // Generate data with clear support/resistance levels
        var srData = GenerateSupportResistanceData();
        zigzag.OnNext(srData);

        // Identify key levels (simplified approach)
        var keySupport = supportLevels.GroupBy(s => Math.Round(s, 1))
                                     .Where(g => g.Count() >= 2)
                                     .OrderByDescending(g => g.Count())
                                     .Take(3)
                                     .Select(g => g.Key)
                                     .ToList();

        var keyResistance = resistanceLevels.GroupBy(r => Math.Round(r, 1))
                                           .Where(g => g.Count() >= 2)
                                           .OrderByDescending(g => g.Count())
                                           .Take(3)
                                           .Select(g => g.Key)
                                           .ToList();

        Console.WriteLine("Key Support Levels:");
        keySupport.ForEach(level => Console.WriteLine($"  ${level:F1}"));
        
        Console.WriteLine("\nKey Resistance Levels:");
        keyResistance.ForEach(level => Console.WriteLine($"  ${level:F1}"));
    }

    /// <summary>
    /// Example showing custom parameter optimization
    /// </summary>
    public static void ParameterOptimizationExample()
    {
        Console.WriteLine("=== Parameter Optimization Example ===\n");

        var testData = GenerateTestData();
        var bestScore = 0.0;
        var bestDeviation = 0.0m;
        var bestDepth = 0;

        // Test different parameter combinations
        var deviations = new[] { 2.0m, 3.0m, 5.0m, 7.0m, 10.0m };
        var depths = new[] { 5, 8, 12, 15, 20 };

        foreach (var deviation in deviations)
        {
            foreach (var depth in depths)
            {
                var parameters = new PZigZag<HLC<decimal>, decimal>
                {
                    Deviation = deviation,
                    Depth = depth,
                    MaxPivotHistory = 50
                };

                var zigzag = ZigZag.Create(parameters);
                zigzag.OnNext(testData);

                // Simple scoring based on number of pivots and trend capture
                var score = CalculateZigZagScore(zigzag);
                
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDeviation = deviation;
                    bestDepth = depth;
                }

                Console.WriteLine($"Deviation: {deviation:F1}%, Depth: {depth}, Score: {score:F2}");
            }
        }

        Console.WriteLine($"\nBest Parameters:");
        Console.WriteLine($"  Deviation: {bestDeviation:F1}%");
        Console.WriteLine($"  Depth: {bestDepth}");
        Console.WriteLine($"  Score: {bestScore:F2}");
    }

    #region Helper Methods

    private static string GetDirectionText(int direction)
    {
        return direction switch
        {
            1 => "UP",
            -1 => "DOWN", 
            _ => "NEUTRAL"
        };
    }

    private static void AnalyzeTrend(IReadOnlyList<ZigZagPivot<decimal>> pivots)
    {
        if (pivots.Count < 4) return;

        var recentPivots = pivots.TakeLast(4).ToArray();
        var highs = recentPivots.Where(p => p.IsHigh).Select(p => p.Price).ToArray();
        var lows = recentPivots.Where(p => !p.IsHigh).Select(p => p.Price).ToArray();

        if (highs.Length >= 2 && lows.Length >= 2)
        {
            var isHigherHighs = highs[^1] > highs[^2];
            var isHigherLows = lows[^1] > lows[^2];
            
            var trend = (isHigherHighs, isHigherLows) switch
            {
                (true, true) => "UPTREND (Higher Highs & Higher Lows)",
                (false, false) => "DOWNTREND (Lower Highs & Lower Lows)",
                (true, false) => "CONSOLIDATION (Higher Highs, Lower Lows)",
                (false, true) => "REVERSAL PATTERN (Lower Highs, Higher Lows)"
            };

            Console.WriteLine($"Trend Analysis: {trend}");
        }
    }

    private static List<HLC<decimal>> GenerateTestData()
    {
        var data = new List<HLC<decimal>>();
        var random = new Random(42);
        decimal price = 100.0m;

        for (int i = 0; i < 100; i++)
        {
            // Create price movement with various swing sizes
            var trend = (decimal)(Math.Sin(i * 0.1) * 5); // Long-term movement
            var noise = (decimal)((random.NextDouble() - 0.5) * 4); // Random noise
            price += trend + noise;

            var spread = (decimal)(random.NextDouble() * 2 + 0.5);
            var high = price + spread;
            var low = price - spread;

            data.Add(new HLC<decimal> 
            { 
                High = high, 
                Low = low, 
                Close = price 
            });
        }

        return data;
    }

    private static List<HLC<decimal>> GenerateTrendingData()
    {
        var data = new List<HLC<decimal>>();
        decimal price = 100.0m;

        for (int i = 0; i < 50; i++)
        {
            // Create uptrend with pullbacks
            var trend = i < 25 ? 0.5m : -0.3m; // Up then down
            var volatility = (decimal)(Math.Sin(i * 0.3) * 2);
            
            price += trend + volatility;
            
            var spread = 1.0m;
            var high = price + spread;
            var low = price - spread;

            data.Add(new HLC<decimal> 
            { 
                High = high, 
                Low = low, 
                Close = price 
            });
        }

        return data;
    }

    private static List<HLC<decimal>> GenerateSupportResistanceData()
    {
        var data = new List<HLC<decimal>>();
        decimal price = 100.0m;
        var support = 95.0m;
        var resistance = 105.0m;

        for (int i = 0; i < 80; i++)
        {
            // Bounce between support and resistance
            if (price <= support + 1) price += 2.0m;
            if (price >= resistance - 1) price -= 2.0m;
            
            var noise = (decimal)((new Random(i).NextDouble() - 0.5) * 1.5);
            price += noise;

            var spread = 0.8m;
            var high = Math.Min(price + spread, resistance + 0.5m);
            var low = Math.Max(price - spread, support - 0.5m);

            data.Add(new HLC<decimal> 
            { 
                High = high, 
                Low = low, 
                Close = price 
            });
        }

        return data;
    }

    private static double CalculateZigZagScore(IZigZag<HLC<decimal>, decimal> zigzag)
    {
        if (!zigzag.IsReady || zigzag.RecentPivots == null)
            return 0.0;

        var pivotCount = zigzag.RecentPivots.Count;
        var confirmedPivots = zigzag.RecentPivots.Count(p => p.IsConfirmed);
        
        // Simple scoring: balance between capturing trends and avoiding noise
        // Prefer 5-15 pivots, penalize too few or too many
        var pivotScore = pivotCount switch
        {
            < 3 => 0.1,
            >= 3 and <= 15 => 1.0,
            > 15 and <= 25 => 0.7,
            _ => 0.3
        };

        var confirmationRatio = pivotCount > 0 ? (double)confirmedPivots / pivotCount : 0;
        
        return pivotScore * confirmationRatio;
    }

    #endregion
}