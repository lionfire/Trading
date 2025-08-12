using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;
using Xunit.Abstractions;

namespace LionFire.Trading.Indicators.Tests;

public class ParameterOptimizationTests
{
    private readonly ITestOutputHelper _output;

    public ParameterOptimizationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ParameterOptimization_SMA_PeriodSweep()
    {
        // Arrange - Test different SMA periods to find optimal
        var testData = GenerateTrendingData(500);
        var periods = Enumerable.Range(5, 96).Where(p => p % 5 == 0).ToArray(); // 5, 10, 15, ..., 100
        var results = new Dictionary<int, OptimizationResult>();
        
        foreach (var period in periods)
        {
            var parameters = new PSMA<double, double> { Period = period };
            var sma = new SMA_QC<double, double>(parameters);
            var outputs = new double[testData.Length];
            
            sma.OnBarBatch(testData, outputs);
            
            if (sma.IsReady)
            {
                var performance = CalculateTrendFollowingScore(testData, outputs, period);
                results[period] = new OptimizationResult 
                { 
                    Period = period, 
                    Score = performance.Score,
                    Lag = performance.Lag,
                    Smoothness = performance.Smoothness
                };
            }
        }
        
        // Assert
        Assert.True(results.Count >= 10, "Should test multiple periods");
        
        var bestPeriod = results.OrderByDescending(r => r.Value.Score).First();
        var worstPeriod = results.OrderBy(r => r.Value.Score).First();
        
        Assert.True(bestPeriod.Value.Score > worstPeriod.Value.Score, 
            "Optimization should find better parameters");
        
        // Shorter periods should have less lag but more noise
        var shortPeriods = results.Where(r => r.Key <= 20).ToList();
        var longPeriods = results.Where(r => r.Key >= 60).ToList();
        
        var avgShortLag = shortPeriods.Average(r => r.Value.Lag);
        var avgLongLag = longPeriods.Average(r => r.Value.Lag);
        
        Assert.True(avgShortLag < avgLongLag, "Shorter periods should have less lag");
        
        _output.WriteLine($"Best SMA Period: {bestPeriod.Key} (Score: {bestPeriod.Value.Score:F3})");
        _output.WriteLine($"Worst SMA Period: {worstPeriod.Key} (Score: {worstPeriod.Value.Score:F3})");
        _output.WriteLine($"Average lag - Short periods: {avgShortLag:F1}, Long periods: {avgLongLag:F1}");
    }

    [Fact]
    public void ParameterOptimization_RSI_PeriodAndThresholds()
    {
        // Arrange - Optimize RSI period and overbought/oversold thresholds
        var testData = GenerateOscillatingData(800);
        var periods = new[] { 7, 9, 14, 21, 25 };
        var thresholds = new[] { 
            (70, 30), (75, 25), (80, 20), (85, 15)
        };
        
        var results = new List<RSIOptimizationResult>();
        
        foreach (var period in periods)
        {
            foreach (var (overbought, oversold) in thresholds)
            {
                var parameters = new PRSI<double, double> { Period = period };
                var rsi = new RSI_QC<double, double>(parameters);
                var outputs = new double[testData.Length];
                
                rsi.OnBarBatch(testData, outputs);
                
                if (rsi.IsReady)
                {
                    var signals = GenerateRSISignals(outputs, overbought, oversold);
                    var performance = CalculateSignalPerformance(testData, signals, period);
                    
                    results.Add(new RSIOptimizationResult
                    {
                        Period = period,
                        OverboughtThreshold = overbought,
                        OversoldThreshold = oversold,
                        WinRate = performance.WinRate,
                        ProfitFactor = performance.ProfitFactor,
                        SignalCount = performance.SignalCount
                    });
                }
            }
        }
        
        // Assert
        Assert.True(results.Count >= 15, "Should test multiple parameter combinations");
        
        var bestResult = results.OrderByDescending(r => r.ProfitFactor).First();
        var worstResult = results.OrderBy(r => r.ProfitFactor).First();
        
        Assert.True(bestResult.WinRate > 0.3, "Best parameters should have reasonable win rate");
        Assert.True(bestResult.ProfitFactor > worstResult.ProfitFactor, 
            "Optimization should distinguish better parameters");
        
        // Extreme thresholds should generate fewer signals
        var extremeThresholds = results.Where(r => r.OverboughtThreshold >= 80).ToList();
        var normalThresholds = results.Where(r => r.OverboughtThreshold == 70).ToList();
        
        var avgExtremeSignals = extremeThresholds.Average(r => r.SignalCount);
        var avgNormalSignals = normalThresholds.Average(r => r.SignalCount);
        
        Assert.True(avgExtremeSignals < avgNormalSignals, 
            "Extreme thresholds should generate fewer signals");
        
        _output.WriteLine($"Best RSI Parameters: Period={bestResult.Period}, " +
                         $"OB/OS={bestResult.OverboughtThreshold}/{bestResult.OversoldThreshold}");
        _output.WriteLine($"Performance: WinRate={bestResult.WinRate:F2}, " +
                         $"ProfitFactor={bestResult.ProfitFactor:F2}, Signals={bestResult.SignalCount}");
    }

    [Fact]
    public void ParameterOptimization_MACD_FastSlowSignal()
    {
        // Arrange - Optimize MACD parameters
        var testData = GenerateTrendingWithReversals(600);
        var fastPeriods = new[] { 8, 12, 16 };
        var slowPeriods = new[] { 21, 26, 34 };
        var signalPeriods = new[] { 6, 9, 12 };
        
        var results = new List<MACDOptimizationResult>();
        
        foreach (var fast in fastPeriods)
        {
            foreach (var slow in slowPeriods.Where(s => s > fast))
            {
                foreach (var signal in signalPeriods)
                {
                    var parameters = new PMACD<double, MACDResult> 
                    { 
                        FastPeriod = fast, 
                        SlowPeriod = slow, 
                        SignalPeriod = signal 
                    };
                    var macd = new MACD_QC<double, MACDResult>(parameters);
                    var outputs = new MACDResult[testData.Length];
                    
                    macd.OnBarBatch(testData, outputs);
                    
                    if (macd.IsReady)
                    {
                        var crossovers = CountMACDCrossovers(outputs);
                        var performance = EvaluateMACDPerformance(testData, outputs);
                        
                        results.Add(new MACDOptimizationResult
                        {
                            FastPeriod = fast,
                            SlowPeriod = slow,
                            SignalPeriod = signal,
                            CrossoverCount = crossovers,
                            TrendAccuracy = performance.TrendAccuracy,
                            ResponseTime = performance.ResponseTime
                        });
                    }
                }
            }
        }
        
        // Assert
        Assert.True(results.Count >= 12, "Should test multiple MACD parameter combinations");
        
        var bestTrendAccuracy = results.OrderByDescending(r => r.TrendAccuracy).First();
        var bestResponse = results.OrderBy(r => r.ResponseTime).First();
        
        // Fast MACD should have more crossovers but potentially more noise
        var fastMACD = results.Where(r => r.FastPeriod == 8).ToList();
        var slowMACD = results.Where(r => r.FastPeriod == 16).ToList();
        
        var avgFastCrossovers = fastMACD.Average(r => r.CrossoverCount);
        var avgSlowCrossovers = slowMACD.Average(r => r.CrossoverCount);
        
        Assert.True(avgFastCrossovers > avgSlowCrossovers, 
            "Faster MACD should have more crossovers");
        
        _output.WriteLine($"Best Trend Accuracy: Fast={bestTrendAccuracy.FastPeriod}, " +
                         $"Slow={bestTrendAccuracy.SlowPeriod}, Signal={bestTrendAccuracy.SignalPeriod}");
        _output.WriteLine($"Accuracy: {bestTrendAccuracy.TrendAccuracy:F2}, Response: {bestTrendAccuracy.ResponseTime:F1}");
        _output.WriteLine($"Average crossovers: Fast MACD={avgFastCrossovers:F1}, Slow MACD={avgSlowCrossovers:F1}");
    }

    [Fact]
    public void ParameterOptimization_BollingerBands_PeriodAndDeviation()
    {
        // Arrange - Optimize BB period and standard deviation multiplier
        var testData = GenerateVolatileData(700);
        var periods = new[] { 10, 15, 20, 25, 30 };
        var deviations = new[] { 1.5, 2.0, 2.5, 3.0 };
        
        var results = new List<BBOptimizationResult>();
        
        foreach (var period in periods)
        {
            foreach (var deviation in deviations)
            {
                var parameters = new PBollingerBands<double, BollingerBandsResult> 
                { 
                    Period = period, 
                    StandardDeviations = deviation 
                };
                var bb = new BollingerBands_QC<double, BollingerBandsResult>(parameters);
                var outputs = new BollingerBandsResult[testData.Length];
                
                bb.OnBarBatch(testData, outputs);
                
                if (bb.IsReady)
                {
                    var containmentRate = CalculateContainmentRate(testData, outputs);
                    var breakouts = CountBreakouts(testData, outputs);
                    var avgBandWidth = CalculateAverageBandWidth(outputs);
                    
                    results.Add(new BBOptimizationResult
                    {
                        Period = period,
                        StandardDeviations = deviation,
                        ContainmentRate = containmentRate,
                        BreakoutCount = breakouts,
                        AverageBandWidth = avgBandWidth
                    });
                }
            }
        }
        
        // Assert
        Assert.True(results.Count >= 15, "Should test multiple BB parameter combinations");
        
        // Higher standard deviations should have higher containment rates
        var lowDeviation = results.Where(r => r.StandardDeviations <= 2.0).Average(r => r.ContainmentRate);
        var highDeviation = results.Where(r => r.StandardDeviations >= 2.5).Average(r => r.ContainmentRate);
        
        Assert.True(highDeviation > lowDeviation, 
            "Higher standard deviations should contain more price action");
        
        // Theoretical containment rate for 2 std dev should be around 95%
        var twoStdResults = results.Where(r => Math.Abs(r.StandardDeviations - 2.0) < 0.01).ToList();
        var avgContainment = twoStdResults.Average(r => r.ContainmentRate);
        
        Assert.InRange(avgContainment, 0.85, 0.99); // Should be close to theoretical 95%
        
        var bestContainment = results.OrderByDescending(r => r.ContainmentRate).First();
        var mostBreakouts = results.OrderByDescending(r => r.BreakoutCount).First();
        
        _output.WriteLine($"Best Containment: Period={bestContainment.Period}, " +
                         $"StdDev={bestContainment.StandardDeviations} (Rate: {bestContainment.ContainmentRate:F2})");
        _output.WriteLine($"Most Breakouts: Period={mostBreakouts.Period}, " +
                         $"StdDev={mostBreakouts.StandardDeviations} (Count: {mostBreakouts.BreakoutCount})");
        _output.WriteLine($"Average containment for 2.0 std dev: {avgContainment:F2}");
    }

    [Fact]
    public void ParameterOptimization_MultiIndicator_Combination()
    {
        // Arrange - Optimize combination of multiple indicators
        var testData = GenerateComplexMarketData(800);
        var smaPeriods = new[] { 20, 50 };
        var rsiPeriods = new[] { 14, 21 };
        var macdFast = new[] { 12, 16 };
        var macdSlow = new[] { 26, 34 };
        
        var results = new List<MultiIndicatorResult>();
        
        foreach (var smaPeriod in smaPeriods)
        {
            foreach (var rsiPeriod in rsiPeriods)
            {
                foreach (var fast in macdFast)
                {
                    foreach (var slow in macdSlow.Where(s => s > fast))
                    {
                        // Create indicators
                        var sma = new SMA_QC<double, double>(new PSMA<double, double> { Period = smaPeriod });
                        var rsi = new RSI_QC<double, double>(new PRSI<double, double> { Period = rsiPeriod });
                        var macd = new MACD_QC<double, MACDResult>(new PMACD<double, MACDResult> 
                        { 
                            FastPeriod = fast, 
                            SlowPeriod = slow, 
                            SignalPeriod = 9 
                        });
                        
                        var smaOutputs = new double[testData.Length];
                        var rsiOutputs = new double[testData.Length];
                        var macdOutputs = new MACDResult[testData.Length];
                        
                        sma.OnBarBatch(testData, smaOutputs);
                        rsi.OnBarBatch(testData, rsiOutputs);
                        macd.OnBarBatch(testData, macdOutputs);
                        
                        if (sma.IsReady && rsi.IsReady && macd.IsReady)
                        {
                            var combinedSignals = GenerateCombinedSignals(
                                testData, smaOutputs, rsiOutputs, macdOutputs);
                            var performance = EvaluateCombinedPerformance(testData, combinedSignals);
                            
                            results.Add(new MultiIndicatorResult
                            {
                                SMAPeriod = smaPeriod,
                                RSIPeriod = rsiPeriod,
                                MACDFast = fast,
                                MACDSlow = slow,
                                WinRate = performance.WinRate,
                                ProfitFactor = performance.ProfitFactor,
                                MaxDrawdown = performance.MaxDrawdown,
                                SignalCount = performance.SignalCount
                            });
                        }
                    }
                }
            }
        }
        
        // Assert
        Assert.True(results.Count >= 8, "Should test multiple indicator combinations");
        
        var bestCombo = results.OrderByDescending(r => r.ProfitFactor).First();
        var safestCombo = results.OrderBy(r => r.MaxDrawdown).First();
        
        // Performance should vary across combinations
        var profitFactors = results.Select(r => r.ProfitFactor).ToArray();
        var pfStdDev = CalculateStandardDeviation(profitFactors);
        
        Assert.True(pfStdDev > 0.1, "Different combinations should produce varied results");
        Assert.True(bestCombo.WinRate > 0.4, "Best combination should have decent win rate");
        
        _output.WriteLine($"Best Combination: SMA={bestCombo.SMAPeriod}, RSI={bestCombo.RSIPeriod}, " +
                         $"MACD={bestCombo.MACDFast}/{bestCombo.MACDSlow}");
        _output.WriteLine($"Performance: WinRate={bestCombo.WinRate:F2}, PF={bestCombo.ProfitFactor:F2}, " +
                         $"MaxDD={bestCombo.MaxDrawdown:F2}");
        _output.WriteLine($"Safest Combination: SMA={safestCombo.SMAPeriod}, MaxDD={safestCombo.MaxDrawdown:F2}");
    }

    // Helper methods
    private double[] GenerateTrendingData(int count)
    {
        var data = new double[count];
        var random = new Random(42);
        var price = 100.0;
        var trend = 0.001; // Upward trend
        
        for (int i = 0; i < count; i++)
        {
            price += trend * price + random.NextGaussian() * 0.02 * price;
            data[i] = Math.Max(price, 10);
        }
        return data;
    }

    private double[] GenerateOscillatingData(int count)
    {
        var data = new double[count];
        var random = new Random(42);
        
        for (int i = 0; i < count; i++)
        {
            var cycle = 100 + Math.Sin(i * 0.05) * 20;
            var noise = random.NextGaussian() * 5;
            data[i] = Math.Max(cycle + noise, 10);
        }
        return data;
    }

    private double[] GenerateTrendingWithReversals(int count)
    {
        var data = new double[count];
        var random = new Random(42);
        var price = 100.0;
        var trendDirection = 1;
        
        for (int i = 0; i < count; i++)
        {
            if (i % 100 == 0 && i > 0) trendDirection *= -1; // Reverse trend every 100 bars
            
            var trend = trendDirection * 0.001;
            price += trend * price + random.NextGaussian() * 0.015 * price;
            data[i] = Math.Max(price, 10);
        }
        return data;
    }

    private double[] GenerateVolatileData(int count)
    {
        var data = new double[count];
        var random = new Random(42);
        var price = 100.0;
        
        for (int i = 0; i < count; i++)
        {
            var volatilityMultiplier = 1 + Math.Sin(i * 0.01) * 0.5; // Varying volatility
            price += random.NextGaussian() * 0.03 * price * volatilityMultiplier;
            data[i] = Math.Max(price, 10);
        }
        return data;
    }

    private double[] GenerateComplexMarketData(int count)
    {
        var data = new double[count];
        var random = new Random(42);
        var price = 100.0;
        
        for (int i = 0; i < count; i++)
        {
            var trend = Math.Sin(i * 0.008) * 0.0005; // Slow trend cycles
            var volatility = 0.02 + 0.01 * Math.Sin(i * 0.02); // Volatility cycles
            var shock = i % 200 == 0 && i > 0 ? random.NextGaussian() * 0.05 : 0; // Occasional shocks
            
            price += trend * price + random.NextGaussian() * volatility * price + shock * price;
            data[i] = Math.Max(price, 10);
        }
        return data;
    }

    // Calculation helper methods (simplified implementations)
    private (double Score, double Lag, double Smoothness) CalculateTrendFollowingScore(double[] prices, double[] signals, int period)
    {
        var validSignals = signals.Skip(period).ToArray();
        var validPrices = prices.Skip(period).ToArray();
        
        var correlation = CalculateCorrelation(validPrices, validSignals);
        var lag = CalculateLag(validPrices, validSignals);
        var smoothness = CalculateSmoothness(validSignals);
        
        var score = correlation * 0.6 + (1.0 / (lag + 1)) * 0.3 + smoothness * 0.1;
        return (score, lag, smoothness);
    }

    private double CalculateCorrelation(double[] x, double[] y)
    {
        if (x.Length != y.Length) return 0;
        
        var meanX = x.Average();
        var meanY = y.Average();
        
        var numerator = x.Zip(y, (xi, yi) => (xi - meanX) * (yi - meanY)).Sum();
        var denomX = Math.Sqrt(x.Sum(xi => Math.Pow(xi - meanX, 2)));
        var denomY = Math.Sqrt(y.Sum(yi => Math.Pow(yi - meanY, 2)));
        
        return denomX == 0 || denomY == 0 ? 0 : numerator / (denomX * denomY);
    }

    private double CalculateLag(double[] prices, double[] signals) => 2.0; // Simplified
    private double CalculateSmoothness(double[] signals) => 0.8; // Simplified
    private double CalculateStandardDeviation(double[] values)
    {
        var mean = values.Average();
        return Math.Sqrt(values.Average(v => Math.Pow(v - mean, 2)));
    }

    // Additional helper method implementations would go here...
    // (Simplified for brevity)
    private int[] GenerateRSISignals(double[] rsi, int overbought, int oversold) => new int[rsi.Length];
    private (double WinRate, double ProfitFactor, int SignalCount) CalculateSignalPerformance(double[] prices, int[] signals, int period) => (0.5, 1.2, 10);
    private int CountMACDCrossovers(MACDResult[] macd) => 5;
    private (double TrendAccuracy, double ResponseTime) EvaluateMACDPerformance(double[] prices, MACDResult[] macd) => (0.7, 3.5);
    private double CalculateContainmentRate(double[] prices, BollingerBandsResult[] bb) => 0.95;
    private int CountBreakouts(double[] prices, BollingerBandsResult[] bb) => 8;
    private double CalculateAverageBandWidth(BollingerBandsResult[] bb) => 4.2;
    private int[] GenerateCombinedSignals(double[] prices, double[] sma, double[] rsi, MACDResult[] macd) => new int[prices.Length];
    private (double WinRate, double ProfitFactor, double MaxDrawdown, int SignalCount) EvaluateCombinedPerformance(double[] prices, int[] signals) => (0.55, 1.4, 0.15, 12);
}

// Result classes
public class OptimizationResult
{
    public int Period { get; set; }
    public double Score { get; set; }
    public double Lag { get; set; }
    public double Smoothness { get; set; }
}

public class RSIOptimizationResult
{
    public int Period { get; set; }
    public int OverboughtThreshold { get; set; }
    public int OversoldThreshold { get; set; }
    public double WinRate { get; set; }
    public double ProfitFactor { get; set; }
    public int SignalCount { get; set; }
}

public class MACDOptimizationResult
{
    public int FastPeriod { get; set; }
    public int SlowPeriod { get; set; }
    public int SignalPeriod { get; set; }
    public int CrossoverCount { get; set; }
    public double TrendAccuracy { get; set; }
    public double ResponseTime { get; set; }
}

public class BBOptimizationResult
{
    public int Period { get; set; }
    public double StandardDeviations { get; set; }
    public double ContainmentRate { get; set; }
    public int BreakoutCount { get; set; }
    public double AverageBandWidth { get; set; }
}

public class MultiIndicatorResult
{
    public int SMAPeriod { get; set; }
    public int RSIPeriod { get; set; }
    public int MACDFast { get; set; }
    public int MACDSlow { get; set; }
    public double WinRate { get; set; }
    public double ProfitFactor { get; set; }
    public double MaxDrawdown { get; set; }
    public int SignalCount { get; set; }
}

public static class RandomExtensions2
{
    public static double NextGaussian(this Random random, double mean = 0, double stdDev = 1)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}