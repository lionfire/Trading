using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ZigZagBenchmark : IndicatorBenchmarkBase
{
    private ZigZag_FP<decimal, decimal> _fpZigZag = null!;
    private PZigZag<HLC<decimal>, decimal> _parameters = null!;
    
    private ZigZag_FP<decimal, decimal> _fpZigZagSensitive = null!;
    private ZigZag_FP<decimal, decimal> _fpZigZagConservative = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PZigZag<HLC<decimal>, decimal> 
        { 
            DeviationPercent = 5.0m,
            Depth = 12,
            Backstep = 3,
            MaxPivotHistory = 100
        };
        _fpZigZag = new ZigZag_FP<decimal, decimal>(_parameters);
        
        // Sensitive (more zigzag lines)
        _fpZigZagSensitive = new ZigZag_FP<decimal, decimal>(new PZigZag<HLC<decimal>, decimal> 
        { 
            DeviationPercent = 2.0m,
            Depth = 8,
            Backstep = 2,
            MaxPivotHistory = 100
        });
        
        // Conservative (fewer zigzag lines)
        _fpZigZagConservative = new ZigZag_FP<decimal, decimal>(new PZigZag<HLC<decimal>, decimal> 
        { 
            DeviationPercent = 10.0m,
            Depth = 20,
            Backstep = 5,
            MaxPivotHistory = 100
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ZigZag", "FinancialPython")]
    public decimal[] ZigZag_FinancialPython_Batch()
    {
        var output = new decimal[DataSize];
        
        _fpZigZag.Clear();
        _fpZigZag.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("ZigZag", "FinancialPython")]
    public List<decimal?> ZigZag_FinancialPython_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _fpZigZag.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpZigZag.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_fpZigZag.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("ZigZag", "PivotIdentification")]
    public (int pivot_highs, int pivot_lows, int total_pivots) ZigZag_IdentifyPivots()
    {
        _fpZigZag.Clear();
        var output = new decimal[1];
        int pivotHighs = 0;
        int pivotLows = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpZigZag.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpZigZag.IsReady && _fpZigZag.RecentPivots != null)
            {
                var pivots = _fpZigZag.RecentPivots;
                pivotHighs = pivots.Count(p => p.IsHigh);
                pivotLows = pivots.Count(p => !p.IsHigh);
            }
        }
        
        return (pivotHighs, pivotLows, pivotHighs + pivotLows);
    }

    [Benchmark]
    [BenchmarkCategory("ZigZag", "SwingAnalysis")]
    public (decimal avg_swing_size, decimal max_swing, decimal min_swing) ZigZag_AnalyzeSwings()
    {
        _fpZigZag.Clear();
        var output = new decimal[1];
        var swingSizes = new List<decimal>();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpZigZag.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpZigZag.IsReady && _fpZigZag.RecentPivots != null)
            {
                var pivots = _fpZigZag.RecentPivots.ToList();
                
                // Calculate swing sizes between consecutive pivots
                for (int j = 1; j < pivots.Count; j++)
                {
                    var swingSize = Math.Abs(pivots[j].Price - pivots[j-1].Price);
                    swingSizes.Add(swingSize);
                }
            }
        }
        
        if (swingSizes.Count == 0)
            return (0, 0, 0);
            
        return (swingSizes.Average(), swingSizes.Max(), swingSizes.Min());
    }

    [Benchmark]
    [BenchmarkCategory("ZigZag", "TrendDirection")]
    public (int uptrend_periods, int downtrend_periods, int sideways_periods) ZigZag_AnalyzeTrends()
    {
        _fpZigZag.Clear();
        var output = new decimal[1];
        int uptrendPeriods = 0;
        int downtrendPeriods = 0;
        int sidewaysPeriods = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpZigZag.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpZigZag.IsReady)
            {
                var direction = _fpZigZag.Direction;
                
                if (direction > 0) uptrendPeriods++;
                else if (direction < 0) downtrendPeriods++;
                else sidewaysPeriods++;
            }
        }
        
        return (uptrendPeriods, downtrendPeriods, sidewaysPeriods);
    }

    [Benchmark]
    [BenchmarkCategory("ZigZag", "SensitivityComparison")]
    public (int sensitive_pivots, int standard_pivots, int conservative_pivots) ZigZag_CompareSensitivity()
    {
        _fpZigZagSensitive.Clear();
        _fpZigZag.Clear();
        _fpZigZagConservative.Clear();
        
        var outputSensitive = new decimal[1];
        var outputStandard = new decimal[1];
        var outputConservative = new decimal[1];
        
        int sensitivePivots = 0;
        int standardPivots = 0;
        int conservativePivots = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpZigZagSensitive.OnBarBatch(new[] { HLCData[i] }, outputSensitive);
            _fpZigZag.OnBarBatch(new[] { HLCData[i] }, outputStandard);
            _fpZigZagConservative.OnBarBatch(new[] { HLCData[i] }, outputConservative);
        }
        
        // Count final pivot counts
        if (_fpZigZagSensitive.RecentPivots != null)
            sensitivePivots = _fpZigZagSensitive.RecentPivots.Count;
            
        if (_fpZigZag.RecentPivots != null)
            standardPivots = _fpZigZag.RecentPivots.Count;
            
        if (_fpZigZagConservative.RecentPivots != null)
            conservativePivots = _fpZigZagConservative.RecentPivots.Count;
        
        return (sensitivePivots, standardPivots, conservativePivots);
    }

    [Benchmark]
    [BenchmarkCategory("ZigZag", "ReversalDetection")]
    public (int trend_reversals, decimal avg_reversal_size) ZigZag_DetectReversals()
    {
        _fpZigZag.Clear();
        var output = new decimal[1];
        var reversals = new List<decimal>();
        int previousDirection = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpZigZag.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpZigZag.IsReady)
            {
                var currentDirection = _fpZigZag.Direction;
                
                // Detect direction change (reversal)
                if (previousDirection != 0 && currentDirection != 0 && 
                    previousDirection != currentDirection)
                {
                    // Calculate reversal magnitude
                    var reversalSize = Math.Abs(_fpZigZag.LastPivotHigh - _fpZigZag.LastPivotLow);
                    if (reversalSize > 0)
                        reversals.Add(reversalSize);
                }
                
                previousDirection = currentDirection;
            }
        }
        
        decimal avgReversalSize = reversals.Count > 0 ? reversals.Average() : 0;
        return (reversals.Count, avgReversalSize);
    }

    [Benchmark]
    [BenchmarkCategory("ZigZag", "SupportResistance")]
    public (int support_tests, int resistance_tests, decimal level_accuracy) ZigZag_TestSupportResistance()
    {
        _fpZigZag.Clear();
        var output = new decimal[1];
        int supportTests = 0;
        int resistanceTests = 0;
        int accurateTests = 0;
        int totalTests = 0;
        
        for (int i = 0; i < HLCData.Length - 1; i++)
        {
            _fpZigZag.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpZigZag.IsReady && _fpZigZag.RecentPivots != null)
            {
                var pivots = _fpZigZag.RecentPivots.ToList();
                var currentBar = HLCData[i];
                var nextBar = HLCData[i + 1];
                
                // Test recent pivot levels as support/resistance
                foreach (var pivot in pivots.TakeLast(5)) // Test last 5 pivots
                {
                    decimal tolerance = pivot.Price * 0.01m; // 1% tolerance
                    
                    // Check if current price is near the pivot level
                    if (Math.Abs(currentBar.Close - pivot.Price) <= tolerance)
                    {
                        totalTests++;
                        
                        if (pivot.IsHigh) // Resistance test
                        {
                            resistanceTests++;
                            // Check if price bounced down
                            if (nextBar.Close < currentBar.Close)
                                accurateTests++;
                        }
                        else // Support test
                        {
                            supportTests++;
                            // Check if price bounced up
                            if (nextBar.Close > currentBar.Close)
                                accurateTests++;
                        }
                    }
                }
            }
        }
        
        decimal levelAccuracy = totalTests > 0 ? (decimal)accurateTests / totalTests : 0;
        return (supportTests, resistanceTests, levelAccuracy);
    }

    [Benchmark]
    [BenchmarkCategory("ZigZag", "WavePatterns")]
    public (int elliott_waves, int abc_patterns, int flag_patterns) ZigZag_IdentifyPatterns()
    {
        _fpZigZag.Clear();
        var output = new decimal[1];
        int elliottWaves = 0;
        int abcPatterns = 0;
        int flagPatterns = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpZigZag.OnBarBatch(new[] { HLCData[i] }, output);
        }
        
        if (_fpZigZag.RecentPivots != null)
        {
            var pivots = _fpZigZag.RecentPivots.ToList();
            
            // Look for 5-wave Elliott patterns
            for (int i = 4; i < pivots.Count; i++)
            {
                var wave1 = Math.Abs(pivots[i-3].Price - pivots[i-4].Price);
                var wave2 = Math.Abs(pivots[i-2].Price - pivots[i-3].Price);
                var wave3 = Math.Abs(pivots[i-1].Price - pivots[i-2].Price);
                var wave4 = Math.Abs(pivots[i].Price - pivots[i-1].Price);
                
                // Simple Elliott wave detection (wave 3 is typically the longest)
                if (wave3 > wave1 && wave3 > wave4 && wave2 < wave1 * 0.8m)
                    elliottWaves++;
            }
            
            // Look for ABC correction patterns
            for (int i = 2; i < pivots.Count; i++)
            {
                if (i >= 2)
                {
                    var a = Math.Abs(pivots[i-2].Price - pivots[i-1].Price);
                    var b = Math.Abs(pivots[i-1].Price - pivots[i].Price);
                    
                    // ABC pattern: wave C similar in size to wave A
                    if (Math.Abs(a - b) / Math.Max(a, b) < 0.2m)
                        abcPatterns++;
                }
            }
        }
        
        return (elliottWaves, abcPatterns, flagPatterns);
    }

    [Benchmark]
    [BenchmarkCategory("ZigZag", "NoiseFiltering")]
    public (decimal original_noise, decimal filtered_noise, decimal noise_reduction) ZigZag_MeasureNoiseFiltering()
    {
        _fpZigZag.Clear();
        var output = new decimal[1];
        
        // Calculate original price noise
        var originalChanges = new List<decimal>();
        for (int i = 1; i < HLCData.Length; i++)
        {
            var change = Math.Abs(HLCData[i].Close - HLCData[i-1].Close);
            originalChanges.Add(change);
        }
        
        // Run ZigZag and measure filtered changes
        var filteredChanges = new List<decimal>();
        decimal? previousZigZagValue = null;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpZigZag.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpZigZag.IsReady)
            {
                var currentZigZagValue = _fpZigZag.CurrentValue;
                
                if (previousZigZagValue.HasValue)
                {
                    var change = Math.Abs(currentZigZagValue - previousZigZagValue.Value);
                    if (change > 0)
                        filteredChanges.Add(change);
                }
                
                previousZigZagValue = currentZigZagValue;
            }
        }
        
        decimal originalNoise = originalChanges.Count > 0 ? originalChanges.Average() : 0;
        decimal filteredNoise = filteredChanges.Count > 0 ? filteredChanges.Average() : 0;
        decimal noiseReduction = originalNoise > 0 ? (originalNoise - filteredNoise) / originalNoise : 0;
        
        return (originalNoise, filteredNoise, noiseReduction);
    }

    [Benchmark]
    [BenchmarkCategory("ZigZag", "PivotSpacing")]
    public (decimal avg_pivot_spacing, int min_spacing, int max_spacing) ZigZag_AnalyzePivotSpacing()
    {
        _fpZigZag.Clear();
        var output = new decimal[1];
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpZigZag.OnBarBatch(new[] { HLCData[i] }, output);
        }
        
        if (_fpZigZag.RecentPivots == null || _fpZigZag.RecentPivots.Count < 2)
            return (0, 0, 0);
            
        var pivots = _fpZigZag.RecentPivots.ToList();
        var spacings = new List<int>();
        
        for (int i = 1; i < pivots.Count; i++)
        {
            var spacing = pivots[i].BarIndex - pivots[i-1].BarIndex;
            spacings.Add(spacing);
        }
        
        if (spacings.Count == 0)
            return (0, 0, 0);
            
        return (spacings.Average(), spacings.Min(), spacings.Max());
    }
    
    [Benchmark]
    [BenchmarkCategory("ZigZag", "Memory")]
    public long ZigZag_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new ZigZag_FP<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _fpZigZag?.Clear();
        _fpZigZagSensitive?.Clear();
        _fpZigZagConservative?.Clear();
        base.GlobalCleanup();
    }
}