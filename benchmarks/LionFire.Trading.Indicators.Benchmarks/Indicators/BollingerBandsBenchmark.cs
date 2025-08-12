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
/// Comprehensive benchmarks comparing Bollinger Bands implementations
/// Tests QC vs FP implementations with various standard deviations and market conditions
/// </summary>
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class BollingerBandsBenchmark : IndicatorBenchmarkBase
{
    private BollingerBands_QC<decimal, decimal> _qcBB = null!;
    private BollingerBands_FP<decimal, decimal> _fpBB = null!;
    private PBollingerBands<decimal, decimal> _parameters = null!;
    
    // Different standard deviation configurations
    private BollingerBands_QC<decimal, decimal> _qcBB2 = null!;
    private BollingerBands_QC<decimal, decimal> _qcBB25 = null!;
    private BollingerBands_QC<decimal, decimal> _qcBB3 = null!;
    private BollingerBands_FP<decimal, decimal> _fpBB2 = null!;
    private BollingerBands_FP<decimal, decimal> _fpBB25 = null!;
    private BollingerBands_FP<decimal, decimal> _fpBB3 = null!;
    
    [Params(2.0, 2.5, 3.0)]
    public decimal StandardDeviations { get; set; }

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Initialize primary indicators with current parameters
        _parameters = new PBollingerBands<decimal, decimal> 
        { 
            Period = Period,
            StandardDeviations = StandardDeviations
        };
        _qcBB = new BollingerBands_QC<decimal, decimal>(_parameters);
        _fpBB = new BollingerBands_FP<decimal, decimal>(_parameters);
        
        // Initialize indicators with specific standard deviations for comparison
        _qcBB2 = new BollingerBands_QC<decimal, decimal>(new PBollingerBands<decimal, decimal> { Period = 20, StandardDeviations = 2.0m });
        _qcBB25 = new BollingerBands_QC<decimal, decimal>(new PBollingerBands<decimal, decimal> { Period = 20, StandardDeviations = 2.5m });
        _qcBB3 = new BollingerBands_QC<decimal, decimal>(new PBollingerBands<decimal, decimal> { Period = 20, StandardDeviations = 3.0m });
        _fpBB2 = new BollingerBands_FP<decimal, decimal>(new PBollingerBands<decimal, decimal> { Period = 20, StandardDeviations = 2.0m });
        _fpBB25 = new BollingerBands_FP<decimal, decimal>(new PBollingerBands<decimal, decimal> { Period = 20, StandardDeviations = 2.5m });
        _fpBB3 = new BollingerBands_FP<decimal, decimal>(new PBollingerBands<decimal, decimal> { Period = 20, StandardDeviations = 3.0m });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("BollingerBands", "QuantConnect")]
    public decimal[] BollingerBands_QuantConnect_Batch()
    {
        var output = new decimal[DataSize * 3]; // 3 values per data point (upper, middle, lower)
        
        // Reset indicator state
        _qcBB.Clear();
        
        // Process batch
        _qcBB.OnBarBatch(PriceData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("BollingerBands", "QuantConnect")]
    public List<(decimal upper, decimal middle, decimal lower)> BollingerBands_QuantConnect_Streaming()
    {
        var results = new List<(decimal, decimal, decimal)>(DataSize);
        var output = new decimal[3];
        
        // Reset indicator state
        _qcBB.Clear();
        
        // Process streaming (one at a time)
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcBB.OnBarBatch(new[] { PriceData[i] }, output);
            if (_qcBB.IsReady)
            {
                results.Add((output[0], output[1], output[2]));
            }
            else
            {
                results.Add((0, 0, 0));
            }
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("BollingerBands", "FinancialPython")]
    public decimal[] BollingerBands_FinancialPython_Batch()
    {
        var output = new decimal[DataSize * 3]; // 3 values per data point
        
        // Reset indicator state
        _fpBB.Clear();
        
        // Process batch
        _fpBB.OnBarBatch(PriceData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("BollingerBands", "FinancialPython")]
    public List<(decimal upper, decimal middle, decimal lower)> BollingerBands_FinancialPython_Streaming()
    {
        var results = new List<(decimal, decimal, decimal)>(DataSize);
        var output = new decimal[3];
        
        // Reset indicator state
        _fpBB.Clear();
        
        // Process streaming (one at a time)
        for (int i = 0; i < PriceData.Length; i++)
        {
            _fpBB.OnBarBatch(new[] { PriceData[i] }, output);
            if (_fpBB.IsReady)
            {
                results.Add((output[0], output[1], output[2]));
            }
            else
            {
                results.Add((0, 0, 0));
            }
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("BollingerBands", "BandCalculations")]
    public (decimal percentB, decimal bandWidth)[] BollingerBands_QuantConnect_PercentBAndWidth()
    {
        _qcBB.Clear();
        var results = new List<(decimal, decimal)>(DataSize);
        var output = new decimal[3];
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcBB.OnBarBatch(new[] { PriceData[i] }, output);
            if (_qcBB.IsReady)
            {
                var upper = output[0];
                var middle = output[1];
                var lower = output[2];
                var bandWidth = upper - lower;
                var percentB = bandWidth != 0 ? (PriceData[i] - lower) / bandWidth : 0;
                results.Add((percentB, bandWidth));
            }
            else
            {
                results.Add((0, 0));
            }
        }
        
        return results.ToArray();
    }
    
    [Benchmark]
    [BenchmarkCategory("BollingerBands", "BandCalculations")]
    public (decimal percentB, decimal bandWidth)[] BollingerBands_FinancialPython_PercentBAndWidth()
    {
        _fpBB.Clear();
        var results = new List<(decimal, decimal)>(DataSize);
        var output = new decimal[3];
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _fpBB.OnBarBatch(new[] { PriceData[i] }, output);
            if (_fpBB.IsReady)
            {
                var upper = output[0];
                var middle = output[1];
                var lower = output[2];
                var bandWidth = upper - lower;
                var percentB = bandWidth != 0 ? (PriceData[i] - lower) / bandWidth : 0;
                results.Add((percentB, bandWidth));
            }
            else
            {
                results.Add((0, 0));
            }
        }
        
        return results.ToArray();
    }

    [Benchmark]
    [BenchmarkCategory("BollingerBands", "StandardDeviations")]
    public decimal[] BollingerBands_CompareStdDev_2()
    {
        _qcBB2.Clear();
        var output = new decimal[DataSize * 3];
        _qcBB2.OnBarBatch(PriceData, output);
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("BollingerBands", "StandardDeviations")]
    public decimal[] BollingerBands_CompareStdDev_25()
    {
        _qcBB25.Clear();
        var output = new decimal[DataSize * 3];
        _qcBB25.OnBarBatch(PriceData, output);
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("BollingerBands", "StandardDeviations")]
    public decimal[] BollingerBands_CompareStdDev_3()
    {
        _qcBB3.Clear();
        var output = new decimal[DataSize * 3];
        _qcBB3.OnBarBatch(PriceData, output);
        return output;
    }

    [Benchmark]
    [BenchmarkCategory("BollingerBands", "Accuracy")]
    public bool BollingerBands_CompareAccuracy()
    {
        _qcBB.Clear();
        _fpBB.Clear();
        
        var qcOutput = new decimal[DataSize * 3];
        var fpOutput = new decimal[DataSize * 3];
        
        _qcBB.OnBarBatch(PriceData, qcOutput);
        _fpBB.OnBarBatch(PriceData, fpOutput);
        
        // Validate all three band outputs are equivalent
        bool upperMatch = ValidateOutputs(
            qcOutput.Where((v, i) => i % 3 == 0).Select(v => v == default ? (decimal?)null : v),
            fpOutput.Where((v, i) => i % 3 == 0).Select(v => v == default ? (decimal?)null : v),
            tolerance: 0.01m);
            
        bool middleMatch = ValidateOutputs(
            qcOutput.Where((v, i) => i % 3 == 1).Select(v => v == default ? (decimal?)null : v),
            fpOutput.Where((v, i) => i % 3 == 1).Select(v => v == default ? (decimal?)null : v),
            tolerance: 0.01m);
            
        bool lowerMatch = ValidateOutputs(
            qcOutput.Where((v, i) => i % 3 == 2).Select(v => v == default ? (decimal?)null : v),
            fpOutput.Where((v, i) => i % 3 == 2).Select(v => v == default ? (decimal?)null : v),
            tolerance: 0.01m);
        
        return upperMatch && middleMatch && lowerMatch;
    }
    
    [Benchmark]
    [BenchmarkCategory("BollingerBands", "Memory")]
    public long BollingerBands_QuantConnect_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new BollingerBands_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize * 3];
            indicator.OnBarBatch(PriceData, output);
        });
    }
    
    [Benchmark]
    [BenchmarkCategory("BollingerBands", "Memory")]
    public long BollingerBands_FinancialPython_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new BollingerBands_FP<decimal, decimal>(_parameters);
            var output = new decimal[DataSize * 3];
            indicator.OnBarBatch(PriceData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcBB?.Clear();
        _fpBB?.Clear();
        _qcBB2?.Clear();
        _qcBB25?.Clear();
        _qcBB3?.Clear();
        _fpBB2?.Clear();
        _fpBB25?.Clear();
        _fpBB3?.Clear();
        base.GlobalCleanup();
    }
}
#endif
