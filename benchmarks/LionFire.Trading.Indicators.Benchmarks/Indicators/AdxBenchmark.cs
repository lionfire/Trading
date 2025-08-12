using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class AdxBenchmark : IndicatorBenchmarkBase
{
    private ADX_QC<decimal, decimal> _qcAdx = null!;
    private PADX<decimal, decimal> _parameters = null!;
    
    private ADX_QC<decimal, decimal> _qcAdx7 = null!;
    private ADX_QC<decimal, decimal> _qcAdx14 = null!;
    private ADX_QC<decimal, decimal> _qcAdx28 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PADX<decimal, decimal> { Period = Period };
        _qcAdx = new ADX_QC<decimal, decimal>(_parameters);
        
        _qcAdx7 = new ADX_QC<decimal, decimal>(new PADX<decimal, decimal> { Period = 7 });
        _qcAdx14 = new ADX_QC<decimal, decimal>(new PADX<decimal, decimal> { Period = 14 });
        _qcAdx28 = new ADX_QC<decimal, decimal>(new PADX<decimal, decimal> { Period = 28 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ADX", "QuantConnect")]
    public decimal[] ADX_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcAdx.Clear();
        _qcAdx.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("ADX", "QuantConnect")]
    public List<decimal?> ADX_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcAdx.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAdx.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcAdx.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("ADX", "TrendStrength")]
    public (int strong, int moderate, int weak) ADX_DetectTrendStrength()
    {
        _qcAdx14.Clear();
        var output = new decimal[1];
        int strongTrend = 0;
        int moderateTrend = 0;
        int weakTrend = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAdx14.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcAdx14.IsReady)
            {
                if (output[0] > 50m) strongTrend++;
                else if (output[0] > 25m) moderateTrend++;
                else weakTrend++;
            }
        }
        
        return (strongTrend, moderateTrend, weakTrend);
    }

    [Benchmark]
    [BenchmarkCategory("ADX", "Convergence")]
    public (int period7, int period14, int period28) ADX_ConvergenceSpeed()
    {
        var results = (0, 0, 0);
        
        results.Item1 = MeasureConvergence(_qcAdx7);
        results.Item2 = MeasureConvergence(_qcAdx14);
        results.Item3 = MeasureConvergence(_qcAdx28);
        
        return results;
    }
    
    private int MeasureConvergence(ADX_QC<decimal, decimal> indicator)
    {
        indicator.Clear();
        var output = new decimal[1];
        decimal? previousValue = null;
        int pointsToConverge = 0;
        
        for (int i = 0; i < Math.Min(HLCData.Length, 500); i++)
        {
            indicator.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (indicator.IsReady && previousValue.HasValue)
            {
                var diff = Math.Abs(output[0] - previousValue.Value);
                if (diff < 0.1m)
                {
                    pointsToConverge = i;
                    break;
                }
            }
            
            previousValue = indicator.IsReady ? output[0] : (decimal?)null;
        }
        
        return pointsToConverge;
    }

    [Benchmark]
    [BenchmarkCategory("ADX", "TrendChange")]
    public int ADX_DetectTrendChanges()
    {
        _qcAdx14.Clear();
        var output = new decimal[1];
        decimal? previousValue = null;
        int trendChanges = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAdx14.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcAdx14.IsReady && previousValue.HasValue)
            {
                bool wasStrong = previousValue.Value > 25m;
                bool isStrong = output[0] > 25m;
                
                if (wasStrong != isStrong)
                {
                    trendChanges++;
                }
            }
            
            previousValue = _qcAdx14.IsReady ? output[0] : (decimal?)null;
        }
        
        return trendChanges;
    }
    
    [Benchmark]
    [BenchmarkCategory("ADX", "Memory")]
    public long ADX_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new ADX_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcAdx?.Clear();
        _qcAdx7?.Clear();
        _qcAdx14?.Clear();
        _qcAdx28?.Clear();
        base.GlobalCleanup();
    }
}