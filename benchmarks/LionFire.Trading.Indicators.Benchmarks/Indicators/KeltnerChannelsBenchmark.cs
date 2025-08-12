using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class KeltnerChannelsBenchmark : IndicatorBenchmarkBase
{
    private KeltnerChannels_QC<decimal, decimal> _qcKeltner = null!;
    private PKeltnerChannels<decimal, decimal> _parameters = null!;
    
    private KeltnerChannels_QC<decimal, decimal> _qcKeltner20 = null!;
    private KeltnerChannels_QC<decimal, decimal> _qcKeltnerWide = null!;
    private KeltnerChannels_QC<decimal, decimal> _qcKeltnerNarrow = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PKeltnerChannels<decimal, decimal> 
        { 
            Period = Period,
            Multiplier = 2.0m,
            AtrPeriod = Period
        };
        _qcKeltner = new KeltnerChannels_QC<decimal, decimal>(_parameters);
        
        // Standard settings
        _qcKeltner20 = new KeltnerChannels_QC<decimal, decimal>(new PKeltnerChannels<decimal, decimal> 
        { 
            Period = 20,
            Multiplier = 2.0m,
            AtrPeriod = 20
        });
        
        // Wide channels (more volatile)
        _qcKeltnerWide = new KeltnerChannels_QC<decimal, decimal>(new PKeltnerChannels<decimal, decimal> 
        { 
            Period = 20,
            Multiplier = 3.0m,
            AtrPeriod = 20
        });
        
        // Narrow channels (less volatile)
        _qcKeltnerNarrow = new KeltnerChannels_QC<decimal, decimal>(new PKeltnerChannels<decimal, decimal> 
        { 
            Period = 20,
            Multiplier = 1.5m,
            AtrPeriod = 20
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("KeltnerChannels", "QuantConnect")]
    public KeltnerChannelsOutput<decimal>[] KeltnerChannels_QuantConnect_Batch()
    {
        var output = new KeltnerChannelsOutput<decimal>[DataSize];
        
        _qcKeltner.Clear();
        _qcKeltner.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("KeltnerChannels", "QuantConnect")]
    public List<KeltnerChannelsOutput<decimal>?> KeltnerChannels_QuantConnect_Streaming()
    {
        var results = new List<KeltnerChannelsOutput<decimal>?>(DataSize);
        var output = new KeltnerChannelsOutput<decimal>[1];
        
        _qcKeltner.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcKeltner.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcKeltner.IsReady ? output[0] : null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("KeltnerChannels", "ChannelPosition")]
    public (int upper, int middle, int lower) KeltnerChannels_PricePosition()
    {
        _qcKeltner20.Clear();
        var output = new KeltnerChannelsOutput<decimal>[1];
        int nearUpper = 0;
        int nearMiddle = 0;
        int nearLower = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcKeltner20.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcKeltner20.IsReady && output[0] != null)
            {
                var close = HLCData[i].Close;
                var upper = output[0].Upper;
                var lower = output[0].Lower;
                var middle = output[0].Middle;
                
                var upperDist = Math.Abs(close - upper);
                var lowerDist = Math.Abs(close - lower);
                var middleDist = Math.Abs(close - middle);
                
                if (upperDist < lowerDist && upperDist < middleDist)
                    nearUpper++;
                else if (lowerDist < upperDist && lowerDist < middleDist)
                    nearLower++;
                else
                    nearMiddle++;
            }
        }
        
        return (nearUpper, nearMiddle, nearLower);
    }

    [Benchmark]
    [BenchmarkCategory("KeltnerChannels", "Breakouts")]
    public (int upper_breaks, int lower_breaks) KeltnerChannels_DetectBreakouts()
    {
        _qcKeltner20.Clear();
        var output = new KeltnerChannelsOutput<decimal>[1];
        KeltnerChannelsOutput<decimal>? previousOutput = null;
        decimal? previousClose = null;
        int upperBreaks = 0;
        int lowerBreaks = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcKeltner20.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcKeltner20.IsReady && output[0] != null && 
                previousOutput != null && previousClose.HasValue)
            {
                var currentClose = HLCData[i].Close;
                
                // Check for channel breaks
                if (previousClose.Value <= previousOutput.Upper && 
                    currentClose > output[0].Upper)
                {
                    upperBreaks++;
                }
                
                if (previousClose.Value >= previousOutput.Lower && 
                    currentClose < output[0].Lower)
                {
                    lowerBreaks++;
                }
                
                previousOutput = output[0];
                previousClose = currentClose;
            }
            else if (_qcKeltner20.IsReady && output[0] != null)
            {
                previousOutput = output[0];
                previousClose = HLCData[i].Close;
            }
        }
        
        return (upperBreaks, lowerBreaks);
    }

    [Benchmark]
    [BenchmarkCategory("KeltnerChannels", "WidthComparison")]
    public (decimal narrow, decimal standard, decimal wide) KeltnerChannels_CompareWidths()
    {
        _qcKeltnerNarrow.Clear();
        _qcKeltner20.Clear();
        _qcKeltnerWide.Clear();
        
        var narrowOutput = new KeltnerChannelsOutput<decimal>[DataSize];
        var standardOutput = new KeltnerChannelsOutput<decimal>[DataSize];
        var wideOutput = new KeltnerChannelsOutput<decimal>[DataSize];
        
        _qcKeltnerNarrow.OnBarBatch(HLCData, narrowOutput);
        _qcKeltner20.OnBarBatch(HLCData, standardOutput);
        _qcKeltnerWide.OnBarBatch(HLCData, wideOutput);
        
        // Calculate average channel width for each multiplier
        decimal avgNarrow = 0, avgStandard = 0, avgWide = 0;
        int count = 0;
        
        for (int i = 0; i < DataSize; i++)
        {
            if (narrowOutput[i] != null && standardOutput[i] != null && wideOutput[i] != null)
            {
                avgNarrow += narrowOutput[i].Upper - narrowOutput[i].Lower;
                avgStandard += standardOutput[i].Upper - standardOutput[i].Lower;
                avgWide += wideOutput[i].Upper - wideOutput[i].Lower;
                count++;
            }
        }
        
        return (
            count > 0 ? avgNarrow / count : 0,
            count > 0 ? avgStandard / count : 0,
            count > 0 ? avgWide / count : 0
        );
    }

    [Benchmark]
    [BenchmarkCategory("KeltnerChannels", "Squeeze")]
    public int KeltnerChannels_DetectSqueeze()
    {
        _qcKeltner20.Clear();
        var keltnerOutput = new KeltnerChannelsOutput<decimal>[DataSize];
        _qcKeltner20.OnBarBatch(HLCData, keltnerOutput);
        
        // Would compare with Bollinger Bands for squeeze detection
        // For now, detect when channel width is contracting
        int squeezes = 0;
        decimal? previousWidth = null;
        int contractionCount = 0;
        
        for (int i = 0; i < DataSize; i++)
        {
            if (keltnerOutput[i] != null)
            {
                var currentWidth = keltnerOutput[i].Upper - keltnerOutput[i].Lower;
                
                if (previousWidth.HasValue)
                {
                    if (currentWidth < previousWidth.Value)
                    {
                        contractionCount++;
                        if (contractionCount >= 5) // 5 consecutive contractions
                        {
                            squeezes++;
                            contractionCount = 0;
                        }
                    }
                    else
                    {
                        contractionCount = 0;
                    }
                }
                
                previousWidth = currentWidth;
            }
        }
        
        return squeezes;
    }

    [Benchmark]
    [BenchmarkCategory("KeltnerChannels", "TrendStrength")]
    public (int strong_trend, int weak_trend) KeltnerChannels_MeasureTrend()
    {
        _qcKeltner20.Clear();
        var output = new KeltnerChannelsOutput<decimal>[1];
        int strongTrend = 0;
        int weakTrend = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcKeltner20.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcKeltner20.IsReady && output[0] != null)
            {
                var close = HLCData[i].Close;
                var middle = output[0].Middle;
                var upper = output[0].Upper;
                var lower = output[0].Lower;
                
                // Strong trend when price is near upper or lower band
                var distFromMiddle = Math.Abs(close - middle);
                var channelHalfWidth = (upper - lower) / 2;
                
                if (distFromMiddle > channelHalfWidth * 0.8m)
                {
                    strongTrend++;
                }
                else if (distFromMiddle < channelHalfWidth * 0.2m)
                {
                    weakTrend++;
                }
            }
        }
        
        return (strongTrend, weakTrend);
    }
    
    [Benchmark]
    [BenchmarkCategory("KeltnerChannels", "Memory")]
    public long KeltnerChannels_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new KeltnerChannels_QC<decimal, decimal>(_parameters);
            var output = new KeltnerChannelsOutput<decimal>[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcKeltner?.Clear();
        _qcKeltner20?.Clear();
        _qcKeltnerWide?.Clear();
        _qcKeltnerNarrow?.Clear();
        base.GlobalCleanup();
    }
}

public class KeltnerChannelsOutput<T>
{
    public T Upper { get; set; }
    public T Middle { get; set; }
    public T Lower { get; set; }
}