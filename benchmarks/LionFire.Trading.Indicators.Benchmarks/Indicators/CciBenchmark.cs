using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class CciBenchmark : IndicatorBenchmarkBase
{
    private CCI_QC<decimal, decimal> _qcCci = null!;
    private PCCI<decimal, decimal> _parameters = null!;
    
    private CCI_QC<decimal, decimal> _qcCci14 = null!;
    private CCI_QC<decimal, decimal> _qcCci20 = null!;
    private CCI_QC<decimal, decimal> _qcCci50 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PCCI<decimal, decimal> { Period = Period };
        _qcCci = new CCI_QC<decimal, decimal>(_parameters);
        
        _qcCci14 = new CCI_QC<decimal, decimal>(new PCCI<decimal, decimal> { Period = 14 });
        _qcCci20 = new CCI_QC<decimal, decimal>(new PCCI<decimal, decimal> { Period = 20 });
        _qcCci50 = new CCI_QC<decimal, decimal>(new PCCI<decimal, decimal> { Period = 50 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CCI", "QuantConnect")]
    public decimal[] CCI_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcCci.Clear();
        _qcCci.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("CCI", "QuantConnect")]
    public List<decimal?> CCI_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcCci.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcCci.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcCci.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("CCI", "OversoldOverbought")]
    public (int oversold, int overbought, int neutral) CCI_DetectLevels()
    {
        _qcCci20.Clear();
        var output = new decimal[1];
        int oversoldCount = 0;
        int overboughtCount = 0;
        int neutralCount = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcCci20.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcCci20.IsReady)
            {
                if (output[0] < -100m) oversoldCount++;
                else if (output[0] > 100m) overboughtCount++;
                else neutralCount++;
            }
        }
        
        return (oversoldCount, overboughtCount, neutralCount);
    }

    [Benchmark]
    [BenchmarkCategory("CCI", "ExtremeLevels")]
    public (int extreme_oversold, int extreme_overbought) CCI_DetectExtremeLevels()
    {
        _qcCci20.Clear();
        var output = new decimal[1];
        int extremeOversoldCount = 0;
        int extremeOverboughtCount = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcCci20.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcCci20.IsReady)
            {
                if (output[0] < -200m) extremeOversoldCount++;
                else if (output[0] > 200m) extremeOverboughtCount++;
            }
        }
        
        return (extremeOversoldCount, extremeOverboughtCount);
    }

    [Benchmark]
    [BenchmarkCategory("CCI", "Convergence")]
    public (int period14, int period20, int period50) CCI_ConvergenceSpeed()
    {
        var results = (0, 0, 0);
        
        results.Item1 = MeasureConvergence(_qcCci14);
        results.Item2 = MeasureConvergence(_qcCci20);
        results.Item3 = MeasureConvergence(_qcCci50);
        
        return results;
    }
    
    private int MeasureConvergence(CCI_QC<decimal, decimal> indicator)
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
                if (diff < 1m)
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
    [BenchmarkCategory("CCI", "ZeroCrossings")]
    public int CCI_CountZeroCrossings()
    {
        _qcCci20.Clear();
        var output = new decimal[1];
        decimal? previousValue = null;
        int crossings = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcCci20.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcCci20.IsReady && previousValue.HasValue)
            {
                if ((previousValue.Value < 0 && output[0] > 0) || 
                    (previousValue.Value > 0 && output[0] < 0))
                {
                    crossings++;
                }
            }
            
            previousValue = _qcCci20.IsReady ? output[0] : (decimal?)null;
        }
        
        return crossings;
    }
    
    [Benchmark]
    [BenchmarkCategory("CCI", "Memory")]
    public long CCI_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new CCI_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcCci?.Clear();
        _qcCci14?.Clear();
        _qcCci20?.Clear();
        _qcCci50?.Clear();
        base.GlobalCleanup();
    }
}