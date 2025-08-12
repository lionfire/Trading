using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class FisherTransformBenchmark : IndicatorBenchmarkBase
{
    private FisherTransform_QC<decimal, decimal> _qcFisher = null!;
    private PFisherTransform<decimal, decimal> _parameters = null!;
    
    private FisherTransform_QC<decimal, decimal> _qcFisher5 = null!;
    private FisherTransform_QC<decimal, decimal> _qcFisher10 = null!;
    private FisherTransform_QC<decimal, decimal> _qcFisher20 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PFisherTransform<decimal, decimal> { Period = Period };
        _qcFisher = new FisherTransform_QC<decimal, decimal>(_parameters);
        
        _qcFisher5 = new FisherTransform_QC<decimal, decimal>(new PFisherTransform<decimal, decimal> { Period = 5 });
        _qcFisher10 = new FisherTransform_QC<decimal, decimal>(new PFisherTransform<decimal, decimal> { Period = 10 });
        _qcFisher20 = new FisherTransform_QC<decimal, decimal>(new PFisherTransform<decimal, decimal> { Period = 20 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("FisherTransform", "QuantConnect")]
    public FisherTransformOutput<decimal>[] FisherTransform_QuantConnect_Batch()
    {
        var output = new FisherTransformOutput<decimal>[DataSize];
        
        _qcFisher.Clear();
        _qcFisher.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("FisherTransform", "QuantConnect")]
    public List<FisherTransformOutput<decimal>?> FisherTransform_QuantConnect_Streaming()
    {
        var results = new List<FisherTransformOutput<decimal>?>(DataSize);
        var output = new FisherTransformOutput<decimal>[1];
        
        _qcFisher.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcFisher.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcFisher.IsReady ? output[0] : null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("FisherTransform", "TurningPoints")]
    public int FisherTransform_DetectTurningPoints()
    {
        _qcFisher10.Clear();
        var output = new FisherTransformOutput<decimal>[1];
        FisherTransformOutput<decimal>? previousOutput = null;
        int turningPoints = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcFisher10.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcFisher10.IsReady && output[0] != null && previousOutput != null)
            {
                // Detect turning points where Fisher changes direction significantly
                var currentFisher = output[0].Fisher;
                var previousFisher = previousOutput.Fisher;
                var currentTrigger = output[0].Trigger;
                var previousTrigger = previousOutput.Trigger;
                
                // Check for Fisher-Trigger crossovers (turning points)
                if ((previousFisher <= previousTrigger && currentFisher > currentTrigger) ||
                    (previousFisher >= previousTrigger && currentFisher < currentTrigger))
                {
                    turningPoints++;
                }
                
                previousOutput = output[0];
            }
            else if (_qcFisher10.IsReady && output[0] != null)
            {
                previousOutput = output[0];
            }
        }
        
        return turningPoints;
    }

    [Benchmark]
    [BenchmarkCategory("FisherTransform", "ExtremeLevels")]
    public (int extreme_high, int extreme_low) FisherTransform_DetectExtremes()
    {
        _qcFisher10.Clear();
        var output = new FisherTransformOutput<decimal>[1];
        int extremeHigh = 0;
        int extremeLow = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcFisher10.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcFisher10.IsReady && output[0] != null)
            {
                var fisher = output[0].Fisher;
                
                // Fisher Transform typically ranges from -3 to +3, with extremes at ±2
                if (fisher > 2.0m) extremeHigh++;
                else if (fisher < -2.0m) extremeLow++;
            }
        }
        
        return (extremeHigh, extremeLow);
    }

    [Benchmark]
    [BenchmarkCategory("FisherTransform", "ZeroCrossings")]
    public int FisherTransform_CountZeroCrossings()
    {
        _qcFisher10.Clear();
        var output = new FisherTransformOutput<decimal>[1];
        decimal? previousFisher = null;
        int crossings = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcFisher10.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcFisher10.IsReady && output[0] != null && previousFisher.HasValue)
            {
                var currentFisher = output[0].Fisher;
                
                if ((previousFisher.Value < 0 && currentFisher > 0) ||
                    (previousFisher.Value > 0 && currentFisher < 0))
                {
                    crossings++;
                }
                
                previousFisher = currentFisher;
            }
            else if (_qcFisher10.IsReady && output[0] != null)
            {
                previousFisher = output[0].Fisher;
            }
        }
        
        return crossings;
    }

    [Benchmark]
    [BenchmarkCategory("FisherTransform", "Oscillation")]
    public decimal FisherTransform_MeasureOscillation()
    {
        _qcFisher10.Clear();
        var output = new FisherTransformOutput<decimal>[DataSize];
        _qcFisher10.OnBarBatch(HLCData, output);
        
        var fisherValues = output.Where(o => o != null).Select(o => o.Fisher).ToList();
        if (fisherValues.Count < 2) return 0;
        
        // Calculate oscillation as average absolute difference between consecutive values
        decimal totalOscillation = 0;
        for (int i = 1; i < fisherValues.Count; i++)
        {
            totalOscillation += Math.Abs(fisherValues[i] - fisherValues[i - 1]);
        }
        
        return totalOscillation / (fisherValues.Count - 1);
    }

    [Benchmark]
    [BenchmarkCategory("FisherTransform", "PeriodSensitivity")]
    public (decimal fast_range, decimal medium_range, decimal slow_range) FisherTransform_CompareSensitivity()
    {
        _qcFisher5.Clear();
        _qcFisher10.Clear();
        _qcFisher20.Clear();
        
        var output5 = new FisherTransformOutput<decimal>[DataSize];
        var output10 = new FisherTransformOutput<decimal>[DataSize];
        var output20 = new FisherTransformOutput<decimal>[DataSize];
        
        _qcFisher5.OnBarBatch(HLCData, output5);
        _qcFisher10.OnBarBatch(HLCData, output10);
        _qcFisher20.OnBarBatch(HLCData, output20);
        
        // Calculate the range of Fisher values for each period
        var range5 = CalculateFisherRange(output5);
        var range10 = CalculateFisherRange(output10);
        var range20 = CalculateFisherRange(output20);
        
        return (range5, range10, range20);
    }
    
    private decimal CalculateFisherRange(FisherTransformOutput<decimal>[] outputs)
    {
        var values = outputs.Where(o => o != null).Select(o => o.Fisher).ToList();
        if (values.Count == 0) return 0;
        
        return values.Max() - values.Min();
    }
    
    [Benchmark]
    [BenchmarkCategory("FisherTransform", "Memory")]
    public long FisherTransform_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new FisherTransform_QC<decimal, decimal>(_parameters);
            var output = new FisherTransformOutput<decimal>[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcFisher?.Clear();
        _qcFisher5?.Clear();
        _qcFisher10?.Clear();
        _qcFisher20?.Clear();
        base.GlobalCleanup();
    }
}

public class FisherTransformOutput<T>
{
    public T Fisher { get; set; }
    public T Trigger { get; set; }
}