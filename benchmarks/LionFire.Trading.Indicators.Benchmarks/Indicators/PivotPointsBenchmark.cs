using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class PivotPointsBenchmark : IndicatorBenchmarkBase
{
    private PivotPoints_QC<HLCData, decimal> _qcPivot = null!;
    private PPivotPoints<HLCData, decimal> _parameters = null!;
    
    private PivotPoints_QC<HLCData, decimal> _qcPivotStandard = null!;
    private PivotPoints_QC<HLCData, decimal> _qcPivotFibonacci = null!;
    private PivotPoints_QC<HLCData, decimal> _qcPivotCamarilla = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PPivotPoints<HLCData, decimal> 
        { 
            PivotPointType = PivotPointType.Standard
        };
        _qcPivot = new PivotPoints_QC<HLCData, decimal>(_parameters);
        
        // Standard pivot points (most common)
        _qcPivotStandard = new PivotPoints_QC<HLCData, decimal>(new PPivotPoints<HLCData, decimal> 
        { 
            PivotPointType = PivotPointType.Standard
        });
        
        // Fibonacci pivot points
        _qcPivotFibonacci = new PivotPoints_QC<HLCData, decimal>(new PPivotPoints<HLCData, decimal> 
        { 
            PivotPointType = PivotPointType.Fibonacci
        });
        
        // Camarilla pivot points
        _qcPivotCamarilla = new PivotPoints_QC<HLCData, decimal>(new PPivotPoints<HLCData, decimal> 
        { 
            PivotPointType = PivotPointType.Camarilla
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("PivotPoints", "QuantConnect")]
    public PivotPointsOutput<decimal>[] PivotPoints_QuantConnect_Batch()
    {
        var output = new PivotPointsOutput<decimal>[DataSize];
        
        _qcPivot.Clear();
        _qcPivot.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("PivotPoints", "QuantConnect")]
    public List<PivotPointsOutput<decimal>?> PivotPoints_QuantConnect_Streaming()
    {
        var results = new List<PivotPointsOutput<decimal>?>(DataSize);
        var output = new PivotPointsOutput<decimal>[1];
        
        _qcPivot.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcPivot.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcPivot.IsReady ? output[0] : null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("PivotPoints", "SupportResistance")]
    public (int support_tests, int resistance_tests) PivotPoints_TestSupportResistance()
    {
        _qcPivotStandard.Clear();
        var output = new PivotPointsOutput<decimal>[1];
        PivotPointsOutput<decimal>? previousPivots = null;
        int supportTests = 0;
        int resistanceTests = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcPivotStandard.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcPivotStandard.IsReady && output[0] != null && previousPivots != null)
            {
                var currentBar = HLCData[i];
                var pivots = output[0];
                
                // Test support levels
                decimal tolerance = 0.002m; // 0.2% tolerance
                
                // S1 support test
                if (currentBar.Low <= pivots.Support1 * (1 + tolerance) && 
                    currentBar.Low >= pivots.Support1 * (1 - tolerance))
                {
                    supportTests++;
                }
                
                // S2 support test
                if (currentBar.Low <= pivots.Support2 * (1 + tolerance) && 
                    currentBar.Low >= pivots.Support2 * (1 - tolerance))
                {
                    supportTests++;
                }
                
                // R1 resistance test
                if (currentBar.High >= pivots.Resistance1 * (1 - tolerance) && 
                    currentBar.High <= pivots.Resistance1 * (1 + tolerance))
                {
                    resistanceTests++;
                }
                
                // R2 resistance test
                if (currentBar.High >= pivots.Resistance2 * (1 - tolerance) && 
                    currentBar.High <= pivots.Resistance2 * (1 + tolerance))
                {
                    resistanceTests++;
                }
                
                previousPivots = pivots;
            }
            else if (_qcPivotStandard.IsReady && output[0] != null)
            {
                previousPivots = output[0];
            }
        }
        
        return (supportTests, resistanceTests);
    }

    [Benchmark]
    [BenchmarkCategory("PivotPoints", "TypeComparison")]
    public (decimal standard_range, decimal fibonacci_range, decimal camarilla_range) PivotPoints_CompareTypes()
    {
        _qcPivotStandard.Clear();
        _qcPivotFibonacci.Clear();
        _qcPivotCamarilla.Clear();
        
        var outputStandard = new PivotPointsOutput<decimal>[DataSize];
        var outputFibonacci = new PivotPointsOutput<decimal>[DataSize];
        var outputCamarilla = new PivotPointsOutput<decimal>[DataSize];
        
        _qcPivotStandard.OnBarBatch(HLCData, outputStandard);
        _qcPivotFibonacci.OnBarBatch(HLCData, outputFibonacci);
        _qcPivotCamarilla.OnBarBatch(HLCData, outputCamarilla);
        
        // Calculate average range for each type
        decimal standardRange = CalculateAverageRange(outputStandard);
        decimal fibonacciRange = CalculateAverageRange(outputFibonacci);
        decimal camarillaRange = CalculateAverageRange(outputCamarilla);
        
        return (standardRange, fibonacciRange, camarillaRange);
    }
    
    private decimal CalculateAverageRange(PivotPointsOutput<decimal>[] outputs)
    {
        decimal totalRange = 0;
        int count = 0;
        
        foreach (var output in outputs)
        {
            if (output != null)
            {
                var range = output.Resistance2 - output.Support2;
                totalRange += range;
                count++;
            }
        }
        
        return count > 0 ? totalRange / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("PivotPoints", "BreakoutDetection")]
    public (int r1_breaks, int r2_breaks, int s1_breaks, int s2_breaks) PivotPoints_DetectBreakouts()
    {
        _qcPivotStandard.Clear();
        var output = new PivotPointsOutput<decimal>[1];
        PivotPointsOutput<decimal>? previousPivots = null;
        decimal? previousClose = null;
        
        int r1Breaks = 0, r2Breaks = 0, s1Breaks = 0, s2Breaks = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcPivotStandard.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcPivotStandard.IsReady && output[0] != null && 
                previousPivots != null && previousClose.HasValue)
            {
                var currentClose = HLCData[i].Close;
                var pivots = output[0];
                
                // R1 breakout
                if (previousClose.Value <= pivots.Resistance1 && currentClose > pivots.Resistance1)
                    r1Breaks++;
                
                // R2 breakout
                if (previousClose.Value <= pivots.Resistance2 && currentClose > pivots.Resistance2)
                    r2Breaks++;
                
                // S1 breakdown
                if (previousClose.Value >= pivots.Support1 && currentClose < pivots.Support1)
                    s1Breaks++;
                
                // S2 breakdown
                if (previousClose.Value >= pivots.Support2 && currentClose < pivots.Support2)
                    s2Breaks++;
                
                previousPivots = pivots;
                previousClose = currentClose;
            }
            else if (_qcPivotStandard.IsReady && output[0] != null)
            {
                previousPivots = output[0];
                previousClose = HLCData[i].Close;
            }
        }
        
        return (r1Breaks, r2Breaks, s1Breaks, s2Breaks);
    }

    [Benchmark]
    [BenchmarkCategory("PivotPoints", "CentralPivot")]
    public (int above_pivot, int below_pivot, int at_pivot) PivotPoints_CentralPivotPosition()
    {
        _qcPivotStandard.Clear();
        var output = new PivotPointsOutput<decimal>[1];
        int abovePivot = 0;
        int belowPivot = 0;
        int atPivot = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcPivotStandard.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcPivotStandard.IsReady && output[0] != null)
            {
                var close = HLCData[i].Close;
                var pivot = output[0].PivotPoint;
                var tolerance = pivot * 0.001m; // 0.1% tolerance
                
                if (close > pivot + tolerance) abovePivot++;
                else if (close < pivot - tolerance) belowPivot++;
                else atPivot++;
            }
        }
        
        return (abovePivot, belowPivot, atPivot);
    }

    [Benchmark]
    [BenchmarkCategory("PivotPoints", "LevelAccuracy")]
    public decimal PivotPoints_MeasureLevelAccuracy()
    {
        _qcPivotStandard.Clear();
        var output = new PivotPointsOutput<decimal>[DataSize];
        _qcPivotStandard.OnBarBatch(HLCData, output);
        
        int totalTouches = 0;
        int accurateTouches = 0;
        decimal tolerance = 0.003m; // 0.3% tolerance
        
        for (int i = 0; i < DataSize; i++)
        {
            if (output[i] != null)
            {
                var bar = HLCData[i];
                var pivots = output[i];
                
                // Check all pivot levels
                var levels = new decimal[] 
                {
                    pivots.Support2, pivots.Support1, pivots.PivotPoint,
                    pivots.Resistance1, pivots.Resistance2
                };
                
                foreach (var level in levels)
                {
                    // Check if price touched this level
                    if (bar.Low <= level * (1 + tolerance) && bar.High >= level * (1 - tolerance))
                    {
                        totalTouches++;
                        
                        // Check if it provided support/resistance (price bounced)
                        if (i < DataSize - 1)
                        {
                            var nextBar = HLCData[i + 1];
                            bool bounced = false;
                            
                            if (level < bar.Close) // Resistance test
                            {
                                bounced = nextBar.Close < bar.Close;
                            }
                            else // Support test
                            {
                                bounced = nextBar.Close > bar.Close;
                            }
                            
                            if (bounced) accurateTouches++;
                        }
                    }
                }
            }
        }
        
        return totalTouches > 0 ? (decimal)accurateTouches / totalTouches : 0;
    }
    
    [Benchmark]
    [BenchmarkCategory("PivotPoints", "Memory")]
    public long PivotPoints_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new PivotPoints_QC<HLCData, decimal>(_parameters);
            var output = new PivotPointsOutput<decimal>[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcPivot?.Clear();
        _qcPivotStandard?.Clear();
        _qcPivotFibonacci?.Clear();
        _qcPivotCamarilla?.Clear();
        base.GlobalCleanup();
    }
}

public class PivotPointsOutput<T>
{
    public T PivotPoint { get; set; }
    public T Resistance1 { get; set; }
    public T Support1 { get; set; }
    public T Resistance2 { get; set; }
    public T Support2 { get; set; }
    public T Resistance3 { get; set; }
    public T Support3 { get; set; }
}

public enum PivotPointType
{
    Standard,
    Fibonacci,
    Camarilla,
    Woodie,
    DeMark
}