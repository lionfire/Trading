using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class ParabolicSarBenchmark : IndicatorBenchmarkBase
{
    private ParabolicSAR_QC<decimal, decimal> _qcSar = null!;
    private PParabolicSAR<decimal, decimal> _parameters = null!;
    
    private ParabolicSAR_QC<decimal, decimal> _qcSarStandard = null!;
    private ParabolicSAR_QC<decimal, decimal> _qcSarSensitive = null!;
    private ParabolicSAR_QC<decimal, decimal> _qcSarConservative = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PParabolicSAR<decimal, decimal> 
        { 
            AccelerationFactorStart = 0.02m,
            AccelerationFactorIncrement = 0.02m,
            AccelerationFactorMax = 0.2m
        };
        _qcSar = new ParabolicSAR_QC<decimal, decimal>(_parameters);
        
        // Standard settings
        _qcSarStandard = new ParabolicSAR_QC<decimal, decimal>(new PParabolicSAR<decimal, decimal> 
        { 
            AccelerationFactorStart = 0.02m,
            AccelerationFactorIncrement = 0.02m,
            AccelerationFactorMax = 0.2m
        });
        
        // Sensitive settings (faster reaction)
        _qcSarSensitive = new ParabolicSAR_QC<decimal, decimal>(new PParabolicSAR<decimal, decimal> 
        { 
            AccelerationFactorStart = 0.03m,
            AccelerationFactorIncrement = 0.03m,
            AccelerationFactorMax = 0.3m
        });
        
        // Conservative settings (slower reaction)
        _qcSarConservative = new ParabolicSAR_QC<decimal, decimal>(new PParabolicSAR<decimal, decimal> 
        { 
            AccelerationFactorStart = 0.01m,
            AccelerationFactorIncrement = 0.01m,
            AccelerationFactorMax = 0.1m
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("ParabolicSAR", "QuantConnect")]
    public decimal[] ParabolicSAR_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcSar.Clear();
        _qcSar.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("ParabolicSAR", "QuantConnect")]
    public List<decimal?> ParabolicSAR_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcSar.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcSar.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcSar.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("ParabolicSAR", "TrendDirection")]
    public (int bullish, int bearish) ParabolicSAR_TrendDirection()
    {
        _qcSarStandard.Clear();
        var output = new decimal[1];
        int bullishPeriods = 0;
        int bearishPeriods = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcSarStandard.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcSarStandard.IsReady)
            {
                var close = HLCData[i].Close;
                var sar = output[0];
                
                if (close > sar) bullishPeriods++;
                else bearishPeriods++;
            }
        }
        
        return (bullishPeriods, bearishPeriods);
    }

    [Benchmark]
    [BenchmarkCategory("ParabolicSAR", "TrendReversals")]
    public int ParabolicSAR_DetectReversals()
    {
        _qcSarStandard.Clear();
        var output = new decimal[1];
        decimal? previousSar = null;
        decimal? previousClose = null;
        int reversals = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcSarStandard.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcSarStandard.IsReady && previousSar.HasValue && previousClose.HasValue)
            {
                var currentClose = HLCData[i].Close;
                var currentSar = output[0];
                
                // Check for reversal: SAR flips from below to above price or vice versa
                bool wasAbove = previousSar.Value > previousClose.Value;
                bool isAbove = currentSar > currentClose;
                
                if (wasAbove != isAbove)
                {
                    reversals++;
                }
                
                previousSar = currentSar;
                previousClose = currentClose;
            }
            else if (_qcSarStandard.IsReady)
            {
                previousSar = output[0];
                previousClose = HLCData[i].Close;
            }
        }
        
        return reversals;
    }

    [Benchmark]
    [BenchmarkCategory("ParabolicSAR", "SensitivityComparison")]
    public (int standard, int sensitive, int conservative) ParabolicSAR_CompareSensitivity()
    {
        _qcSarStandard.Clear();
        _qcSarSensitive.Clear();
        _qcSarConservative.Clear();
        
        var standardOutput = new decimal[1];
        var sensitiveOutput = new decimal[1];
        var conservativeOutput = new decimal[1];
        
        decimal? prevStandardSar = null;
        decimal? prevSensitiveSar = null;
        decimal? prevConservativeSar = null;
        decimal? prevClose = null;
        
        int standardReversals = 0;
        int sensitiveReversals = 0;
        int conservativeReversals = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcSarStandard.OnBarBatch(new[] { HLCData[i] }, standardOutput);
            _qcSarSensitive.OnBarBatch(new[] { HLCData[i] }, sensitiveOutput);
            _qcSarConservative.OnBarBatch(new[] { HLCData[i] }, conservativeOutput);
            
            var currentClose = HLCData[i].Close;
            
            if (_qcSarStandard.IsReady && prevStandardSar.HasValue && prevClose.HasValue)
            {
                bool wasAbove = prevStandardSar.Value > prevClose.Value;
                bool isAbove = standardOutput[0] > currentClose;
                if (wasAbove != isAbove) standardReversals++;
            }
            
            if (_qcSarSensitive.IsReady && prevSensitiveSar.HasValue && prevClose.HasValue)
            {
                bool wasAbove = prevSensitiveSar.Value > prevClose.Value;
                bool isAbove = sensitiveOutput[0] > currentClose;
                if (wasAbove != isAbove) sensitiveReversals++;
            }
            
            if (_qcSarConservative.IsReady && prevConservativeSar.HasValue && prevClose.HasValue)
            {
                bool wasAbove = prevConservativeSar.Value > prevClose.Value;
                bool isAbove = conservativeOutput[0] > currentClose;
                if (wasAbove != isAbove) conservativeReversals++;
            }
            
            if (_qcSarStandard.IsReady) prevStandardSar = standardOutput[0];
            if (_qcSarSensitive.IsReady) prevSensitiveSar = sensitiveOutput[0];
            if (_qcSarConservative.IsReady) prevConservativeSar = conservativeOutput[0];
            prevClose = currentClose;
        }
        
        return (standardReversals, sensitiveReversals, conservativeReversals);
    }

    [Benchmark]
    [BenchmarkCategory("ParabolicSAR", "StopLossDistance")]
    public decimal ParabolicSAR_AverageStopDistance()
    {
        _qcSarStandard.Clear();
        var output = new decimal[1];
        decimal totalDistance = 0;
        int count = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcSarStandard.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcSarStandard.IsReady)
            {
                var close = HLCData[i].Close;
                var sar = output[0];
                var distance = Math.Abs(close - sar);
                totalDistance += distance;
                count++;
            }
        }
        
        return count > 0 ? totalDistance / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("ParabolicSAR", "AccelerationFactor")]
    public decimal ParabolicSAR_TrackAcceleration()
    {
        _qcSarStandard.Clear();
        var output = new decimal[DataSize];
        
        _qcSarStandard.OnBarBatch(HLCData, output);
        
        // Return the last SAR value as a proxy for acceleration tracking
        return output.LastOrDefault(v => v != default);
    }
    
    [Benchmark]
    [BenchmarkCategory("ParabolicSAR", "Memory")]
    public long ParabolicSAR_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new ParabolicSAR_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcSar?.Clear();
        _qcSarStandard?.Clear();
        _qcSarSensitive?.Clear();
        _qcSarConservative?.Clear();
        base.GlobalCleanup();
    }
}