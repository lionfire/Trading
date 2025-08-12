using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class SupertrendBenchmark : IndicatorBenchmarkBase
{
    private Supertrend_QC<decimal, decimal> _qcSupertrend = null!;
    private PSupertrend<decimal, decimal> _parameters = null!;
    
    private Supertrend_QC<decimal, decimal> _qcSupertrendFast = null!;
    private Supertrend_QC<decimal, decimal> _qcSupertrendMedium = null!;
    private Supertrend_QC<decimal, decimal> _qcSupertrendSlow = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PSupertrend<decimal, decimal> 
        { 
            Period = Period,
            Multiplier = 3.0m
        };
        _qcSupertrend = new Supertrend_QC<decimal, decimal>(_parameters);
        
        // Fast settings (more signals)
        _qcSupertrendFast = new Supertrend_QC<decimal, decimal>(new PSupertrend<decimal, decimal> 
        { 
            Period = 7,
            Multiplier = 2.0m
        });
        
        // Medium settings (balanced)
        _qcSupertrendMedium = new Supertrend_QC<decimal, decimal>(new PSupertrend<decimal, decimal> 
        { 
            Period = 10,
            Multiplier = 3.0m
        });
        
        // Slow settings (fewer signals)
        _qcSupertrendSlow = new Supertrend_QC<decimal, decimal>(new PSupertrend<decimal, decimal> 
        { 
            Period = 20,
            Multiplier = 4.0m
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Supertrend", "QuantConnect")]
    public SupertrendOutput<decimal>[] Supertrend_QuantConnect_Batch()
    {
        var output = new SupertrendOutput<decimal>[DataSize];
        
        _qcSupertrend.Clear();
        _qcSupertrend.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("Supertrend", "QuantConnect")]
    public List<SupertrendOutput<decimal>?> Supertrend_QuantConnect_Streaming()
    {
        var results = new List<SupertrendOutput<decimal>?>(DataSize);
        var output = new SupertrendOutput<decimal>[1];
        
        _qcSupertrend.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcSupertrend.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcSupertrend.IsReady ? output[0] : null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("Supertrend", "TrendDirection")]
    public (int bullish, int bearish) Supertrend_TrendDirection()
    {
        _qcSupertrendMedium.Clear();
        var output = new SupertrendOutput<decimal>[1];
        int bullishPeriods = 0;
        int bearishPeriods = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcSupertrendMedium.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcSupertrendMedium.IsReady && output[0] != null)
            {
                if (output[0].IsBullish)
                    bullishPeriods++;
                else
                    bearishPeriods++;
            }
        }
        
        return (bullishPeriods, bearishPeriods);
    }

    [Benchmark]
    [BenchmarkCategory("Supertrend", "TrendChanges")]
    public int Supertrend_DetectTrendChanges()
    {
        _qcSupertrendMedium.Clear();
        var output = new SupertrendOutput<decimal>[1];
        bool? previousTrend = null;
        int trendChanges = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcSupertrendMedium.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcSupertrendMedium.IsReady && output[0] != null)
            {
                if (previousTrend.HasValue && previousTrend.Value != output[0].IsBullish)
                {
                    trendChanges++;
                }
                previousTrend = output[0].IsBullish;
            }
        }
        
        return trendChanges;
    }

    [Benchmark]
    [BenchmarkCategory("Supertrend", "StopLoss")]
    public decimal Supertrend_AverageStopDistance()
    {
        _qcSupertrendMedium.Clear();
        var output = new SupertrendOutput<decimal>[1];
        decimal totalDistance = 0;
        int count = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcSupertrendMedium.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcSupertrendMedium.IsReady && output[0] != null)
            {
                var close = HLCData[i].Close;
                var stopLevel = output[0].Value;
                var distance = Math.Abs(close - stopLevel);
                totalDistance += distance;
                count++;
            }
        }
        
        return count > 0 ? totalDistance / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("Supertrend", "SensitivityComparison")]
    public (int fast_signals, int medium_signals, int slow_signals) Supertrend_CompareSensitivity()
    {
        _qcSupertrendFast.Clear();
        _qcSupertrendMedium.Clear();
        _qcSupertrendSlow.Clear();
        
        var fastOutput = new SupertrendOutput<decimal>[1];
        var mediumOutput = new SupertrendOutput<decimal>[1];
        var slowOutput = new SupertrendOutput<decimal>[1];
        
        bool? prevFast = null, prevMedium = null, prevSlow = null;
        int fastSignals = 0, mediumSignals = 0, slowSignals = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcSupertrendFast.OnBarBatch(new[] { HLCData[i] }, fastOutput);
            _qcSupertrendMedium.OnBarBatch(new[] { HLCData[i] }, mediumOutput);
            _qcSupertrendSlow.OnBarBatch(new[] { HLCData[i] }, slowOutput);
            
            if (_qcSupertrendFast.IsReady && fastOutput[0] != null && prevFast.HasValue)
            {
                if (prevFast.Value != fastOutput[0].IsBullish) fastSignals++;
            }
            
            if (_qcSupertrendMedium.IsReady && mediumOutput[0] != null && prevMedium.HasValue)
            {
                if (prevMedium.Value != mediumOutput[0].IsBullish) mediumSignals++;
            }
            
            if (_qcSupertrendSlow.IsReady && slowOutput[0] != null && prevSlow.HasValue)
            {
                if (prevSlow.Value != slowOutput[0].IsBullish) slowSignals++;
            }
            
            if (_qcSupertrendFast.IsReady && fastOutput[0] != null) prevFast = fastOutput[0].IsBullish;
            if (_qcSupertrendMedium.IsReady && mediumOutput[0] != null) prevMedium = mediumOutput[0].IsBullish;
            if (_qcSupertrendSlow.IsReady && slowOutput[0] != null) prevSlow = slowOutput[0].IsBullish;
        }
        
        return (fastSignals, mediumSignals, slowSignals);
    }

    [Benchmark]
    [BenchmarkCategory("Supertrend", "TrendStrength")]
    public decimal Supertrend_MeasureTrendStrength()
    {
        _qcSupertrendMedium.Clear();
        var output = new SupertrendOutput<decimal>[DataSize];
        _qcSupertrendMedium.OnBarBatch(HLCData, output);
        
        decimal totalStrength = 0;
        int count = 0;
        
        for (int i = 0; i < DataSize; i++)
        {
            if (output[i] != null && _qcSupertrendMedium.IsReady)
            {
                var close = HLCData[i].Close;
                var supertrend = output[i].Value;
                
                // Measure strength as percentage distance from Supertrend line
                var strength = Math.Abs((close - supertrend) / supertrend) * 100;
                totalStrength += strength;
                count++;
            }
        }
        
        return count > 0 ? totalStrength / count : 0;
    }
    
    [Benchmark]
    [BenchmarkCategory("Supertrend", "Memory")]
    public long Supertrend_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new Supertrend_QC<decimal, decimal>(_parameters);
            var output = new SupertrendOutput<decimal>[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcSupertrend?.Clear();
        _qcSupertrendFast?.Clear();
        _qcSupertrendMedium?.Clear();
        _qcSupertrendSlow?.Clear();
        base.GlobalCleanup();
    }
}

public class SupertrendOutput<T>
{
    public T Value { get; set; }
    public bool IsBullish { get; set; }
}