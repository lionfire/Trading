using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LionFire.Trading.Indicators.Native;

namespace LionFire.Trading.Indicators.Benchmarks;

/// <summary>
/// Simple benchmark runner that doesn't require BenchmarkDotNet
/// Useful for quick tests and when BenchmarkDotNet has issues
/// </summary>
public class SimpleRunner
{
    private readonly TestDataGenerator _dataGenerator;
    private readonly int _warmupIterations = 100;
    private readonly int _testIterations = 1000;
    private readonly List<SimpleResult> _results;

    public SimpleRunner()
    {
        _dataGenerator = new TestDataGenerator();
        _results = new List<SimpleResult>();
    }

    public static void RunSimpleBenchmarks(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("   Simple Performance Test Runner");
        Console.WriteLine("========================================");
        Console.WriteLine();

        var runner = new SimpleRunner();
        
        // Parse arguments
        string filter = "*";
        bool verbose = false;
        
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--filter" && i + 1 < args.Length)
            {
                filter = args[i + 1];
            }
            else if (args[i] == "--verbose")
            {
                verbose = true;
            }
        }

        runner.Run(filter, verbose);
    }

    public void Run(string filter = "*", bool verbose = false)
    {
        Console.WriteLine($"Test Configuration:");
        Console.WriteLine($"  Warmup Iterations: {_warmupIterations}");
        Console.WriteLine($"  Test Iterations: {_testIterations}");
        Console.WriteLine($"  Filter: {filter}");
        Console.WriteLine();

        // Generate test data
        Console.WriteLine("Generating test data...");
        var smallData = _dataGenerator.GenerateRandomPrices(100);
        var mediumData = _dataGenerator.GenerateRandomPrices(1000);
        var largeData = _dataGenerator.GenerateRandomPrices(10000);

        // Run benchmarks for each indicator
        if (MatchesFilter("SMA", filter))
        {
            Console.WriteLine("\n--- Simple Moving Average (SMA) ---");
            BenchmarkSMA(smallData, mediumData, largeData, verbose);
        }

        if (MatchesFilter("EMA", filter))
        {
            Console.WriteLine("\n--- Exponential Moving Average (EMA) ---");
            BenchmarkEMA(smallData, mediumData, largeData, verbose);
        }

        if (MatchesFilter("RSI", filter))
        {
            Console.WriteLine("\n--- Relative Strength Index (RSI) ---");
            BenchmarkRSI(smallData, mediumData, largeData, verbose);
        }

        if (MatchesFilter("MACD", filter))
        {
            Console.WriteLine("\n--- MACD ---");
            BenchmarkMACD(smallData, mediumData, largeData, verbose);
        }

        if (MatchesFilter("Bollinger", filter))
        {
            Console.WriteLine("\n--- Bollinger Bands ---");
            BenchmarkBollingerBands(smallData, mediumData, largeData, verbose);
        }

        if (MatchesFilter("Stochastic", filter))
        {
            Console.WriteLine("\n--- Stochastic Oscillator ---");
            BenchmarkStochastic(smallData, mediumData, largeData, verbose);
        }

        // Generate report
        GenerateReport();
    }

    private void BenchmarkSMA(decimal[] small, decimal[] medium, decimal[] large, bool verbose)
    {
        var period = 20;

        // Test FP implementation
        RunBenchmark("SMA_FP_Small", () =>
        {
            var sma = new FpSimpleMovingAverage(period);
            foreach (var price in small)
            {
                sma.Update(price);
            }
            return sma.Current;
        }, verbose);

        RunBenchmark("SMA_FP_Medium", () =>
        {
            var sma = new FpSimpleMovingAverage(period);
            foreach (var price in medium)
            {
                sma.Update(price);
            }
            return sma.Current;
        }, verbose);

        RunBenchmark("SMA_FP_Large", () =>
        {
            var sma = new FpSimpleMovingAverage(period);
            foreach (var price in large)
            {
                sma.Update(price);
            }
            return sma.Current;
        }, verbose);

        // Test QC implementation
        RunBenchmark("SMA_QC_Small", () =>
        {
            var sma = new QcSimpleMovingAverage(period);
            foreach (var price in small)
            {
                sma.Update(price);
            }
            return sma.Current;
        }, verbose);

        RunBenchmark("SMA_QC_Medium", () =>
        {
            var sma = new QcSimpleMovingAverage(period);
            foreach (var price in medium)
            {
                sma.Update(price);
            }
            return sma.Current;
        }, verbose);

        RunBenchmark("SMA_QC_Large", () =>
        {
            var sma = new QcSimpleMovingAverage(period);
            foreach (var price in large)
            {
                sma.Update(price);
            }
            return sma.Current;
        }, verbose);
    }

    private void BenchmarkEMA(decimal[] small, decimal[] medium, decimal[] large, bool verbose)
    {
        var period = 20;

        // Test FP implementation
        RunBenchmark("EMA_FP_Small", () =>
        {
            var ema = new FpExponentialMovingAverage(period);
            foreach (var price in small)
            {
                ema.Update(price);
            }
            return ema.Current;
        }, verbose);

        RunBenchmark("EMA_FP_Medium", () =>
        {
            var ema = new FpExponentialMovingAverage(period);
            foreach (var price in medium)
            {
                ema.Update(price);
            }
            return ema.Current;
        }, verbose);

        RunBenchmark("EMA_FP_Large", () =>
        {
            var ema = new FpExponentialMovingAverage(period);
            foreach (var price in large)
            {
                ema.Update(price);
            }
            return ema.Current;
        }, verbose);

        // Test QC implementation
        RunBenchmark("EMA_QC_Small", () =>
        {
            var ema = new QcExponentialMovingAverage(period);
            foreach (var price in small)
            {
                ema.Update(price);
            }
            return ema.Current;
        }, verbose);

        RunBenchmark("EMA_QC_Medium", () =>
        {
            var ema = new QcExponentialMovingAverage(period);
            foreach (var price in medium)
            {
                ema.Update(price);
            }
            return ema.Current;
        }, verbose);

        RunBenchmark("EMA_QC_Large", () =>
        {
            var ema = new QcExponentialMovingAverage(period);
            foreach (var price in large)
            {
                ema.Update(price);
            }
            return ema.Current;
        }, verbose);
    }

    private void BenchmarkRSI(decimal[] small, decimal[] medium, decimal[] large, bool verbose)
    {
        var period = 14;

        // Test FP implementation
        RunBenchmark("RSI_FP_Small", () =>
        {
            var rsi = new FpRelativeStrengthIndex(period);
            foreach (var price in small)
            {
                rsi.Update(price);
            }
            return rsi.Current;
        }, verbose);

        RunBenchmark("RSI_FP_Medium", () =>
        {
            var rsi = new FpRelativeStrengthIndex(period);
            foreach (var price in medium)
            {
                rsi.Update(price);
            }
            return rsi.Current;
        }, verbose);

        RunBenchmark("RSI_FP_Large", () =>
        {
            var rsi = new FpRelativeStrengthIndex(period);
            foreach (var price in large)
            {
                rsi.Update(price);
            }
            return rsi.Current;
        }, verbose);

        // Test QC implementation
        RunBenchmark("RSI_QC_Small", () =>
        {
            var rsi = new QcRelativeStrengthIndex(period);
            foreach (var price in small)
            {
                rsi.Update(price);
            }
            return rsi.Current;
        }, verbose);

        RunBenchmark("RSI_QC_Medium", () =>
        {
            var rsi = new QcRelativeStrengthIndex(period);
            foreach (var price in medium)
            {
                rsi.Update(price);
            }
            return rsi.Current;
        }, verbose);

        RunBenchmark("RSI_QC_Large", () =>
        {
            var rsi = new QcRelativeStrengthIndex(period);
            foreach (var price in large)
            {
                rsi.Update(price);
            }
            return rsi.Current;
        }, verbose);
    }

    private void BenchmarkMACD(decimal[] small, decimal[] medium, decimal[] large, bool verbose)
    {
        // Test FP implementation
        RunBenchmark("MACD_FP_Small", () =>
        {
            var macd = new FpMACD(12, 26, 9);
            foreach (var price in small)
            {
                macd.Update(price);
            }
            return macd.Current;
        }, verbose);

        RunBenchmark("MACD_FP_Medium", () =>
        {
            var macd = new FpMACD(12, 26, 9);
            foreach (var price in medium)
            {
                macd.Update(price);
            }
            return macd.Current;
        }, verbose);

        RunBenchmark("MACD_FP_Large", () =>
        {
            var macd = new FpMACD(12, 26, 9);
            foreach (var price in large)
            {
                macd.Update(price);
            }
            return macd.Current;
        }, verbose);

        // Test QC implementation
        RunBenchmark("MACD_QC_Small", () =>
        {
            var macd = new QcMACD(12, 26, 9);
            foreach (var price in small)
            {
                macd.Update(price);
            }
            return macd.Current;
        }, verbose);

        RunBenchmark("MACD_QC_Medium", () =>
        {
            var macd = new QcMACD(12, 26, 9);
            foreach (var price in medium)
            {
                macd.Update(price);
            }
            return macd.Current;
        }, verbose);

        RunBenchmark("MACD_QC_Large", () =>
        {
            var macd = new QcMACD(12, 26, 9);
            foreach (var price in large)
            {
                macd.Update(price);
            }
            return macd.Current;
        }, verbose);
    }

    private void BenchmarkBollingerBands(decimal[] small, decimal[] medium, decimal[] large, bool verbose)
    {
        // Test FP implementation
        RunBenchmark("Bollinger_FP_Small", () =>
        {
            var bb = new FpBollingerBands(20, 2);
            foreach (var price in small)
            {
                bb.Update(price);
            }
            return bb.Current;
        }, verbose);

        RunBenchmark("Bollinger_FP_Medium", () =>
        {
            var bb = new FpBollingerBands(20, 2);
            foreach (var price in medium)
            {
                bb.Update(price);
            }
            return bb.Current;
        }, verbose);

        RunBenchmark("Bollinger_FP_Large", () =>
        {
            var bb = new FpBollingerBands(20, 2);
            foreach (var price in large)
            {
                bb.Update(price);
            }
            return bb.Current;
        }, verbose);

        // Test QC implementation
        RunBenchmark("Bollinger_QC_Small", () =>
        {
            var bb = new QcBollingerBands(20, 2);
            foreach (var price in small)
            {
                bb.Update(price);
            }
            return bb.Current;
        }, verbose);

        RunBenchmark("Bollinger_QC_Medium", () =>
        {
            var bb = new QcBollingerBands(20, 2);
            foreach (var price in medium)
            {
                bb.Update(price);
            }
            return bb.Current;
        }, verbose);

        RunBenchmark("Bollinger_QC_Large", () =>
        {
            var bb = new QcBollingerBands(20, 2);
            foreach (var price in large)
            {
                bb.Update(price);
            }
            return bb.Current;
        }, verbose);
    }

    private void BenchmarkStochastic(decimal[] small, decimal[] medium, decimal[] large, bool verbose)
    {
        // Generate high/low/close data
        var smallHLC = GenerateHLCData(small);
        var mediumHLC = GenerateHLCData(medium);
        var largeHLC = GenerateHLCData(large);

        // Test FP implementation
        RunBenchmark("Stochastic_FP_Small", () =>
        {
            var stoch = new FpStochasticOscillator(14, 3, 3);
            foreach (var hlc in smallHLC)
            {
                stoch.Update(hlc.high, hlc.low, hlc.close);
            }
            return stoch.Current;
        }, verbose);

        RunBenchmark("Stochastic_FP_Medium", () =>
        {
            var stoch = new FpStochasticOscillator(14, 3, 3);
            foreach (var hlc in mediumHLC)
            {
                stoch.Update(hlc.high, hlc.low, hlc.close);
            }
            return stoch.Current;
        }, verbose);

        RunBenchmark("Stochastic_FP_Large", () =>
        {
            var stoch = new FpStochasticOscillator(14, 3, 3);
            foreach (var hlc in largeHLC)
            {
                stoch.Update(hlc.high, hlc.low, hlc.close);
            }
            return stoch.Current;
        }, verbose);

        // Test QC implementation
        RunBenchmark("Stochastic_QC_Small", () =>
        {
            var stoch = new QcStochasticOscillator(14, 3, 3);
            foreach (var hlc in smallHLC)
            {
                stoch.Update(hlc.high, hlc.low, hlc.close);
            }
            return stoch.Current;
        }, verbose);

        RunBenchmark("Stochastic_QC_Medium", () =>
        {
            var stoch = new QcStochasticOscillator(14, 3, 3);
            foreach (var hlc in mediumHLC)
            {
                stoch.Update(hlc.high, hlc.low, hlc.close);
            }
            return stoch.Current;
        }, verbose);

        RunBenchmark("Stochastic_QC_Large", () =>
        {
            var stoch = new QcStochasticOscillator(14, 3, 3);
            foreach (var hlc in largeHLC)
            {
                stoch.Update(hlc.high, hlc.low, hlc.close);
            }
            return stoch.Current;
        }, verbose);
    }

    private (decimal high, decimal low, decimal close)[] GenerateHLCData(decimal[] prices)
    {
        var result = new (decimal high, decimal low, decimal close)[prices.Length];
        var random = new Random(42);
        
        for (int i = 0; i < prices.Length; i++)
        {
            var variance = prices[i] * 0.02m; // 2% variance
            result[i] = (
                high: prices[i] + variance * (decimal)random.NextDouble(),
                low: prices[i] - variance * (decimal)random.NextDouble(),
                close: prices[i]
            );
        }
        
        return result;
    }

    private void RunBenchmark(string name, Func<decimal> action, bool verbose)
    {
        if (verbose)
        {
            Console.Write($"  {name,-30} ");
        }

        // Warmup
        for (int i = 0; i < _warmupIterations; i++)
        {
            action();
        }

        // Measure
        var times = new List<double>();
        var sw = new Stopwatch();
        
        for (int i = 0; i < _testIterations; i++)
        {
            sw.Restart();
            action();
            sw.Stop();
            times.Add(sw.Elapsed.TotalMilliseconds);
        }

        // Calculate statistics
        var mean = times.Average();
        var stdDev = Math.Sqrt(times.Select(x => Math.Pow(x - mean, 2)).Average());
        var min = times.Min();
        var max = times.Max();
        var median = GetMedian(times);

        var result = new SimpleResult
        {
            Name = name,
            Mean = mean,
            StdDev = stdDev,
            Min = min,
            Max = max,
            Median = median,
            Iterations = _testIterations
        };

        _results.Add(result);

        if (verbose)
        {
            Console.WriteLine($"Mean: {mean:F4}ms, StdDev: {stdDev:F4}ms, Median: {median:F4}ms");
        }
        else
        {
            Console.Write(".");
        }
    }

    private double GetMedian(List<double> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        int n = sorted.Count;
        
        if (n % 2 == 0)
        {
            return (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
        }
        else
        {
            return sorted[n / 2];
        }
    }

    private bool MatchesFilter(string indicator, string filter)
    {
        if (filter == "*")
            return true;
            
        return indicator.ToLower().Contains(filter.ToLower()) ||
               filter.ToLower().Contains(indicator.ToLower());
    }

    private void GenerateReport()
    {
        Console.WriteLine("\n\n========================================");
        Console.WriteLine("           BENCHMARK RESULTS");
        Console.WriteLine("========================================\n");

        // Group results by indicator
        var groups = _results.GroupBy(r => r.Name.Split('_')[0]);

        foreach (var group in groups)
        {
            Console.WriteLine($"\n{group.Key} Results:");
            Console.WriteLine(new string('-', 80));
            Console.WriteLine($"{"Test",-30} {"Mean (ms)",-12} {"StdDev",-12} {"Min",-12} {"Max",-12} {"Median",-12}");
            Console.WriteLine(new string('-', 80));

            foreach (var result in group.OrderBy(r => r.Name))
            {
                Console.WriteLine($"{result.Name,-30} {result.Mean,-12:F4} {result.StdDev,-12:F4} {result.Min,-12:F4} {result.Max,-12:F4} {result.Median,-12:F4}");
            }

            // Calculate and display comparison
            var fpResults = group.Where(r => r.Name.Contains("_FP_")).ToList();
            var qcResults = group.Where(r => r.Name.Contains("_QC_")).ToList();

            if (fpResults.Any() && qcResults.Any())
            {
                var fpAvg = fpResults.Average(r => r.Mean);
                var qcAvg = qcResults.Average(r => r.Mean);
                var ratio = qcAvg / fpAvg;

                Console.WriteLine($"\nComparison: FP Avg: {fpAvg:F4}ms, QC Avg: {qcAvg:F4}ms");
                Console.WriteLine($"Performance Ratio: {ratio:F2}x (FP is {(ratio > 1 ? "faster" : "slower")})");
            }
        }

        // Save results to CSV
        SaveResultsToCsv();
    }

    private void SaveResultsToCsv()
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
        
        if (!Directory.Exists(reportsDir))
        {
            Directory.CreateDirectory(reportsDir);
        }

        var csvPath = Path.Combine(reportsDir, $"SimpleRunner_{timestamp}.csv");
        
        using (var writer = new StreamWriter(csvPath))
        {
            writer.WriteLine("Name,Mean (ms),StdDev (ms),Min (ms),Max (ms),Median (ms),Iterations");
            
            foreach (var result in _results.OrderBy(r => r.Name))
            {
                writer.WriteLine($"{result.Name},{result.Mean:F6},{result.StdDev:F6},{result.Min:F6},{result.Max:F6},{result.Median:F6},{result.Iterations}");
            }
        }

        Console.WriteLine($"\n\nResults saved to: {csvPath}");
    }

    private class SimpleResult
    {
        public string Name { get; set; }
        public double Mean { get; set; }
        public double StdDev { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Median { get; set; }
        public int Iterations { get; set; }
    }
}