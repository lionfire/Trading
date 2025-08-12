using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class WilliamsRBenchmark : IndicatorBenchmarkBase
{
    private WilliamsR_QC<decimal, decimal> _qcWilliamsR = null!;
    private PWilliamsR<decimal, decimal> _parameters = null!;
    
    private WilliamsR_QC<decimal, decimal> _qcWilliamsR7 = null!;
    private WilliamsR_QC<decimal, decimal> _qcWilliamsR14 = null!;
    private WilliamsR_QC<decimal, decimal> _qcWilliamsR28 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PWilliamsR<decimal, decimal> { Period = Period };
        _qcWilliamsR = new WilliamsR_QC<decimal, decimal>(_parameters);
        
        _qcWilliamsR7 = new WilliamsR_QC<decimal, decimal>(new PWilliamsR<decimal, decimal> { Period = 7 });
        _qcWilliamsR14 = new WilliamsR_QC<decimal, decimal>(new PWilliamsR<decimal, decimal> { Period = 14 });
        _qcWilliamsR28 = new WilliamsR_QC<decimal, decimal>(new PWilliamsR<decimal, decimal> { Period = 28 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("WilliamsR", "QuantConnect")]
    public decimal[] WilliamsR_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcWilliamsR.Clear();
        _qcWilliamsR.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("WilliamsR", "QuantConnect")]
    public List<decimal?> WilliamsR_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcWilliamsR.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcWilliamsR.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcWilliamsR.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("WilliamsR", "OversoldOverbought")]
    public (int oversold, int overbought, int neutral) WilliamsR_DetectLevels()
    {
        _qcWilliamsR14.Clear();
        var output = new decimal[1];
        int oversoldCount = 0;
        int overboughtCount = 0;
        int neutralCount = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcWilliamsR14.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcWilliamsR14.IsReady)
            {
                if (output[0] < -80m) oversoldCount++;
                else if (output[0] > -20m) overboughtCount++;
                else neutralCount++;
            }
        }
        
        return (oversoldCount, overboughtCount, neutralCount);
    }

    [Benchmark]
    [BenchmarkCategory("WilliamsR", "ExtremeLevels")]
    public (int extreme_oversold, int extreme_overbought) WilliamsR_DetectExtremeLevels()
    {
        _qcWilliamsR14.Clear();
        var output = new decimal[1];
        int extremeOversoldCount = 0;
        int extremeOverboughtCount = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcWilliamsR14.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcWilliamsR14.IsReady)
            {
                if (output[0] < -95m) extremeOversoldCount++;
                else if (output[0] > -5m) extremeOverboughtCount++;
            }
        }
        
        return (extremeOversoldCount, extremeOverboughtCount);
    }

    [Benchmark]
    [BenchmarkCategory("WilliamsR", "Convergence")]
    public (int period7, int period14, int period28) WilliamsR_ConvergenceSpeed()
    {
        var results = (0, 0, 0);
        
        results.Item1 = MeasureConvergence(_qcWilliamsR7);
        results.Item2 = MeasureConvergence(_qcWilliamsR14);
        results.Item3 = MeasureConvergence(_qcWilliamsR28);
        
        return results;
    }
    
    private int MeasureConvergence(WilliamsR_QC<decimal, decimal> indicator)
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
                if (diff < 0.5m)
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
    [BenchmarkCategory("WilliamsR", "Reversals")]
    public int WilliamsR_DetectReversals()
    {
        _qcWilliamsR14.Clear();
        var output = new decimal[1];
        decimal? previousValue = null;
        int reversals = 0;
        bool wasOversold = false;
        bool wasOverbought = false;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcWilliamsR14.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcWilliamsR14.IsReady)
            {
                bool isOversold = output[0] < -80m;
                bool isOverbought = output[0] > -20m;
                
                if (wasOversold && !isOversold && output[0] > -70m)
                {
                    reversals++;
                }
                else if (wasOverbought && !isOverbought && output[0] < -30m)
                {
                    reversals++;
                }
                
                wasOversold = isOversold;
                wasOverbought = isOverbought;
            }
        }
        
        return reversals;
    }
    
    [Benchmark]
    [BenchmarkCategory("WilliamsR", "Memory")]
    public long WilliamsR_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new WilliamsR_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcWilliamsR?.Clear();
        _qcWilliamsR7?.Clear();
        _qcWilliamsR14?.Clear();
        _qcWilliamsR28?.Clear();
        base.GlobalCleanup();
    }
}