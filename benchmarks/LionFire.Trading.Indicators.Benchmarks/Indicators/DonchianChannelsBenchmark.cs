using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class DonchianChannelsBenchmark : IndicatorBenchmarkBase
{
    private DonchianChannels_QC<decimal, decimal> _qcDonchian = null!;
    private PDonchianChannels<decimal, decimal> _parameters = null!;
    
    private DonchianChannels_QC<decimal, decimal> _qcDonchian20 = null!;
    private DonchianChannels_QC<decimal, decimal> _qcDonchian50 = null!;
    private DonchianChannels_QC<decimal, decimal> _qcDonchian100 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PDonchianChannels<decimal, decimal> { Period = Period };
        _qcDonchian = new DonchianChannels_QC<decimal, decimal>(_parameters);
        
        _qcDonchian20 = new DonchianChannels_QC<decimal, decimal>(new PDonchianChannels<decimal, decimal> { Period = 20 });
        _qcDonchian50 = new DonchianChannels_QC<decimal, decimal>(new PDonchianChannels<decimal, decimal> { Period = 50 });
        _qcDonchian100 = new DonchianChannels_QC<decimal, decimal>(new PDonchianChannels<decimal, decimal> { Period = 100 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("DonchianChannels", "QuantConnect")]
    public DonchianChannelsOutput<decimal>[] DonchianChannels_QuantConnect_Batch()
    {
        var output = new DonchianChannelsOutput<decimal>[DataSize];
        
        _qcDonchian.Clear();
        _qcDonchian.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("DonchianChannels", "QuantConnect")]
    public List<DonchianChannelsOutput<decimal>?> DonchianChannels_QuantConnect_Streaming()
    {
        var results = new List<DonchianChannelsOutput<decimal>?>(DataSize);
        var output = new DonchianChannelsOutput<decimal>[1];
        
        _qcDonchian.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcDonchian.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcDonchian.IsReady ? output[0] : null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("DonchianChannels", "ChannelPosition")]
    public (int upper, int middle, int lower) DonchianChannels_PricePosition()
    {
        _qcDonchian20.Clear();
        var output = new DonchianChannelsOutput<decimal>[1];
        int nearUpper = 0;
        int nearMiddle = 0;
        int nearLower = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcDonchian20.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcDonchian20.IsReady && output[0] != null)
            {
                var close = HLCData[i].Close;
                var upper = output[0].Upper;
                var lower = output[0].Lower;
                var middle = output[0].Middle;
                
                var rangeSize = upper - lower;
                if (rangeSize <= 0) continue;
                
                var position = (close - lower) / rangeSize;
                
                if (position > 0.7m) nearUpper++;
                else if (position < 0.3m) nearLower++;
                else nearMiddle++;
            }
        }
        
        return (nearUpper, nearMiddle, nearLower);
    }

    [Benchmark]
    [BenchmarkCategory("DonchianChannels", "Breakouts")]
    public (int upper_breakouts, int lower_breakouts) DonchianChannels_DetectBreakouts()
    {
        _qcDonchian20.Clear();
        var output = new DonchianChannelsOutput<decimal>[1];
        DonchianChannelsOutput<decimal>? previousOutput = null;
        decimal? previousClose = null;
        int upperBreakouts = 0;
        int lowerBreakouts = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcDonchian20.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcDonchian20.IsReady && output[0] != null && 
                previousOutput != null && previousClose.HasValue)
            {
                var currentClose = HLCData[i].Close;
                
                // Check for upper channel breakout
                if (previousClose.Value <= previousOutput.Upper && 
                    currentClose > output[0].Upper)
                {
                    upperBreakouts++;
                }
                
                // Check for lower channel breakout
                if (previousClose.Value >= previousOutput.Lower && 
                    currentClose < output[0].Lower)
                {
                    lowerBreakouts++;
                }
                
                previousOutput = output[0];
                previousClose = currentClose;
            }
            else if (_qcDonchian20.IsReady && output[0] != null)
            {
                previousOutput = output[0];
                previousClose = HLCData[i].Close;
            }
        }
        
        return (upperBreakouts, lowerBreakouts);
    }

    [Benchmark]
    [BenchmarkCategory("DonchianChannels", "ChannelWidth")]
    public decimal DonchianChannels_AverageWidth()
    {
        _qcDonchian20.Clear();
        var output = new DonchianChannelsOutput<decimal>[1];
        decimal totalWidth = 0;
        int count = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcDonchian20.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcDonchian20.IsReady && output[0] != null)
            {
                var width = output[0].Upper - output[0].Lower;
                totalWidth += width;
                count++;
            }
        }
        
        return count > 0 ? totalWidth / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("DonchianChannels", "PeriodComparison")]
    public (decimal short_width, decimal medium_width, decimal long_width) DonchianChannels_ComparePeriods()
    {
        _qcDonchian20.Clear();
        _qcDonchian50.Clear();
        _qcDonchian100.Clear();
        
        var output20 = new DonchianChannelsOutput<decimal>[DataSize];
        var output50 = new DonchianChannelsOutput<decimal>[DataSize];
        var output100 = new DonchianChannelsOutput<decimal>[DataSize];
        
        _qcDonchian20.OnBarBatch(HLCData, output20);
        _qcDonchian50.OnBarBatch(HLCData, output50);
        _qcDonchian100.OnBarBatch(HLCData, output100);
        
        // Calculate average channel width for each period
        decimal avgWidth20 = 0, avgWidth50 = 0, avgWidth100 = 0;
        int count20 = 0, count50 = 0, count100 = 0;
        
        for (int i = 0; i < DataSize; i++)
        {
            if (output20[i] != null)
            {
                avgWidth20 += output20[i].Upper - output20[i].Lower;
                count20++;
            }
            if (output50[i] != null)
            {
                avgWidth50 += output50[i].Upper - output50[i].Lower;
                count50++;
            }
            if (output100[i] != null)
            {
                avgWidth100 += output100[i].Upper - output100[i].Lower;
                count100++;
            }
        }
        
        return (
            count20 > 0 ? avgWidth20 / count20 : 0,
            count50 > 0 ? avgWidth50 / count50 : 0,
            count100 > 0 ? avgWidth100 / count100 : 0
        );
    }

    [Benchmark]
    [BenchmarkCategory("DonchianChannels", "Squeeze")]
    public int DonchianChannels_DetectSqueeze()
    {
        _qcDonchian20.Clear();
        var output = new DonchianChannelsOutput<decimal>[DataSize];
        _qcDonchian20.OnBarBatch(HLCData, output);
        
        int squeezes = 0;
        int lookback = 50;
        
        for (int i = lookback; i < DataSize; i++)
        {
            if (output[i] == null) continue;
            
            // Calculate average width over lookback period
            decimal avgWidth = 0;
            int count = 0;
            for (int j = i - lookback; j < i; j++)
            {
                if (output[j] != null)
                {
                    avgWidth += output[j].Upper - output[j].Lower;
                    count++;
                }
            }
            
            if (count > 0)
            {
                avgWidth /= count;
                var currentWidth = output[i].Upper - output[i].Lower;
                
                // Detect squeeze when current width is less than 70% of average
                if (currentWidth < avgWidth * 0.7m)
                {
                    squeezes++;
                }
            }
        }
        
        return squeezes;
    }
    
    [Benchmark]
    [BenchmarkCategory("DonchianChannels", "Memory")]
    public long DonchianChannels_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new DonchianChannels_QC<decimal, decimal>(_parameters);
            var output = new DonchianChannelsOutput<decimal>[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcDonchian?.Clear();
        _qcDonchian20?.Clear();
        _qcDonchian50?.Clear();
        _qcDonchian100?.Clear();
        base.GlobalCleanup();
    }
}

public class DonchianChannelsOutput<T>
{
    public T Upper { get; set; }
    public T Middle { get; set; }
    public T Lower { get; set; }
}