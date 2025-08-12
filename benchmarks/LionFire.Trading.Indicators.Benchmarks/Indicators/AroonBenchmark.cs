using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class AroonBenchmark : IndicatorBenchmarkBase
{
    private Aroon_QC<decimal, decimal> _qcAroon = null!;
    private PAroon<decimal, decimal> _parameters = null!;
    
    private Aroon_QC<decimal, decimal> _qcAroon14 = null!;
    private Aroon_QC<decimal, decimal> _qcAroon25 = null!;
    private Aroon_QC<decimal, decimal> _qcAroon50 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PAroon<decimal, decimal> { Period = Period };
        _qcAroon = new Aroon_QC<decimal, decimal>(_parameters);
        
        _qcAroon14 = new Aroon_QC<decimal, decimal>(new PAroon<decimal, decimal> { Period = 14 });
        _qcAroon25 = new Aroon_QC<decimal, decimal>(new PAroon<decimal, decimal> { Period = 25 });
        _qcAroon50 = new Aroon_QC<decimal, decimal>(new PAroon<decimal, decimal> { Period = 50 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Aroon", "QuantConnect")]
    public AroonOutput<decimal>[] Aroon_QuantConnect_Batch()
    {
        var output = new AroonOutput<decimal>[DataSize];
        
        _qcAroon.Clear();
        _qcAroon.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("Aroon", "QuantConnect")]
    public List<AroonOutput<decimal>?> Aroon_QuantConnect_Streaming()
    {
        var results = new List<AroonOutput<decimal>?>(DataSize);
        var output = new AroonOutput<decimal>[1];
        
        _qcAroon.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAroon.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcAroon.IsReady ? output[0] : null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("Aroon", "TrendStrength")]
    public (int strong_up, int strong_down, int neutral) Aroon_TrendStrength()
    {
        _qcAroon25.Clear();
        var output = new AroonOutput<decimal>[1];
        int strongUptrend = 0;
        int strongDowntrend = 0;
        int neutral = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAroon25.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcAroon25.IsReady && output[0] != null)
            {
                var aroonUp = output[0].AroonUp;
                var aroonDown = output[0].AroonDown;
                
                if (aroonUp > 70m && aroonDown < 30m)
                {
                    strongUptrend++;
                }
                else if (aroonDown > 70m && aroonUp < 30m)
                {
                    strongDowntrend++;
                }
                else
                {
                    neutral++;
                }
            }
        }
        
        return (strongUptrend, strongDowntrend, neutral);
    }

    [Benchmark]
    [BenchmarkCategory("Aroon", "Crossovers")]
    public int Aroon_DetectCrossovers()
    {
        _qcAroon25.Clear();
        var output = new AroonOutput<decimal>[1];
        AroonOutput<decimal>? previousOutput = null;
        int crossovers = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAroon25.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcAroon25.IsReady && output[0] != null && previousOutput != null)
            {
                // Check for Aroon Up/Down crossover
                bool prevUpAbove = previousOutput.AroonUp > previousOutput.AroonDown;
                bool currUpAbove = output[0].AroonUp > output[0].AroonDown;
                
                if (prevUpAbove != currUpAbove)
                {
                    crossovers++;
                }
                
                previousOutput = output[0];
            }
            else if (_qcAroon25.IsReady && output[0] != null)
            {
                previousOutput = output[0];
            }
        }
        
        return crossovers;
    }

    [Benchmark]
    [BenchmarkCategory("Aroon", "Oscillator")]
    public (int positive, int negative, int neutral) Aroon_OscillatorValues()
    {
        _qcAroon25.Clear();
        var output = new AroonOutput<decimal>[1];
        int positive = 0;
        int negative = 0;
        int neutral = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAroon25.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcAroon25.IsReady && output[0] != null)
            {
                var oscillator = output[0].AroonUp - output[0].AroonDown;
                
                if (oscillator > 50m) positive++;
                else if (oscillator < -50m) negative++;
                else neutral++;
            }
        }
        
        return (positive, negative, neutral);
    }

    [Benchmark]
    [BenchmarkCategory("Aroon", "NewHighsLows")]
    public (int new_highs, int new_lows) Aroon_DetectNewExtremes()
    {
        _qcAroon25.Clear();
        var output = new AroonOutput<decimal>[1];
        int newHighs = 0;
        int newLows = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAroon25.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcAroon25.IsReady && output[0] != null)
            {
                // Aroon Up = 100 means new high within period
                if (output[0].AroonUp == 100m) newHighs++;
                
                // Aroon Down = 100 means new low within period
                if (output[0].AroonDown == 100m) newLows++;
            }
        }
        
        return (newHighs, newLows);
    }

    [Benchmark]
    [BenchmarkCategory("Aroon", "PeriodComparison")]
    public (decimal short_volatility, decimal medium_volatility, decimal long_volatility) Aroon_ComparePeriods()
    {
        _qcAroon14.Clear();
        _qcAroon25.Clear();
        _qcAroon50.Clear();
        
        var output14 = new AroonOutput<decimal>[DataSize];
        var output25 = new AroonOutput<decimal>[DataSize];
        var output50 = new AroonOutput<decimal>[DataSize];
        
        _qcAroon14.OnBarBatch(HLCData, output14);
        _qcAroon25.OnBarBatch(HLCData, output25);
        _qcAroon50.OnBarBatch(HLCData, output50);
        
        // Calculate oscillator volatility for each period
        decimal vol14 = CalculateOscillatorVolatility(output14);
        decimal vol25 = CalculateOscillatorVolatility(output25);
        decimal vol50 = CalculateOscillatorVolatility(output50);
        
        return (vol14, vol25, vol50);
    }
    
    private decimal CalculateOscillatorVolatility(AroonOutput<decimal>[] outputs)
    {
        var oscillatorValues = new List<decimal>();
        
        foreach (var output in outputs)
        {
            if (output != null)
            {
                oscillatorValues.Add(output.AroonUp - output.AroonDown);
            }
        }
        
        if (oscillatorValues.Count < 2) return 0;
        
        decimal mean = oscillatorValues.Average();
        decimal sumSquaredDiff = oscillatorValues.Sum(v => (v - mean) * (v - mean));
        
        return (decimal)Math.Sqrt((double)(sumSquaredDiff / oscillatorValues.Count));
    }

    [Benchmark]
    [BenchmarkCategory("Aroon", "TrendChanges")]
    public int Aroon_DetectTrendChanges()
    {
        _qcAroon25.Clear();
        var output = new AroonOutput<decimal>[1];
        AroonOutput<decimal>? previousOutput = null;
        int trendChanges = 0;
        string previousTrend = "neutral";
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcAroon25.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcAroon25.IsReady && output[0] != null)
            {
                string currentTrend;
                
                if (output[0].AroonUp > 70m && output[0].AroonDown < 30m)
                    currentTrend = "up";
                else if (output[0].AroonDown > 70m && output[0].AroonUp < 30m)
                    currentTrend = "down";
                else
                    currentTrend = "neutral";
                
                if (currentTrend != previousTrend && previousTrend != "neutral" && currentTrend != "neutral")
                {
                    trendChanges++;
                }
                
                previousTrend = currentTrend;
            }
        }
        
        return trendChanges;
    }
    
    [Benchmark]
    [BenchmarkCategory("Aroon", "Memory")]
    public long Aroon_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new Aroon_QC<decimal, decimal>(_parameters);
            var output = new AroonOutput<decimal>[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcAroon?.Clear();
        _qcAroon14?.Clear();
        _qcAroon25?.Clear();
        _qcAroon50?.Clear();
        base.GlobalCleanup();
    }
}

public class AroonOutput<T>
{
    public T AroonUp { get; set; }
    public T AroonDown { get; set; }
}