using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class HeikinAshiBenchmark : IndicatorBenchmarkBase
{
    private HeikinAshi_FP<OHLCData, decimal> _fpHeikinAshi = null!;
    private PHeikinAshi<OHLCData, decimal> _parameters = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PHeikinAshi<OHLCData, decimal>();
        _fpHeikinAshi = new HeikinAshi_FP<OHLCData, decimal>(_parameters);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("HeikinAshi", "FinancialPython")]
    public decimal[] HeikinAshi_FinancialPython_Batch()
    {
        var output = new decimal[DataSize * 4]; // OHLC output
        
        _fpHeikinAshi.Clear();
        _fpHeikinAshi.OnBarBatch(OHLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("HeikinAshi", "FinancialPython")]
    public List<HeikinAshiResult?> HeikinAshi_FinancialPython_Streaming()
    {
        var results = new List<HeikinAshiResult?>(DataSize);
        var output = new decimal[4]; // OHLC output
        
        _fpHeikinAshi.Clear();
        
        for (int i = 0; i < OHLCData.Length; i++)
        {
            _fpHeikinAshi.OnBarBatch(new[] { OHLCData[i] }, output);
            
            if (_fpHeikinAshi.IsReady)
            {
                results.Add(new HeikinAshiResult
                {
                    Open = _fpHeikinAshi.HA_Open,
                    High = _fpHeikinAshi.HA_High,
                    Low = _fpHeikinAshi.HA_Low,
                    Close = _fpHeikinAshi.HA_Close
                });
            }
            else
            {
                results.Add(null);
            }
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("HeikinAshi", "TrendSmoothing")]
    public (int ha_up_candles, int ha_down_candles, int ha_doji_candles) HeikinAshi_AnalyzeTrendSmoothing()
    {
        _fpHeikinAshi.Clear();
        var output = new decimal[4];
        int upCandles = 0;
        int downCandles = 0;
        int dojiCandles = 0;
        
        for (int i = 0; i < OHLCData.Length; i++)
        {
            _fpHeikinAshi.OnBarBatch(new[] { OHLCData[i] }, output);
            
            if (_fpHeikinAshi.IsReady)
            {
                var haOpen = _fpHeikinAshi.HA_Open;
                var haClose = _fpHeikinAshi.HA_Close;
                
                if (haClose > haOpen) upCandles++;
                else if (haClose < haOpen) downCandles++;
                else dojiCandles++;
            }
        }
        
        return (upCandles, downCandles, dojiCandles);
    }

    [Benchmark]
    [BenchmarkCategory("HeikinAshi", "NoiseReduction")]
    public (decimal original_volatility, decimal ha_volatility, decimal noise_reduction) HeikinAshi_MeasureNoiseReduction()
    {
        _fpHeikinAshi.Clear();
        var output = new decimal[4];
        
        var originalRanges = new List<decimal>();
        var haRanges = new List<decimal>();
        
        for (int i = 0; i < OHLCData.Length; i++)
        {
            var originalBar = OHLCData[i];
            _fpHeikinAshi.OnBarBatch(new[] { originalBar }, output);
            
            if (_fpHeikinAshi.IsReady)
            {
                // Calculate original range
                var originalRange = originalBar.High - originalBar.Low;
                originalRanges.Add(originalRange);
                
                // Calculate Heikin-Ashi range
                var haRange = _fpHeikinAshi.HA_High - _fpHeikinAshi.HA_Low;
                haRanges.Add(haRange);
            }
        }
        
        var originalVolatility = originalRanges.Count > 0 ? originalRanges.Average() : 0;
        var haVolatility = haRanges.Count > 0 ? haRanges.Average() : 0;
        var noiseReduction = originalVolatility > 0 ? (originalVolatility - haVolatility) / originalVolatility : 0;
        
        return (originalVolatility, haVolatility, noiseReduction);
    }

    [Benchmark]
    [BenchmarkCategory("HeikinAshi", "TrendIdentification")]
    public (int trend_changes, decimal avg_trend_length, int strong_trends) HeikinAshi_IdentifyTrends()
    {
        _fpHeikinAshi.Clear();
        var output = new decimal[4];
        
        int trendChanges = 0;
        var trendLengths = new List<int>();
        int currentTrendLength = 0;
        bool wasUpTrend = false;
        int strongTrends = 0;
        
        for (int i = 0; i < OHLCData.Length; i++)
        {
            _fpHeikinAshi.OnBarBatch(new[] { OHLCData[i] }, output);
            
            if (_fpHeikinAshi.IsReady)
            {
                var haOpen = _fpHeikinAshi.HA_Open;
                var haClose = _fpHeikinAshi.HA_Close;
                bool isUpTrend = haClose > haOpen;
                
                if (i > 0 && isUpTrend != wasUpTrend)
                {
                    if (currentTrendLength > 0)
                    {
                        trendLengths.Add(currentTrendLength);
                        
                        // Strong trend: 5+ consecutive candles in same direction
                        if (currentTrendLength >= 5) strongTrends++;
                        
                        trendChanges++;
                    }
                    currentTrendLength = 1;
                }
                else
                {
                    currentTrendLength++;
                }
                
                wasUpTrend = isUpTrend;
            }
        }
        
        decimal avgTrendLength = trendLengths.Count > 0 ? trendLengths.Average() : 0;
        return (trendChanges, avgTrendLength, strongTrends);
    }

    [Benchmark]
    [BenchmarkCategory("HeikinAshi", "ShadowAnalysis")]
    public (decimal avg_upper_shadow, decimal avg_lower_shadow, int hammer_patterns, int shooting_star_patterns) HeikinAshi_AnalyzeShadows()
    {
        _fpHeikinAshi.Clear();
        var output = new decimal[4];
        
        var upperShadows = new List<decimal>();
        var lowerShadows = new List<decimal>();
        int hammerPatterns = 0;
        int shootingStarPatterns = 0;
        
        for (int i = 0; i < OHLCData.Length; i++)
        {
            _fpHeikinAshi.OnBarBatch(new[] { OHLCData[i] }, output);
            
            if (_fpHeikinAshi.IsReady)
            {
                var haOpen = _fpHeikinAshi.HA_Open;
                var haHigh = _fpHeikinAshi.HA_High;
                var haLow = _fpHeikinAshi.HA_Low;
                var haClose = _fpHeikinAshi.HA_Close;
                
                var bodyTop = Math.Max(haOpen, haClose);
                var bodyBottom = Math.Min(haOpen, haClose);
                var bodySize = bodyTop - bodyBottom;
                
                var upperShadow = haHigh - bodyTop;
                var lowerShadow = bodyBottom - haLow;
                
                upperShadows.Add(upperShadow);
                lowerShadows.Add(lowerShadow);
                
                // Hammer pattern: long lower shadow, short upper shadow
                if (lowerShadow > bodySize * 2 && upperShadow < bodySize * 0.5m)
                    hammerPatterns++;
                
                // Shooting star pattern: long upper shadow, short lower shadow
                if (upperShadow > bodySize * 2 && lowerShadow < bodySize * 0.5m)
                    shootingStarPatterns++;
            }
        }
        
        decimal avgUpperShadow = upperShadows.Count > 0 ? upperShadows.Average() : 0;
        decimal avgLowerShadow = lowerShadows.Count > 0 ? lowerShadows.Average() : 0;
        
        return (avgUpperShadow, avgLowerShadow, hammerPatterns, shootingStarPatterns);
    }

    [Benchmark]
    [BenchmarkCategory("HeikinAshi", "TrendContinuation")]
    public (int continuation_signals, int reversal_signals, decimal signal_accuracy) HeikinAshi_TrendContinuationSignals()
    {
        _fpHeikinAshi.Clear();
        var output = new decimal[4];
        
        int continuationSignals = 0;
        int reversalSignals = 0;
        int correctSignals = 0;
        
        for (int i = 5; i < OHLCData.Length - 5; i++)
        {
            _fpHeikinAshi.OnBarBatch(new[] { OHLCData[i] }, output);
            
            if (_fpHeikinAshi.IsReady)
            {
                var haOpen = _fpHeikinAshi.HA_Open;
                var haClose = _fpHeikinAshi.HA_Close;
                var haHigh = _fpHeikinAshi.HA_High;
                var haLow = _fpHeikinAshi.HA_Low;
                
                // Continuation signal: strong body with minimal shadows
                bool strongUp = haClose > haOpen && (haClose - haOpen) > (haHigh - haLow) * 0.7m;
                bool strongDown = haOpen > haClose && (haOpen - haClose) > (haHigh - haLow) * 0.7m;
                
                if (strongUp || strongDown)
                {
                    continuationSignals++;
                    
                    // Check if trend continued for next 3 bars
                    bool trendContinued = true;
                    for (int j = i + 1; j <= i + 3; j++)
                    {
                        var futureOriginal = OHLCData[j];
                        if (strongUp && futureOriginal.Close <= OHLCData[i].Close)
                        {
                            trendContinued = false;
                            break;
                        }
                        if (strongDown && futureOriginal.Close >= OHLCData[i].Close)
                        {
                            trendContinued = false;
                            break;
                        }
                    }
                    
                    if (trendContinued) correctSignals++;
                }
                
                // Reversal signal: small body with long shadows
                var bodySize = Math.Abs(haClose - haOpen);
                var totalRange = haHigh - haLow;
                
                if (bodySize < totalRange * 0.3m && totalRange > 0)
                {
                    reversalSignals++;
                }
            }
        }
        
        decimal signalAccuracy = continuationSignals > 0 ? (decimal)correctSignals / continuationSignals : 0;
        return (continuationSignals, reversalSignals, signalAccuracy);
    }

    [Benchmark]
    [BenchmarkCategory("HeikinAshi", "GapAnalysis")]
    public (int gaps_reduced, int gaps_created, decimal gap_reduction_ratio) HeikinAshi_AnalyzeGaps()
    {
        _fpHeikinAshi.Clear();
        var output = new decimal[4];
        
        int originalGaps = 0;
        int haGaps = 0;
        int gapsReduced = 0;
        int gapsCreated = 0;
        
        decimal? previousOriginalClose = null;
        decimal? previousHAClose = null;
        
        for (int i = 0; i < OHLCData.Length; i++)
        {
            var originalBar = OHLCData[i];
            _fpHeikinAshi.OnBarBatch(new[] { originalBar }, output);
            
            if (_fpHeikinAshi.IsReady && previousOriginalClose.HasValue && previousHAClose.HasValue)
            {
                // Check for gaps in original data
                bool originalGap = (originalBar.Open > previousOriginalClose.Value * 1.005m) || 
                                 (originalBar.Open < previousOriginalClose.Value * 0.995m);
                
                // Check for gaps in Heikin-Ashi data
                bool haGap = (_fpHeikinAshi.HA_Open > previousHAClose.Value * 1.005m) || 
                           (_fpHeikinAshi.HA_Open < previousHAClose.Value * 0.995m);
                
                if (originalGap) originalGaps++;
                if (haGap) haGaps++;
                
                // Track gap reductions and creations
                if (originalGap && !haGap) gapsReduced++;
                if (!originalGap && haGap) gapsCreated++;
            }
            
            if (_fpHeikinAshi.IsReady)
            {
                previousOriginalClose = originalBar.Close;
                previousHAClose = _fpHeikinAshi.HA_Close;
            }
        }
        
        decimal gapReductionRatio = originalGaps > 0 ? (decimal)gapsReduced / originalGaps : 0;
        return (gapsReduced, gapsCreated, gapReductionRatio);
    }

    [Benchmark]
    [BenchmarkCategory("HeikinAshi", "PriceSmoothing")]
    public (decimal original_price_variance, decimal ha_price_variance, decimal smoothing_factor) HeikinAshi_MeasurePriceSmoothing()
    {
        _fpHeikinAshi.Clear();
        var output = new decimal[4];
        
        var originalCloses = new List<decimal>();
        var haCloses = new List<decimal>();
        
        for (int i = 0; i < OHLCData.Length; i++)
        {
            var originalBar = OHLCData[i];
            _fpHeikinAshi.OnBarBatch(new[] { originalBar }, output);
            
            originalCloses.Add(originalBar.Close);
            
            if (_fpHeikinAshi.IsReady)
            {
                haCloses.Add(_fpHeikinAshi.HA_Close);
            }
        }
        
        // Calculate variances
        decimal originalMean = originalCloses.Average();
        decimal originalVariance = originalCloses.Sum(x => (x - originalMean) * (x - originalMean)) / originalCloses.Count;
        
        decimal haMean = haCloses.Count > 0 ? haCloses.Average() : 0;
        decimal haVariance = haCloses.Count > 0 ? haCloses.Sum(x => (x - haMean) * (x - haMean)) / haCloses.Count : 0;
        
        decimal smoothingFactor = originalVariance > 0 ? 1 - (haVariance / originalVariance) : 0;
        
        return (originalVariance, haVariance, smoothingFactor);
    }
    
    [Benchmark]
    [BenchmarkCategory("HeikinAshi", "Memory")]
    public long HeikinAshi_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new HeikinAshi_FP<OHLCData, decimal>(_parameters);
            var output = new decimal[DataSize * 4]; // OHLC output
            indicator.OnBarBatch(OHLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _fpHeikinAshi?.Clear();
        base.GlobalCleanup();
    }
    
    public class HeikinAshiResult
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
    }
}