// Temporarily disabled
#if false
using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

/// <summary>
/// Comprehensive benchmarks comparing Stochastic Oscillator implementations
/// Tests QC vs FP implementations with HLC data, %K and %D calculations
/// </summary>
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class StochasticBenchmark : IndicatorBenchmarkBase
{
    private Stochastic_QC<decimal, decimal> _qcStoch = null!;
    private Stochastic_FP<decimal, decimal> _fpStoch = null!;
    private PStochastic<decimal, decimal> _parameters = null!;
    
    // Different smoothing period configurations
    private Stochastic_QC<decimal, decimal> _qcStochFast = null!;
    private Stochastic_QC<decimal, decimal> _qcStochSlow = null!;
    private Stochastic_FP<decimal, decimal> _fpStochFast = null!;
    private Stochastic_FP<decimal, decimal> _fpStochSlow = null!;
    
    [Params(14, 21)]
    public int FastPeriod { get; set; }
    
    [Params(3, 5)]
    public int SlowKPeriod { get; set; }
    
    [Params(3, 5)]
    public int SlowDPeriod { get; set; }

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Initialize primary indicators with current parameters
        _parameters = new PStochastic<decimal, decimal> 
        { 
            FastPeriod = FastPeriod,
            SlowKPeriod = SlowKPeriod,
            SlowDPeriod = SlowDPeriod
        };
        _qcStoch = new Stochastic_QC<decimal, decimal>(_parameters);
        _fpStoch = new Stochastic_FP<decimal, decimal>(_parameters);
        
        // Initialize fast and slow configurations for comparison
        var fastParams = new PStochastic<decimal, decimal> { FastPeriod = 5, SlowKPeriod = 3, SlowDPeriod = 3 };
        var slowParams = new PStochastic<decimal, decimal> { FastPeriod = 21, SlowKPeriod = 5, SlowDPeriod = 5 };
        
        _qcStochFast = new Stochastic_QC<decimal, decimal>(fastParams);
        _qcStochSlow = new Stochastic_QC<decimal, decimal>(slowParams);
        _fpStochFast = new Stochastic_FP<decimal, decimal>(fastParams);
        _fpStochSlow = new Stochastic_FP<decimal, decimal>(slowParams);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Stochastic", "QuantConnect")]
    public decimal[] Stochastic_QuantConnect_Batch()
    {
        var output = new decimal[DataSize * 2]; // 2 values per data point (%K, %D)
        
        // Reset indicator state
        _qcStoch.Clear();
        
        // Process batch with HLC data
        _qcStoch.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("Stochastic", "QuantConnect")]
    public List<(decimal k, decimal d)> Stochastic_QuantConnect_Streaming()
    {
        var results = new List<(decimal, decimal)>(DataSize);
        var output = new decimal[2];
        
        // Reset indicator state
        _qcStoch.Clear();
        
        // Process streaming (one at a time)
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcStoch.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcStoch.IsReady)
            {
                results.Add((output[0], output[1]));
            }
            else
            {
                results.Add((0, 0));
            }
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("Stochastic", "FinancialPython")]
    public decimal[] Stochastic_FinancialPython_Batch()
    {
        var output = new decimal[DataSize * 2]; // 2 values per data point
        
        // Reset indicator state
        _fpStoch.Clear();
        
        // Process batch with HLC data
        _fpStoch.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("Stochastic", "FinancialPython")]
    public List<(decimal k, decimal d)> Stochastic_FinancialPython_Streaming()
    {
        var results = new List<(decimal, decimal)>(DataSize);
        var output = new decimal[2];
        
        // Reset indicator state
        _fpStoch.Clear();
        
        // Process streaming (one at a time)
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpStoch.OnBarBatch(new[] { HLCData[i] }, output);
            if (_fpStoch.IsReady)
            {
                results.Add((output[0], output[1]));
            }
            else
            {
                results.Add((0, 0));
            }
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("Stochastic", "Crossovers")]
    public int Stochastic_QuantConnect_DetectCrossovers()
    {
        _qcStoch.Clear();
        var output = new decimal[2];
        int crossoverCount = 0;
        decimal? previousK = null;
        decimal? previousD = null;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcStoch.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcStoch.IsReady && previousK.HasValue && previousD.HasValue)
            {
                var currentK = output[0];
                var currentD = output[1];
                
                // Detect crossover
                if ((previousK < previousD && currentK > currentD) ||
                    (previousK > previousD && currentK < currentD))
                {
                    crossoverCount++;
                }
                
                previousK = currentK;
                previousD = currentD;
            }
            else if (_qcStoch.IsReady)
            {
                previousK = output[0];
                previousD = output[1];
            }
        }
        
        return crossoverCount;
    }
    
    [Benchmark]
    [BenchmarkCategory("Stochastic", "Crossovers")]
    public int Stochastic_FinancialPython_DetectCrossovers()
    {
        _fpStoch.Clear();
        var output = new decimal[2];
        int crossoverCount = 0;
        decimal? previousK = null;
        decimal? previousD = null;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpStoch.OnBarBatch(new[] { HLCData[i] }, output);
            if (_fpStoch.IsReady && previousK.HasValue && previousD.HasValue)
            {
                var currentK = output[0];
                var currentD = output[1];
                
                // Detect crossover
                if ((previousK < previousD && currentK > currentD) ||
                    (previousK > previousD && currentK < currentD))
                {
                    crossoverCount++;
                }
                
                previousK = currentK;
                previousD = currentD;
            }
            else if (_fpStoch.IsReady)
            {
                previousK = output[0];
                previousD = output[1];
            }
        }
        
        return crossoverCount;
    }

    [Benchmark]
    [BenchmarkCategory("Stochastic", "OversoldOverbought")]
    public (int oversold, int overbought) Stochastic_QuantConnect_DetectLevels()
    {
        _qcStoch.Clear();
        var output = new decimal[2];
        int oversoldCount = 0;
        int overboughtCount = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcStoch.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcStoch.IsReady)
            {
                var k = output[0];
                if (k < 20m) oversoldCount++;
                else if (k > 80m) overboughtCount++;
            }
        }
        
        return (oversoldCount, overboughtCount);
    }
    
    [Benchmark]
    [BenchmarkCategory("Stochastic", "OversoldOverbought")]
    public (int oversold, int overbought) Stochastic_FinancialPython_DetectLevels()
    {
        _fpStoch.Clear();
        var output = new decimal[2];
        int oversoldCount = 0;
        int overboughtCount = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpStoch.OnBarBatch(new[] { HLCData[i] }, output);
            if (_fpStoch.IsReady)
            {
                var k = output[0];
                if (k < 20m) oversoldCount++;
                else if (k > 80m) overboughtCount++;
            }
        }
        
        return (oversoldCount, overboughtCount);
    }

    [Benchmark]
    [BenchmarkCategory("Stochastic", "SmoothingComparison")]
    public decimal[] Stochastic_FastSettings()
    {
        _qcStochFast.Clear();
        var output = new decimal[DataSize * 2];
        _qcStochFast.OnBarBatch(HLCData, output);
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("Stochastic", "SmoothingComparison")]
    public decimal[] Stochastic_SlowSettings()
    {
        _qcStochSlow.Clear();
        var output = new decimal[DataSize * 2];
        _qcStochSlow.OnBarBatch(HLCData, output);
        return output;
    }

    [Benchmark]
    [BenchmarkCategory("Stochastic", "Accuracy")]
    public bool Stochastic_CompareAccuracy()
    {
        _qcStoch.Clear();
        _fpStoch.Clear();
        
        var qcOutput = new decimal[DataSize * 2];
        var fpOutput = new decimal[DataSize * 2];
        
        _qcStoch.OnBarBatch(HLCData, qcOutput);
        _fpStoch.OnBarBatch(HLCData, fpOutput);
        
        // Validate both %K and %D outputs are equivalent
        bool kMatch = ValidateOutputs(
            qcOutput.Where((v, i) => i % 2 == 0).Select(v => v == default ? (decimal?)null : v),
            fpOutput.Where((v, i) => i % 2 == 0).Select(v => v == default ? (decimal?)null : v),
            tolerance: 0.01m);
            
        bool dMatch = ValidateOutputs(
            qcOutput.Where((v, i) => i % 2 == 1).Select(v => v == default ? (decimal?)null : v),
            fpOutput.Where((v, i) => i % 2 == 1).Select(v => v == default ? (decimal?)null : v),
            tolerance: 0.01m);
        
        return kMatch && dMatch;
    }
    
    [Benchmark]
    [BenchmarkCategory("Stochastic", "Memory")]
    public long Stochastic_QuantConnect_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new Stochastic_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize * 2];
            indicator.OnBarBatch(HLCData, output);
        });
    }
    
    [Benchmark]
    [BenchmarkCategory("Stochastic", "Memory")]
    public long Stochastic_FinancialPython_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new Stochastic_FP<decimal, decimal>(_parameters);
            var output = new decimal[DataSize * 2];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcStoch?.Clear();
        _fpStoch?.Clear();
        _qcStochFast?.Clear();
        _qcStochSlow?.Clear();
        _fpStochFast?.Clear();
        _fpStochSlow?.Clear();
        base.GlobalCleanup();
    }
}
#endif
