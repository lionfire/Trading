using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class AtrBenchmark : IndicatorBenchmarkBase
{
    private ATR_QC<decimal, decimal> _qcAtr = null!;
    private PAverageTrueRange<decimal, decimal> _parameters = null!;
    
    private ATR_QC<decimal, decimal> _qcAtr7 = null!;
    private ATR_QC<decimal, decimal> _qcAtr14 = null!;
    private ATR_QC<decimal, decimal> _qcAtr21 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PAverageTrueRange<decimal, decimal> { Period = Period };
        _qcAtr = new ATR_QC<decimal, decimal>(_parameters);
        
        _qcAtr7 = new ATR_QC<decimal, decimal>(new PAverageTrueRange<decimal, decimal> { Period = 7 });
        _qcAtr14 = new ATR_QC<decimal, decimal>(new PAverageTrueRange<decimal, decimal> { Period = 14 });
        _qcAtr21 = new ATR_QC<decimal, decimal>(new PAverageTrueRange<decimal, decimal> { Period = 21 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ATR", "QuantConnect")]
    public decimal[] ATR_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcAtr.Clear();
        _qcAtr.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("ATR", "QuantConnect")]
    public List<decimal?> ATR_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcAtr.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAtr.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcAtr.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("ATR", "VolatilityDetection")]
    public (int high, int normal, int low) ATR_DetectVolatilityLevels()
    {
        _qcAtr14.Clear();
        var output = new decimal[1];
        int highVolCount = 0;
        int normalVolCount = 0;
        int lowVolCount = 0;
        
        decimal sum = 0;
        int count = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAtr14.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcAtr14.IsReady)
            {
                sum += output[0];
                count++;
            }
        }
        
        if (count > 0)
        {
            decimal avgAtr = sum / count;
            decimal highThreshold = avgAtr * 1.5m;
            decimal lowThreshold = avgAtr * 0.5m;
            
            _qcAtr14.Clear();
            for (int i = 0; i < HLCData.Length; i++)
            {
                _qcAtr14.OnBarBatch(new[] { HLCData[i] }, output);
                if (_qcAtr14.IsReady)
                {
                    if (output[0] > highThreshold) highVolCount++;
                    else if (output[0] < lowThreshold) lowVolCount++;
                    else normalVolCount++;
                }
            }
        }
        
        return (highVolCount, normalVolCount, lowVolCount);
    }

    [Benchmark]
    [BenchmarkCategory("ATR", "Convergence")]
    public (int period7, int period14, int period21) ATR_ConvergenceSpeed()
    {
        var results = (0, 0, 0);
        
        results.Item1 = MeasureConvergence(_qcAtr7);
        results.Item2 = MeasureConvergence(_qcAtr14);
        results.Item3 = MeasureConvergence(_qcAtr21);
        
        return results;
    }
    
    private int MeasureConvergence(ATR_QC<decimal, decimal> indicator)
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
                if (diff < 0.01m)
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
    [BenchmarkCategory("ATR", "Memory")]
    public long ATR_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new ATR_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcAtr?.Clear();
        _qcAtr7?.Clear();
        _qcAtr14?.Clear();
        _qcAtr21?.Clear();
        base.GlobalCleanup();
    }
}