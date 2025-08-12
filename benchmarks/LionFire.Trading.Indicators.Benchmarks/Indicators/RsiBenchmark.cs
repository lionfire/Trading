// Temporarily disabled
#if false
using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

/// <summary>
/// Comprehensive benchmarks comparing Relative Strength Index implementations
/// Tests QC vs FP implementations with various periods and market conditions
/// </summary>
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class RsiBenchmark : IndicatorBenchmarkBase
{
    private RSI_QC<decimal, decimal> _qcRsi = null!;
    private RSI_FP<decimal, decimal> _fpRsi = null!;
    private LionFire.Trading.Indicators.Parameters.PRSI<decimal, decimal> _parameters = null!;
    
    // Different RSI periods for testing
    private RSI_QC<decimal, decimal> _qcRsi14 = null!;
    private RSI_QC<decimal, decimal> _qcRsi28 = null!;
    private RSI_QC<decimal, decimal> _qcRsi50 = null!;
    private RSI_FP<decimal, decimal> _fpRsi14 = null!;
    private RSI_FP<decimal, decimal> _fpRsi28 = null!;
    private RSI_FP<decimal, decimal> _fpRsi50 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Initialize primary indicators with current Period
        _parameters = new PRSI<decimal, decimal> { Period = Period };
        _qcRsi = new RSI_QC<decimal, decimal>(_parameters);
        _fpRsi = new RSI_FP<decimal, decimal>(_parameters);
        
        // Initialize indicators with specific periods for convergence tests
        _qcRsi14 = new RSI_QC<decimal, decimal>(new PRSI<decimal, decimal> { Period = 14 });
        _qcRsi28 = new RSI_QC<decimal, decimal>(new PRSI<decimal, decimal> { Period = 28 });
        _qcRsi50 = new RSI_QC<decimal, decimal>(new PRSI<decimal, decimal> { Period = 50 });
        _fpRsi14 = new RSI_FP<decimal, decimal>(new PRSI<decimal, decimal> { Period = 14 });
        _fpRsi28 = new RSI_FP<decimal, decimal>(new PRSI<decimal, decimal> { Period = 28 });
        _fpRsi50 = new RSI_FP<decimal, decimal>(new PRSI<decimal, decimal> { Period = 50 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("RSI", "QuantConnect")]
    public decimal[] RSI_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        // Reset indicator state
        _qcRsi.Clear();
        
        // Process batch
        _qcRsi.OnBarBatch(PriceData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("RSI", "QuantConnect")]
    public List<decimal?> RSI_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        // Reset indicator state
        _qcRsi.Clear();
        
        // Process streaming (one at a time)
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcRsi.OnBarBatch(new[] { PriceData[i] }, output);
            results.Add(_qcRsi.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("RSI", "FinancialPython")]
    public decimal[] RSI_FinancialPython_Batch()
    {
        var output = new decimal[DataSize];
        
        // Reset indicator state
        _fpRsi.Clear();
        
        // Process batch
        _fpRsi.OnBarBatch(PriceData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("RSI", "FinancialPython")]
    public List<decimal?> RSI_FinancialPython_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        // Reset indicator state
        _fpRsi.Clear();
        
        // Process streaming (one at a time)
        for (int i = 0; i < PriceData.Length; i++)
        {
            _fpRsi.OnBarBatch(new[] { PriceData[i] }, output);
            results.Add(_fpRsi.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("RSI", "OversoldOverbought")]
    public (int oversold, int overbought) RSI_QuantConnect_DetectLevels()
    {
        _qcRsi.Clear();
        var output = new decimal[1];
        int oversoldCount = 0;
        int overboughtCount = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcRsi.OnBarBatch(new[] { PriceData[i] }, output);
            if (_qcRsi.IsReady)
            {
                if (output[0] < 30m) oversoldCount++;
                else if (output[0] > 70m) overboughtCount++;
            }
        }
        
        return (oversoldCount, overboughtCount);
    }
    
    [Benchmark]
    [BenchmarkCategory("RSI", "OversoldOverbought")]
    public (int oversold, int overbought) RSI_FinancialPython_DetectLevels()
    {
        _fpRsi.Clear();
        var output = new decimal[1];
        int oversoldCount = 0;
        int overboughtCount = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _fpRsi.OnBarBatch(new[] { PriceData[i] }, output);
            if (_fpRsi.IsReady)
            {
                if (output[0] < 30m) oversoldCount++;
                else if (output[0] > 70m) overboughtCount++;
            }
        }
        
        return (oversoldCount, overboughtCount);
    }

    [Benchmark]
    [BenchmarkCategory("RSI", "Convergence")]
    public (int period14, int period28, int period50) RSI_QuantConnect_ConvergenceSpeed()
    {
        // Test convergence with different periods
        var results = (0, 0, 0);
        
        results.Item1 = MeasureConvergence(_qcRsi14);
        results.Item2 = MeasureConvergence(_qcRsi28);
        results.Item3 = MeasureConvergence(_qcRsi50);
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("RSI", "Convergence")]
    public (int period14, int period28, int period50) RSI_FinancialPython_ConvergenceSpeed()
    {
        // Test convergence with different periods
        var results = (0, 0, 0);
        
        results.Item1 = MeasureConvergence(_fpRsi14);
        results.Item2 = MeasureConvergence(_fpRsi28);
        results.Item3 = MeasureConvergence(_fpRsi50);
        
        return results;
    }
    
    private int MeasureConvergence<T>(T indicator) where T : IIndicator2
    {
        indicator.Clear();
        var output = new decimal[1];
        decimal? previousValue = null;
        int pointsToConverge = 0;
        
        for (int i = 0; i < Math.Min(PriceData.Length, 500); i++)
        {
            if (indicator is RSI_QC<decimal, decimal> qc)
            {
                qc.OnBarBatch(new[] { PriceData[i] }, output);
            }
            else if (indicator is RSI_FP<decimal, decimal> fp)
            {
                fp.OnBarBatch(new[] { PriceData[i] }, output);
            }
            
            if (indicator.IsReady && previousValue.HasValue)
            {
                var diff = Math.Abs(output[0] - previousValue.Value);
                if (diff < 0.01m) // Considered converged
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
    [BenchmarkCategory("RSI", "Accuracy")]
    public bool RSI_CompareAccuracy()
    {
        _qcRsi.Clear();
        _fpRsi.Clear();
        
        var qcOutput = new decimal[DataSize];
        var fpOutput = new decimal[DataSize];
        
        _qcRsi.OnBarBatch(PriceData, qcOutput);
        _fpRsi.OnBarBatch(PriceData, fpOutput);
        
        // Validate outputs are equivalent
        return ValidateOutputs(
            qcOutput.Select(v => v == default ? (decimal?)null : v),
            fpOutput.Select(v => v == default ? (decimal?)null : v),
            tolerance: 0.01m);
    }
    
    [Benchmark]
    [BenchmarkCategory("RSI", "Memory")]
    public long RSI_QuantConnect_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new RSI_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(PriceData, output);
        });
    }
    
    [Benchmark]
    [BenchmarkCategory("RSI", "Memory")]
    public long RSI_FinancialPython_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new RSI_FP<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(PriceData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcRsi?.Clear();
        _fpRsi?.Clear();
        _qcRsi14?.Clear();
        _qcRsi28?.Clear();
        _qcRsi50?.Clear();
        _fpRsi14?.Clear();
        _fpRsi28?.Clear();
        _fpRsi50?.Clear();
        base.GlobalCleanup();
    }
}
#endif
