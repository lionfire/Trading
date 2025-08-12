using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class RocBenchmark : IndicatorBenchmarkBase
{
    private ROC_QC<decimal, decimal> _qcRoc = null!;
    private PROC<decimal, decimal> _parameters = null!;
    
    private ROC_QC<decimal, decimal> _qcRoc10 = null!;
    private ROC_QC<decimal, decimal> _qcRoc20 = null!;
    private ROC_QC<decimal, decimal> _qcRoc50 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PROC<decimal, decimal> { Period = Period };
        _qcRoc = new ROC_QC<decimal, decimal>(_parameters);
        
        _qcRoc10 = new ROC_QC<decimal, decimal>(new PROC<decimal, decimal> { Period = 10 });
        _qcRoc20 = new ROC_QC<decimal, decimal>(new PROC<decimal, decimal> { Period = 20 });
        _qcRoc50 = new ROC_QC<decimal, decimal>(new PROC<decimal, decimal> { Period = 50 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ROC", "QuantConnect")]
    public decimal[] ROC_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcRoc.Clear();
        _qcRoc.OnBarBatch(PriceData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("ROC", "QuantConnect")]
    public List<decimal?> ROC_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcRoc.Clear();
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcRoc.OnBarBatch(new[] { PriceData[i] }, output);
            results.Add(_qcRoc.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("ROC", "MomentumDetection")]
    public (int positive, int negative, int neutral) ROC_MomentumDirection()
    {
        _qcRoc20.Clear();
        var output = new decimal[1];
        int positiveMomentum = 0;
        int negativeMomentum = 0;
        int neutralMomentum = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcRoc20.OnBarBatch(new[] { PriceData[i] }, output);
            if (_qcRoc20.IsReady)
            {
                if (output[0] > 1m) positiveMomentum++;
                else if (output[0] < -1m) negativeMomentum++;
                else neutralMomentum++;
            }
        }
        
        return (positiveMomentum, negativeMomentum, neutralMomentum);
    }

    [Benchmark]
    [BenchmarkCategory("ROC", "ExtremeLevels")]
    public (int extreme_positive, int extreme_negative) ROC_DetectExtremes()
    {
        _qcRoc20.Clear();
        var output = new decimal[1];
        int extremePositive = 0;
        int extremeNegative = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcRoc20.OnBarBatch(new[] { PriceData[i] }, output);
            if (_qcRoc20.IsReady)
            {
                if (output[0] > 10m) extremePositive++;
                else if (output[0] < -10m) extremeNegative++;
            }
        }
        
        return (extremePositive, extremeNegative);
    }

    [Benchmark]
    [BenchmarkCategory("ROC", "ZeroCrossings")]
    public int ROC_CountZeroCrossings()
    {
        _qcRoc20.Clear();
        var output = new decimal[1];
        decimal? previousValue = null;
        int crossings = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcRoc20.OnBarBatch(new[] { PriceData[i] }, output);
            
            if (_qcRoc20.IsReady && previousValue.HasValue)
            {
                if ((previousValue.Value < 0 && output[0] > 0) || 
                    (previousValue.Value > 0 && output[0] < 0))
                {
                    crossings++;
                }
            }
            
            previousValue = _qcRoc20.IsReady ? output[0] : (decimal?)null;
        }
        
        return crossings;
    }

    [Benchmark]
    [BenchmarkCategory("ROC", "PeriodComparison")]
    public (decimal short_term, decimal medium_term, decimal long_term) ROC_ComparePeriods()
    {
        _qcRoc10.Clear();
        _qcRoc20.Clear();
        _qcRoc50.Clear();
        
        var output10 = new decimal[DataSize];
        var output20 = new decimal[DataSize];
        var output50 = new decimal[DataSize];
        
        _qcRoc10.OnBarBatch(PriceData, output10);
        _qcRoc20.OnBarBatch(PriceData, output20);
        _qcRoc50.OnBarBatch(PriceData, output50);
        
        // Calculate average absolute ROC for each period
        decimal avg10 = 0, avg20 = 0, avg50 = 0;
        int count10 = 0, count20 = 0, count50 = 0;
        
        for (int i = 0; i < DataSize; i++)
        {
            if (output10[i] != default) { avg10 += Math.Abs(output10[i]); count10++; }
            if (output20[i] != default) { avg20 += Math.Abs(output20[i]); count20++; }
            if (output50[i] != default) { avg50 += Math.Abs(output50[i]); count50++; }
        }
        
        return (
            count10 > 0 ? avg10 / count10 : 0,
            count20 > 0 ? avg20 / count20 : 0,
            count50 > 0 ? avg50 / count50 : 0
        );
    }

    [Benchmark]
    [BenchmarkCategory("ROC", "Divergence")]
    public int ROC_DetectDivergence()
    {
        _qcRoc20.Clear();
        var output = new decimal[DataSize];
        _qcRoc20.OnBarBatch(PriceData, output);
        
        int divergences = 0;
        int lookback = 20;
        
        for (int i = lookback + 20; i < DataSize; i++)
        {
            if (output[i] == default || output[i - lookback] == default) continue;
            
            // Price trend
            var priceTrend = PriceData[i] - PriceData[i - lookback];
            
            // ROC trend
            var rocTrend = output[i] - output[i - lookback];
            
            // Check for divergence
            if ((priceTrend > 0 && rocTrend < 0) || (priceTrend < 0 && rocTrend > 0))
            {
                divergences++;
            }
        }
        
        return divergences;
    }

    [Benchmark]
    [BenchmarkCategory("ROC", "Volatility")]
    public decimal ROC_MeasureVolatility()
    {
        _qcRoc20.Clear();
        var output = new decimal[DataSize];
        _qcRoc20.OnBarBatch(PriceData, output);
        
        decimal sumSquaredDiff = 0;
        int count = 0;
        decimal sum = 0;
        
        // Calculate mean
        for (int i = 0; i < DataSize; i++)
        {
            if (output[i] != default)
            {
                sum += output[i];
                count++;
            }
        }
        
        if (count == 0) return 0;
        decimal mean = sum / count;
        
        // Calculate variance
        for (int i = 0; i < DataSize; i++)
        {
            if (output[i] != default)
            {
                var diff = output[i] - mean;
                sumSquaredDiff += diff * diff;
            }
        }
        
        // Return standard deviation
        return (decimal)Math.Sqrt((double)(sumSquaredDiff / count));
    }
    
    [Benchmark]
    [BenchmarkCategory("ROC", "Memory")]
    public long ROC_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new ROC_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(PriceData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcRoc?.Clear();
        _qcRoc10?.Clear();
        _qcRoc20?.Clear();
        _qcRoc50?.Clear();
        base.GlobalCleanup();
    }
}