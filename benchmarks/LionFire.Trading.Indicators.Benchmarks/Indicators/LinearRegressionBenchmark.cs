using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class LinearRegressionBenchmark : IndicatorBenchmarkBase
{
    private LinearRegression_QC<decimal, decimal> _qcLinReg = null!;
    private PLinearRegression<decimal, decimal> _parameters = null!;
    
    private LinearRegression_QC<decimal, decimal> _qcLinReg10 = null!;
    private LinearRegression_QC<decimal, decimal> _qcLinReg20 = null!;
    private LinearRegression_QC<decimal, decimal> _qcLinReg50 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PLinearRegression<decimal, decimal> { Period = Period };
        _qcLinReg = new LinearRegression_QC<decimal, decimal>(_parameters);
        
        _qcLinReg10 = new LinearRegression_QC<decimal, decimal>(new PLinearRegression<decimal, decimal> { Period = 10 });
        _qcLinReg20 = new LinearRegression_QC<decimal, decimal>(new PLinearRegression<decimal, decimal> { Period = 20 });
        _qcLinReg50 = new LinearRegression_QC<decimal, decimal>(new PLinearRegression<decimal, decimal> { Period = 50 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("LinearRegression", "QuantConnect")]
    public decimal[] LinearRegression_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcLinReg.Clear();
        _qcLinReg.OnBarBatch(PriceData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("LinearRegression", "QuantConnect")]
    public List<decimal?> LinearRegression_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcLinReg.Clear();
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcLinReg.OnBarBatch(new[] { PriceData[i] }, output);
            results.Add(_qcLinReg.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("LinearRegression", "TrendStrength")]
    public decimal LinearRegression_MeasureTrendStrength()
    {
        _qcLinReg20.Clear();
        var output = new decimal[DataSize];
        _qcLinReg20.OnBarBatch(PriceData, output);
        
        // Calculate average slope to measure trend strength
        decimal totalSlope = 0;
        int count = 0;
        
        for (int i = 21; i < DataSize; i++)
        {
            if (output[i] != default && output[i - 1] != default)
            {
                var slope = output[i] - output[i - 1];
                totalSlope += Math.Abs(slope);
                count++;
            }
        }
        
        return count > 0 ? totalSlope / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("LinearRegression", "FitQuality")]
    public decimal LinearRegression_MeasureFitQuality()
    {
        _qcLinReg20.Clear();
        var output = new decimal[DataSize];
        _qcLinReg20.OnBarBatch(PriceData, output);
        
        // Calculate R-squared approximation (how well line fits data)
        decimal totalSquaredError = 0;
        decimal totalVariance = 0;
        int count = 0;
        
        for (int i = 20; i < DataSize - 20; i++)
        {
            if (output[i] != default)
            {
                // Calculate mean of surrounding prices
                decimal mean = 0;
                for (int j = i - 10; j <= i + 10; j++)
                {
                    mean += PriceData[j];
                }
                mean /= 21;
                
                // Calculate squared error and total variance
                var error = PriceData[i] - output[i];
                var variance = PriceData[i] - mean;
                
                totalSquaredError += error * error;
                totalVariance += variance * variance;
                count++;
            }
        }
        
        // Return approximation of R-squared (1 - SSE/SST)
        if (totalVariance > 0)
        {
            return Math.Max(0, 1 - (totalSquaredError / totalVariance));
        }
        
        return 0;
    }

    [Benchmark]
    [BenchmarkCategory("LinearRegression", "Support")]
    public int LinearRegression_TestAsSupport()
    {
        _qcLinReg20.Clear();
        var output = new decimal[1];
        decimal? previousLinReg = null;
        decimal? previousPrice = null;
        int supportTests = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcLinReg20.OnBarBatch(new[] { PriceData[i] }, output);
            
            if (_qcLinReg20.IsReady && previousLinReg.HasValue && previousPrice.HasValue)
            {
                var currentPrice = PriceData[i];
                var currentLinReg = output[0];
                
                // Test if price bounced off linear regression line (support/resistance)
                if (previousPrice.Value > previousLinReg.Value && 
                    currentPrice <= currentLinReg * 1.005m && 
                    currentPrice >= currentLinReg * 0.995m)
                {
                    supportTests++;
                }
            }
            
            previousLinReg = _qcLinReg20.IsReady ? output[0] : (decimal?)null;
            previousPrice = PriceData[i];
        }
        
        return supportTests;
    }

    [Benchmark]
    [BenchmarkCategory("LinearRegression", "ChannelWidth")]
    public decimal LinearRegression_AverageChannelWidth()
    {
        _qcLinReg20.Clear();
        var output = new decimal[DataSize];
        _qcLinReg20.OnBarBatch(PriceData, output);
        
        decimal totalWidth = 0;
        int count = 0;
        
        // Calculate channel width as standard deviation from regression line
        for (int i = 20; i < DataSize - 10; i++)
        {
            if (output[i] != default)
            {
                decimal sumSquaredDeviations = 0;
                for (int j = i - 10; j <= i + 10; j++)
                {
                    if (j >= 0 && j < DataSize)
                    {
                        var deviation = PriceData[j] - output[i];
                        sumSquaredDeviations += deviation * deviation;
                    }
                }
                
                var standardDeviation = (decimal)Math.Sqrt((double)(sumSquaredDeviations / 21));
                totalWidth += standardDeviation * 2; // 2 standard deviations for channel
                count++;
            }
        }
        
        return count > 0 ? totalWidth / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("LinearRegression", "PeriodComparison")]
    public (decimal short_slope, decimal medium_slope, decimal long_slope) LinearRegression_CompareSlopes()
    {
        _qcLinReg10.Clear();
        _qcLinReg20.Clear();
        _qcLinReg50.Clear();
        
        var output10 = new decimal[DataSize];
        var output20 = new decimal[DataSize];
        var output50 = new decimal[DataSize];
        
        _qcLinReg10.OnBarBatch(PriceData, output10);
        _qcLinReg20.OnBarBatch(PriceData, output20);
        _qcLinReg50.OnBarBatch(PriceData, output50);
        
        // Calculate average slopes for each period
        decimal slope10 = CalculateAverageSlope(output10, 10);
        decimal slope20 = CalculateAverageSlope(output20, 20);
        decimal slope50 = CalculateAverageSlope(output50, 50);
        
        return (slope10, slope20, slope50);
    }
    
    private decimal CalculateAverageSlope(decimal[] values, int startIndex)
    {
        decimal totalSlope = 0;
        int count = 0;
        
        for (int i = startIndex + 1; i < values.Length; i++)
        {
            if (values[i] != default && values[i - 1] != default)
            {
                totalSlope += Math.Abs(values[i] - values[i - 1]);
                count++;
            }
        }
        
        return count > 0 ? totalSlope / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("LinearRegression", "Breakouts")]
    public int LinearRegression_DetectBreakouts()
    {
        _qcLinReg20.Clear();
        var output = new decimal[DataSize];
        _qcLinReg20.OnBarBatch(PriceData, output);
        
        int breakouts = 0;
        decimal breakoutThreshold = 0.02m; // 2% breakout threshold
        
        for (int i = 21; i < DataSize; i++)
        {
            if (output[i] != default)
            {
                var price = PriceData[i];
                var regression = output[i];
                var deviation = Math.Abs((price - regression) / regression);
                
                if (deviation > breakoutThreshold)
                {
                    breakouts++;
                }
            }
        }
        
        return breakouts;
    }
    
    [Benchmark]
    [BenchmarkCategory("LinearRegression", "Memory")]
    public long LinearRegression_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new LinearRegression_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(PriceData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcLinReg?.Clear();
        _qcLinReg10?.Clear();
        _qcLinReg20?.Clear();
        _qcLinReg50?.Clear();
        base.GlobalCleanup();
    }
}