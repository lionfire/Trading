using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class VwmaBenchmark : IndicatorBenchmarkBase
{
    private VWMA_QC<IKline<decimal>, decimal> _qcVwma = null!;
    private PVWMA<IKline<decimal>, decimal> _parameters = null!;
    
    private VWMA_QC<IKline<decimal>, decimal> _qcVwma10 = null!;
    private VWMA_QC<IKline<decimal>, decimal> _qcVwma20 = null!;
    private VWMA_QC<IKline<decimal>, decimal> _qcVwma50 = null!;
    
    private IKline<decimal>[] _vwmaData = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Generate volume data for VWMA
        var generator = new TestDataGenerator();
        var dataPoints = generator.GenerateRealisticData(DataSize);
        
        _vwmaData = dataPoints.Select(d => new TestKline
        {
            Open = d.Open,
            High = d.High,
            Low = d.Low,
            Close = d.Close,
            Volume = d.Volume,
            Timestamp = d.Timestamp
        }).ToArray();
        
        _parameters = new PVWMA<IKline<decimal>, decimal> { Period = Period };
        _qcVwma = new VWMA_QC<IKline<decimal>, decimal>(_parameters);
        
        _qcVwma10 = new VWMA_QC<IKline<decimal>, decimal>(new PVWMA<IKline<decimal>, decimal> { Period = 10 });
        _qcVwma20 = new VWMA_QC<IKline<decimal>, decimal>(new PVWMA<IKline<decimal>, decimal> { Period = 20 });
        _qcVwma50 = new VWMA_QC<IKline<decimal>, decimal>(new PVWMA<IKline<decimal>, decimal> { Period = 50 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("VWMA", "QuantConnect")]
    public decimal[] VWMA_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcVwma.Clear();
        _qcVwma.OnBarBatch(_vwmaData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("VWMA", "QuantConnect")]
    public List<decimal?> VWMA_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcVwma.Clear();
        
        for (int i = 0; i < _vwmaData.Length; i++)
        {
            _qcVwma.OnBarBatch(new[] { _vwmaData[i] }, output);
            results.Add(_qcVwma.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("VWMA", "VolumeWeighting")]
    public decimal VWMA_VolumeImpactAnalysis()
    {
        _qcVwma20.Clear();
        var vwmaOutput = new decimal[DataSize];
        _qcVwma20.OnBarBatch(_vwmaData, vwmaOutput);
        
        // Compare VWMA with simple average to measure volume impact
        decimal totalDifference = 0;
        int count = 0;
        
        for (int i = 20; i < DataSize; i++)
        {
            if (vwmaOutput[i] != default)
            {
                // Calculate simple average for same period
                decimal simpleAvg = 0;
                for (int j = i - 19; j <= i; j++)
                {
                    simpleAvg += _vwmaData[j].Close;
                }
                simpleAvg /= 20;
                
                // Measure difference
                var difference = Math.Abs(vwmaOutput[i] - simpleAvg);
                totalDifference += difference;
                count++;
            }
        }
        
        return count > 0 ? totalDifference / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("VWMA", "TrendFollowing")]
    public (int uptrend, int downtrend, int sideways) VWMA_TrendClassification()
    {
        _qcVwma20.Clear();
        var output = new decimal[1];
        decimal? previousVwma = null;
        int uptrendCount = 0;
        int downtrendCount = 0;
        int sidewaysCount = 0;
        
        for (int i = 0; i < _vwmaData.Length; i++)
        {
            _qcVwma20.OnBarBatch(new[] { _vwmaData[i] }, output);
            
            if (_qcVwma20.IsReady && previousVwma.HasValue)
            {
                var change = output[0] - previousVwma.Value;
                var changePercent = Math.Abs(change / previousVwma.Value);
                
                if (changePercent < 0.001m) sidewaysCount++;
                else if (change > 0) uptrendCount++;
                else downtrendCount++;
            }
            
            previousVwma = _qcVwma20.IsReady ? output[0] : (decimal?)null;
        }
        
        return (uptrendCount, downtrendCount, sidewaysCount);
    }

    [Benchmark]
    [BenchmarkCategory("VWMA", "PeriodComparison")]
    public (decimal short_smooth, decimal medium_smooth, decimal long_smooth) VWMA_CompareSmoothness()
    {
        _qcVwma10.Clear();
        _qcVwma20.Clear();
        _qcVwma50.Clear();
        
        var output10 = new decimal[DataSize];
        var output20 = new decimal[DataSize];
        var output50 = new decimal[DataSize];
        
        _qcVwma10.OnBarBatch(_vwmaData, output10);
        _qcVwma20.OnBarBatch(_vwmaData, output20);
        _qcVwma50.OnBarBatch(_vwmaData, output50);
        
        // Calculate smoothness as inverse of volatility
        decimal smooth10 = CalculateSmoothness(output10, 10);
        decimal smooth20 = CalculateSmoothness(output20, 20);
        decimal smooth50 = CalculateSmoothness(output50, 50);
        
        return (smooth10, smooth20, smooth50);
    }
    
    private decimal CalculateSmoothness(decimal[] values, int startIndex)
    {
        var changes = new List<decimal>();
        
        for (int i = startIndex + 1; i < values.Length; i++)
        {
            if (values[i] != default && values[i - 1] != default)
            {
                var change = Math.Abs((values[i] - values[i - 1]) / values[i - 1]);
                changes.Add(change);
            }
        }
        
        if (changes.Count == 0) return 0;
        
        // Return inverse of average change (higher = smoother)
        var avgChange = changes.Average();
        return avgChange > 0 ? 1 / avgChange : 1000;
    }

    [Benchmark]
    [BenchmarkCategory("VWMA", "VolumeSpikes")]
    public int VWMA_DetectVolumeInfluence()
    {
        _qcVwma20.Clear();
        var output = new decimal[1];
        decimal? previousVwma = null;
        decimal? previousClose = null;
        int volumeInfluencedMoves = 0;
        
        for (int i = 0; i < _vwmaData.Length; i++)
        {
            _qcVwma20.OnBarBatch(new[] { _vwmaData[i] }, output);
            
            if (_qcVwma20.IsReady && previousVwma.HasValue && previousClose.HasValue && i > 0)
            {
                var vwmaChange = Math.Abs(output[0] - previousVwma.Value);
                var priceChange = Math.Abs(_vwmaData[i].Close - previousClose.Value);
                var currentVolume = _vwmaData[i].Volume;
                var previousVolume = _vwmaData[i - 1].Volume;
                
                // Detect when VWMA moves more than expected due to volume
                if (currentVolume > previousVolume * 1.5m && vwmaChange > priceChange * 0.8m)
                {
                    volumeInfluencedMoves++;
                }
            }
            
            previousVwma = _qcVwma20.IsReady ? output[0] : (decimal?)null;
            previousClose = _vwmaData[i].Close;
        }
        
        return volumeInfluencedMoves;
    }

    [Benchmark]
    [BenchmarkCategory("VWMA", "CrossoverSignals")]
    public int VWMA_DetectCrossovers()
    {
        _qcVwma20.Clear();
        var output = new decimal[1];
        int crossovers = 0;
        
        for (int i = 0; i < _vwmaData.Length; i++)
        {
            _qcVwma20.OnBarBatch(new[] { _vwmaData[i] }, output);
            
            if (_qcVwma20.IsReady && i > 0)
            {
                var currentClose = _vwmaData[i].Close;
                var previousClose = _vwmaData[i - 1].Close;
                var vwma = output[0];
                
                // Detect price crossing VWMA
                if ((previousClose <= vwma && currentClose > vwma) ||
                    (previousClose >= vwma && currentClose < vwma))
                {
                    crossovers++;
                }
            }
        }
        
        return crossovers;
    }
    
    [Benchmark]
    [BenchmarkCategory("VWMA", "Memory")]
    public long VWMA_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new VWMA_QC<IKline<decimal>, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(_vwmaData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcVwma?.Clear();
        _qcVwma10?.Clear();
        _qcVwma20?.Clear();
        _qcVwma50?.Clear();
        _vwmaData = null!;
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