using BenchmarkDotNet.Attributes;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

/// <summary>
/// Benchmark suite focused on indicator parameter optimization scenarios
/// Tests performance across different parameter ranges and optimization strategies
/// </summary>
[GroupBenchmarksBy(BenchmarkDotNet.Configs.BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class OptimizationBenchmark : IndicatorBenchmarkBase
{
    private readonly List<EMA_FP<decimal, decimal>> _emaIndicators = new();
    private readonly List<RSI_FP<decimal, decimal>> _rsiIndicators = new();
    private readonly List<MACD_FP<decimal, decimal>> _macdIndicators = new();
    
    // Parameter ranges for optimization
    private readonly int[] _emaPeriods = { 5, 8, 12, 15, 20, 26, 30, 35, 50, 100, 200 };
    private readonly int[] _rsiPeriods = { 7, 9, 11, 14, 17, 21, 25, 30 };
    private readonly (int fast, int slow, int signal)[] _macdParameters = 
    {
        (6, 13, 5), (8, 17, 9), (12, 26, 9), (15, 30, 10), (20, 40, 15)
    };

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Pre-create indicators for parameter optimization testing
        foreach (var period in _emaPeriods)
        {
            _emaIndicators.Add(new EMA_FP<decimal, decimal>(new PEMA<decimal, decimal> { Period = period }));
        }
        
        foreach (var period in _rsiPeriods)
        {
            _rsiIndicators.Add(new RSI_FP<decimal, decimal>(new PRSI<decimal, decimal> { Period = period }));
        }
        
        foreach (var (fast, slow, signal) in _macdParameters)
        {
            _macdIndicators.Add(new MACD_FP<decimal, decimal>(new PMACD<decimal, decimal> 
            { 
                FastPeriod = fast, 
                SlowPeriod = slow, 
                SignalPeriod = signal 
            }));
        }
    }

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Optimization", "ParameterSweep")]
    public (int best_ema_period, decimal best_ema_performance) Optimization_EMAPeriodSweep()
    {
        var performances = new Dictionary<int, decimal>();
        
        for (int i = 0; i < _emaIndicators.Count; i++)
        {
            var ema = _emaIndicators[i];
            var period = _emaPeriods[i];
            
            ema.Clear();
            var output = new decimal[1];
            var returns = new List<decimal>();
            
            decimal? previousEma = null;
            
            for (int j = 0; j < CloseData.Length - 1; j++)
            {
                ema.OnBarBatch(new[] { CloseData[j] }, output);
                
                if (ema.IsReady && previousEma.HasValue)
                {
                    // Simple trend-following strategy
                    bool longSignal = output[0] > previousEma.Value;
                    decimal futureReturn = (CloseData[j + 1] - CloseData[j]) / CloseData[j];
                    
                    if (longSignal)
                        returns.Add(futureReturn);
                    else
                        returns.Add(-futureReturn); // Short signal
                }
                
                if (ema.IsReady)
                    previousEma = output[0];
            }
            
            performances[period] = returns.Count > 0 ? returns.Average() : 0;
        }
        
        var bestPeriod = performances.OrderByDescending(p => p.Value).First();
        return (bestPeriod.Key, bestPeriod.Value);
    }

    [Benchmark]
    [BenchmarkCategory("Optimization", "MultiParameterGrid")]
    public ((int fast, int slow, int signal), decimal performance) Optimization_MACDParameterGrid()
    {
        var performances = new Dictionary<(int, int, int), decimal>();
        
        for (int i = 0; i < _macdIndicators.Count; i++)
        {
            var macd = _macdIndicators[i];
            var parameters = _macdParameters[i];
            
            macd.Clear();
            var output = new MACDOutput<decimal>[1];
            var returns = new List<decimal>();
            
            for (int j = 0; j < CloseData.Length - 3; j++)
            {
                macd.OnBarBatch(new[] { CloseData[j] }, output);
                
                if (macd.IsReady)
                {
                    // MACD crossover strategy
                    bool longSignal = output[0].MACD > output[0].Signal;
                    decimal futureReturn = (CloseData[j + 3] - CloseData[j]) / CloseData[j];
                    
                    if (longSignal)
                        returns.Add(futureReturn);
                    else
                        returns.Add(-futureReturn);
                }
            }
            
            performances[parameters] = returns.Count > 0 ? returns.Average() : 0;
        }
        
        var bestParameters = performances.OrderByDescending(p => p.Value).First();
        return (bestParameters.Key, bestParameters.Value);
    }

    [Benchmark]
    [BenchmarkCategory("Optimization", "RiskAdjustedOptimization")]
    public (int best_rsi_period, decimal sharpe_ratio) Optimization_RSISharpeRatioOptimization()
    {
        var sharpeRatios = new Dictionary<int, decimal>();
        
        for (int i = 0; i < _rsiIndicators.Count; i++)
        {
            var rsi = _rsiIndicators[i];
            var period = _rsiPeriods[i];
            
            rsi.Clear();
            var output = new decimal[1];
            var returns = new List<decimal>();
            
            for (int j = 0; j < CloseData.Length - 2; j++)
            {
                rsi.OnBarBatch(new[] { CloseData[j] }, output);
                
                if (rsi.IsReady)
                {
                    // Mean reversion strategy
                    bool longSignal = output[0] < 30m;  // Oversold
                    bool shortSignal = output[0] > 70m; // Overbought
                    
                    if (longSignal || shortSignal)
                    {
                        decimal futureReturn = (CloseData[j + 2] - CloseData[j]) / CloseData[j];
                        
                        if (longSignal)
                            returns.Add(futureReturn);
                        else
                            returns.Add(-futureReturn);
                    }
                }
            }
            
            // Calculate Sharpe ratio (simplified: return/volatility)
            if (returns.Count > 1)
            {
                decimal meanReturn = returns.Average();
                decimal volatility = CalculateStandardDeviation(returns);
                sharpeRatios[period] = volatility > 0 ? meanReturn / volatility : 0;
            }
            else
            {
                sharpeRatios[period] = 0;
            }
        }
        
        var bestRsi = sharpeRatios.OrderByDescending(s => s.Value).First();
        return (bestRsi.Key, bestRsi.Value);
    }

    [Benchmark]
    [BenchmarkCategory("Optimization", "WalkForwardOptimization")]
    public (decimal in_sample_performance, decimal out_of_sample_performance, decimal degradation) Optimization_WalkForwardAnalysis()
    {
        int totalPeriods = CloseData.Length;
        int inSamplePeriods = totalPeriods * 60 / 100; // 60% in-sample
        int outOfSamplePeriods = totalPeriods - inSamplePeriods; // 40% out-of-sample
        
        // In-sample optimization (find best EMA period)
        var inSampleData = CloseData.Take(inSamplePeriods).ToArray();
        var bestInSamplePeriod = 0;
        var bestInSamplePerformance = decimal.MinValue;
        
        foreach (var period in _emaPeriods.Take(5)) // Test subset for performance
        {
            var ema = new EMA_FP<decimal, decimal>(new PEMA<decimal, decimal> { Period = period });
            var output = new decimal[1];
            var returns = new List<decimal>();
            decimal? previousEma = null;
            
            for (int i = 0; i < inSampleData.Length - 1; i++)
            {
                ema.OnBarBatch(new[] { inSampleData[i] }, output);
                
                if (ema.IsReady && previousEma.HasValue)
                {
                    bool longSignal = output[0] > previousEma.Value;
                    decimal futureReturn = (inSampleData[i + 1] - inSampleData[i]) / inSampleData[i];
                    
                    returns.Add(longSignal ? futureReturn : -futureReturn);
                }
                
                if (ema.IsReady)
                    previousEma = output[0];
            }
            
            decimal performance = returns.Count > 0 ? returns.Average() : 0;
            if (performance > bestInSamplePerformance)
            {
                bestInSamplePerformance = performance;
                bestInSamplePeriod = period;
            }
        }
        
        // Out-of-sample testing (use best period from in-sample)
        var outOfSampleData = CloseData.Skip(inSamplePeriods).ToArray();
        var emaOutOfSample = new EMA_FP<decimal, decimal>(new PEMA<decimal, decimal> { Period = bestInSamplePeriod });
        var outputOutOfSample = new decimal[1];
        var outOfSampleReturns = new List<decimal>();
        decimal? previousEmaOutOfSample = null;
        
        for (int i = 0; i < outOfSampleData.Length - 1; i++)
        {
            emaOutOfSample.OnBarBatch(new[] { outOfSampleData[i] }, outputOutOfSample);
            
            if (emaOutOfSample.IsReady && previousEmaOutOfSample.HasValue)
            {
                bool longSignal = outputOutOfSample[0] > previousEmaOutOfSample.Value;
                decimal futureReturn = (outOfSampleData[i + 1] - outOfSampleData[i]) / outOfSampleData[i];
                
                outOfSampleReturns.Add(longSignal ? futureReturn : -futureReturn);
            }
            
            if (emaOutOfSample.IsReady)
                previousEmaOutOfSample = outputOutOfSample[0];
        }
        
        decimal outOfSamplePerformance = outOfSampleReturns.Count > 0 ? outOfSampleReturns.Average() : 0;
        decimal degradation = bestInSamplePerformance - outOfSamplePerformance;
        
        return (bestInSamplePerformance, outOfSamplePerformance, degradation);
    }

    [Benchmark]
    [BenchmarkCategory("Optimization", "AdaptiveParameters")]
    public (decimal static_performance, decimal adaptive_performance, decimal improvement) Optimization_AdaptiveParameterSelection()
    {
        // Static strategy: use fixed EMA period
        var staticEma = new EMA_FP<decimal, decimal>(new PEMA<decimal, decimal> { Period = 20 });
        var staticOutput = new decimal[1];
        var staticReturns = new List<decimal>();
        decimal? previousStaticEma = null;
        
        // Adaptive strategy: change EMA period based on market volatility
        var adaptiveEmas = new Dictionary<int, EMA_FP<decimal, decimal>>();
        foreach (var period in new[] { 10, 20, 40 })
        {
            adaptiveEmas[period] = new EMA_FP<decimal, decimal>(new PEMA<decimal, decimal> { Period = period });
        }
        var adaptiveReturns = new List<decimal>();
        
        // Calculate volatility for adaptive selection
        var volatilityPeriod = 20;
        var priceChanges = new Queue<decimal>();
        
        for (int i = 0; i < CloseData.Length - 1; i++)
        {
            // Static strategy
            staticEma.OnBarBatch(new[] { CloseData[i] }, staticOutput);
            if (staticEma.IsReady && previousStaticEma.HasValue)
            {
                bool longSignal = staticOutput[0] > previousStaticEma.Value;
                decimal futureReturn = (CloseData[i + 1] - CloseData[i]) / CloseData[i];
                staticReturns.Add(longSignal ? futureReturn : -futureReturn);
            }
            if (staticEma.IsReady)
                previousStaticEma = staticOutput[0];
            
            // Calculate current volatility
            if (i > 0)
            {
                decimal priceChange = Math.Abs((CloseData[i] - CloseData[i - 1]) / CloseData[i - 1]);
                priceChanges.Enqueue(priceChange);
                if (priceChanges.Count > volatilityPeriod)
                    priceChanges.Dequeue();
            }
            
            // Adaptive strategy (select EMA period based on volatility)
            if (priceChanges.Count == volatilityPeriod && i > 50)
            {
                decimal volatility = priceChanges.Average();
                int selectedPeriod = volatility > 0.02m ? 10 : (volatility > 0.01m ? 20 : 40);
                
                var adaptiveEma = adaptiveEmas[selectedPeriod];
                var adaptiveOutput = new decimal[1];
                adaptiveEma.OnBarBatch(new[] { CloseData[i] }, adaptiveOutput);
                
                if (adaptiveEma.IsReady && i > 100)
                {
                    // Get previous value for signal generation
                    var prevOutput = new decimal[1];
                    adaptiveEma.OnBarBatch(new[] { CloseData[i - 1] }, prevOutput);
                    
                    if (adaptiveOutput[0] != prevOutput[0]) // Has changed
                    {
                        bool longSignal = adaptiveOutput[0] > prevOutput[0];
                        decimal futureReturn = (CloseData[i + 1] - CloseData[i]) / CloseData[i];
                        adaptiveReturns.Add(longSignal ? futureReturn : -futureReturn);
                    }
                }
            }
        }
        
        decimal staticPerformance = staticReturns.Count > 0 ? staticReturns.Average() : 0;
        decimal adaptivePerformance = adaptiveReturns.Count > 0 ? adaptiveReturns.Average() : 0;
        decimal improvement = adaptivePerformance - staticPerformance;
        
        return (staticPerformance, adaptivePerformance, improvement);
    }

    [Benchmark]
    [BenchmarkCategory("Optimization", "ParameterStability")]
    public (decimal stability_score, int stable_parameters, int unstable_parameters) Optimization_ParameterStabilityAnalysis()
    {
        var parameterStabilities = new Dictionary<int, List<decimal>>();
        int windowSize = CloseData.Length / 5; // 5 rolling windows
        
        // Test EMA stability across rolling windows
        for (int window = 0; window < 4; window++) // 4 overlapping windows
        {
            int startIndex = window * windowSize / 2;
            int endIndex = Math.Min(startIndex + windowSize, CloseData.Length - 1);
            var windowData = CloseData.Skip(startIndex).Take(endIndex - startIndex).ToArray();
            
            foreach (var period in _emaPeriods.Take(5)) // Test subset
            {
                var ema = new EMA_FP<decimal, decimal>(new PEMA<decimal, decimal> { Period = period });
                var output = new decimal[1];
                var returns = new List<decimal>();
                decimal? previousEma = null;
                
                for (int i = 0; i < windowData.Length - 1; i++)
                {
                    ema.OnBarBatch(new[] { windowData[i] }, output);
                    
                    if (ema.IsReady && previousEma.HasValue)
                    {
                        bool longSignal = output[0] > previousEma.Value;
                        decimal futureReturn = (windowData[i + 1] - windowData[i]) / windowData[i];
                        returns.Add(longSignal ? futureReturn : -futureReturn);
                    }
                    
                    if (ema.IsReady)
                        previousEma = output[0];
                }
                
                decimal performance = returns.Count > 0 ? returns.Average() : 0;
                
                if (!parameterStabilities.ContainsKey(period))
                    parameterStabilities[period] = new List<decimal>();
                parameterStabilities[period].Add(performance);
            }
        }
        
        // Calculate stability (lower variance = more stable)
        var stabilities = new Dictionary<int, decimal>();
        foreach (var param in parameterStabilities)
        {
            if (param.Value.Count > 1)
            {
                decimal variance = CalculateVariance(param.Value);
                stabilities[param.Key] = 1.0m / (1.0m + variance); // Stability score (higher is better)
            }
        }
        
        decimal averageStability = stabilities.Values.Count > 0 ? stabilities.Values.Average() : 0;
        int stableParameters = stabilities.Count(s => s.Value > 0.5m); // Arbitrary threshold
        int unstableParameters = stabilities.Count(s => s.Value <= 0.5m);
        
        return (averageStability, stableParameters, unstableParameters);
    }

    [Benchmark]
    [BenchmarkCategory("Optimization", "Memory")]
    public long Optimization_MemoryAllocation()
    {
        return MeasureAllocations(() =>
        {
            // Simulate parameter optimization memory usage
            var indicators = new List<EMA_FP<decimal, decimal>>();
            var outputs = new List<decimal[]>();
            
            foreach (var period in _emaPeriods)
            {
                indicators.Add(new EMA_FP<decimal, decimal>(new PEMA<decimal, decimal> { Period = period }));
                outputs.Add(new decimal[DataSize]);
            }
            
            // Run all indicators
            for (int i = 0; i < indicators.Count; i++)
            {
                indicators[i].OnBarBatch(CloseData, outputs[i]);
            }
        });
    }

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        foreach (var ema in _emaIndicators)
            ema?.Clear();
        
        foreach (var rsi in _rsiIndicators)
            rsi?.Clear();
            
        foreach (var macd in _macdIndicators)
            macd?.Clear();
        
        _emaIndicators.Clear();
        _rsiIndicators.Clear();
        _macdIndicators.Clear();
        
        base.GlobalCleanup();
    }

    private decimal CalculateStandardDeviation(List<decimal> values)
    {
        if (values.Count <= 1) return 0;
        
        decimal mean = values.Average();
        decimal sumSquaredDeviations = values.Sum(x => (x - mean) * (x - mean));
        return (decimal)Math.Sqrt((double)(sumSquaredDeviations / (values.Count - 1)));
    }

    private decimal CalculateVariance(List<decimal> values)
    {
        if (values.Count <= 1) return 0;
        
        decimal mean = values.Average();
        return values.Sum(x => (x - mean) * (x - mean)) / (values.Count - 1);
    }
}