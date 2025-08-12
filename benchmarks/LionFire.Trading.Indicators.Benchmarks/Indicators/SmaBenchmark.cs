// Temporarily disabled
#if false
using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Native;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

/// <summary>
/// Benchmarks comparing Simple Moving Average implementations
/// </summary>
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class SmaBenchmark : IndicatorBenchmarkBase
{
    private SMA_QC<decimal, decimal> _qcSma = null!;
    private SMA_FP<decimal, decimal> _fpSma = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Initialize indicators
        _qcSma = new QCSmaIndicator($"SMA_{Period}", Period);
        _fpSma = new FPSmaIndicator($"SMA_{Period}", Period);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("SMA", "QuantConnect")]
    public List<decimal?> SMA_QuantConnect()
    {
        var results = new List<decimal?>(DataSize);
        
        // Reset indicator state
        _qcSma.Reset();
        
        // Process all data points
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcSma.Update(Timestamps[i], PriceData[i]);
            results.Add(_qcSma.Current?.Value);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("SMA", "FinancialPython")]
    public List<decimal?> SMA_FinancialPython()
    {
        var results = new List<decimal?>(DataSize);
        
        // Reset indicator state
        _fpSma.Reset();
        
        // Process all data points
        for (int i = 0; i < PriceData.Length; i++)
        {
            _fpSma.Update(Timestamps[i], PriceData[i]);
            results.Add(_fpSma.Current?.Value);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("SMA", "BatchProcessing")]
    public decimal?[] SMA_QuantConnect_Batch()
    {
        // Process entire array at once if supported
        return _qcSma.ProcessBatch(PriceData, Timestamps);
    }

    [Benchmark]
    [BenchmarkCategory("SMA", "BatchProcessing")]
    public decimal?[] SMA_FinancialPython_Batch()
    {
        // Process entire array at once if supported
        return _fpSma.ProcessBatch(PriceData, Timestamps);
    }

    [Benchmark]
    [BenchmarkCategory("SMA", "Streaming")]
    public void SMA_QuantConnect_Streaming()
    {
        _qcSma.Reset();
        
        // Simulate streaming data processing without storing results
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcSma.Update(Timestamps[i], PriceData[i]);
            _ = _qcSma.Current?.Value; // Access but don't store
        }
    }

    [Benchmark]
    [BenchmarkCategory("SMA", "Streaming")]
    public void SMA_FinancialPython_Streaming()
    {
        _fpSma.Reset();
        
        // Simulate streaming data processing without storing results
        for (int i = 0; i < PriceData.Length; i++)
        {
            _fpSma.Update(Timestamps[i], PriceData[i]);
            _ = _fpSma.Current?.Value; // Access but don't store
        }
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcSma?.Dispose();
        _fpSma?.Dispose();
        base.GlobalCleanup();
    }
}
#endif
