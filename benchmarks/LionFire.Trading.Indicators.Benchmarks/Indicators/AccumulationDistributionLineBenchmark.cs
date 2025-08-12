using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class AccumulationDistributionLineBenchmark : IndicatorBenchmarkBase
{
    private AccumulationDistributionLine_QC<IKline<decimal>, decimal> _qcAdl = null!;
    private PAccumulationDistributionLine<IKline<decimal>, decimal> _parameters = null!;
    
    private IKline<decimal>[] _adlData = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Generate data with volume for ADL
        var generator = new TestDataGenerator();
        var dataPoints = generator.GenerateRealisticData(DataSize);
        
        _adlData = dataPoints.Select(d => new TestKline
        {
            Open = d.Open,
            High = d.High,
            Low = d.Low,
            Close = d.Close,
            Volume = d.Volume,
            Timestamp = d.Timestamp
        }).ToArray();
        
        _parameters = new PAccumulationDistributionLine<IKline<decimal>, decimal>();
        _qcAdl = new AccumulationDistributionLine_QC<IKline<decimal>, decimal>(_parameters);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ADL", "QuantConnect")]
    public decimal[] ADL_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcAdl.Clear();
        _qcAdl.OnBarBatch(_adlData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("ADL", "QuantConnect")]
    public List<decimal?> ADL_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcAdl.Clear();
        
        for (int i = 0; i < _adlData.Length; i++)
        {
            _qcAdl.OnBarBatch(new[] { _adlData[i] }, output);
            results.Add(_qcAdl.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("ADL", "TrendConfirmation")]
    public (int confirmed, int divergent) ADL_TrendConfirmation()
    {
        _qcAdl.Clear();
        var adlOutput = new decimal[DataSize];
        _qcAdl.OnBarBatch(_adlData, adlOutput);
        
        int confirmedTrends = 0;
        int divergentTrends = 0;
        int lookback = 20;
        
        for (int i = lookback; i < DataSize; i++)
        {
            if (adlOutput[i] != default && adlOutput[i - lookback] != default)
            {
                // Calculate price trend
                var priceTrend = _adlData[i].Close - _adlData[i - lookback].Close;
                
                // Calculate ADL trend  
                var adlTrend = adlOutput[i] - adlOutput[i - lookback];
                
                // Check if trends align
                if ((priceTrend > 0 && adlTrend > 0) || (priceTrend < 0 && adlTrend < 0))
                {
                    confirmedTrends++;
                }
                else if ((priceTrend > 0 && adlTrend < 0) || (priceTrend < 0 && adlTrend > 0))
                {
                    divergentTrends++;
                }
            }
        }
        
        return (confirmedTrends, divergentTrends);
    }

    [Benchmark]
    [BenchmarkCategory("ADL", "MoneyFlowPattern")]
    public (int accumulation, int distribution, int neutral) ADL_MoneyFlowPattern()
    {
        _qcAdl.Clear();
        var output = new decimal[1];
        decimal? previousAdl = null;
        int accumulationPeriods = 0;
        int distributionPeriods = 0;
        int neutralPeriods = 0;
        
        for (int i = 0; i < _adlData.Length; i++)
        {
            _qcAdl.OnBarBatch(new[] { _adlData[i] }, output);
            
            if (_qcAdl.IsReady && previousAdl.HasValue)
            {
                var adlChange = output[0] - previousAdl.Value;
                var volumeWeight = _adlData[i].Volume;
                
                // Weighted by volume to identify significant flows
                var weightedChange = adlChange * volumeWeight / 1000000m; // Normalize volume
                
                if (weightedChange > 1m) accumulationPeriods++;
                else if (weightedChange < -1m) distributionPeriods++;
                else neutralPeriods++;
            }
            
            previousAdl = _qcAdl.IsReady ? output[0] : (decimal?)null;
        }
        
        return (accumulationPeriods, distributionPeriods, neutralPeriods);
    }

    [Benchmark]
    [BenchmarkCategory("ADL", "VolumeCorrelation")]
    public decimal ADL_VolumeCorrelation()
    {
        _qcAdl.Clear();
        var adlOutput = new decimal[DataSize];
        _qcAdl.OnBarBatch(_adlData, adlOutput);
        
        // Calculate correlation between ADL changes and volume
        var adlChanges = new List<decimal>();
        var volumes = new List<decimal>();
        
        for (int i = 1; i < DataSize; i++)
        {
            if (adlOutput[i] != default && adlOutput[i - 1] != default)
            {
                adlChanges.Add(Math.Abs(adlOutput[i] - adlOutput[i - 1]));
                volumes.Add(_adlData[i].Volume);
            }
        }
        
        if (adlChanges.Count < 2) return 0;
        
        // Simple correlation calculation
        decimal adlMean = adlChanges.Average();
        decimal volumeMean = volumes.Average();
        
        decimal covariance = 0;
        decimal adlVariance = 0;
        decimal volumeVariance = 0;
        
        for (int i = 0; i < adlChanges.Count; i++)
        {
            var adlDiff = adlChanges[i] - adlMean;
            var volDiff = volumes[i] - volumeMean;
            
            covariance += adlDiff * volDiff;
            adlVariance += adlDiff * adlDiff;
            volumeVariance += volDiff * volDiff;
        }
        
        if (adlVariance == 0 || volumeVariance == 0) return 0;
        
        return covariance / (decimal)Math.Sqrt((double)(adlVariance * volumeVariance));
    }

    [Benchmark]
    [BenchmarkCategory("ADL", "BreakoutPrediction")]
    public int ADL_PredictBreakouts()
    {
        _qcAdl.Clear();
        var adlOutput = new decimal[DataSize];
        _qcAdl.OnBarBatch(_adlData, adlOutput);
        
        int successfulPredictions = 0;
        int lookforward = 5; // Look 5 periods ahead for breakout
        
        for (int i = 50; i < DataSize - lookforward; i++)
        {
            if (adlOutput[i] != default)
            {
                // Check if ADL is making new highs while price consolidates
                bool adlNewHigh = true;
                for (int j = i - 20; j < i; j++)
                {
                    if (j >= 0 && adlOutput[j] != default && adlOutput[j] >= adlOutput[i])
                    {
                        adlNewHigh = false;
                        break;
                    }
                }
                
                // Check if price was consolidating
                var priceRange = _adlData.Skip(i - 10).Take(10).Max(k => k.High) - 
                               _adlData.Skip(i - 10).Take(10).Min(k => k.Low);
                var avgPrice = _adlData.Skip(i - 10).Take(10).Average(k => k.Close);
                bool priceConsolidating = priceRange / avgPrice < 0.05m; // Less than 5% range
                
                if (adlNewHigh && priceConsolidating)
                {
                    // Check if price broke out in next 5 periods
                    var futureHigh = _adlData.Skip(i + 1).Take(lookforward).Max(k => k.High);
                    if (futureHigh > _adlData[i].High * 1.02m) // 2% breakout
                    {
                        successfulPredictions++;
                    }
                }
            }
        }
        
        return successfulPredictions;
    }

    [Benchmark]
    [BenchmarkCategory("ADL", "MoneyFlowMultiplier")]
    public decimal ADL_AverageMoneyFlowMultiplier()
    {
        _qcAdl.Clear();
        var output = new decimal[1];
        decimal totalMultiplier = 0;
        int count = 0;
        
        for (int i = 0; i < _adlData.Length; i++)
        {
            _qcAdl.OnBarBatch(new[] { _adlData[i] }, output);
            
            if (_qcAdl.IsReady)
            {
                // Calculate Money Flow Multiplier: ((Close - Low) - (High - Close)) / (High - Low)
                var bar = _adlData[i];
                var range = bar.High - bar.Low;
                
                if (range > 0)
                {
                    var multiplier = ((bar.Close - bar.Low) - (bar.High - bar.Close)) / range;
                    totalMultiplier += Math.Abs(multiplier);
                    count++;
                }
            }
        }
        
        return count > 0 ? totalMultiplier / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("ADL", "TrendReversals")]
    public int ADL_DetectReversals()
    {
        _qcAdl.Clear();
        var adlOutput = new decimal[DataSize];
        _qcAdl.OnBarBatch(_adlData, adlOutput);
        
        int reversals = 0;
        int trendLength = 10;
        
        for (int i = trendLength * 2; i < DataSize; i++)
        {
            if (adlOutput[i] != default)
            {
                // Calculate recent ADL trend
                var recentTrend = adlOutput[i] - adlOutput[i - trendLength];
                var previousTrend = adlOutput[i - trendLength] - adlOutput[i - (trendLength * 2)];
                
                // Detect trend reversal
                if ((recentTrend > 0 && previousTrend < 0) || (recentTrend < 0 && previousTrend > 0))
                {
                    // Confirm with significant change
                    if (Math.Abs(recentTrend) > Math.Abs(previousTrend) * 0.5m)
                    {
                        reversals++;
                    }
                }
            }
        }
        
        return reversals;
    }
    
    [Benchmark]
    [BenchmarkCategory("ADL", "Memory")]
    public long ADL_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new AccumulationDistributionLine_QC<IKline<decimal>, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(_adlData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcAdl?.Clear();
        _adlData = null!;
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