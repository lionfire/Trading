using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ObvBenchmark : IndicatorBenchmarkBase
{
    private OBV_QC<IKline<decimal>, decimal> _qcObv = null!;
    private POBV<IKline<decimal>, decimal> _parameters = null!;
    
    private IKline<decimal>[] _volumeData = null!;
    
    // For divergence testing
    private decimal[] _priceData = null!;
    private decimal[] _obvValues = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Generate volume data
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
        
        _priceData = dataPoints.Select(d => d.Close).ToArray();
        _obvValues = new decimal[DataSize];
        
        _parameters = new POBV<IKline<decimal>, decimal>();
        _qcObv = new OBV_QC<IKline<decimal>, decimal>(_parameters);
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("OBV", "QuantConnect")]
    public decimal[] OBV_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcObv.Clear();
        _qcObv.OnBarBatch(_volumeData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("OBV", "QuantConnect")]
    public List<decimal?> OBV_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcObv.Clear();
        
        for (int i = 0; i < _volumeData.Length; i++)
        {
            _qcObv.OnBarBatch(new[] { _volumeData[i] }, output);
            results.Add(_qcObv.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("OBV", "TrendConfirmation")]
    public (int confirmed, int divergent) OBV_TrendConfirmation()
    {
        _qcObv.Clear();
        _qcObv.OnBarBatch(_volumeData, _obvValues);
        
        int confirmedTrends = 0;
        int divergentTrends = 0;
        int lookback = 20;
        
        for (int i = lookback; i < DataSize; i++)
        {
            if (!_qcObv.IsReady) continue;
            
            // Calculate price trend
            var priceTrend = _priceData[i] - _priceData[i - lookback];
            
            // Calculate OBV trend
            var obvTrend = _obvValues[i] - _obvValues[i - lookback];
            
            // Check if trends align
            if ((priceTrend > 0 && obvTrend > 0) || (priceTrend < 0 && obvTrend < 0))
            {
                confirmedTrends++;
            }
            else if ((priceTrend > 0 && obvTrend < 0) || (priceTrend < 0 && obvTrend > 0))
            {
                divergentTrends++;
            }
        }
        
        return (confirmedTrends, divergentTrends);
    }

    [Benchmark]
    [BenchmarkCategory("OBV", "VolumeAnalysis")]
    public (int accumulation, int distribution) OBV_AccumulationDistribution()
    {
        _qcObv.Clear();
        var output = new decimal[1];
        decimal? previousObv = null;
        int accumulationDays = 0;
        int distributionDays = 0;
        
        for (int i = 0; i < _volumeData.Length; i++)
        {
            _qcObv.OnBarBatch(new[] { _volumeData[i] }, output);
            
            if (_qcObv.IsReady && previousObv.HasValue)
            {
                var obvChange = output[0] - previousObv.Value;
                
                if (obvChange > 0)
                {
                    accumulationDays++; // Buying pressure
                }
                else if (obvChange < 0)
                {
                    distributionDays++; // Selling pressure
                }
            }
            
            previousObv = _qcObv.IsReady ? output[0] : (decimal?)null;
        }
        
        return (accumulationDays, distributionDays);
    }

    [Benchmark]
    [BenchmarkCategory("OBV", "BreakoutDetection")]
    public int OBV_DetectBreakouts()
    {
        _qcObv.Clear();
        _qcObv.OnBarBatch(_volumeData, _obvValues);
        
        int breakouts = 0;
        int lookback = 50;
        
        for (int i = lookback; i < DataSize; i++)
        {
            if (!_qcObv.IsReady) continue;
            
            // Find the highest OBV in the lookback period
            decimal maxObv = _obvValues[i - lookback];
            decimal minObv = _obvValues[i - lookback];
            
            for (int j = i - lookback + 1; j < i; j++)
            {
                if (_obvValues[j] > maxObv) maxObv = _obvValues[j];
                if (_obvValues[j] < minObv) minObv = _obvValues[j];
            }
            
            // Check for breakout
            if (_obvValues[i] > maxObv * 1.02m || _obvValues[i] < minObv * 0.98m)
            {
                breakouts++;
            }
        }
        
        return breakouts;
    }

    [Benchmark]
    [BenchmarkCategory("OBV", "SlopeAnalysis")]
    public decimal OBV_AverageSlope()
    {
        _qcObv.Clear();
        _qcObv.OnBarBatch(_volumeData, _obvValues);
        
        decimal totalSlope = 0;
        int count = 0;
        int period = 10;
        
        for (int i = period; i < DataSize; i++)
        {
            if (!_qcObv.IsReady) continue;
            
            var slope = (_obvValues[i] - _obvValues[i - period]) / period;
            totalSlope += Math.Abs(slope);
            count++;
        }
        
        return count > 0 ? totalSlope / count : 0;
    }
    
    [Benchmark]
    [BenchmarkCategory("OBV", "Memory")]
    public long OBV_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new OBV_QC<IKline<decimal>, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(_volumeData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcObv?.Clear();
        _volumeData = null!;
        _priceData = null!;
        _obvValues = null!;
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