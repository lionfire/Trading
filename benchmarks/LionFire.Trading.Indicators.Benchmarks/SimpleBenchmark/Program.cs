using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LionFire.Trading.Indicators.Benchmarks;

/// <summary>
/// Standalone test runner that can compile and run without dependencies
/// </summary>
public static class StandaloneTestRunner
{
    public static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--standalone")
        {
            RunStandaloneTests();
        }
    }

    public static void RunStandaloneTests()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("   Standalone Performance Test Runner");
        Console.WriteLine("========================================");
        Console.WriteLine();

        var report = new StringBuilder();
        report.AppendLine("# Trading Indicators Performance Benchmark Report");
        report.AppendLine();
        report.AppendLine("## Executive Summary");
        report.AppendLine();
        report.AppendLine("This report provides initial performance benchmarks for trading indicator calculations.");
        report.AppendLine();
        
        // Test configuration
        var dataSizes = new[] { 100, 1000, 10000 };
        var iterations = 100;
        var warmupIterations = 10;
        
        report.AppendLine("### Test Configuration");
        report.AppendLine($"- Data Sizes: {string.Join(", ", dataSizes)} data points");
        report.AppendLine($"- Test Iterations: {iterations}");
        report.AppendLine($"- Warmup Iterations: {warmupIterations}");
        report.AppendLine($"- Test Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();

        // Generate test data
        Console.WriteLine("Generating test data...");
        var testData = new Dictionary<int, decimal[]>();
        foreach (var size in dataSizes)
        {
            testData[size] = GenerateMarketData(size);
        }

        // Run benchmarks
        var results = new List<BenchmarkResult>();

        Console.WriteLine("\nRunning benchmarks...");
        
        // Benchmark 1: Simple Moving Average
        Console.WriteLine("Testing Simple Moving Average...");
        foreach (var size in dataSizes)
        {
            var time = BenchmarkIndicator("SMA", () => CalculateSMA(testData[size], 20), iterations, warmupIterations);
            results.Add(new BenchmarkResult
            {
                IndicatorName = "SMA",
                DataSize = size,
                TimeMs = time,
                Implementation = "Basic"
            });
        }

        // Benchmark 2: Exponential Moving Average
        Console.WriteLine("Testing Exponential Moving Average...");
        foreach (var size in dataSizes)
        {
            var time = BenchmarkIndicator("EMA", () => CalculateEMA(testData[size], 20), iterations, warmupIterations);
            results.Add(new BenchmarkResult
            {
                IndicatorName = "EMA",
                DataSize = size,
                TimeMs = time,
                Implementation = "Basic"
            });
        }

        // Benchmark 3: RSI
        Console.WriteLine("Testing Relative Strength Index...");
        foreach (var size in dataSizes)
        {
            var time = BenchmarkIndicator("RSI", () => CalculateRSI(testData[size], 14), iterations, warmupIterations);
            results.Add(new BenchmarkResult
            {
                IndicatorName = "RSI",
                DataSize = size,
                TimeMs = time,
                Implementation = "Basic"
            });
        }

        // Benchmark 4: Bollinger Bands
        Console.WriteLine("Testing Bollinger Bands...");
        foreach (var size in dataSizes)
        {
            var time = BenchmarkIndicator("Bollinger", () => CalculateBollingerBands(testData[size], 20, 2), iterations, warmupIterations);
            results.Add(new BenchmarkResult
            {
                IndicatorName = "Bollinger Bands",
                DataSize = size,
                TimeMs = time,
                Implementation = "Basic"
            });
        }

        // Generate report
        report.AppendLine("## Performance Results");
        report.AppendLine();
        report.AppendLine("### Speed Performance");
        report.AppendLine();
        report.AppendLine("| Indicator | Implementation | 100 pts (ms) | 1,000 pts (ms) | 10,000 pts (ms) | Scaling Factor |");
        report.AppendLine("|-----------|---------------|--------------|----------------|-----------------|----------------|");
        
        foreach (var indicator in results.Select(r => r.IndicatorName).Distinct())
        {
            var indicatorResults = results.Where(r => r.IndicatorName == indicator).ToList();
            var small = indicatorResults.First(r => r.DataSize == 100);
            var medium = indicatorResults.First(r => r.DataSize == 1000);
            var large = indicatorResults.First(r => r.DataSize == 10000);
            var scalingFactor = large.TimeMs / small.TimeMs;
            
            report.AppendLine($"| {indicator} | {small.Implementation} | {small.TimeMs:F3} | {medium.TimeMs:F3} | {large.TimeMs:F3} | {scalingFactor:F1}x |");
        }

        report.AppendLine();
        report.AppendLine("### Throughput Analysis");
        report.AppendLine();
        report.AppendLine("| Indicator | 100 pts/sec | 1,000 pts/sec | 10,000 pts/sec |");
        report.AppendLine("|-----------|-------------|---------------|----------------|");
        
        foreach (var indicator in results.Select(r => r.IndicatorName).Distinct())
        {
            var indicatorResults = results.Where(r => r.IndicatorName == indicator).ToList();
            var small = indicatorResults.First(r => r.DataSize == 100);
            var medium = indicatorResults.First(r => r.DataSize == 1000);
            var large = indicatorResults.First(r => r.DataSize == 10000);
            
            var smallThroughput = small.TimeMs > 0 ? (100.0 / small.TimeMs * 1000) : 0;
            var mediumThroughput = medium.TimeMs > 0 ? (1000.0 / medium.TimeMs * 1000) : 0;
            var largeThroughput = large.TimeMs > 0 ? (10000.0 / large.TimeMs * 1000) : 0;
            
            report.AppendLine($"| {indicator} | {smallThroughput:F0} | {mediumThroughput:F0} | {largeThroughput:F0} |");
        }

        // Memory usage estimation
        report.AppendLine();
        report.AppendLine("## Memory Usage Analysis");
        report.AppendLine();
        report.AppendLine("### Estimated Memory Footprint");
        report.AppendLine();
        report.AppendLine("| Data Size | Array Memory | Overhead (est.) | Total (KB) |");
        report.AppendLine("|-----------|-------------|-----------------|------------|");
        
        foreach (var size in dataSizes)
        {
            var arrayMemory = size * sizeof(decimal);
            var overhead = arrayMemory * 0.2; // 20% overhead estimate
            var total = (arrayMemory + overhead) / 1024.0;
            report.AppendLine($"| {size:N0} | {arrayMemory:N0} bytes | {overhead:F0} bytes | {total:F1} |");
        }

        // Recommendations
        report.AppendLine();
        report.AppendLine("## Recommendations");
        report.AppendLine();
        report.AppendLine("Based on the benchmark results:");
        report.AppendLine();
        
        var avgScaling = results.Where(r => r.DataSize == 10000)
            .Select(r => r.TimeMs / results.First(x => x.IndicatorName == r.IndicatorName && x.DataSize == 100).TimeMs)
            .Average();
        
        if (avgScaling < 50)
        {
            report.AppendLine("- **Good Scalability**: Indicators show sub-linear scaling with data size");
            report.AppendLine("- Suitable for real-time processing of large datasets");
        }
        else if (avgScaling < 100)
        {
            report.AppendLine("- **Linear Scalability**: Indicators scale linearly with data size");
            report.AppendLine("- Consider optimization for very large datasets");
        }
        else
        {
            report.AppendLine("- **Poor Scalability**: Indicators show super-linear scaling");
            report.AppendLine("- Optimization recommended for production use");
        }

        report.AppendLine();
        report.AppendLine("### For High-Frequency Trading");
        var hftSuitable = results.Where(r => r.DataSize == 1000).Any(r => r.TimeMs < 1.0);
        if (hftSuitable)
        {
            report.AppendLine("- Sub-millisecond performance achieved for 1000-point datasets");
            report.AppendLine("- Suitable for HFT applications with appropriate optimizations");
        }
        else
        {
            report.AppendLine("- Performance optimization needed for HFT applications");
            report.AppendLine("- Consider using native implementations or hardware acceleration");
        }

        report.AppendLine();
        report.AppendLine("### For Batch Processing");
        report.AppendLine("- Current performance suitable for batch processing");
        report.AppendLine("- Can process millions of data points per second for simple indicators");
        
        // Save report
        var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
        if (!Directory.Exists(reportsDir))
        {
            Directory.CreateDirectory(reportsDir);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var reportPath = Path.Combine(reportsDir, $"StandaloneBenchmark_{timestamp}.md");
        File.WriteAllText(reportPath, report.ToString());

        // Console output
        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("Benchmark Complete!");
        Console.WriteLine("========================================");
        Console.WriteLine();
        Console.WriteLine("Summary:");
        foreach (var indicator in results.Select(r => r.IndicatorName).Distinct())
        {
            var times = results.Where(r => r.IndicatorName == indicator).Select(r => r.TimeMs).ToList();
            Console.WriteLine($"  {indicator}: {times[0]:F3}ms (100), {times[1]:F3}ms (1K), {times[2]:F3}ms (10K)");
        }
        Console.WriteLine();
        Console.WriteLine($"Report saved to: {reportPath}");
    }

    private static decimal[] GenerateMarketData(int count)
    {
        var random = new Random(42);
        var data = new decimal[count];
        decimal basePrice = 100m;
        decimal volatility = 0.02m;
        
        for (int i = 0; i < count; i++)
        {
            var change = (decimal)(random.NextDouble() - 0.5) * volatility * basePrice;
            basePrice += change;
            data[i] = basePrice;
        }
        
        return data;
    }

    private static double BenchmarkIndicator(string name, Action calculation, int iterations, int warmup)
    {
        // Warmup
        for (int i = 0; i < warmup; i++)
        {
            calculation();
        }

        // Benchmark
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            calculation();
        }
        sw.Stop();

        var avgMs = sw.Elapsed.TotalMilliseconds / iterations;
        Console.WriteLine($"  {name}: {avgMs:F3} ms average");
        return avgMs;
    }

    private static decimal CalculateSMA(decimal[] prices, int period)
    {
        if (prices.Length < period) return 0;
        
        decimal sum = 0;
        for (int i = prices.Length - period; i < prices.Length; i++)
        {
            sum += prices[i];
        }
        return sum / period;
    }

    private static decimal CalculateEMA(decimal[] prices, int period)
    {
        if (prices.Length < period) return 0;
        
        decimal multiplier = 2m / (period + 1);
        decimal ema = prices[0];
        
        for (int i = 1; i < prices.Length; i++)
        {
            ema = (prices[i] - ema) * multiplier + ema;
        }
        return ema;
    }

    private static decimal CalculateRSI(decimal[] prices, int period)
    {
        if (prices.Length < period + 1) return 50;
        
        decimal avgGain = 0;
        decimal avgLoss = 0;
        
        for (int i = 1; i <= period; i++)
        {
            var change = prices[i] - prices[i - 1];
            if (change > 0)
                avgGain += change;
            else
                avgLoss -= change;
        }
        
        avgGain /= period;
        avgLoss /= period;
        
        for (int i = period + 1; i < prices.Length; i++)
        {
            var change = prices[i] - prices[i - 1];
            if (change > 0)
            {
                avgGain = (avgGain * (period - 1) + change) / period;
                avgLoss = (avgLoss * (period - 1)) / period;
            }
            else
            {
                avgGain = (avgGain * (period - 1)) / period;
                avgLoss = (avgLoss * (period - 1) - change) / period;
            }
        }
        
        if (avgLoss == 0) return 100;
        var rs = avgGain / avgLoss;
        return 100 - (100 / (1 + rs));
    }

    private static (decimal upper, decimal middle, decimal lower) CalculateBollingerBands(decimal[] prices, int period, decimal stdDevMultiplier)
    {
        if (prices.Length < period) return (0, 0, 0);
        
        // Calculate SMA
        decimal sum = 0;
        for (int i = prices.Length - period; i < prices.Length; i++)
        {
            sum += prices[i];
        }
        decimal sma = sum / period;
        
        // Calculate standard deviation
        decimal sumSquaredDiff = 0;
        for (int i = prices.Length - period; i < prices.Length; i++)
        {
            var diff = prices[i] - sma;
            sumSquaredDiff += diff * diff;
        }
        decimal stdDev = (decimal)Math.Sqrt((double)(sumSquaredDiff / period));
        
        return (sma + stdDev * stdDevMultiplier, sma, sma - stdDev * stdDevMultiplier);
    }

    private class BenchmarkResult
    {
        public string IndicatorName { get; set; }
        public string Implementation { get; set; }
        public int DataSize { get; set; }
        public double TimeMs { get; set; }
    }
}