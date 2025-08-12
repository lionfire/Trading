using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class FibonacciRetracementBenchmark : IndicatorBenchmarkBase
{
    private FibonacciRetracement_FP<decimal, decimal> _fpFibonacci = null!;
    private PFibonacciRetracement<HLC<decimal>, decimal> _parameters = null!;
    
    private FibonacciRetracement_FP<decimal, decimal> _fpFibonacciShort = null!;
    private FibonacciRetracement_FP<decimal, decimal> _fpFibonacciNoExtensions = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PFibonacciRetracement<HLC<decimal>, decimal> 
        { 
            LookbackPeriod = 100,
            IncludeExtensionLevels = true
        };
        _fpFibonacci = new FibonacciRetracement_FP<decimal, decimal>(_parameters);
        
        // Short lookback period
        _fpFibonacciShort = new FibonacciRetracement_FP<decimal, decimal>(new PFibonacciRetracement<HLC<decimal>, decimal> 
        { 
            LookbackPeriod = 50,
            IncludeExtensionLevels = true
        });
        
        // Without extensions
        _fpFibonacciNoExtensions = new FibonacciRetracement_FP<decimal, decimal>(new PFibonacciRetracement<HLC<decimal>, decimal> 
        { 
            LookbackPeriod = 100,
            IncludeExtensionLevels = false
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("FibonacciRetracement", "FinancialPython")]
    public decimal[] FibonacciRetracement_FinancialPython_Batch()
    {
        var output = new decimal[DataSize * 9]; // 9 levels with extensions
        
        _fpFibonacci.Clear();
        _fpFibonacci.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("FibonacciRetracement", "FinancialPython")]
    public List<decimal[]?> FibonacciRetracement_FinancialPython_Streaming()
    {
        var results = new List<decimal[]?>(DataSize);
        var output = new decimal[9]; // 9 levels with extensions
        
        _fpFibonacci.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpFibonacci.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpFibonacci.IsReady)
            {
                var levelsCopy = new decimal[9];
                Array.Copy(output, levelsCopy, 9);
                results.Add(levelsCopy);
            }
            else
            {
                results.Add(null);
            }
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("FibonacciRetracement", "SupportResistance")]
    public (int level_236_hits, int level_382_hits, int level_500_hits, int level_618_hits) FibonacciRetracement_TestSupportResistance()
    {
        _fpFibonacci.Clear();
        var output = new decimal[9];
        int level236Hits = 0;
        int level382Hits = 0;
        int level500Hits = 0;
        int level618Hits = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpFibonacci.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpFibonacci.IsReady)
            {
                var currentClose = HLCData[i].Close;
                decimal tolerance = 0.002m; // 0.2% tolerance
                
                // Check if price is near Fibonacci levels
                var level236 = _fpFibonacci.Level236;
                var level382 = _fpFibonacci.Level382;
                var level500 = _fpFibonacci.Level500;
                var level618 = _fpFibonacci.Level618;
                
                if (Math.Abs(currentClose - level236) <= level236 * tolerance) level236Hits++;
                if (Math.Abs(currentClose - level382) <= level382 * tolerance) level382Hits++;
                if (Math.Abs(currentClose - level500) <= level500 * tolerance) level500Hits++;
                if (Math.Abs(currentClose - level618) <= level618 * tolerance) level618Hits++;
            }
        }
        
        return (level236Hits, level382Hits, level500Hits, level618Hits);
    }

    [Benchmark]
    [BenchmarkCategory("FibonacciRetracement", "SwingPointAnalysis")]
    public (decimal avg_swing_range, int swing_point_changes) FibonacciRetracement_SwingPointAnalysis()
    {
        _fpFibonacci.Clear();
        var output = new decimal[9];
        var swingRanges = new List<decimal>();
        decimal previousSwingHigh = 0;
        decimal previousSwingLow = 0;
        int swingPointChanges = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpFibonacci.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpFibonacci.IsReady)
            {
                var currentSwingHigh = _fpFibonacci.SwingHigh;
                var currentSwingLow = _fpFibonacci.SwingLow;
                
                // Track swing point changes
                if (previousSwingHigh != 0 && 
                    (currentSwingHigh != previousSwingHigh || currentSwingLow != previousSwingLow))
                {
                    swingPointChanges++;
                }
                
                // Calculate swing range
                var swingRange = currentSwingHigh - currentSwingLow;
                swingRanges.Add(swingRange);
                
                previousSwingHigh = currentSwingHigh;
                previousSwingLow = currentSwingLow;
            }
        }
        
        decimal avgSwingRange = swingRanges.Count > 0 ? swingRanges.Average() : 0;
        return (avgSwingRange, swingPointChanges);
    }

    [Benchmark]
    [BenchmarkCategory("FibonacciRetracement", "ExtensionLevels")]
    public (int extension_1618_tests, int extension_2618_tests, decimal max_extension) FibonacciRetracement_TestExtensionLevels()
    {
        _fpFibonacci.Clear();
        var output = new decimal[9];
        int extension1618Tests = 0;
        int extension2618Tests = 0;
        decimal maxExtension = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpFibonacci.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpFibonacci.IsReady)
            {
                var currentPrice = HLCData[i].Close;
                var level1618 = _fpFibonacci.Level1618;
                var level2618 = _fpFibonacci.Level2618;
                var swingHigh = _fpFibonacci.SwingHigh;
                
                // Test if price reaches extension levels
                if (currentPrice > level1618) extension1618Tests++;
                if (currentPrice > level2618) extension2618Tests++;
                
                // Track maximum extension beyond swing high
                if (currentPrice > swingHigh)
                {
                    var extension = (currentPrice - swingHigh) / (swingHigh - _fpFibonacci.SwingLow);
                    maxExtension = Math.Max(maxExtension, extension);
                }
            }
        }
        
        return (extension1618Tests, extension2618Tests, maxExtension);
    }

    [Benchmark]
    [BenchmarkCategory("FibonacciRetracement", "LookbackComparison")]
    public (decimal short_avg_range, decimal long_avg_range, int short_changes, int long_changes) FibonacciRetracement_CompareLookbacks()
    {
        _fpFibonacciShort.Clear();
        _fpFibonacci.Clear();
        
        var outputShort = new decimal[9];
        var outputLong = new decimal[9];
        
        var shortRanges = new List<decimal>();
        var longRanges = new List<decimal>();
        int shortChanges = 0;
        int longChanges = 0;
        
        decimal prevShortHigh = 0, prevShortLow = 0;
        decimal prevLongHigh = 0, prevLongLow = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpFibonacciShort.OnBarBatch(new[] { HLCData[i] }, outputShort);
            _fpFibonacci.OnBarBatch(new[] { HLCData[i] }, outputLong);
            
            if (_fpFibonacciShort.IsReady)
            {
                var shortHigh = _fpFibonacciShort.SwingHigh;
                var shortLow = _fpFibonacciShort.SwingLow;
                shortRanges.Add(shortHigh - shortLow);
                
                if (prevShortHigh != 0 && (shortHigh != prevShortHigh || shortLow != prevShortLow))
                    shortChanges++;
                
                prevShortHigh = shortHigh;
                prevShortLow = shortLow;
            }
            
            if (_fpFibonacci.IsReady)
            {
                var longHigh = _fpFibonacci.SwingHigh;
                var longLow = _fpFibonacci.SwingLow;
                longRanges.Add(longHigh - longLow);
                
                if (prevLongHigh != 0 && (longHigh != prevLongHigh || longLow != prevLongLow))
                    longChanges++;
                
                prevLongHigh = longHigh;
                prevLongLow = longLow;
            }
        }
        
        decimal shortAvgRange = shortRanges.Count > 0 ? shortRanges.Average() : 0;
        decimal longAvgRange = longRanges.Count > 0 ? longRanges.Average() : 0;
        
        return (shortAvgRange, longAvgRange, shortChanges, longChanges);
    }

    [Benchmark]
    [BenchmarkCategory("FibonacciRetracement", "RetracementDepth")]
    public (int shallow_retracements, int deep_retracements, decimal avg_retracement_depth) FibonacciRetracement_AnalyzeRetracementDepth()
    {
        _fpFibonacci.Clear();
        var output = new decimal[9];
        int shallowRetracements = 0; // < 38.2%
        int deepRetracements = 0;    // > 61.8%
        var retracementDepths = new List<decimal>();
        
        for (int i = 100; i < HLCData.Length - 10; i++)
        {
            _fpFibonacci.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpFibonacci.IsReady)
            {
                var swingHigh = _fpFibonacci.SwingHigh;
                var swingLow = _fpFibonacci.SwingLow;
                var currentPrice = HLCData[i].Close;
                
                // Calculate retracement depth
                if (swingHigh > swingLow)
                {
                    var retracement = (swingHigh - currentPrice) / (swingHigh - swingLow);
                    
                    if (retracement > 0 && retracement < 1)
                    {
                        retracementDepths.Add(retracement);
                        
                        if (retracement < 0.382m) shallowRetracements++;
                        else if (retracement > 0.618m) deepRetracements++;
                    }
                }
            }
        }
        
        decimal avgRetracementDepth = retracementDepths.Count > 0 ? retracementDepths.Average() : 0;
        return (shallowRetracements, deepRetracements, avgRetracementDepth);
    }

    [Benchmark]
    [BenchmarkCategory("FibonacciRetracement", "LevelAccuracy")]
    public decimal FibonacciRetracement_MeasureLevelAccuracy()
    {
        _fpFibonacci.Clear();
        var output = new decimal[9];
        int totalTouches = 0;
        int accurateBounces = 0;
        
        for (int i = 0; i < HLCData.Length - 1; i++)
        {
            _fpFibonacci.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpFibonacci.IsReady)
            {
                var currentBar = HLCData[i];
                var nextBar = HLCData[i + 1];
                decimal tolerance = 0.003m; // 0.3% tolerance
                
                // Check all Fibonacci levels
                var levels = new decimal[] 
                {
                    _fpFibonacci.Level236, _fpFibonacci.Level382, _fpFibonacci.Level500, 
                    _fpFibonacci.Level618, _fpFibonacci.Level786
                };
                
                foreach (var level in levels)
                {
                    // Check if price touched this level
                    if (currentBar.Low <= level * (1 + tolerance) && currentBar.High >= level * (1 - tolerance))
                    {
                        totalTouches++;
                        
                        // Check if it provided support/resistance (price bounced)
                        bool bounced = false;
                        
                        if (level < currentBar.Close) // Resistance test
                        {
                            bounced = nextBar.Close < currentBar.Close;
                        }
                        else // Support test  
                        {
                            bounced = nextBar.Close > currentBar.Close;
                        }
                        
                        if (bounced) accurateBounces++;
                    }
                }
            }
        }
        
        return totalTouches > 0 ? (decimal)accurateBounces / totalTouches : 0;
    }

    [Benchmark]
    [BenchmarkCategory("FibonacciRetracement", "PriceTargeting")]
    public int FibonacciRetracement_PredictPriceTargets()
    {
        _fpFibonacci.Clear();
        var output = new decimal[9];
        int successfulTargets = 0;
        int lookforward = 10;
        
        for (int i = 150; i < HLCData.Length - lookforward; i++)
        {
            _fpFibonacci.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpFibonacci.IsReady)
            {
                var currentPrice = HLCData[i].Close;
                var level618 = _fpFibonacci.Level618;
                var level500 = _fpFibonacci.Level500;
                
                // If price is near 61.8% level, predict it will test 50% level
                if (Math.Abs(currentPrice - level618) <= level618 * 0.01m) // Within 1%
                {
                    // Check if price reaches 50% level in next periods
                    for (int j = i + 1; j <= i + lookforward; j++)
                    {
                        if (Math.Abs(HLCData[j].Close - level500) <= level500 * 0.015m)
                        {
                            successfulTargets++;
                            break;
                        }
                    }
                }
            }
        }
        
        return successfulTargets;
    }
    
    [Benchmark]
    [BenchmarkCategory("FibonacciRetracement", "Memory")]
    public long FibonacciRetracement_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new FibonacciRetracement_FP<decimal, decimal>(_parameters);
            var output = new decimal[DataSize * 9]; // 9 levels with extensions
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _fpFibonacci?.Clear();
        _fpFibonacciShort?.Clear();
        _fpFibonacciNoExtensions?.Clear();
        base.GlobalCleanup();
    }
}