using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class AwesomeOscillatorBenchmark : IndicatorBenchmarkBase
{
    private AwesomeOscillator_QC<decimal, decimal> _qcAo = null!;
    private PAwesomeOscillator<decimal, decimal> _parameters = null!;
    
    private AwesomeOscillator_QC<decimal, decimal> _qcAoFast = null!;
    private AwesomeOscillator_QC<decimal, decimal> _qcAoStandard = null!;
    private AwesomeOscillator_QC<decimal, decimal> _qcAoSlow = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PAwesomeOscillator<decimal, decimal> 
        { 
            FastPeriod = 5,
            SlowPeriod = 34
        };
        _qcAo = new AwesomeOscillator_QC<decimal, decimal>(_parameters);
        
        // Fast settings
        _qcAoFast = new AwesomeOscillator_QC<decimal, decimal>(new PAwesomeOscillator<decimal, decimal> 
        { 
            FastPeriod = 3,
            SlowPeriod = 20
        });
        
        // Standard settings (Bill Williams)
        _qcAoStandard = new AwesomeOscillator_QC<decimal, decimal>(new PAwesomeOscillator<decimal, decimal> 
        { 
            FastPeriod = 5,
            SlowPeriod = 34
        });
        
        // Slow settings
        _qcAoSlow = new AwesomeOscillator_QC<decimal, decimal>(new PAwesomeOscillator<decimal, decimal> 
        { 
            FastPeriod = 8,
            SlowPeriod = 50
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("AwesomeOscillator", "QuantConnect")]
    public decimal[] AwesomeOscillator_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcAo.Clear();
        _qcAo.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("AwesomeOscillator", "QuantConnect")]
    public List<decimal?> AwesomeOscillator_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcAo.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAo.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcAo.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("AwesomeOscillator", "MomentumDirection")]
    public (int bullish, int bearish) AwesomeOscillator_MomentumDirection()
    {
        _qcAoStandard.Clear();
        var output = new decimal[1];
        int bullishBars = 0;
        int bearishBars = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAoStandard.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcAoStandard.IsReady)
            {
                if (output[0] > 0) bullishBars++;
                else bearishBars++;
            }
        }
        
        return (bullishBars, bearishBars);
    }

    [Benchmark]
    [BenchmarkCategory("AwesomeOscillator", "ZeroCrossings")]
    public int AwesomeOscillator_CountZeroCrossings()
    {
        _qcAoStandard.Clear();
        var output = new decimal[1];
        decimal? previousValue = null;
        int crossings = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAoStandard.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcAoStandard.IsReady && previousValue.HasValue)
            {
                if ((previousValue.Value < 0 && output[0] > 0) ||
                    (previousValue.Value > 0 && output[0] < 0))
                {
                    crossings++;
                }
            }
            
            previousValue = _qcAoStandard.IsReady ? output[0] : (decimal?)null;
        }
        
        return crossings;
    }

    [Benchmark]
    [BenchmarkCategory("AwesomeOscillator", "Saucer")]
    public int AwesomeOscillator_DetectSaucerSignals()
    {
        _qcAoStandard.Clear();
        var output = new decimal[DataSize];
        _qcAoStandard.OnBarBatch(HLCData, output);
        
        int saucers = 0;
        
        // Look for saucer pattern: 3 consecutive bars below zero, with middle bar lowest
        for (int i = 36; i < DataSize - 2; i++)
        {
            if (output[i] != default && output[i + 1] != default && output[i + 2] != default)
            {
                var bar1 = output[i];
                var bar2 = output[i + 1];
                var bar3 = output[i + 2];
                
                // Bullish saucer: all below zero, middle lowest, third higher than second
                if (bar1 < 0 && bar2 < 0 && bar3 < 0 && 
                    bar2 < bar1 && bar3 > bar2)
                {
                    saucers++;
                }
                // Bearish saucer: all above zero, middle highest, third lower than second
                else if (bar1 > 0 && bar2 > 0 && bar3 > 0 && 
                         bar2 > bar1 && bar3 < bar2)
                {
                    saucers++;
                }
            }
        }
        
        return saucers;
    }

    [Benchmark]
    [BenchmarkCategory("AwesomeOscillator", "TwinPeaks")]
    public int AwesomeOscillator_DetectTwinPeaks()
    {
        _qcAoStandard.Clear();
        var output = new decimal[DataSize];
        _qcAoStandard.OnBarBatch(HLCData, output);
        
        int twinPeaks = 0;
        
        // Look for twin peaks pattern: two peaks above zero with valley between
        for (int i = 50; i < DataSize - 20; i++)
        {
            if (output[i] != default && output[i] > 0)
            {
                // Look for a peak
                bool isPeak = true;
                for (int j = 1; j <= 5; j++)
                {
                    if (i - j >= 0 && output[i - j] != default && output[i - j] >= output[i])
                        isPeak = false;
                    if (i + j < DataSize && output[i + j] != default && output[i + j] >= output[i])
                        isPeak = false;
                }
                
                if (isPeak)
                {
                    // Look for second peak within next 15-30 bars
                    for (int k = i + 10; k < Math.Min(i + 30, DataSize - 5); k++)
                    {
                        if (output[k] != default && output[k] > 0 && output[k] < output[i])
                        {
                            bool isSecondPeak = true;
                            for (int j = 1; j <= 3; j++)
                            {
                                if (k - j >= 0 && output[k - j] != default && output[k - j] >= output[k])
                                    isSecondPeak = false;
                                if (k + j < DataSize && output[k + j] != default && output[k + j] >= output[k])
                                    isSecondPeak = false;
                            }
                            
                            if (isSecondPeak)
                            {
                                twinPeaks++;
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        return twinPeaks;
    }

    [Benchmark]
    [BenchmarkCategory("AwesomeOscillator", "PeriodComparison")]
    public (decimal fast_volatility, decimal standard_volatility, decimal slow_volatility) AwesomeOscillator_ComparePeriods()
    {
        _qcAoFast.Clear();
        _qcAoStandard.Clear();
        _qcAoSlow.Clear();
        
        var outputFast = new decimal[DataSize];
        var outputStandard = new decimal[DataSize];
        var outputSlow = new decimal[DataSize];
        
        _qcAoFast.OnBarBatch(HLCData, outputFast);
        _qcAoStandard.OnBarBatch(HLCData, outputStandard);
        _qcAoSlow.OnBarBatch(HLCData, outputSlow);
        
        // Calculate volatility for each setting
        decimal volFast = CalculateVolatility(outputFast, 20);
        decimal volStandard = CalculateVolatility(outputStandard, 34);
        decimal volSlow = CalculateVolatility(outputSlow, 50);
        
        return (volFast, volStandard, volSlow);
    }
    
    private decimal CalculateVolatility(decimal[] values, int startIndex)
    {
        var changes = new List<decimal>();
        
        for (int i = startIndex + 1; i < values.Length; i++)
        {
            if (values[i] != default && values[i - 1] != default)
            {
                changes.Add(Math.Abs(values[i] - values[i - 1]));
            }
        }
        
        return changes.Count > 0 ? changes.Average() : 0;
    }

    [Benchmark]
    [BenchmarkCategory("AwesomeOscillator", "Divergence")]
    public int AwesomeOscillator_DetectDivergence()
    {
        _qcAoStandard.Clear();
        var output = new decimal[DataSize];
        _qcAoStandard.OnBarBatch(HLCData, output);
        
        int divergences = 0;
        int lookback = 25;
        
        for (int i = lookback + 34; i < DataSize; i++)
        {
            if (output[i] == default || output[i - lookback] == default) continue;
            
            // Calculate median price trend
            var currentMedian = (HLCData[i].High + HLCData[i].Low) / 2;
            var previousMedian = (HLCData[i - lookback].High + HLCData[i - lookback].Low) / 2;
            var priceTrend = currentMedian - previousMedian;
            
            // AO trend
            var aoTrend = output[i] - output[i - lookback];
            
            // Check for divergence
            if ((priceTrend > 0 && aoTrend < 0) || (priceTrend < 0 && aoTrend > 0))
            {
                divergences++;
            }
        }
        
        return divergences;
    }
    
    [Benchmark]
    [BenchmarkCategory("AwesomeOscillator", "Memory")]
    public long AwesomeOscillator_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new AwesomeOscillator_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcAo?.Clear();
        _qcAoFast?.Clear();
        _qcAoStandard?.Clear();
        _qcAoSlow?.Clear();
        base.GlobalCleanup();
    }
}