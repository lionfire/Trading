using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class TemaBenchmark : IndicatorBenchmarkBase
{
    private TEMA_QC<decimal, decimal> _qcTema = null!;
    private PTEMA<decimal, decimal> _parameters = null!;
    
    private TEMA_QC<decimal, decimal> _qcTema9 = null!;
    private TEMA_QC<decimal, decimal> _qcTema21 = null!;
    private TEMA_QC<decimal, decimal> _qcTema50 = null!;

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        _parameters = new PTEMA<decimal, decimal> { Period = Period };
        _qcTema = new TEMA_QC<decimal, decimal>(_parameters);
        
        _qcTema9 = new TEMA_QC<decimal, decimal>(new PTEMA<decimal, decimal> { Period = 9 });
        _qcTema21 = new TEMA_QC<decimal, decimal>(new PTEMA<decimal, decimal> { Period = 21 });
        _qcTema50 = new TEMA_QC<decimal, decimal>(new PTEMA<decimal, decimal> { Period = 50 });
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TEMA", "QuantConnect")]
    public decimal[] TEMA_QuantConnect_Batch()
    {
        var output = new decimal[DataSize];
        
        _qcTema.Clear();
        _qcTema.OnBarBatch(PriceData, output);
        
        return output;
    }
    
    [Benchmark]
    [BenchmarkCategory("TEMA", "QuantConnect")]
    public List<decimal?> TEMA_QuantConnect_Streaming()
    {
        var results = new List<decimal?>(DataSize);
        var output = new decimal[1];
        
        _qcTema.Clear();
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcTema.OnBarBatch(new[] { PriceData[i] }, output);
            results.Add(_qcTema.IsReady ? output[0] : (decimal?)null);
        }
        
        return results;
    }

    [Benchmark]
    [BenchmarkCategory("TEMA", "Responsiveness")]
    public decimal TEMA_MeasureResponsiveness()
    {
        _qcTema21.Clear();
        var temaOutput = new decimal[DataSize];
        _qcTema21.OnBarBatch(PriceData, temaOutput);
        
        // Compare TEMA response to price changes vs EMA
        decimal totalResponseRatio = 0;
        int count = 0;
        
        for (int i = 63; i < DataSize - 1; i++)
        {
            if (temaOutput[i] != default && temaOutput[i + 1] != default)
            {
                var priceChange = Math.Abs(PriceData[i + 1] - PriceData[i]);
                var temaChange = Math.Abs(temaOutput[i + 1] - temaOutput[i]);
                
                // Calculate simple EMA for comparison
                decimal emaValue = PriceData[i];
                for (int j = Math.Max(0, i - 20); j < i; j++)
                {
                    decimal alpha = 2m / 22m; // 21-period EMA
                    emaValue = alpha * PriceData[j] + (1 - alpha) * emaValue;
                }
                
                decimal nextEmaValue = 2m / 22m * PriceData[i + 1] + (1 - 2m / 22m) * emaValue;
                var emaChange = Math.Abs(nextEmaValue - emaValue);
                
                if (priceChange > 0 && emaChange > 0)
                {
                    var responseRatio = (temaChange / priceChange) / (emaChange / priceChange);
                    totalResponseRatio += responseRatio;
                    count++;
                }
            }
        }
        
        return count > 0 ? totalResponseRatio / count : 0;
    }

    [Benchmark]
    [BenchmarkCategory("TEMA", "LaggingComparison")]
    public (decimal tema_lag, decimal sma_lag, decimal ema_lag) TEMA_CompareLag()
    {
        _qcTema21.Clear();
        var output = new decimal[DataSize];
        _qcTema21.OnBarBatch(PriceData, output);
        
        decimal totalTemaLag = 0;
        decimal totalSmaLag = 0;
        decimal totalEmaLag = 0;
        int count = 0;
        
        for (int i = 63; i < DataSize; i++)
        {
            if (output[i] != default)
            {
                var currentPrice = PriceData[i];
                
                // TEMA lag
                var temaLag = Math.Abs(currentPrice - output[i]);
                totalTemaLag += temaLag;
                
                // SMA comparison
                if (i >= 20)
                {
                    var sma = PriceData.Skip(i - 20).Take(21).Average();
                    var smaLag = Math.Abs(currentPrice - sma);
                    totalSmaLag += smaLag;
                }
                
                // EMA comparison
                decimal ema = PriceData[Math.Max(0, i - 20)];
                for (int j = Math.Max(0, i - 20); j <= i; j++)
                {
                    decimal alpha = 2m / 22m;
                    ema = alpha * PriceData[j] + (1 - alpha) * ema;
                }
                var emaLag = Math.Abs(currentPrice - ema);
                totalEmaLag += emaLag;
                
                count++;
            }
        }
        
        return (
            count > 0 ? totalTemaLag / count : 0,
            count > 0 ? totalSmaLag / count : 0,
            count > 0 ? totalEmaLag / count : 0
        );
    }

    [Benchmark]
    [BenchmarkCategory("TEMA", "SmoothnessMeasure")]
    public decimal TEMA_MeasureSmoothness()
    {
        _qcTema21.Clear();
        var output = new decimal[DataSize];
        _qcTema21.OnBarBatch(PriceData, output);
        
        // Calculate smoothness as inverse of average change
        var changes = new List<decimal>();
        
        for (int i = 64; i < DataSize; i++)
        {
            if (output[i] != default && output[i - 1] != default && output[i - 1] != 0)
            {
                var change = Math.Abs((output[i] - output[i - 1]) / output[i - 1]);
                changes.Add(change);
            }
        }
        
        if (changes.Count == 0) return 0;
        
        var avgChange = changes.Average();
        return avgChange > 0 ? 1 / avgChange : 1000; // Higher = smoother
    }

    [Benchmark]
    [BenchmarkCategory("TEMA", "TrendFollowing")]
    public (int uptrend, int downtrend, int sideways) TEMA_TrendClassification()
    {
        _qcTema21.Clear();
        var output = new decimal[1];
        decimal? previousTema = null;
        int uptrendCount = 0;
        int downtrendCount = 0;
        int sidewaysCount = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcTema21.OnBarBatch(new[] { PriceData[i] }, output);
            
            if (_qcTema21.IsReady && previousTema.HasValue)
            {
                var change = output[0] - previousTema.Value;
                var changePercent = Math.Abs(change / previousTema.Value);
                
                if (changePercent < 0.0002m) sidewaysCount++; // Very tight threshold due to TEMA smoothness
                else if (change > 0) uptrendCount++;
                else downtrendCount++;
            }
            
            previousTema = _qcTema21.IsReady ? output[0] : (decimal?)null;
        }
        
        return (uptrendCount, downtrendCount, sidewaysCount);
    }

    [Benchmark]
    [BenchmarkCategory("TEMA", "PeriodComparison")]
    public (decimal fast_volatility, decimal medium_volatility, decimal slow_volatility) TEMA_ComparePeriods()
    {
        _qcTema9.Clear();
        _qcTema21.Clear();
        _qcTema50.Clear();
        
        var output9 = new decimal[DataSize];
        var output21 = new decimal[DataSize];
        var output50 = new decimal[DataSize];
        
        _qcTema9.OnBarBatch(PriceData, output9);
        _qcTema21.OnBarBatch(PriceData, output21);
        _qcTema50.OnBarBatch(PriceData, output50);
        
        // Calculate volatility for each period
        decimal vol9 = CalculateVolatility(output9, 27);  // 3*9 periods for TEMA readiness
        decimal vol21 = CalculateVolatility(output21, 63); // 3*21 periods
        decimal vol50 = CalculateVolatility(output50, 150); // 3*50 periods
        
        return (vol9, vol21, vol50);
    }
    
    private decimal CalculateVolatility(decimal[] values, int startIndex)
    {
        var changes = new List<decimal>();
        
        for (int i = startIndex + 1; i < values.Length; i++)
        {
            if (values[i] != default && values[i - 1] != default && values[i - 1] != 0)
            {
                var change = Math.Abs((values[i] - values[i - 1]) / values[i - 1]);
                changes.Add(change);
            }
        }
        
        return changes.Count > 0 ? changes.Average() : 0;
    }

    [Benchmark]
    [BenchmarkCategory("TEMA", "CrossoverSignals")]
    public int TEMA_DetectCrossovers()
    {
        _qcTema21.Clear();
        var output = new decimal[1];
        int crossovers = 0;
        
        for (int i = 0; i < PriceData.Length; i++)
        {
            _qcTema21.OnBarBatch(new[] { PriceData[i] }, output);
            
            if (_qcTema21.IsReady && i > 0)
            {
                var currentPrice = PriceData[i];
                var previousPrice = PriceData[i - 1];
                var tema = output[0];
                
                // Simple crossover detection (price vs TEMA)
                if ((previousPrice <= tema && currentPrice > tema) ||
                    (previousPrice >= tema && currentPrice < tema))
                {
                    crossovers++;
                }
            }
        }
        
        return crossovers;
    }
    
    [Benchmark]
    [BenchmarkCategory("TEMA", "Memory")]
    public long TEMA_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new TEMA_QC<decimal, decimal>(_parameters);
            var output = new decimal[DataSize];
            indicator.OnBarBatch(PriceData, output);
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _qcTema?.Clear();
        _qcTema9?.Clear();
        _qcTema21?.Clear();
        _qcTema50?.Clear();
        base.GlobalCleanup();
    }
}