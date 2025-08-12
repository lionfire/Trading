using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class HullMovingAverageBenchmark : IndicatorBenchmarkBase
{
    private HullMovingAverage_QC<decimal, decimal> _qcHma = null!;
    private PHullMovingAverage<decimal, decimal> _parameters = null!;
    
    private HullMovingAverage_QC<decimal, decimal> _qcHma9 = null!;
    private HullMovingAverage_QC<decimal, decimal> _qcHma16 = null!;
    private HullMovingAverage_QC<decimal, decimal> _qcHma25 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PHullMovingAverage<decimal, decimal> { Period = Period };
        _qcHma = new HullMovingAverage_QC<decimal, decimal>(_parameters);
        
        _qcHma9 = new HullMovingAverage_QC<decimal, decimal>(new PHullMovingAverage<decimal, decimal> { Period = 9 });
        _qcHma16 = new HullMovingAverage_QC<decimal, decimal>(new PHullMovingAverage<decimal, decimal> { Period = 16 });
        _qcHma25 = new HullMovingAverage_QC<decimal, decimal>(new PHullMovingAverage<decimal, decimal> { Period = 25 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("HMA", "QuantConnect")]
    public decimal[] HMA_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcHma.Clear();
        _qcHma.OnBarBatch(PriceData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("HMA", "QuantConnect")]
    public List<decimal?> HMA_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcHma.Clear();
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcHma.OnBarBatch(new[] { PriceData[i] }, output);
            results.Add(_qcHma.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("HMA", "Responsiveness")]
    public decimal HMA_MeasureResponsiveness()
    {
        _qcHma16.Clear();
        var hmaOutput = new decimal[DataSize];
        _qcHma16.OnBarBatch(PriceData, hmaOutput);
        
        // Compare HMA response to price changes vs simple MA
        decimal totalResponseDiff = 0;
        int count = 0;
        
        for (int i = 16; i < DataSize - 1; i++)
        {
            if (hmaOutput[i] != default && hmaOutput[i + 1] != default)
            {
                var priceChange = PriceData[i + 1] - PriceData[i];
                var hmaChange = hmaOutput[i + 1] - hmaOutput[i];
                
                // Calculate simple MA change for comparison
                decimal simpleMAChange = 0;
                if (i >= 31)
                {
                    decimal prevSMA = PriceData.Skip(i - 15).Take(16).Average();
                    decimal currSMA = PriceData.Skip(i - 14).Take(16).Average();
                    simpleMAChange = currSMA - prevSMA;
                }
                
                // Measure how much more responsive HMA is
                if (priceChange != 0 && simpleMAChange != 0)
                {
                    var hmaResponse = Math.Abs(hmaChange / priceChange);
                    var smaResponse = Math.Abs(simpleMAChange / priceChange);
                    totalResponseDiff += hmaResponse - smaResponse;
                    count++;
                }
            }
        }
        
        return count > 0 ? totalResponseDiff / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("HMA", "SmoothnesVsLag")]
    public (decimal smoothness, decimal lag) HMA_AnalyzeSmoothnesVsLag()
    {
        _qcHma16.Clear();
        var output = new decimal[DataSize];
        _qcHma16.OnBarBatch(PriceData, output);
        
        // Calculate smoothness (inverse of volatility)
        var hmaChanges = new List<decimal>();
        var lagMeasurements = new List<decimal>();
        
        for (int i = 17; i < DataSize; i++)
        {
            if (output[i] != default && output[i - 1] != default)
            {
                // Smoothness: measure HMA volatility
                var hmaChange = Math.Abs((output[i] - output[i - 1]) / output[i - 1]);
                hmaChanges.Add(hmaChange);
                
                // Lag: measure how far HMA is behind current price
                var lagDistance = Math.Abs(PriceData[i] - output[i]) / PriceData[i];
                lagMeasurements.Add(lagDistance);
            }
        }
        
        decimal smoothness = hmaChanges.Count > 0 ? hmaChanges.Average() : 0;
        decimal lag = lagMeasurements.Count > 0 ? lagMeasurements.Average() : 0;
        
        return (smoothness, lag);
    }

    [Benchmark]
    [BenchmarkCategory("HMA", "TrendDirection")]
    public (int uptrend, int downtrend, int sideways) HMA_TrendDirection()
    {
        _qcHma16.Clear();
        var output = new decimal[1];
        decimal? previousHma = null;
        int uptrendCount = 0;
        int downtrendCount = 0;
        int sidewaysCount = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcHma16.OnBarBatch(new[] { PriceData[i] }, output);
            
            if (_qcHma16.IsReady && previousHma.HasValue)
            {
                var change = output[0] - previousHma.Value;
                var changePercent = Math.Abs(change / previousHma.Value);
                
                if (changePercent < 0.0005m) sidewaysCount++; // Very small threshold due to HMA smoothness
                else if (change > 0) uptrendCount++;
                else downtrendCount++;
            }
            
            previousHma = _qcHma16.IsReady ? output[0] : (decimal?)null;
        }
        
        return (uptrendCount, downtrendCount, sidewaysCount);
    }

    [Benchmark]
    [BenchmarkCategory("HMA", "PeriodComparison")]
    public (decimal fast_volatility, decimal medium_volatility, decimal slow_volatility) HMA_ComparePeriods()
    {
        _qcHma9.Clear();
        _qcHma16.Clear();
        _qcHma25.Clear();
        
        var output9 = new decimal[DataSize];
        var output16 = new decimal[DataSize];
        var output25 = new decimal[DataSize];
        
        _qcHma9.OnBarBatch(PriceData, output9);
        _qcHma16.OnBarBatch(PriceData, output16);
        _qcHma25.OnBarBatch(PriceData, output25);
        
        // Calculate volatility for each period
        decimal vol9 = CalculateVolatility(output9, 9);
        decimal vol16 = CalculateVolatility(output16, 16);
        decimal vol25 = CalculateVolatility(output25, 25);
        
        return (vol9, vol16, vol25);
    }
    
    private decimal CalculateVolatility(decimal[] values, int startIndex)
    {
        var changes = new List<decimal>();
        
        for (int i = startIndex + 1; i < values.Length; i++)
        {
            if (values[i] != default && values[i - 1] != default && values[i - 1] != 0)
            {
                var change = Math.Abs((values[i] - values[i - 1]) / values[i - 1]);
                changes.Add(change);
            }
        }
        
        return changes.Count > 0 ? changes.Average() : 0;
    }

    [Benchmark]
    [BenchmarkCategory("HMA", "CrossoverSignals")]
    public int HMA_DetectCrossovers()
    {
        _qcHma16.Clear();
        var output = new decimal[1];
        int crossovers = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcHma16.OnBarBatch(new[] { PriceData[i] }, output);
            
            if (_qcHma16.IsReady && i > 0)
            {
                var currentPrice = PriceData[i];
                var previousPrice = PriceData[i - 1];
                var hma = output[0];
                
                // Get previous HMA value
                var prevOutput = new decimal[1];
                _qcHma16.Clear();
                for (int j = 0; j < i; j++)
                {
                    _qcHma16.OnBarBatch(new[] { PriceData[j] }, prevOutput);
                }
                var previousHma = _qcHma16.IsReady ? prevOutput[0] : hma;
                
                // Detect price crossing HMA
                if ((previousPrice <= previousHma && currentPrice > hma) ||
                    (previousPrice >= previousHma && currentPrice < hma))
                {
                    crossovers++;
                }
                
                // Reset for next iteration
                _qcHma16.Clear();
                for (int j = 0; j <= i; j++)
                {
                    _qcHma16.OnBarBatch(new[] { PriceData[j] }, output);
                }
            }
        }
        
        return crossovers;
    }

    [Benchmark]
    [BenchmarkCategory("HMA", "NoiseReduction")]
    public decimal HMA_NoiseReductionEfficiency()
    {
        _qcHma16.Clear();
        var output = new decimal[DataSize];
        _qcHma16.OnBarBatch(PriceData, output);
        
        // Compare price noise vs HMA noise
        decimal priceNoise = CalculateNoise(PriceData.Take(DataSize).ToArray());
        decimal hmaNoise = CalculateNoise(output.Where(v => v != default).ToArray());
        
        // Return noise reduction ratio
        return priceNoise > 0 ? hmaNoise / priceNoise : 0;
    }
    
    private decimal CalculateNoise(decimal[] values)
    {
        if (values.Length < 3) return 0;
        
        decimal totalNoise = 0;
        for (int i = 2; i < values.Length; i++)
        {
            // Measure direction changes as noise
            var change1 = values[i - 1] - values[i - 2];
            var change2 = values[i] - values[i - 1];
            
            // If direction changes, it's noise
            if ((change1 > 0 && change2 < 0) || (change1 < 0 && change2 > 0))
            {
                totalNoise += Math.Abs(change2);
            }
        }
        
        return totalNoise / values.Length;
    }
    
    [Benchmark]
    [BenchmarkCategory("HMA", "Memory")]
    public long HMA_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new HullMovingAverage_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(PriceData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcHma?.Clear();
        _qcHma9?.Clear();
        _qcHma16?.Clear();
        _qcHma25?.Clear();
        base.GlobalCleanup();
    }
}