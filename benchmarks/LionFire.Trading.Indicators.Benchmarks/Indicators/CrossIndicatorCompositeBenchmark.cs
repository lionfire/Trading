using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.QuantConnect;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class CrossIndicatorCompositeBenchmark : IndicatorBenchmarkBase
{
    // Trend indicators
    private EMA_FP<decimal, decimal> _emaFast = null!;
    private EMA_FP<decimal, decimal> _emaSlow = null!;
    private MACD_FP<decimal, decimal> _macd = null!;
    private RSI_FP<decimal, decimal> _rsi = null!;
    
    // Volume indicators
    private OBV_FP<IKline<decimal>, decimal> _obv = null!;
    private VWAP_FP<IKline<decimal>, decimal> _vwap = null!;
    
    // Volatility indicators
    private BollingerBands_QC<decimal, decimal> _bollingerBands = null!;
    private AverageTrueRange<decimal, decimal> _atr = null!;
    
    // Support/Resistance indicators
    private DonchianChannels_QC<decimal, decimal> _donchianChannels = null!;
    private PivotPoints_QC<HLCData, decimal> _pivotPoints = null!;
    
    private IKline<decimal>[] _volumeData = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Setup trend indicators
        _emaFast = new EMA_FP<decimal, decimal>(new PEMA<decimal, decimal> { Period = 12 });
        _emaSlow = new EMA_FP<decimal, decimal>(new PEMA<decimal, decimal> { Period = 26 });
        _macd = new MACD_FP<decimal, decimal>(new PMACD<decimal, decimal> { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 });
        _rsi = new RSI_FP<decimal, decimal>(new PRSI<decimal, decimal> { Period = 14 });
        
        // Setup volume indicators
        var generator = new TestDataGenerator();
        var dataPoints = generator.GenerateRealisticData(DataSize);
        
        _volumeData = dataPoints.Select(d => new TestKline
        {
            Open = d.Open,
            High = d.High,
            Low = d.Low,
            Close = d.Close,
            Volume = d.Volume,
            Timestamp = d.Timestamp
        }).ToArray();
        
        _obv = new OBV_FP<IKline<decimal>, decimal>(new POBV<IKline<decimal>, decimal>());
        _vwap = new VWAP_FP<IKline<decimal>, decimal>(new PVWAP<IKline<decimal>, decimal>());
        
        // Setup volatility indicators
        _bollingerBands = new BollingerBands_QC<decimal, decimal>(new PBollingerBands<decimal, decimal> { Period = 20, StandardDeviations = 2.0m });
        _atr = new AverageTrueRange<decimal, decimal>(new PAverageTrueRange<decimal, decimal> { Period = 14 });
        
        // Setup support/resistance indicators
        _donchianChannels = new DonchianChannels_QC<decimal, decimal>(new PDonchianChannels<decimal, decimal> { Period = 20 });
        _pivotPoints = new PivotPoints_QC<HLCData, decimal>(new PPivotPoints<HLCData, decimal>());
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("CrossIndicator", "TrendFollowingStrategy")]
    public (int long_signals, int short_signals, decimal signal_accuracy) CrossIndicator_TrendFollowingStrategy()
    {
        // Clear all indicators
        _emaFast.Clear();
        _emaSlow.Clear();
        _macd.Clear();
        _rsi.Clear();
        
        var emaFastOutput = new decimal[1];
        var emaSlowOutput = new decimal[1];
        var macdOutput = new MACDOutput<decimal>[1];
        var rsiOutput = new decimal[1];
        
        int longSignals = 0;
        int shortSignals = 0;
        int correctSignals = 0;
        int totalSignals = 0;
        
        for (int i = 0; i < CloseData.Length - 5; i++)
        {
            // Update all indicators
            _emaFast.OnBarBatch(new[] { CloseData[i] }, emaFastOutput);
            _emaSlow.OnBarBatch(new[] { CloseData[i] }, emaSlowOutput);
            _macd.OnBarBatch(new[] { CloseData[i] }, macdOutput);
            _rsi.OnBarBatch(new[] { CloseData[i] }, rsiOutput);
            
            // Generate composite signals after warmup period
            if (i > 50 && _emaFast.IsReady && _emaSlow.IsReady && _macd.IsReady && _rsi.IsReady)
            {
                bool emaUptrend = emaFastOutput[0] > emaSlowOutput[0];
                bool macdBullish = macdOutput[0].MACD > macdOutput[0].Signal;
                bool rsiNotOverbought = rsiOutput[0] < 70m;
                bool rsiNotOversold = rsiOutput[0] > 30m;
                
                // Long signal: EMA uptrend + MACD bullish + RSI not overbought
                if (emaUptrend && macdBullish && rsiNotOverbought)
                {
                    longSignals++;
                    totalSignals++;
                    
                    // Check if signal was profitable in next 5 periods
                    if (CloseData[i + 5] > CloseData[i]) correctSignals++;
                }
                
                // Short signal: EMA downtrend + MACD bearish + RSI not oversold
                else if (!emaUptrend && !macdBullish && rsiNotOversold)
                {
                    shortSignals++;
                    totalSignals++;
                    
                    // Check if signal was profitable in next 5 periods
                    if (CloseData[i + 5] < CloseData[i]) correctSignals++;
                }
            }
        }
        
        decimal signalAccuracy = totalSignals > 0 ? (decimal)correctSignals / totalSignals : 0;
        return (longSignals, shortSignals, signalAccuracy);
    }

    [Benchmark]
    [BenchmarkCategory("CrossIndicator", "VolumeConfirmation")]
    public (int volume_confirmed_signals, int volume_rejected_signals, decimal confirmation_rate) CrossIndicator_VolumeConfirmation()
    {
        _emaFast.Clear();
        _emaSlow.Clear();
        _obv.Clear();
        _vwap.Clear();
        
        var emaFastOutput = new decimal[1];
        var emaSlowOutput = new decimal[1];
        var obvOutput = new decimal[1];
        var vwapOutput = new decimal[1];
        
        int volumeConfirmedSignals = 0;
        int volumeRejectedSignals = 0;
        
        for (int i = 0; i < CloseData.Length; i++)
        {
            _emaFast.OnBarBatch(new[] { CloseData[i] }, emaFastOutput);
            _emaSlow.OnBarBatch(new[] { CloseData[i] }, emaSlowOutput);
            _obv.OnBarBatch(new[] { _volumeData[i] }, obvOutput);
            _vwap.OnBarBatch(new[] { _volumeData[i] }, vwapOutput);
            
            if (i > 30 && _emaFast.IsReady && _emaSlow.IsReady && _obv.IsReady && _vwap.IsReady)
            {
                // Price signal: EMA crossover
                bool priceBullish = emaFastOutput[0] > emaSlowOutput[0];
                
                // Volume confirmation: OBV trending up and price above VWAP
                bool volumeConfirmation = i > 0 && obvOutput[0] > 0 && CloseData[i] > vwapOutput[0];
                
                // Check for new signals (crossover)
                if (i > 31)
                {
                    _emaFast.OnBarBatch(new[] { CloseData[i-1] }, new decimal[1]);
                    _emaSlow.OnBarBatch(new[] { CloseData[i-1] }, new decimal[1]);
                    bool previousBullish = emaFastOutput[0] > emaSlowOutput[0];
                    
                    if (priceBullish != previousBullish) // Crossover detected
                    {
                        if (volumeConfirmation)
                            volumeConfirmedSignals++;
                        else
                            volumeRejectedSignals++;
                    }
                }
            }
        }
        
        decimal confirmationRate = (volumeConfirmedSignals + volumeRejectedSignals) > 0 
            ? (decimal)volumeConfirmedSignals / (volumeConfirmedSignals + volumeRejectedSignals) 
            : 0;
        
        return (volumeConfirmedSignals, volumeRejectedSignals, confirmationRate);
    }

    [Benchmark]
    [BenchmarkCategory("CrossIndicator", "VolatilityFiltering")]
    public (int high_volatility_signals, int low_volatility_signals, decimal volatility_edge) CrossIndicator_VolatilityFiltering()
    {
        _macd.Clear();
        _atr.Clear();
        _bollingerBands.Clear();
        
        var macdOutput = new MACDOutput<decimal>[1];
        var atrOutput = new decimal[1];
        var bollingerOutput = new BollingerBandsOutput<decimal>[1];
        
        int highVolatilitySignals = 0;
        int lowVolatilitySignals = 0;
        var highVolatilityReturns = new List<decimal>();
        var lowVolatilityReturns = new List<decimal>();
        
        for (int i = 0; i < CloseData.Length - 3; i++)
        {
            _macd.OnBarBatch(new[] { CloseData[i] }, macdOutput);
            _atr.OnBarBatch(new[] { HLCData[i] }, atrOutput);
            _bollingerBands.OnBarBatch(new[] { CloseData[i] }, bollingerOutput);
            
            if (i > 50 && _macd.IsReady && _atr.IsReady && _bollingerBands.IsReady)
            {
                // MACD signal
                bool macdBullish = macdOutput[0].MACD > macdOutput[0].Signal;
                
                // Volatility measurements
                decimal atrValue = atrOutput[0];
                decimal bollingerWidth = bollingerOutput[0].UpperBand - bollingerOutput[0].LowerBand;
                bool highVolatility = atrValue > 2.0m || bollingerWidth > CloseData[i] * 0.1m;
                
                if (macdBullish) // Only test long signals
                {
                    decimal futureReturn = (CloseData[i + 3] - CloseData[i]) / CloseData[i];
                    
                    if (highVolatility)
                    {
                        highVolatilitySignals++;
                        highVolatilityReturns.Add(futureReturn);
                    }
                    else
                    {
                        lowVolatilitySignals++;
                        lowVolatilityReturns.Add(futureReturn);
                    }
                }
            }
        }
        
        decimal highVolAvgReturn = highVolatilityReturns.Count > 0 ? highVolatilityReturns.Average() : 0;
        decimal lowVolAvgReturn = lowVolatilityReturns.Count > 0 ? lowVolatilityReturns.Average() : 0;
        decimal volatilityEdge = highVolAvgReturn - lowVolAvgReturn;
        
        return (highVolatilitySignals, lowVolatilitySignals, volatilityEdge);
    }

    [Benchmark]
    [BenchmarkCategory("CrossIndicator", "SupportResistanceBreakout")]
    public (int breakout_signals, int false_breakouts, decimal breakout_accuracy) CrossIndicator_SupportResistanceBreakout()
    {
        _donchianChannels.Clear();
        _pivotPoints.Clear();
        _rsi.Clear();
        
        var donchianOutput = new DonchianChannelsOutput<decimal>[1];
        var pivotOutput = new PivotPointsOutput<decimal>[1];
        var rsiOutput = new decimal[1];
        
        int breakoutSignals = 0;
        int falseBreakouts = 0;
        
        for (int i = 0; i < HLCData.Length - 10; i++)
        {
            _donchianChannels.OnBarBatch(new[] { HLCData[i] }, donchianOutput);
            _pivotPoints.OnBarBatch(new[] { HLCData[i] }, pivotOutput);
            _rsi.OnBarBatch(new[] { CloseData[i] }, rsiOutput);
            
            if (i > 50 && _donchianChannels.IsReady && _pivotPoints.IsReady && _rsi.IsReady)
            {
                var currentClose = CloseData[i];
                
                // Donchian breakout
                bool donchianBreakout = currentClose > donchianOutput[0].UpperBand;
                
                // Pivot resistance breakout
                bool pivotBreakout = currentClose > pivotOutput[0].Resistance1;
                
                // RSI confirmation (not overbought)
                bool rsiConfirmation = rsiOutput[0] < 70m;
                
                // Combined breakout signal
                if ((donchianBreakout || pivotBreakout) && rsiConfirmation)
                {
                    breakoutSignals++;
                    
                    // Check if breakout was sustained (price higher after 10 periods)
                    if (CloseData[i + 10] <= currentClose * 1.02m) // Less than 2% gain
                        falseBreakouts++;
                }
            }
        }
        
        decimal breakoutAccuracy = breakoutSignals > 0 ? 1.0m - ((decimal)falseBreakouts / breakoutSignals) : 0;
        return (breakoutSignals, falseBreakouts, breakoutAccuracy);
    }

    [Benchmark]
    [BenchmarkCategory("CrossIndicator", "MultiTimeframeDivergence")]
    public (int bullish_divergences, int bearish_divergences, int confirmed_divergences) CrossIndicator_MultiTimeframeDivergence()
    {
        // Use different period RSI for "multi-timeframe" simulation
        var rsiShort = new RSI_FP<decimal, decimal>(new PRSI<decimal, decimal> { Period = 7 });
        var rsiLong = new RSI_FP<decimal, decimal>(new PRSI<decimal, decimal> { Period = 21 });
        var macdShort = new MACD_FP<decimal, decimal>(new PMACD<decimal, decimal> { FastPeriod = 6, SlowPeriod = 13, SignalPeriod = 5 });
        
        var rsiShortOutput = new decimal[1];
        var rsiLongOutput = new decimal[1];
        var macdShortOutput = new MACDOutput<decimal>[1];
        
        int bullishDivergences = 0;
        int bearishDivergences = 0;
        int confirmedDivergences = 0;
        
        for (int i = 50; i < CloseData.Length - 5; i++)
        {
            rsiShort.OnBarBatch(new[] { CloseData[i] }, rsiShortOutput);
            rsiLong.OnBarBatch(new[] { CloseData[i] }, rsiLongOutput);
            macdShort.OnBarBatch(new[] { CloseData[i] }, macdShortOutput);
            
            if (rsiShort.IsReady && rsiLong.IsReady && macdShort.IsReady && i > 70)
            {
                // Look for divergences over a 20-period lookback
                var priceChange = CloseData[i] - CloseData[i - 20];
                var rsiShortChange = rsiShortOutput[0] - 50m; // Centered around 50
                var rsiLongChange = rsiLongOutput[0] - 50m;   // Centered around 50
                
                // Bullish divergence: price making lower lows, RSI making higher lows
                if (priceChange < 0 && rsiShortChange > 0 && rsiLongChange > 0)
                {
                    bullishDivergences++;
                    
                    // Confirm with MACD
                    if (macdShortOutput[0].MACD > macdShortOutput[0].Signal)
                        confirmedDivergences++;
                }
                
                // Bearish divergence: price making higher highs, RSI making lower highs  
                else if (priceChange > 0 && rsiShortChange < 0 && rsiLongChange < 0)
                {
                    bearishDivergences++;
                    
                    // Confirm with MACD
                    if (macdShortOutput[0].MACD < macdShortOutput[0].Signal)
                        confirmedDivergences++;
                }
            }
        }
        
        return (bullishDivergences, bearishDivergences, confirmedDivergences);
    }

    [Benchmark]
    [BenchmarkCategory("CrossIndicator", "AdaptiveStrategy")]
    public (decimal trending_performance, decimal ranging_performance, decimal adaptive_edge) CrossIndicator_AdaptiveStrategy()
    {
        var adx = new ADX_FP<decimal, decimal>(new PADX<decimal, decimal> { Period = 14 });
        _emaFast.Clear();
        _emaSlow.Clear();
        _rsi.Clear();
        
        var adxOutput = new ADXOutput<decimal>[1];
        var emaFastOutput = new decimal[1];
        var emaSlowOutput = new decimal[1];
        var rsiOutput = new decimal[1];
        
        var trendingReturns = new List<decimal>();
        var rangingReturns = new List<decimal>();
        
        for (int i = 0; i < HLCData.Length - 5; i++)
        {
            adx.OnBarBatch(new[] { HLCData[i] }, adxOutput);
            _emaFast.OnBarBatch(new[] { CloseData[i] }, emaFastOutput);
            _emaSlow.OnBarBatch(new[] { CloseData[i] }, emaSlowOutput);
            _rsi.OnBarBatch(new[] { CloseData[i] }, rsiOutput);
            
            if (i > 50 && adx.IsReady && _emaFast.IsReady && _emaSlow.IsReady && _rsi.IsReady)
            {
                decimal adxValue = adxOutput[0].ADX;
                bool isTrending = adxValue > 25m;
                
                decimal futureReturn = (CloseData[i + 5] - CloseData[i]) / CloseData[i];
                
                if (isTrending)
                {
                    // Trend-following strategy
                    if (emaFastOutput[0] > emaSlowOutput[0]) // Long signal
                        trendingReturns.Add(futureReturn);
                    else // Short signal (inverse return)
                        trendingReturns.Add(-futureReturn);
                }
                else
                {
                    // Mean-reversion strategy
                    if (rsiOutput[0] < 30m) // Oversold, expect bounce
                        rangingReturns.Add(futureReturn);
                    else if (rsiOutput[0] > 70m) // Overbought, expect decline
                        rangingReturns.Add(-futureReturn);
                }
            }
        }
        
        decimal trendingPerformance = trendingReturns.Count > 0 ? trendingReturns.Average() : 0;
        decimal rangingPerformance = rangingReturns.Count > 0 ? rangingReturns.Average() : 0;
        decimal adaptiveEdge = (trendingPerformance + rangingPerformance) / 2; // Combined strategy performance
        
        return (trendingPerformance, rangingPerformance, adaptiveEdge);
    }

    [Benchmark]
    [BenchmarkCategory("CrossIndicator", "Memory")]
    public long CrossIndicator_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            // Simulate running multiple indicators simultaneously
            var ema1 = new EMA_FP<decimal, decimal>(new PEMA<decimal, decimal> { Period = 12 });
            var ema2 = new EMA_FP<decimal, decimal>(new PEMA<decimal, decimal> { Period = 26 });
            var rsi = new RSI_FP<decimal, decimal>(new PRSI<decimal, decimal> { Period = 14 });
            var macd = new MACD_FP<decimal, decimal>(new PMACD<decimal, decimal> { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 });
            
            var ema1Output = new decimal[DataSize];
            var ema2Output = new decimal[DataSize];
            var rsiOutput = new decimal[DataSize];
            var macdOutput = new MACDOutput<decimal>[DataSize];
            
            ema1.OnBarBatch(CloseData, ema1Output);
            ema2.OnBarBatch(CloseData, ema2Output);
            rsi.OnBarBatch(CloseData, rsiOutput);
            macd.OnBarBatch(CloseData, macdOutput);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _emaFast?.Clear();
        _emaSlow?.Clear();
        _macd?.Clear();
        _rsi?.Clear();
        _obv?.Clear();
        _vwap?.Clear();
        _bollingerBands?.Clear();
        _atr?.Clear();
        _donchianChannels?.Clear();
        _pivotPoints?.Clear();
        base.GlobalCleanup();
    }
    
    private class TestKline : IKline<decimal>
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
        public DateTime OpenTime { get; set; }
        public DateTime CloseTime => OpenTime.AddMinutes(1);
        public int TradeCount => 0;
        public decimal QuoteAssetVolume => Volume * Close;
        public decimal TakerBuyBaseAssetVolume => Volume / 2;
        public decimal TakerBuyQuoteAssetVolume => QuoteAssetVolume / 2;
        public DateTime Timestamp => OpenTime;
    }
}

public class MACDOutput<T>
{
    public T MACD { get; set; }
    public T Signal { get; set; }
    public T Histogram { get; set; }
}

public class BollingerBandsOutput<T>
{
    public T UpperBand { get; set; }
    public T MiddleBand { get; set; }
    public T LowerBand { get; set; }
}

public class DonchianChannelsOutput<T>
{
    public T UpperBand { get; set; }
    public T LowerBand { get; set; }
}

public class ADXOutput<T>
{
    public T ADX { get; set; }
    public T PositiveDI { get; set; }
    public T NegativeDI { get; set; }
}