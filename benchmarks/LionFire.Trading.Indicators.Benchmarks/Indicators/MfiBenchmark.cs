using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class MfiBenchmark : IndicatorBenchmarkBase
{
    private MFI_QC<IKline<decimal>, decimal> _qcMfi = null!;
    private PMFI<IKline<decimal>, decimal> _parameters = null!;
    
    private MFI_QC<IKline<decimal>, decimal> _qcMfi14 = null!;
    private MFI_QC<IKline<decimal>, decimal> _qcMfi20 = null!;
    private MFI_QC<IKline<decimal>, decimal> _qcMfi30 = null!;
    
    private IKline<decimal>[] _mfiData = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Generate MFI data with volume
        var generator = new TestDataGenerator();
        var dataPoints = generator.GenerateRealisticData(DataSize);
        
        _mfiData = dataPoints.Select(d => new TestKline
        {
            Open = d.Open,
            High = d.High,
            Low = d.Low,
            Close = d.Close,
            Volume = d.Volume,
            Timestamp = d.Timestamp
        }).ToArray();
        
        _parameters = new PMFI<IKline<decimal>, decimal> { Period = Period };
        _qcMfi = new MFI_QC<IKline<decimal>, decimal>(_parameters);
        
        _qcMfi14 = new MFI_QC<IKline<decimal>, decimal>(new PMFI<IKline<decimal>, decimal> { Period = 14 });
        _qcMfi20 = new MFI_QC<IKline<decimal>, decimal>(new PMFI<IKline<decimal>, decimal> { Period = 20 });
        _qcMfi30 = new MFI_QC<IKline<decimal>, decimal>(new PMFI<IKline<decimal>, decimal> { Period = 30 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("MFI", "QuantConnect")]
    public decimal[] MFI_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcMfi.Clear();
        _qcMfi.OnBarBatch(_mfiData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("MFI", "QuantConnect")]
    public List<decimal?> MFI_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcMfi.Clear();
        
        for (int i = 0; i < _mfiData.Length; i++)
        {
            _qcMfi.OnBarBatch(new[] { _mfiData[i] }, output);
            results.Add(_qcMfi.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("MFI", "OversoldOverbought")]
    public (int oversold, int overbought, int neutral) MFI_DetectLevels()
    {
        _qcMfi14.Clear();
        var output = new decimal[1];
        int oversoldCount = 0;
        int overboughtCount = 0;
        int neutralCount = 0;
        
        for (int i = 0; i < _mfiData.Length; i++)
        {
            _qcMfi14.OnBarBatch(new[] { _mfiData[i] }, output);
            if (_qcMfi14.IsReady)
            {
                if (output[0] < 20m) oversoldCount++;
                else if (output[0] > 80m) overboughtCount++;
                else neutralCount++;
            }
        }
        
        return (oversoldCount, overboughtCount, neutralCount);
    }

    [Benchmark]
    [BenchmarkCategory("MFI", "Divergence")]
    public int MFI_DetectDivergence()
    {
        _qcMfi14.Clear();
        var output = new decimal[DataSize];
        _qcMfi14.OnBarBatch(_mfiData, output);
        
        int divergences = 0;
        int lookback = 20;
        
        for (int i = lookback + 14; i < DataSize; i++)
        {
            if (output[i] == default || output[i - lookback] == default) continue;
            
            // Price trend
            var priceTrend = _mfiData[i].Close - _mfiData[i - lookback].Close;
            
            // MFI trend
            var mfiTrend = output[i] - output[i - lookback];
            
            // Check for divergence
            if ((priceTrend > 0 && mfiTrend < 0) || (priceTrend < 0 && mfiTrend > 0))
            {
                divergences++;
            }
        }
        
        return divergences;
    }

    [Benchmark]
    [BenchmarkCategory("MFI", "MoneyFlowRatio")]
    public (int positive_flow, int negative_flow) MFI_MoneyFlowDirection()
    {
        _qcMfi14.Clear();
        var output = new decimal[1];
        decimal? previousMfi = null;
        int positiveFlow = 0;
        int negativeFlow = 0;
        
        for (int i = 0; i < _mfiData.Length; i++)
        {
            _qcMfi14.OnBarBatch(new[] { _mfiData[i] }, output);
            
            if (_qcMfi14.IsReady && previousMfi.HasValue)
            {
                if (output[0] > previousMfi.Value)
                {
                    positiveFlow++; // Money flowing in
                }
                else if (output[0] < previousMfi.Value)
                {
                    negativeFlow++; // Money flowing out
                }
            }
            
            previousMfi = _qcMfi14.IsReady ? output[0] : (decimal?)null;
        }
        
        return (positiveFlow, negativeFlow);
    }

    [Benchmark]
    [BenchmarkCategory("MFI", "PeriodComparison")]
    public (decimal short_avg, decimal medium_avg, decimal long_avg) MFI_ComparePeriods()
    {
        _qcMfi14.Clear();
        _qcMfi20.Clear();
        _qcMfi30.Clear();
        
        var output14 = new decimal[DataSize];
        var output20 = new decimal[DataSize];
        var output30 = new decimal[DataSize];
        
        _qcMfi14.OnBarBatch(_mfiData, output14);
        _qcMfi20.OnBarBatch(_mfiData, output20);
        _qcMfi30.OnBarBatch(_mfiData, output30);
        
        // Calculate average MFI for each period
        decimal avg14 = output14.Where(v => v != default).DefaultIfEmpty(0).Average();
        decimal avg20 = output20.Where(v => v != default).DefaultIfEmpty(0).Average();
        decimal avg30 = output30.Where(v => v != default).DefaultIfEmpty(0).Average();
        
        return (avg14, avg20, avg30);
    }

    [Benchmark]
    [BenchmarkCategory("MFI", "VolumeWeighting")]
    public decimal MFI_VolumeImpact()
    {
        _qcMfi14.Clear();
        var output = new decimal[DataSize];
        _qcMfi14.OnBarBatch(_mfiData, output);
        
        // Calculate correlation between volume and MFI changes
        var mfiChanges = new List<decimal>();
        var volumes = new List<decimal>();
        
        for (int i = 15; i < DataSize; i++)
        {
            if (output[i] != default && output[i - 1] != default)
            {
                mfiChanges.Add(Math.Abs(output[i] - output[i - 1]));
                volumes.Add(_mfiData[i].Volume);
            }
        }
        
        if (mfiChanges.Count < 2) return 0;
        
        // Simple correlation coefficient calculation
        decimal meanMfiChange = mfiChanges.Average();
        decimal meanVolume = volumes.Average();
        
        decimal covariance = 0;
        decimal mfiVariance = 0;
        decimal volumeVariance = 0;
        
        for (int i = 0; i < mfiChanges.Count; i++)
        {
            var mfiDiff = mfiChanges[i] - meanMfiChange;
            var volDiff = volumes[i] - meanVolume;
            
            covariance += mfiDiff * volDiff;
            mfiVariance += mfiDiff * mfiDiff;
            volumeVariance += volDiff * volDiff;
        }
        
        if (mfiVariance == 0 || volumeVariance == 0) return 0;
        
        return covariance / (decimal)Math.Sqrt((double)(mfiVariance * volumeVariance));
    }

    [Benchmark]
    [BenchmarkCategory("MFI", "Reversals")]
    public int MFI_DetectReversals()
    {
        _qcMfi14.Clear();
        var output = new decimal[1];
        decimal? previousMfi = null;
        int reversals = 0;
        bool wasOversold = false;
        bool wasOverbought = false;
        
        for (int i = 0; i < _mfiData.Length; i++)
        {
            _qcMfi14.OnBarBatch(new[] { _mfiData[i] }, output);
            
            if (_qcMfi14.IsReady)
            {
                bool isOversold = output[0] < 20m;
                bool isOverbought = output[0] > 80m;
                
                // Detect reversal from oversold
                if (wasOversold && !isOversold && output[0] > 30m)
                {
                    reversals++;
                }
                // Detect reversal from overbought
                else if (wasOverbought && !isOverbought && output[0] < 70m)
                {
                    reversals++;
                }
                
                wasOversold = isOversold;
                wasOverbought = isOverbought;
            }
        }
        
        return reversals;
    }
    
    [Benchmark]
    [BenchmarkCategory("MFI", "Memory")]
    public long MFI_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new MFI_QC<IKline<decimal>, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(_mfiData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcMfi?.Clear();
        _qcMfi14?.Clear();
        _qcMfi20?.Clear();
        _qcMfi30?.Clear();
        _mfiData = null!;
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