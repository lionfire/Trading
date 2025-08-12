using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class IchimokuBenchmark : IndicatorBenchmarkBase
{
    private IchimokuCloud_QC<decimal, decimal> _qcIchimoku = null!;
    private PIchimokuCloud<decimal, decimal> _parameters = null!;
    
    private IchimokuCloud_QC<decimal, decimal> _qcIchimokuStandard = null!;
    private IchimokuCloud_QC<decimal, decimal> _qcIchimokuFast = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PIchimokuCloud<decimal, decimal> 
        { 
            TenkanPeriod = 9,
            KijunPeriod = 26,
            SenkouSpanBPeriod = 52,
            ChikouSpanPeriod = 26,
            SenkouSpanOffset = 26
        };
        _qcIchimoku = new IchimokuCloud_QC<decimal, decimal>(_parameters);
        
        _qcIchimokuStandard = new IchimokuCloud_QC<decimal, decimal>(new PIchimokuCloud<decimal, decimal> 
        { 
            TenkanPeriod = 9,
            KijunPeriod = 26,
            SenkouSpanBPeriod = 52,
            ChikouSpanPeriod = 26,
            SenkouSpanOffset = 26
        });
        
        _qcIchimokuFast = new IchimokuCloud_QC<decimal, decimal>(new PIchimokuCloud<decimal, decimal> 
        { 
            TenkanPeriod = 5,
            KijunPeriod = 13,
            SenkouSpanBPeriod = 26,
            ChikouSpanPeriod = 13,
            SenkouSpanOffset = 13
        });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Ichimoku", "QuantConnect")]
    public IchimokuCloudOutput<decimal>[] Ichimoku_QuantConnect_Batch()
    {
        var output = new IchimokuCloudOutput<decimal>[DataSize];
        
        _qcIchimoku.Clear();
        _qcIchimoku.OnBarBatch(HLCData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("Ichimoku", "QuantConnect")]
    public List<IchimokuCloudOutput<decimal>?> Ichimoku_QuantConnect_Streaming()
    {
        var results = new List<IchimokuCloudOutput<decimal>?>(DataSize);
        var output = new IchimokuCloudOutput<decimal>[1];
        
        _qcIchimoku.Clear();
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcIchimoku.OnBarBatch(new[] { HLCData[i] }, output);
            results.Add(_qcIchimoku.IsReady ? output[0] : null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("Ichimoku", "CloudPosition")]
    public (int above, int inside, int below) Ichimoku_CloudPosition()
    {
        _qcIchimokuStandard.Clear();
        var output = new IchimokuCloudOutput<decimal>[1];
        int aboveCloud = 0;
        int insideCloud = 0;
        int belowCloud = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcIchimokuStandard.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcIchimokuStandard.IsReady && output[0] != null)
            {
                var close = HLCData[i].Close;
                var spanA = output[0].SenkouSpanA;
                var spanB = output[0].SenkouSpanB;
                
                var cloudTop = Math.Max(spanA, spanB);
                var cloudBottom = Math.Min(spanA, spanB);
                
                if (close > cloudTop) aboveCloud++;
                else if (close < cloudBottom) belowCloud++;
                else insideCloud++;
            }
        }
        
        return (aboveCloud, insideCloud, belowCloud);
    }

    [Benchmark]
    [BenchmarkCategory("Ichimoku", "CrossSignals")]
    public (int tenkanKijun, int priceKumo) Ichimoku_DetectCrosses()
    {
        _qcIchimokuStandard.Clear();
        var output = new IchimokuCloudOutput<decimal>[1];
        IchimokuCloudOutput<decimal>? previousOutput = null;
        decimal? previousClose = null;
        int tenkanKijunCrosses = 0;
        int priceKumoCrosses = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcIchimokuStandard.OnBarBatch(new[] { HLCData[i] }, output);
            
            if (_qcIchimokuStandard.IsReady && output[0] != null && previousOutput != null && previousClose.HasValue)
            {
                var currentClose = HLCData[i].Close;
                
                // Tenkan-sen / Kijun-sen cross
                if ((previousOutput.TenkanSen < previousOutput.KijunSen && 
                     output[0].TenkanSen > output[0].KijunSen) ||
                    (previousOutput.TenkanSen > previousOutput.KijunSen && 
                     output[0].TenkanSen < output[0].KijunSen))
                {
                    tenkanKijunCrosses++;
                }
                
                // Price / Kumo cross
                var prevCloudTop = Math.Max(previousOutput.SenkouSpanA, previousOutput.SenkouSpanB);
                var prevCloudBottom = Math.Min(previousOutput.SenkouSpanA, previousOutput.SenkouSpanB);
                var cloudTop = Math.Max(output[0].SenkouSpanA, output[0].SenkouSpanB);
                var cloudBottom = Math.Min(output[0].SenkouSpanA, output[0].SenkouSpanB);
                
                if ((previousClose.Value <= prevCloudTop && currentClose > cloudTop) ||
                    (previousClose.Value >= prevCloudBottom && currentClose < cloudBottom))
                {
                    priceKumoCrosses++;
                }
                
                previousOutput = output[0];
                previousClose = currentClose;
            }
            else if (_qcIchimokuStandard.IsReady && output[0] != null)
            {
                previousOutput = output[0];
                previousClose = HLCData[i].Close;
            }
        }
        
        return (tenkanKijunCrosses, priceKumoCrosses);
    }

    [Benchmark]
    [BenchmarkCategory("Ichimoku", "CloudThickness")]
    public decimal Ichimoku_AverageCloudThickness()
    {
        _qcIchimokuStandard.Clear();
        var output = new IchimokuCloudOutput<decimal>[1];
        decimal totalThickness = 0;
        int count = 0;
        
        for (int i = 0; i < HLCData.Length; i++)
        {
            _qcIchimokuStandard.OnBarBatch(new[] { HLCData[i] }, output);
            if (_qcIchimokuStandard.IsReady && output[0] != null)
            {
                var thickness = Math.Abs(output[0].SenkouSpanA - output[0].SenkouSpanB);
                totalThickness += thickness;
                count++;
            }
        }
        
        return count > 0 ? totalThickness / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("Ichimoku", "StandardVsFast")]
    public (decimal standard, decimal fast) Ichimoku_CompareTimeframes()
    {
        _qcIchimokuStandard.Clear();
        _qcIchimokuFast.Clear();
        
        var standardOutput = new IchimokuCloudOutput<decimal>[DataSize];
        var fastOutput = new IchimokuCloudOutput<decimal>[DataSize];
        
        _qcIchimokuStandard.OnBarBatch(HLCData, standardOutput);
        _qcIchimokuFast.OnBarBatch(HLCData, fastOutput);
        
        // Return the last Tenkan-sen values for comparison
        var lastStandard = standardOutput.LastOrDefault(o => o != null)?.TenkanSen ?? 0;
        var lastFast = fastOutput.LastOrDefault(o => o != null)?.TenkanSen ?? 0;
        
        return (lastStandard, lastFast);
    }
    
    [Benchmark]
    [BenchmarkCategory("Ichimoku", "Memory")]
    public long Ichimoku_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new IchimokuCloud_QC<decimal, decimal>(_parameters);
            var output = new IchimokuCloudOutput<decimal>[DataSize];
            indicator.OnBarBatch(HLCData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcIchimoku?.Clear();
        _qcIchimokuStandard?.Clear();
        _qcIchimokuFast?.Clear();
        base.GlobalCleanup();
    }
}

public class IchimokuCloudOutput<T>
{
    public T TenkanSen { get; set; }
    public T KijunSen { get; set; }
    public T SenkouSpanA { get; set; }
    public T SenkouSpanB { get; set; }
    public T ChikouSpan { get; set; }
}