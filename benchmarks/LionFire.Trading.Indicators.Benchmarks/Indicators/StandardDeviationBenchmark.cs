using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class StandardDeviationBenchmark : IndicatorBenchmarkBase
{
    private StandardDeviation_QC<decimal, decimal> _qcStdDev = null!;
    private PStandardDeviation<decimal, decimal> _parameters = null!;
    
    private StandardDeviation_QC<decimal, decimal> _qcStdDev10 = null!;
    private StandardDeviation_QC<decimal, decimal> _qcStdDev20 = null!;
    private StandardDeviation_QC<decimal, decimal> _qcStdDev50 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PStandardDeviation<decimal, decimal> { Period = Period };
        _qcStdDev = new StandardDeviation_QC<decimal, decimal>(_parameters);
        
        _qcStdDev10 = new StandardDeviation_QC<decimal, decimal>(new PStandardDeviation<decimal, decimal> { Period = 10 });
        _qcStdDev20 = new StandardDeviation_QC<decimal, decimal>(new PStandardDeviation<decimal, decimal> { Period = 20 });
        _qcStdDev50 = new StandardDeviation_QC<decimal, decimal>(new PStandardDeviation<decimal, decimal> { Period = 50 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("StandardDeviation", "QuantConnect")]
    public decimal[] StandardDeviation_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcStdDev.Clear();
        _qcStdDev.OnBarBatch(PriceData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("StandardDeviation", "QuantConnect")]
    public List<decimal?> StandardDeviation_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcStdDev.Clear();
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcStdDev.OnBarBatch(new[] { PriceData[i] }, output);
            results.Add(_qcStdDev.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("StandardDeviation", "VolatilityLevels")]
    public (int high, int normal, int low) StandardDeviation_ClassifyVolatility()
    {
        _qcStdDev20.Clear();
        var output = new decimal[DataSize];
        _qcStdDev20.OnBarBatch(PriceData, output);
        
        // Calculate thresholds
        var validValues = output.Where(v => v != default).ToList();
        if (validValues.Count == 0) return (0, 0, 0);
        
        var mean = validValues.Average();
        var highThreshold = mean * 1.5m;
        var lowThreshold = mean * 0.5m;
        
        int highVol = 0;
        int normalVol = 0;
        int lowVol = 0;
        
        foreach (var value in validValues)
        {
            if (value > highThreshold) highVol++;
            else if (value < lowThreshold) lowVol++;
            else normalVol++;
        }
        
        return (highVol, normalVol, lowVol);
    }

    [Benchmark]
    [BenchmarkCategory("StandardDeviation", "BollingerBands")]
    public (int above, int within, int below) StandardDeviation_BollingerBandPosition()
    {
        _qcStdDev20.Clear();
        var output = new decimal[1];
        int aboveBand = 0;
        int withinBands = 0;
        int belowBand = 0;
        
        // Calculate 20-period SMA for middle band
        var smaWindow = new Queue<decimal>();
        decimal sma = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcStdDev20.OnBarBatch(new[] { PriceData[i] }, output);
            
            // Update SMA
            smaWindow.Enqueue(PriceData[i]);
            if (smaWindow.Count > 20)
            {
                smaWindow.Dequeue();
            }
            
            if (smaWindow.Count == 20 && _qcStdDev20.IsReady)
            {
                sma = smaWindow.Average();
                var stdDev = output[0];
                
                var upperBand = sma + (2 * stdDev);
                var lowerBand = sma - (2 * stdDev);
                
                if (PriceData[i] > upperBand) aboveBand++;
                else if (PriceData[i] < lowerBand) belowBand++;
                else withinBands++;
            }
        }
        
        return (aboveBand, withinBands, belowBand);
    }

    [Benchmark]
    [BenchmarkCategory("StandardDeviation", "PeriodComparison")]
    public (decimal short_term, decimal medium_term, decimal long_term) StandardDeviation_ComparePeriods()
    {
        _qcStdDev10.Clear();
        _qcStdDev20.Clear();
        _qcStdDev50.Clear();
        
        var output10 = new decimal[DataSize];
        var output20 = new decimal[DataSize];
        var output50 = new decimal[DataSize];
        
        _qcStdDev10.OnBarBatch(PriceData, output10);
        _qcStdDev20.OnBarBatch(PriceData, output20);
        _qcStdDev50.OnBarBatch(PriceData, output50);
        
        // Calculate average standard deviation for each period
        decimal avg10 = output10.Where(v => v != default).DefaultIfEmpty(0).Average();
        decimal avg20 = output20.Where(v => v != default).DefaultIfEmpty(0).Average();
        decimal avg50 = output50.Where(v => v != default).DefaultIfEmpty(0).Average();
        
        return (avg10, avg20, avg50);
    }

    [Benchmark]
    [BenchmarkCategory("StandardDeviation", "VolatilitySpikes")]
    public int StandardDeviation_DetectSpikes()
    {
        _qcStdDev20.Clear();
        var output = new decimal[DataSize];
        _qcStdDev20.OnBarBatch(PriceData, output);
        
        int spikes = 0;
        var lookback = 50;
        
        for (int i = lookback; i < DataSize; i++)
        {
            if (output[i] == default) continue;
            
            // Calculate average StdDev over lookback period
            decimal avgStdDev = 0;
            int count = 0;
            for (int j = i - lookback; j < i; j++)
            {
                if (output[j] != default)
                {
                    avgStdDev += output[j];
                    count++;
                }
            }
            
            if (count > 0)
            {
                avgStdDev /= count;
                
                // Detect spike if current StdDev is 2x the average
                if (output[i] > avgStdDev * 2m)
                {
                    spikes++;
                }
            }
        }
        
        return spikes;
    }

    [Benchmark]
    [BenchmarkCategory("StandardDeviation", "TrendAnalysis")]
    public (int increasing, int decreasing, int stable) StandardDeviation_VolatilityTrend()
    {
        _qcStdDev20.Clear();
        var output = new decimal[1];
        decimal? previousValue = null;
        int increasingVol = 0;
        int decreasingVol = 0;
        int stableVol = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcStdDev20.OnBarBatch(new[] { PriceData[i] }, output);
            
            if (_qcStdDev20.IsReady && previousValue.HasValue)
            {
                var change = output[0] - previousValue.Value;
                var changePercent = Math.Abs(change / previousValue.Value);
                
                if (changePercent < 0.01m) stableVol++;
                else if (change > 0) increasingVol++;
                else decreasingVol++;
            }
            
            previousValue = _qcStdDev20.IsReady ? output[0] : (decimal?)null;
        }
        
        return (increasingVol, decreasingVol, stableVol);
    }

    [Benchmark]
    [BenchmarkCategory("StandardDeviation", "CoefficientOfVariation")]
    public decimal StandardDeviation_CalculateCV()
    {
        _qcStdDev20.Clear();
        var output = new decimal[DataSize];
        _qcStdDev20.OnBarBatch(PriceData, output);
        
        // Calculate Coefficient of Variation (CV = StdDev / Mean)
        var validIndices = new List<int>();
        for (int i = 0; i < DataSize; i++)
        {
            if (output[i] != default)
            {
                validIndices.Add(i);
            }
        }
        
        if (validIndices.Count == 0) return 0;
        
        decimal totalCV = 0;
        int cvCount = 0;
        
        foreach (var idx in validIndices)
        {
            // Calculate mean of the window used for this StdDev
            var windowStart = Math.Max(0, idx - 19);
            var windowEnd = idx + 1;
            var windowMean = PriceData.Skip(windowStart).Take(windowEnd - windowStart).Average();
            
            if (windowMean != 0)
            {
                totalCV += output[idx] / Math.Abs(windowMean);
                cvCount++;
            }
        }
        
        return cvCount > 0 ? totalCV / cvCount : 0;
    }
    
    [Benchmark]
    [BenchmarkCategory("StandardDeviation", "Memory")]
    public long StandardDeviation_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new StandardDeviation_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(PriceData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcStdDev?.Clear();
        _qcStdDev10?.Clear();
        _qcStdDev20?.Clear();
        _qcStdDev50?.Clear();
        base.GlobalCleanup();
    }
}