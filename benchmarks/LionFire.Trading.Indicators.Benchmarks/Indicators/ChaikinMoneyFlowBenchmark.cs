using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ChaikinMoneyFlowBenchmark : IndicatorBenchmarkBase
{
    private ChaikinMoneyFlow_QC<IKline<decimal>, decimal> _qcCmf = null!;
    private PChaikinMoneyFlow<IKline<decimal>, decimal> _parameters = null!;
    
    private ChaikinMoneyFlow_QC<IKline<decimal>, decimal> _qcCmf10 = null!;
    private ChaikinMoneyFlow_QC<IKline<decimal>, decimal> _qcCmf20 = null!;
    private ChaikinMoneyFlow_QC<IKline<decimal>, decimal> _qcCmf30 = null!;
    
    private IKline<decimal>[] _cmfData = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Generate data with volume for CMF
        var generator = new TestDataGenerator();
        var dataPoints = generator.GenerateRealisticData(DataSize);
        
        _cmfData = dataPoints.Select(d => new TestKline
        {
            Open = d.Open,
            High = d.High,
            Low = d.Low,
            Close = d.Close,
            Volume = d.Volume,
            Timestamp = d.Timestamp
        }).ToArray();
        
        _parameters = new PChaikinMoneyFlow<IKline<decimal>, decimal> { Period = Period };
        _qcCmf = new ChaikinMoneyFlow_QC<IKline<decimal>, decimal>(_parameters);
        
        _qcCmf10 = new ChaikinMoneyFlow_QC<IKline<decimal>, decimal>(new PChaikinMoneyFlow<IKline<decimal>, decimal> { Period = 10 });
        _qcCmf20 = new ChaikinMoneyFlow_QC<IKline<decimal>, decimal>(new PChaikinMoneyFlow<IKline<decimal>, decimal> { Period = 20 });
        _qcCmf30 = new ChaikinMoneyFlow_QC<IKline<decimal>, decimal>(new PChaikinMoneyFlow<IKline<decimal>, decimal> { Period = 30 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ChaikinMoneyFlow", "QuantConnect")]
    public decimal[] ChaikinMoneyFlow_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcCmf.Clear();
        _qcCmf.OnBarBatch(_cmfData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("ChaikinMoneyFlow", "QuantConnect")]
    public List<decimal?> ChaikinMoneyFlow_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcCmf.Clear();
        
        for (int i = 0; i < _cmfData.Length; i++)
        {
            _qcCmf.OnBarBatch(new[] { _cmfData[i] }, output);
            results.Add(_qcCmf.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("ChaikinMoneyFlow", "MoneyFlowDirection")]
    public (int accumulation, int distribution, int neutral) ChaikinMoneyFlow_FlowDirection()
    {
        _qcCmf20.Clear();
        var output = new decimal[1];
        int accumulationPeriods = 0;
        int distributionPeriods = 0;
        int neutralPeriods = 0;
        
        for (int i = 0; i < _cmfData.Length; i++)
        {
            _qcCmf20.OnBarBatch(new[] { _cmfData[i] }, output);
            if (_qcCmf20.IsReady)
            {
                if (output[0] > 0.1m) accumulationPeriods++;
                else if (output[0] < -0.1m) distributionPeriods++;
                else neutralPeriods++;
            }
        }
        
        return (accumulationPeriods, distributionPeriods, neutralPeriods);
    }

    [Benchmark]
    [BenchmarkCategory("ChaikinMoneyFlow", "Divergence")]
    public int ChaikinMoneyFlow_DetectDivergence()
    {
        _qcCmf20.Clear();
        var output = new decimal[DataSize];
        _qcCmf20.OnBarBatch(_cmfData, output);
        
        int divergences = 0;
        int lookback = 20;
        
        for (int i = lookback + 20; i < DataSize; i++)
        {
            if (output[i] == default || output[i - lookback] == default) continue;
            
            // Price trend
            var priceTrend = _cmfData[i].Close - _cmfData[i - lookback].Close;
            
            // CMF trend
            var cmfTrend = output[i] - output[i - lookback];
            
            // Check for divergence
            if ((priceTrend > 0 && cmfTrend < -0.05m) || (priceTrend < 0 && cmfTrend > 0.05m))
            {
                divergences++;
            }
        }
        
        return divergences;
    }

    [Benchmark]
    [BenchmarkCategory("ChaikinMoneyFlow", "ZeroCrossings")]
    public int ChaikinMoneyFlow_CountZeroCrossings()
    {
        _qcCmf20.Clear();
        var output = new decimal[1];
        decimal? previousValue = null;
        int crossings = 0;
        
        for (int i = 0; i < _cmfData.Length; i++)
        {
            _qcCmf20.OnBarBatch(new[] { _cmfData[i] }, output);
            
            if (_qcCmf20.IsReady && previousValue.HasValue)
            {
                if ((previousValue.Value < 0 && output[0] > 0) ||
                    (previousValue.Value > 0 && output[0] < 0))
                {
                    crossings++;
                }
            }
            
            previousValue = _qcCmf20.IsReady ? output[0] : (decimal?)null;
        }
        
        return crossings;
    }

    [Benchmark]
    [BenchmarkCategory("ChaikinMoneyFlow", "VolumeImpact")]
    public decimal ChaikinMoneyFlow_VolumeCorrelation()
    {
        _qcCmf20.Clear();
        var output = new decimal[DataSize];
        _qcCmf20.OnBarBatch(_cmfData, output);
        
        // Calculate correlation between volume and CMF values
        var cmfValues = new List<decimal>();
        var volumes = new List<decimal>();
        
        for (int i = 20; i < DataSize; i++)
        {
            if (output[i] != default)
            {
                cmfValues.Add(Math.Abs(output[i]));
                volumes.Add(_cmfData[i].Volume);
            }
        }
        
        if (cmfValues.Count < 2) return 0;
        
        // Simple correlation calculation
        decimal cmfMean = cmfValues.Average();
        decimal volumeMean = volumes.Average();
        
        decimal covariance = 0;
        decimal cmfVariance = 0;
        decimal volumeVariance = 0;
        
        for (int i = 0; i < cmfValues.Count; i++)
        {
            var cmfDiff = cmfValues[i] - cmfMean;
            var volDiff = volumes[i] - volumeMean;
            
            covariance += cmfDiff * volDiff;
            cmfVariance += cmfDiff * cmfDiff;
            volumeVariance += volDiff * volDiff;
        }
        
        if (cmfVariance == 0 || volumeVariance == 0) return 0;
        
        return covariance / (decimal)Math.Sqrt((double)(cmfVariance * volumeVariance));
    }

    [Benchmark]
    [BenchmarkCategory("ChaikinMoneyFlow", "PeriodComparison")]
    public (decimal short_volatility, decimal medium_volatility, decimal long_volatility) ChaikinMoneyFlow_ComparePeriods()
    {
        _qcCmf10.Clear();
        _qcCmf20.Clear();
        _qcCmf30.Clear();
        
        var output10 = new decimal[DataSize];
        var output20 = new decimal[DataSize];
        var output30 = new decimal[DataSize];
        
        _qcCmf10.OnBarBatch(_cmfData, output10);
        _qcCmf20.OnBarBatch(_cmfData, output20);
        _qcCmf30.OnBarBatch(_cmfData, output30);
        
        // Calculate volatility for each period
        decimal vol10 = CalculateVolatility(output10, 10);
        decimal vol20 = CalculateVolatility(output20, 20);
        decimal vol30 = CalculateVolatility(output30, 30);
        
        return (vol10, vol20, vol30);
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
    [BenchmarkCategory("ChaikinMoneyFlow", "Memory")]
    public long ChaikinMoneyFlow_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new ChaikinMoneyFlow_QC<IKline<decimal>, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(_cmfData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcCmf?.Clear();
        _qcCmf10?.Clear();
        _qcCmf20?.Clear();
        _qcCmf30?.Clear();
        _cmfData = null!;
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