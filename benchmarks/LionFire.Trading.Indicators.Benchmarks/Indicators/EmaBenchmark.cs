// Temporarily disabled
#if false
using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Native;
using System.Collections.Generic;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

/// <summary>
/// Benchmarks comparing Exponential Moving Average implementations
/// </summary>
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class EmaBenchmark : IndicatorBenchmarkBase
{
    private EMA_QC<decimal, decimal> _qcEma = null!;
    private EMA_FP<decimal, decimal> _fpEma = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Initialize indicators
        _qcEma = new QCEmaIndicator($"EMA_{Period}", Period);
        _fpEma = new FPEmaIndicator($"EMA_{Period}", Period);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("EMA", "QuantConnect")]
    public List<decimal?> EMA_QuantConnect()
    {
        var results = new List<decimal?>(DataSize);
        
        // Reset indicator state
        _qcEma.Reset();
        
        // Process all data points
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcEma.Update(Timestamps[i], PriceData[i]);
            results.Add(_qcEma.Current?.Value);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("EMA", "FinancialPython")]
    public List<decimal?> EMA_FinancialPython()
    {
        var results = new List<decimal?>(DataSize);
        
        // Reset indicator state
        _fpEma.Reset();
        
        // Process all data points
        for (int i = 0; i < PriceData.Length; i++)
        {
            _fpEma.Update(Timestamps[i], PriceData[i]);
            results.Add(_fpEma.Current?.Value);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("EMA", "WarmUp")]
    public decimal? EMA_QuantConnect_WithWarmUp()
    {
        _qcEma.Reset();
        
        // Warm up with initial data
        int warmUpSize = Math.Min(Period * 2, PriceData.Length / 2);
        for (int i = 0; i < warmUpSize; i++)
        {
            _qcEma.Update(Timestamps[i], PriceData[i]);
        }
        
        // Process remaining data
        for (int i = warmUpSize; i < PriceData.Length; i++)
        {
            _qcEma.Update(Timestamps[i], PriceData[i]);
        }
        
        return _qcEma.Current?.Value;
    }

    [Benchmark]
    [BenchmarkCategory("EMA", "WarmUp")]
    public decimal? EMA_FinancialPython_WithWarmUp()
    {
        _fpEma.Reset();
        
        // Warm up with initial data
        int warmUpSize = Math.Min(Period * 2, PriceData.Length / 2);
        for (int i = 0; i < warmUpSize; i++)
        {
            _fpEma.Update(Timestamps[i], PriceData[i]);
        }
        
        // Process remaining data
        for (int i = warmUpSize; i < PriceData.Length; i++)
        {
            _fpEma.Update(Timestamps[i], PriceData[i]);
        }
        
        return _fpEma.Current?.Value;
    }

    [Benchmark]
    [BenchmarkCategory("EMA", "Memory")]
    public long EMA_QuantConnect_MemoryAllocations()
    {
        return MeasureAllocations(() =>
        {
            var ema = new QCEmaIndicator($"EMA_Test_{Period}", Period);
            for (int i = 0; i < Math.Min(1000, PriceData.Length); i++)
            {
                ema.Update(Timestamps[i], PriceData[i]);
            }
            ema.Dispose();
        });
    }

    [Benchmark]
    [BenchmarkCategory("EMA", "Memory")]
    public long EMA_FinancialPython_MemoryAllocations()
    {
        return MeasureAllocations(() =>
        {
            var ema = new FPEmaIndicator($"EMA_Test_{Period}", Period);
            for (int i = 0; i < Math.Min(1000, PriceData.Length); i++)
            {
                ema.Update(Timestamps[i], PriceData[i]);
            }
            ema.Dispose();
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcEma?.Dispose();
        _fpEma?.Dispose();
        base.GlobalCleanup();
    }
}
#endif
