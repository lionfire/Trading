using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class VwapBenchmark : IndicatorBenchmarkBase
{
    private VWAP_QC<IKline<decimal>, decimal> _qcVwap = null!;
    private PVWAP<IKline<decimal>, decimal> _parameters = null!;
    
    private VWAP_QC<IKline<decimal>, decimal> _qcVwapDaily = null!;
    private VWAP_QC<IKline<decimal>, decimal> _qcVwapWeekly = null!;
    private VWAP_QC<IKline<decimal>, decimal> _qcVwapMonthly = null!;
    
    private IKline<decimal>[] _vwapData = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Generate VWAP data with volume
        var generator = new TestDataGenerator();
        var dataPoints = generator.GenerateRealisticData(DataSize);
        
        _vwapData = dataPoints.Select(d => new TestKline
        {
            Open = d.Open,
            High = d.High,
            Low = d.Low,
            Close = d.Close,
            Volume = d.Volume,
            Timestamp = d.Timestamp
        }).ToArray();
        
        _parameters = new PVWAP<IKline<decimal>, decimal> 
        { 
            ResetPeriod = VWAPResetPeriod.Daily,
            UseTypicalPrice = true
        };
        _qcVwap = new VWAP_QC<IKline<decimal>, decimal>(_parameters);
        
        _qcVwapDaily = new VWAP_QC<IKline<decimal>, decimal>(new PVWAP<IKline<decimal>, decimal> 
        { 
            ResetPeriod = VWAPResetPeriod.Daily,
            UseTypicalPrice = true
        });
        
        _qcVwapWeekly = new VWAP_QC<IKline<decimal>, decimal>(new PVWAP<IKline<decimal>, decimal> 
        { 
            ResetPeriod = VWAPResetPeriod.Weekly,
            UseTypicalPrice = true
        });
        
        _qcVwapMonthly = new VWAP_QC<IKline<decimal>, decimal>(new PVWAP<IKline<decimal>, decimal> 
        { 
            ResetPeriod = VWAPResetPeriod.Monthly,
            UseTypicalPrice = true
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("VWAP", "QuantConnect")]
    public decimal[] VWAP_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcVwap.Clear();
        _qcVwap.OnBarBatch(_vwapData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("VWAP", "QuantConnect")]
    public List<decimal?> VWAP_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcVwap.Clear();
        
        for (int i = 0; i < _vwapData.Length; i++)
        {
            _qcVwap.OnBarBatch(new[] { _vwapData[i] }, output);
            results.Add(_qcVwap.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("VWAP", "PriceDeviation")]
    public (int above, int below, int at) VWAP_PriceDeviation()
    {
        _qcVwapDaily.Clear();
        var output = new decimal[1];
        int aboveVwap = 0;
        int belowVwap = 0;
        int atVwap = 0;
        
        for (int i = 0; i < _vwapData.Length; i++)
        {
            _qcVwapDaily.OnBarBatch(new[] { _vwapData[i] }, output);
            if (_qcVwapDaily.IsReady)
            {
                var close = _vwapData[i].Close;
                var vwap = output[0];
                var diff = Math.Abs(close - vwap);
                
                if (diff < 0.01m) atVwap++;
                else if (close > vwap) aboveVwap++;
                else belowVwap++;
            }
        }
        
        return (aboveVwap, belowVwap, atVwap);
    }

    [Benchmark]
    [BenchmarkCategory("VWAP", "ResetPeriods")]
    public (decimal daily, decimal weekly, decimal monthly) VWAP_ComparePeriods()
    {
        _qcVwapDaily.Clear();
        _qcVwapWeekly.Clear();
        _qcVwapMonthly.Clear();
        
        var dailyOutput = new decimal[DataSize];
        var weeklyOutput = new decimal[DataSize];
        var monthlyOutput = new decimal[DataSize];
        
        _qcVwapDaily.OnBarBatch(_vwapData, dailyOutput);
        _qcVwapWeekly.OnBarBatch(_vwapData, weeklyOutput);
        _qcVwapMonthly.OnBarBatch(_vwapData, monthlyOutput);
        
        // Return the last valid values
        var lastDaily = dailyOutput.LastOrDefault(v => v != default);
        var lastWeekly = weeklyOutput.LastOrDefault(v => v != default);
        var lastMonthly = monthlyOutput.LastOrDefault(v => v != default);
        
        return (lastDaily, lastWeekly, lastMonthly);
    }

    [Benchmark]
    [BenchmarkCategory("VWAP", "Support")]
    public int VWAP_TestAsSupport()
    {
        _qcVwapDaily.Clear();
        var output = new decimal[1];
        decimal? previousVwap = null;
        decimal? previousClose = null;
        int supportTests = 0;
        
        for (int i = 0; i < _vwapData.Length; i++)
        {
            _qcVwapDaily.OnBarBatch(new[] { _vwapData[i] }, output);
            
            if (_qcVwapDaily.IsReady && previousVwap.HasValue && previousClose.HasValue)
            {
                var currentClose = _vwapData[i].Close;
                var currentVwap = output[0];
                
                // Test if price bounced off VWAP (came from above, touched, and went back up)
                if (previousClose > previousVwap.Value && 
                    currentClose <= currentVwap * 1.01m && 
                    currentClose >= currentVwap * 0.99m)
                {
                    supportTests++;
                }
            }
            
            previousVwap = _qcVwapDaily.IsReady ? output[0] : (decimal?)null;
            previousClose = _vwapData[i].Close;
        }
        
        return supportTests;
    }
    
    [Benchmark]
    [BenchmarkCategory("VWAP", "Memory")]
    public long VWAP_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new VWAP_QC<IKline<decimal>, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(_vwapData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcVwap?.Clear();
        _qcVwapDaily?.Clear();
        _qcVwapWeekly?.Clear();
        _qcVwapMonthly?.Clear();
        _vwapData = null!;
        base.GlobalCleanup();
    }
    
    private class TestKline : IKline<decimal>
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
        public DateTime Timestamp { get; set; }
    }
}