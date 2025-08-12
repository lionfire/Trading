using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

/// <summary>
/// Benchmark suite for pattern recognition algorithms and candlestick pattern detection
/// Tests performance of various pattern recognition techniques using technical indicators
/// </summary>
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class PatternRecognitionBenchmark : IndicatorBenchmarkBase
{
    private ZigZag_FP<decimal, decimal> _zigzag = null!;
    private RSI_FP<decimal, decimal> _rsi = null!;
    private MACD_FP<decimal, decimal> _macd = null!;
    private BollingerBands_QC<decimal, decimal> _bollingerBands = null!;
    private HeikinAshi_FP<OHLCData, decimal> _heikinAshi = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _zigzag = new ZigZag_FP<decimal, decimal>(new PZigZag<HLC<decimal>, decimal> 
        { 
            DeviationPercent = 5.0m,
            Depth = 12,
            Backstep = 3,
            MaxPivotHistory = 100
        });
        
        _rsi = new RSI_FP<decimal, decimal>(new PRSI<decimal, decimal> { Period = 14 });
        _macd = new MACD_FP<decimal, decimal>(new PMACD<decimal, decimal> { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 });
        _bollingerBands = new BollingerBands_QC<decimal, decimal>(new PBollingerBands<decimal, decimal> { Period = 20, StandardDeviations = 2.0m });
        _heikinAshi = new HeikinAshi_FP<OHLCData, decimal>(new PHeikinAshi<OHLCData, decimal>());
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("PatternRecognition", "CandlestickPatterns")]
    public (int hammer_patterns, int shooting_star_patterns, int doji_patterns, int engulfing_patterns) PatternRecognition_CandlestickPatterns()
    {
        int hammerPatterns = 0;
        int shootingStarPatterns = 0;
        int dojiPatterns = 0;
        int engulfingPatterns = 0;
        
        for (int i = 1; i < OHLCData.Length; i++)
        {
            var candle = OHLCData[i];
            var prevCandle = OHLCData[i - 1];
            
            decimal bodySize = Math.Abs(candle.Close - candle.Open);
            decimal totalRange = candle.High - candle.Low;
            decimal upperShadow = candle.High - Math.Max(candle.Open, candle.Close);
            decimal lowerShadow = Math.Min(candle.Open, candle.Close) - candle.Low;
            
            // Hammer pattern: long lower shadow, small body, small upper shadow
            if (lowerShadow > bodySize * 2 && upperShadow < bodySize * 0.5m && totalRange > 0)
            {
                hammerPatterns++;
            }
            
            // Shooting star pattern: long upper shadow, small body, small lower shadow
            if (upperShadow > bodySize * 2 && lowerShadow < bodySize * 0.5m && totalRange > 0)
            {
                shootingStarPatterns++;
            }
            
            // Doji pattern: very small body relative to range
            if (totalRange > 0 && bodySize < totalRange * 0.1m)
            {
                dojiPatterns++;
            }
            
            // Engulfing pattern: current candle completely engulfs previous candle
            bool bullishEngulfing = candle.Close > candle.Open && // Bullish candle
                                   prevCandle.Close < prevCandle.Open && // Previous bearish
                                   candle.Open < prevCandle.Close && // Opens below previous close
                                   candle.Close > prevCandle.Open; // Closes above previous open
            
            bool bearishEngulfing = candle.Close < candle.Open && // Bearish candle
                                   prevCandle.Close > prevCandle.Open && // Previous bullish
                                   candle.Open > prevCandle.Close && // Opens above previous close
                                   candle.Close < prevCandle.Open; // Closes below previous open
            
            if (bullishEngulfing || bearishEngulfing)
            {
                engulfingPatterns++;
            }
        }
        
        return (hammerPatterns, shootingStarPatterns, dojiPatterns, engulfingPatterns);
    }

    [Benchmark]
    [BenchmarkCategory("PatternRecognition", "HeikinAshiPatterns")]
    public (int smooth_trends, int choppy_periods, int reversal_signals) PatternRecognition_HeikinAshiPatterns()
    {
        _heikinAshi.Clear();
        var output = new decimal[4]; // OHLC output
        
        int smoothTrends = 0;
        int choppyPeriods = 0;
        int reversalSignals = 0;
        
        var trendLength = 0;
        bool previousBullish = false;
        bool isFirstBar = true;
        
        for (int i = 0; i < OHLCData.Length; i++)
        {
            _heikinAshi.OnBarBatch(new[] { OHLCData[i] }, output);
            
            if (_heikinAshi.IsReady)
            {
                var haOpen = _heikinAshi.HA_Open;
                var haHigh = _heikinAshi.HA_High;
                var haLow = _heikinAshi.HA_Low;
                var haClose = _heikinAshi.HA_Close;
                
                bool currentBullish = haClose > haOpen;
                decimal bodySize = Math.Abs(haClose - haOpen);
                decimal totalRange = haHigh - haLow;
                
                if (!isFirstBar)
                {
                    // Trend continuation
                    if (currentBullish == previousBullish)
                    {
                        trendLength++;
                    }
                    else
                    {
                        // Trend change
                        if (trendLength >= 3) // Smooth trend (3+ consecutive bars)
                            smoothTrends++;
                        
                        reversalSignals++;
                        trendLength = 1;
                    }
                    
                    // Choppy market: small body with large shadows
                    if (totalRange > 0 && bodySize < totalRange * 0.3m)
                    {
                        choppyPeriods++;
                    }
                }
                
                previousBullish = currentBullish;
                isFirstBar = false;
            }
        }
        
        return (smoothTrends, choppyPeriods, reversalSignals);
    }

    [Benchmark]
    [BenchmarkCategory("PatternRecognition", "DivergencePatterns")]
    public (int bullish_divergences, int bearish_divergences, int hidden_divergences) PatternRecognition_DivergencePatterns()
    {
        _rsi.Clear();
        _macd.Clear();
        
        var rsiOutput = new decimal[1];
        var macdOutput = new MACDOutput<decimal>[1];
        
        int bullishDivergences = 0;
        int bearishDivergences = 0;
        int hiddenDivergences = 0;
        
        var priceHistory = new List<decimal>();
        var rsiHistory = new List<decimal>();
        var macdHistory = new List<decimal>();
        
        for (int i = 0; i < CloseData.Length; i++)
        {
            _rsi.OnBarBatch(new[] { CloseData[i] }, rsiOutput);
            _macd.OnBarBatch(new[] { CloseData[i] }, macdOutput);
            
            if (_rsi.IsReady && _macd.IsReady)
            {
                priceHistory.Add(CloseData[i]);
                rsiHistory.Add(rsiOutput[0]);
                macdHistory.Add(macdOutput[0].MACD);
                
                // Look for divergences over last 20 periods
                if (priceHistory.Count >= 20)
                {
                    int lookback = 20;
                    var recentPrices = priceHistory.TakeLast(lookback).ToList();
                    var recentRsi = rsiHistory.TakeLast(lookback).ToList();
                    var recentMacd = macdHistory.TakeLast(lookback).ToList();
                    
                    // Find local highs and lows
                    int priceHighIndex = FindHighestIndex(recentPrices);
                    int priceLowIndex = FindLowestIndex(recentPrices);
                    int rsiHighIndex = FindHighestIndex(recentRsi);
                    int rsiLowIndex = FindLowestIndex(recentRsi);
                    
                    // Regular bullish divergence: price makes lower low, RSI makes higher low
                    if (priceLowIndex > 10 && rsiLowIndex > 10 && 
                        priceLowIndex != rsiLowIndex && 
                        recentPrices[priceLowIndex] < recentPrices.Take(priceLowIndex).Min() &&
                        recentRsi[rsiLowIndex] > recentRsi.Take(rsiLowIndex).Min())
                    {
                        bullishDivergences++;
                    }
                    
                    // Regular bearish divergence: price makes higher high, RSI makes lower high
                    if (priceHighIndex > 10 && rsiHighIndex > 10 && 
                        priceHighIndex != rsiHighIndex &&
                        recentPrices[priceHighIndex] > recentPrices.Take(priceHighIndex).Max() &&
                        recentRsi[rsiHighIndex] < recentRsi.Take(rsiHighIndex).Max())
                    {
                        bearishDivergences++;
                    }
                    
                    // Hidden divergence: price makes higher low, MACD makes lower low (continuation signal)
                    if (priceLowIndex > 10 && 
                        recentPrices[priceLowIndex] > recentPrices.Take(priceLowIndex).Min() &&
                        recentMacd[priceLowIndex] < recentMacd.Take(priceLowIndex).Min())
                    {
                        hiddenDivergences++;
                    }
                }
                
                // Keep only recent history to manage memory
                if (priceHistory.Count > 50)
                {
                    priceHistory.RemoveAt(0);
                    rsiHistory.RemoveAt(0);
                    macdHistory.RemoveAt(0);
                }
            }
        }
        
        return (bullishDivergences, bearishDivergences, hiddenDivergences);
    }

    [Benchmark]
    [BenchmarkCategory("PatternRecognition", "ChartPatterns")]
    public (int head_shoulders, int double_tops, int double_bottoms, int triangles) PatternRecognition_ChartPatterns()
    {
        _zigzag.Clear();
        var output = new decimal[1];
        
        // Run ZigZag to identify pivot points
        for (int i = 0; i < HLCData.Length; i++)
        {
            _zigzag.OnBarBatch(new[] { HLCData[i] }, output);
        }
        
        int headShoulders = 0;
        int doubleTops = 0;
        int doubleBottoms = 0;
        int triangles = 0;
        
        if (_zigzag.RecentPivots != null && _zigzag.RecentPivots.Count >= 5)
        {
            var pivots = _zigzag.RecentPivots.ToList();
            
            // Look for patterns using pivot points
            for (int i = 4; i < pivots.Count; i++)
            {
                var p1 = pivots[i - 4];
                var p2 = pivots[i - 3];
                var p3 = pivots[i - 2];
                var p4 = pivots[i - 1];
                var p5 = pivots[i];
                
                // Head and Shoulders: Low-High-Low-Higher High-Low (or inverse)
                if (!p1.IsHigh && p2.IsHigh && !p3.IsHigh && p4.IsHigh && !p5.IsHigh)
                {
                    if (p4.Price > p2.Price && // Head higher than shoulders
                        Math.Abs(p1.Price - p5.Price) < (p4.Price - p3.Price) * 0.1m) // Neckline level
                    {
                        headShoulders++;
                    }
                }
                
                // Double Top: Low-High-Low-High-Low with similar highs
                if (!p1.IsHigh && p2.IsHigh && !p3.IsHigh && p4.IsHigh && !p5.IsHigh)
                {
                    decimal priceDifference = Math.Abs(p2.Price - p4.Price);
                    decimal avgPrice = (p2.Price + p4.Price) / 2;
                    
                    if (priceDifference < avgPrice * 0.02m) // Within 2%
                    {
                        doubleTops++;
                    }
                }
                
                // Double Bottom: High-Low-High-Low-High with similar lows
                if (p1.IsHigh && !p2.IsHigh && p3.IsHigh && !p4.IsHigh && p5.IsHigh)
                {
                    decimal priceDifference = Math.Abs(p2.Price - p4.Price);
                    decimal avgPrice = (p2.Price + p4.Price) / 2;
                    
                    if (priceDifference < avgPrice * 0.02m) // Within 2%
                    {
                        doubleBottoms++;
                    }
                }
                
                // Triangle: converging trend lines (simplified detection)
                if (i >= 6)
                {
                    var highs = pivots.Skip(i - 6).Take(7).Where(p => p.IsHigh).ToList();
                    var lows = pivots.Skip(i - 6).Take(7).Where(p => !p.IsHigh).ToList();
                    
                    if (highs.Count >= 3 && lows.Count >= 3)
                    {
                        // Check if highs are declining and lows are rising (converging)
                        bool decliningHighs = highs.Count >= 2 && highs.Last().Price < highs.First().Price;
                        bool risingLows = lows.Count >= 2 && lows.Last().Price > lows.First().Price;
                        
                        if (decliningHighs && risingLows)
                        {
                            triangles++;
                        }
                    }
                }
            }
        }
        
        return (headShoulders, doubleTops, doubleBottoms, triangles);
    }

    [Benchmark]
    [BenchmarkCategory("PatternRecognition", "BollingerBandPatterns")]
    public (int squeeze_patterns, int band_walks, int reversal_patterns) PatternRecognition_BollingerBandPatterns()
    {
        _bollingerBands.Clear();
        var output = new BollingerBandsOutput<decimal>[1];
        
        int squeezePatterns = 0;
        int bandWalks = 0;
        int reversalPatterns = 0;
        
        var bandWidthHistory = new List<decimal>();
        int consecutiveUpperTouches = 0;
        int consecutiveLowerTouches = 0;
        
        for (int i = 0; i < CloseData.Length; i++)
        {
            _bollingerBands.OnBarBatch(new[] { CloseData[i] }, output);
            
            if (_bollingerBands.IsReady)
            {
                var bands = output[0];
                decimal bandWidth = bands.UpperBand - bands.LowerBand;
                decimal currentPrice = CloseData[i];
                
                bandWidthHistory.Add(bandWidth);
                
                // Bollinger Band Squeeze: band width contracting
                if (bandWidthHistory.Count >= 20)
                {
                    var recentBandWidths = bandWidthHistory.TakeLast(20).ToList();
                    var earlierBandWidths = bandWidthHistory.Skip(Math.Max(0, bandWidthHistory.Count - 40)).Take(20).ToList();
                    
                    if (recentBandWidths.Count == 20 && earlierBandWidths.Count == 20)
                    {
                        decimal currentAvgWidth = recentBandWidths.Average();
                        decimal previousAvgWidth = earlierBandWidths.Average();
                        
                        if (currentAvgWidth < previousAvgWidth * 0.8m) // 20% reduction
                        {
                            squeezePatterns++;
                        }
                    }
                }
                
                // Band walking: consecutive touches of upper or lower band
                decimal tolerance = bandWidth * 0.05m; // 5% of band width
                
                if (currentPrice >= bands.UpperBand - tolerance)
                {
                    consecutiveUpperTouches++;
                    consecutiveLowerTouches = 0;
                }
                else if (currentPrice <= bands.LowerBand + tolerance)
                {
                    consecutiveLowerTouches++;
                    consecutiveUpperTouches = 0;
                }
                else
                {
                    // Check if we had a band walk (3+ consecutive touches)
                    if (consecutiveUpperTouches >= 3 || consecutiveLowerTouches >= 3)
                    {
                        bandWalks++;
                    }
                    
                    consecutiveUpperTouches = 0;
                    consecutiveLowerTouches = 0;
                }
                
                // Reversal pattern: price touches band and immediately reverses
                if (i > 2)
                {
                    decimal prevPrice = CloseData[i - 1];
                    decimal prev2Price = CloseData[i - 2];
                    
                    // Upper band reversal
                    bool upperReversal = prevPrice >= bands.UpperBand - tolerance && // Touched upper band
                                        currentPrice < prevPrice && // Price declining
                                        prev2Price < prevPrice; // Previous was rising
                    
                    // Lower band reversal
                    bool lowerReversal = prevPrice <= bands.LowerBand + tolerance && // Touched lower band
                                        currentPrice > prevPrice && // Price rising
                                        prev2Price > prevPrice; // Previous was declining
                    
                    if (upperReversal || lowerReversal)
                    {
                        reversalPatterns++;
                    }
                }
                
                // Keep band width history manageable
                if (bandWidthHistory.Count > 100)
                {
                    bandWidthHistory.RemoveAt(0);
                }
            }
        }
        
        return (squeezePatterns, bandWalks, reversalPatterns);
    }

    [Benchmark]
    [BenchmarkCategory("PatternRecognition", "VolumePatterns")]
    public (int climax_volumes, int dry_up_volumes, int accumulation_patterns) PatternRecognition_VolumePatterns()
    {
        int climaxVolumes = 0;
        int dryUpVolumes = 0;
        int accumulationPatterns = 0;
        
        // Calculate volume statistics for pattern recognition
        var volumeHistory = new List<decimal>();
        var priceHistory = new List<decimal>();
        
        for (int i = 0; i < OHLCData.Length; i++)
        {
            var bar = OHLCData[i];
            volumeHistory.Add(bar.Volume);
            priceHistory.Add(bar.Close);
            
            if (volumeHistory.Count >= 20)
            {
                var recentVolumes = volumeHistory.TakeLast(20).ToList();
                var recentPrices = priceHistory.TakeLast(20).ToList();
                
                decimal avgVolume = recentVolumes.Average();
                decimal currentVolume = bar.Volume;
                
                // Volume climax: extremely high volume with price reversal
                if (currentVolume > avgVolume * 3 && i > 0) // 3x average volume
                {
                    decimal priceChange = Math.Abs(bar.Close - priceHistory[i - 1]) / priceHistory[i - 1];
                    if (priceChange > 0.02m) // Significant price movement
                    {
                        climaxVolumes++;
                    }
                }
                
                // Volume dry up: very low volume during consolidation
                if (currentVolume < avgVolume * 0.3m) // 30% of average volume
                {
                    // Check if price is consolidating (small range)
                    var recentRange = recentPrices.Max() - recentPrices.Min();
                    var avgPrice = recentPrices.Average();
                    
                    if (recentRange / avgPrice < 0.05m) // Less than 5% range
                    {
                        dryUpVolumes++;
                    }
                }
                
                // Accumulation pattern: increasing volume with sideways price
                if (recentVolumes.Count >= 10)
                {
                    var firstHalf = recentVolumes.Take(10).Average();
                    var secondHalf = recentVolumes.Skip(10).Average();
                    
                    // Volume increasing
                    if (secondHalf > firstHalf * 1.2m)
                    {
                        // Price relatively stable
                        var firstHalfPrices = recentPrices.Take(10).ToList();
                        var secondHalfPrices = recentPrices.Skip(10).ToList();
                        
                        decimal priceChangePercent = Math.Abs(secondHalfPrices.Average() - firstHalfPrices.Average()) / firstHalfPrices.Average();
                        
                        if (priceChangePercent < 0.03m) // Less than 3% price change
                        {
                            accumulationPatterns++;
                        }
                    }
                }
                
                // Keep history manageable
                if (volumeHistory.Count > 50)
                {
                    volumeHistory.RemoveAt(0);
                    priceHistory.RemoveAt(0);
                }
            }
        }
        
        return (climaxVolumes, dryUpVolumes, accumulationPatterns);
    }

    [Benchmark]
    [BenchmarkCategory("PatternRecognition", "Memory")]
    public long PatternRecognition_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            // Simulate pattern recognition memory usage
            var rsi = new RSI_FP<decimal, decimal>(new PRSI<decimal, decimal> { Period = 14 });
            var zigzag = new ZigZag_FP<decimal, decimal>(new PZigZag<HLC<decimal>, decimal> 
            { 
                DeviationPercent = 5.0m, 
                Depth = 12, 
                Backstep = 3, 
                MaxPivotHistory = 100 
            });
            
            var rsiOutput = new decimal[DataSize];
            var zigzagOutput = new decimal[DataSize];
            var patterns = new List<string>();
            
            rsi.OnBarBatch(CloseData, rsiOutput);
            zigzag.OnBarBatch(HLCData, zigzagOutput);
            
            // Pattern detection simulation
            for (int i = 0; i < DataSize; i++)
            {
                if (rsiOutput[i] > 70) patterns.Add("Overbought");
                if (rsiOutput[i] < 30) patterns.Add("Oversold");
                if (zigzagOutput[i] != default) patterns.Add("PivotPoint");
            }
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _zigzag?.Clear();
        _rsi?.Clear();
        _macd?.Clear();
        _bollingerBands?.Clear();
        _heikinAshi?.Clear();
        base.GlobalCleanup();
    }

    private int FindHighestIndex(List<decimal> values)
    {
        if (values.Count == 0) return -1;
        
        decimal maxValue = values.Max();
        return values.IndexOf(maxValue);
    }

    private int FindLowestIndex(List<decimal> values)
    {
        if (values.Count == 0) return -1;
        
        decimal minValue = values.Min();
        return values.IndexOf(minValue);
    }
}