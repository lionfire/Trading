using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class KlingerOscillatorBenchmark : IndicatorBenchmarkBase
{
    private KlingerOscillator_FP<IKline<decimal>, decimal> _fpKlinger = null!;
    private PKlingerOscillator<IKline<decimal>, decimal> _parameters = null!;
    
    private KlingerOscillator_FP<IKline<decimal>, decimal> _fpKlingerFast = null!;
    private KlingerOscillator_FP<IKline<decimal>, decimal> _fpKlingerSlow = null!;
    
    private IKline<decimal>[] _klingerData = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Generate data with volume for Klinger Oscillator
        var generator = new TestDataGenerator();
        var dataPoints = generator.GenerateRealisticData(DataSize);
        
        _klingerData = dataPoints.Select(d => new TestKline
        {
            Open = d.Open,
            High = d.High,
            Low = d.Low,
            Close = d.Close,
            Volume = d.Volume,
            Timestamp = d.Timestamp
        }).ToArray();
        
        _parameters = new PKlingerOscillator<IKline<decimal>, decimal> 
        { 
            FastPeriod = 34,
            SlowPeriod = 55,
            SignalPeriod = 13
        };
        _fpKlinger = new KlingerOscillator_FP<IKline<decimal>, decimal>(_parameters);
        
        // Fast settings
        _fpKlingerFast = new KlingerOscillator_FP<IKline<decimal>, decimal>(new PKlingerOscillator<IKline<decimal>, decimal> 
        { 
            FastPeriod = 21,
            SlowPeriod = 34,
            SignalPeriod = 9
        });
        
        // Slow settings
        _fpKlingerSlow = new KlingerOscillator_FP<IKline<decimal>, decimal>(new PKlingerOscillator<IKline<decimal>, decimal> 
        { 
            FastPeriod = 50,
            SlowPeriod = 89,
            SignalPeriod = 21
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("KlingerOscillator", "FinancialPython")]
    public KlingerOscillatorOutput<decimal>[] KlingerOscillator_FinancialPython_Batch()
    {
        var output = new KlingerOscillatorOutput<decimal>[DataSize];
        
        _fpKlinger.Clear();
        _fpKlinger.OnBarBatch(_klingerData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("KlingerOscillator", "FinancialPython")]
    public List<KlingerOscillatorOutput<decimal>?> KlingerOscillator_FinancialPython_Streaming()
    {
        var results = new List<KlingerOscillatorOutput<decimal>?>(DataSize);
        var output = new KlingerOscillatorOutput<decimal>[1];
        
        _fpKlinger.Clear();
        
        for (int i = 0; i < _klingerData.Length; i++)
        {
            _fpKlinger.OnBarBatch(new[] { _klingerData[i] }, output);
            results.Add(_fpKlinger.IsReady ? output[0] : null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("KlingerOscillator", "VolumeFlow")]
    public (int positive_flow, int negative_flow) KlingerOscillator_VolumeFlowDirection()
    {
        _fpKlinger.Clear();
        var output = new KlingerOscillatorOutput<decimal>[1];
        int positiveFlow = 0;
        int negativeFlow = 0;
        
        for (int i = 0; i < _klingerData.Length; i++)
        {
            _fpKlinger.OnBarBatch(new[] { _klingerData[i] }, output);
            
            if (_fpKlinger.IsReady && output[0] != null)
            {
                if (output[0].KlingerOscillator > 0) positiveFlow++;
                else negativeFlow++;
            }
        }
        
        return (positiveFlow, negativeFlow);
    }

    [Benchmark]
    [BenchmarkCategory("KlingerOscillator", "SignalCrossings")]
    public int KlingerOscillator_DetectSignalCrossings()
    {
        _fpKlinger.Clear();
        var output = new KlingerOscillatorOutput<decimal>[1];
        KlingerOscillatorOutput<decimal>? previousOutput = null;
        int crossings = 0;
        
        for (int i = 0; i < _klingerData.Length; i++)
        {
            _fpKlinger.OnBarBatch(new[] { _klingerData[i] }, output);
            
            if (_fpKlinger.IsReady && output[0] != null && previousOutput != null)
            {
                // Check for Klinger-Signal line crossover
                bool prevAbove = previousOutput.KlingerOscillator > previousOutput.Signal;
                bool currAbove = output[0].KlingerOscillator > output[0].Signal;
                
                if (prevAbove != currAbove)
                {
                    crossings++;
                }
                
                previousOutput = output[0];
            }
            else if (_fpKlinger.IsReady && output[0] != null)
            {
                previousOutput = output[0];
            }
        }
        
        return crossings;
    }

    [Benchmark]
    [BenchmarkCategory("KlingerOscillator", "ZeroCrossings")]
    public int KlingerOscillator_CountZeroCrossings()
    {
        _fpKlinger.Clear();
        var output = new KlingerOscillatorOutput<decimal>[1];
        decimal? previousKlinger = null;
        int crossings = 0;
        
        for (int i = 0; i < _klingerData.Length; i++)
        {
            _fpKlinger.OnBarBatch(new[] { _klingerData[i] }, output);
            
            if (_fpKlinger.IsReady && output[0] != null && previousKlinger.HasValue)
            {
                if ((previousKlinger.Value < 0 && output[0].KlingerOscillator > 0) ||
                    (previousKlinger.Value > 0 && output[0].KlingerOscillator < 0))
                {
                    crossings++;
                }
                
                previousKlinger = output[0].KlingerOscillator;
            }
            else if (_fpKlinger.IsReady && output[0] != null)
            {
                previousKlinger = output[0].KlingerOscillator;
            }
        }
        
        return crossings;
    }

    [Benchmark]
    [BenchmarkCategory("KlingerOscillator", "TrendDivergence")]
    public int KlingerOscillator_DetectDivergence()
    {
        _fpKlinger.Clear();
        var klingerOutput = new KlingerOscillatorOutput<decimal>[DataSize];
        _fpKlinger.OnBarBatch(_klingerData, klingerOutput);
        
        int divergences = 0;
        int lookback = 20;
        
        for (int i = lookback + 55; i < DataSize; i++)
        {
            if (klingerOutput[i] != null && klingerOutput[i - lookback] != null)
            {
                // Price trend
                var priceTrend = _klingerData[i].Close - _klingerData[i - lookback].Close;
                
                // Klinger trend
                var klingerTrend = klingerOutput[i].KlingerOscillator - klingerOutput[i - lookback].KlingerOscillator;
                
                // Check for divergence
                if ((priceTrend > 0 && klingerTrend < 0) || (priceTrend < 0 && klingerTrend > 0))
                {
                    // Confirm significant divergence
                    if (Math.Abs(klingerTrend) > Math.Abs(klingerOutput[i].KlingerOscillator) * 0.1m)
                    {
                        divergences++;
                    }
                }
            }
        }
        
        return divergences;
    }

    [Benchmark]
    [BenchmarkCategory("KlingerOscillator", "VolumeCorrelation")]
    public decimal KlingerOscillator_VolumeCorrelation()
    {
        _fpKlinger.Clear();
        var klingerOutput = new KlingerOscillatorOutput<decimal>[DataSize];
        _fpKlinger.OnBarBatch(_klingerData, klingerOutput);
        
        // Calculate correlation between Klinger values and volume changes
        var klingerValues = new List<decimal>();
        var volumeChanges = new List<decimal>();
        
        for (int i = 56; i < DataSize; i++)
        {
            if (klingerOutput[i] != null && i > 0)
            {
                klingerValues.Add(Math.Abs(klingerOutput[i].KlingerOscillator));
                
                var volumeChange = Math.Abs(_klingerData[i].Volume - _klingerData[i - 1].Volume);
                volumeChanges.Add(volumeChange);
            }
        }
        
        if (klingerValues.Count < 2) return 0;
        
        // Simple correlation calculation
        decimal klingerMean = klingerValues.Average();
        decimal volumeMean = volumeChanges.Average();
        
        decimal covariance = 0;
        decimal klingerVariance = 0;
        decimal volumeVariance = 0;
        
        for (int i = 0; i < klingerValues.Count; i++)
        {
            var klingerDiff = klingerValues[i] - klingerMean;
            var volDiff = volumeChanges[i] - volumeMean;
            
            covariance += klingerDiff * volDiff;
            klingerVariance += klingerDiff * klingerDiff;
            volumeVariance += volDiff * volDiff;
        }
        
        if (klingerVariance == 0 || volumeVariance == 0) return 0;
        
        return covariance / (decimal)Math.Sqrt((double)(klingerVariance * volumeVariance));
    }

    [Benchmark]
    [BenchmarkCategory("KlingerOscillator", "PeriodComparison")]
    public (decimal fast_volatility, decimal standard_volatility, decimal slow_volatility) KlingerOscillator_ComparePeriods()
    {
        _fpKlingerFast.Clear();
        _fpKlinger.Clear();
        _fpKlingerSlow.Clear();
        
        var outputFast = new KlingerOscillatorOutput<decimal>[DataSize];
        var outputStandard = new KlingerOscillatorOutput<decimal>[DataSize];
        var outputSlow = new KlingerOscillatorOutput<decimal>[DataSize];
        
        _fpKlingerFast.OnBarBatch(_klingerData, outputFast);
        _fpKlinger.OnBarBatch(_klingerData, outputStandard);
        _fpKlingerSlow.OnBarBatch(_klingerData, outputSlow);
        
        // Calculate volatility for each setting
        decimal volFast = CalculateVolatility(outputFast, 34);
        decimal volStandard = CalculateVolatility(outputStandard, 55);
        decimal volSlow = CalculateVolatility(outputSlow, 89);
        
        return (volFast, volStandard, volSlow);
    }
    
    private decimal CalculateVolatility(KlingerOscillatorOutput<decimal>[] outputs, int startIndex)
    {
        var changes = new List<decimal>();
        
        for (int i = startIndex + 1; i < outputs.Length; i++)
        {
            if (outputs[i] != null && outputs[i - 1] != null)
            {
                var change = Math.Abs(outputs[i].KlingerOscillator - outputs[i - 1].KlingerOscillator);
                changes.Add(change);
            }
        }
        
        return changes.Count > 0 ? changes.Average() : 0;
    }
    
    [Benchmark]
    [BenchmarkCategory("KlingerOscillator", "Memory")]
    public long KlingerOscillator_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new KlingerOscillator_FP<IKline<decimal>, decimal>(_parameters);
            var output = new KlingerOscillatorOutput<decimal>[DataSize];
            indicator.OnBarBatch(_klingerData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _fpKlinger?.Clear();
        _fpKlingerFast?.Clear();
        _fpKlingerSlow?.Clear();
        _klingerData = null!;
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

public class KlingerOscillatorOutput<T>
{
    public T KlingerOscillator { get; set; }
    public T Signal { get; set; }
}