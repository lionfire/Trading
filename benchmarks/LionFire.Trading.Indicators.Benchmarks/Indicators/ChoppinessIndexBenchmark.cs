using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ChoppinessIndexBenchmark : IndicatorBenchmarkBase
{
    private ChoppinessIndex_FP<decimal, decimal> _fpChoppiness = null!;
    private PChoppinessIndex<decimal, decimal> _parameters = null!;
    
    private ChoppinessIndex_FP<decimal, decimal> _fpChoppinessShort = null!;
    private ChoppinessIndex_FP<decimal, decimal> _fpChoppinessLong = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PChoppinessIndex<decimal, decimal> 
        { 
            Period = 14,
            ChoppyThreshold = 61.8m,
            TrendingThreshold = 38.2m
        };
        _fpChoppiness = new ChoppinessIndex_FP<decimal, decimal>(_parameters);
        
        // Short period (more sensitive)
        _fpChoppinessShort = new ChoppinessIndex_FP<decimal, decimal>(new PChoppinessIndex<decimal, decimal> 
        { 
            Period = 7,
            ChoppyThreshold = 65m,
            TrendingThreshold = 35m
        });
        
        // Long period (less sensitive) 
        _fpChoppinessLong = new ChoppinessIndex_FP<decimal, decimal>(new PChoppinessIndex<decimal, decimal> 
        { 
            Period = 28,
            ChoppyThreshold = 58m,
            TrendingThreshold = 42m
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ChoppinessIndex", "FinancialPython")]
    public decimal[] ChoppinessIndex_FinancialPython_Batch()
    {
        var output = new decimal[DataSize];
        
        _fpChoppiness.Clear();
        _fpChoppiness.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("ChoppinessIndex", "FinancialPython")]
    public List<decimal?> ChoppinessIndex_FinancialPython_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _fpChoppiness.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpChoppiness.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_fpChoppiness.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("ChoppinessIndex", "MarketCondition")]
    public (int choppy_periods, int trending_periods, int neutral_periods) ChoppinessIndex_ClassifyMarketConditions()
    {
        _fpChoppiness.Clear();
        var output = new decimal[1];
        int choppyPeriods = 0;
        int trendingPeriods = 0;
        int neutralPeriods = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpChoppiness.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpChoppiness.IsReady)
            {
                var value = output[0];
                if (value > _parameters.ChoppyThreshold) choppyPeriods++;
                else if (value < _parameters.TrendingThreshold) trendingPeriods++;
                else neutralPeriods++;
            }
        }
        
        return (choppyPeriods, trendingPeriods, neutralPeriods);
    }

    [Benchmark]
    [BenchmarkCategory("ChoppinessIndex", "TrendDetection")]
    public (int trend_changes, decimal avg_trend_length) ChoppinessIndex_DetectTrendChanges()
    {
        _fpChoppiness.Clear();
        var choppinessOutput = new decimal[DataSize];
        _fpChoppiness.OnBarBatch(HLCData, choppinessOutput);
        
        int trendChanges = 0;
        var trendLengths = new List<int>();
        int currentTrendLength = 0;
        bool wasTrending = false;
        
        for (int i = 14; i < DataSize; i++)
        {
            if (choppinessOutput[i] != default)
            {
                bool isTrending = choppinessOutput[i] < _parameters.TrendingThreshold;
                
                if (isTrending != wasTrending && i > 14)
                {
                    if (currentTrendLength > 0)
                    {
                        trendLengths.Add(currentTrendLength);
                        trendChanges++;
                    }
                    currentTrendLength = 1;
                }
                else if (isTrending)
                {
                    currentTrendLength++;
                }
                else
                {
                    if (currentTrendLength > 0 && wasTrending)
                    {
                        trendLengths.Add(currentTrendLength);
                        currentTrendLength = 0;
                    }
                }
                
                wasTrending = isTrending;
            }
        }
        
        decimal avgTrendLength = trendLengths.Count > 0 ? trendLengths.Average() : 0;
        return (trendChanges, avgTrendLength);
    }

    [Benchmark]
    [BenchmarkCategory("ChoppinessIndex", "VolatilityCorrelation")]
    public decimal ChoppinessIndex_VolatilityCorrelation()
    {
        _fpChoppiness.Clear();
        var choppinessOutput = new decimal[DataSize];
        _fpChoppiness.OnBarBatch(HLCData, choppinessOutput);
        
        // Calculate true range for volatility comparison
        var volatilities = new List<decimal>();
        var choppinessValues = new List<decimal>();
        
        for (int i = 15; i < DataSize; i++)
        {
            if (choppinessOutput[i] != default)
            {
                // Calculate True Range as volatility measure
                var trueRange = Math.Max(HLCData[i].High - HLCData[i].Low,
                    Math.Max(Math.Abs(HLCData[i].High - HLCData[i-1].Close),
                           Math.Abs(HLCData[i].Low - HLCData[i-1].Close)));
                
                volatilities.Add(trueRange);
                choppinessValues.Add(choppinessOutput[i]);
            }
        }
        
        if (choppinessValues.Count < 2) return 0;
        
        // Simple correlation calculation
        decimal choppinessMean = choppinessValues.Average();
        decimal volatilityMean = volatilities.Average();
        
        decimal covariance = 0;
        decimal choppinessVariance = 0;
        decimal volatilityVariance = 0;
        
        for (int i = 0; i < choppinessValues.Count; i++)
        {
            var choppinessDiff = choppinessValues[i] - choppinessMean;
            var volatilityDiff = volatilities[i] - volatilityMean;
            
            covariance += choppinessDiff * volatilityDiff;
            choppinessVariance += choppinessDiff * choppinessDiff;
            volatilityVariance += volatilityDiff * volatilityDiff;
        }
        
        if (choppinessVariance == 0 || volatilityVariance == 0) return 0;
        
        return covariance / (decimal)Math.Sqrt((double)(choppinessVariance * volatilityVariance));
    }

    [Benchmark]
    [BenchmarkCategory("ChoppinessIndex", "PeriodComparison")]
    public (decimal short_avg, decimal standard_avg, decimal long_avg) ChoppinessIndex_ComparePeriods()
    {
        _fpChoppinessShort.Clear();
        _fpChoppiness.Clear();
        _fpChoppinessLong.Clear();
        
        var outputShort = new decimal[DataSize];
        var outputStandard = new decimal[DataSize];
        var outputLong = new decimal[DataSize];
        
        _fpChoppinessShort.OnBarBatch(HLCData, outputShort);
        _fpChoppiness.OnBarBatch(HLCData, outputStandard);
        _fpChoppinessLong.OnBarBatch(HLCData, outputLong);
        
        // Calculate averages for ready values
        decimal shortAvg = CalculateAverage(outputShort, 7);
        decimal standardAvg = CalculateAverage(outputStandard, 14);
        decimal longAvg = CalculateAverage(outputLong, 28);
        
        return (shortAvg, standardAvg, longAvg);
    }
    
    private decimal CalculateAverage(decimal[] values, int startIndex)
    {
        var validValues = values.Skip(startIndex).Where(v => v != default).ToArray();
        return validValues.Length > 0 ? validValues.Average() : 0;
    }

    [Benchmark]
    [BenchmarkCategory("ChoppinessIndex", "ThresholdSensitivity")]
    public (int strict_choppy, int normal_choppy, int strict_trending, int normal_trending) ChoppinessIndex_ThresholdSensitivity()
    {
        _fpChoppiness.Clear();
        var choppinessOutput = new decimal[DataSize];
        _fpChoppiness.OnBarBatch(HLCData, choppinessOutput);
        
        int strictChoppy = 0;      // > 70
        int normalChoppy = 0;      // > 61.8 (default)
        int strictTrending = 0;    // < 30
        int normalTrending = 0;    // < 38.2 (default)
        
        for (int i = 14; i < DataSize; i++)
        {
            if (choppinessOutput[i] != default)
            {
                var value = choppinessOutput[i];
                
                if (value > 70m) strictChoppy++;
                if (value > 61.8m) normalChoppy++;
                if (value < 30m) strictTrending++;
                if (value < 38.2m) normalTrending++;
            }
        }
        
        return (strictChoppy, normalChoppy, strictTrending, normalTrending);
    }

    [Benchmark]
    [BenchmarkCategory("ChoppinessIndex", "BreakoutPrediction")]
    public int ChoppinessIndex_PredictBreakouts()
    {
        _fpChoppiness.Clear();
        var choppinessOutput = new decimal[DataSize];
        _fpChoppiness.OnBarBatch(HLCData, choppinessOutput);
        
        int successfulPredictions = 0;
        int lookforward = 5; // Look 5 periods ahead for breakout
        
        for (int i = 50; i < DataSize - lookforward; i++)
        {
            if (choppinessOutput[i] != default)
            {
                // Check if choppiness was high (choppy conditions)
                if (choppinessOutput[i] > _parameters.ChoppyThreshold)
                {
                    // Check for breakout in next periods
                    var currentRange = HLCData[i].High - HLCData[i].Low;
                    bool breakoutOccurred = false;
                    
                    for (int j = i + 1; j <= i + lookforward; j++)
                    {
                        var futureRange = HLCData[j].High - HLCData[j].Low;
                        if (futureRange > currentRange * 1.5m) // 50% increase in range
                        {
                            breakoutOccurred = true;
                            break;
                        }
                    }
                    
                    if (breakoutOccurred)
                    {
                        successfulPredictions++;
                    }
                }
            }
        }
        
        return successfulPredictions;
    }

    [Benchmark]
    [BenchmarkCategory("ChoppinessIndex", "TrueRangeAnalysis")]
    public (decimal avg_true_range_sum, decimal avg_max_range, decimal efficiency_ratio) ChoppinessIndex_TrueRangeAnalysis()
    {
        _fpChoppiness.Clear();
        var output = new decimal[1];
        var trueRangeSums = new List<decimal>();
        var maxRanges = new List<decimal>();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _fpChoppiness.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_fpChoppiness.IsReady)
            {
                trueRangeSums.Add(_fpChoppiness.TrueRangeSum);
                maxRanges.Add(_fpChoppiness.MaxRange);
            }
        }
        
        decimal avgTrueRangeSum = trueRangeSums.Count > 0 ? trueRangeSums.Average() : 0;
        decimal avgMaxRange = maxRanges.Count > 0 ? maxRanges.Average() : 0;
        decimal efficiencyRatio = avgMaxRange > 0 ? avgTrueRangeSum / avgMaxRange : 0;
        
        return (avgTrueRangeSum, avgMaxRange, efficiencyRatio);
    }
    
    [Benchmark]
    [BenchmarkCategory("ChoppinessIndex", "Memory")]
    public long ChoppinessIndex_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new ChoppinessIndex_FP<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _fpChoppiness?.Clear();
        _fpChoppinessShort?.Clear();
        _fpChoppinessLong?.Clear();
        base.GlobalCleanup();
    }
}