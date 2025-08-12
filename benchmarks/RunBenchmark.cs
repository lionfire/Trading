#!/usr/bin/env dotnet-script
#r "nuget: System.Diagnostics.Stopwatch, 4.3.0"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

// Simplified benchmark runner without dependencies
public class IndicatorBenchmark
{
    static void Main()
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("   Indicator Performance Benchmark Report");
        Console.WriteLine("===========================================");
        Console.WriteLine();
        
        var results = new List<BenchmarkResult>();
        
        // Test different data sizes
        int[] dataSizes = { 1000, 10000, 100000, 1000000 };
        
        foreach (var size in dataSizes)
        {
            Console.WriteLine($"\n📊 Testing with {size:N0} data points:");
            Console.WriteLine("----------------------------------------");
            
            // Generate test data
            var prices = GenerateTestData(size);
            var hlcData = GenerateHLCData(size);
            
            // SMA Benchmark
            results.Add(BenchmarkIndicator("SMA", size, () => SimulateSMA(prices, 20)));
            
            // EMA Benchmark  
            results.Add(BenchmarkIndicator("EMA", size, () => SimulateEMA(prices, 20)));
            
            // RSI Benchmark
            results.Add(BenchmarkIndicator("RSI", size, () => SimulateRSI(prices, 14)));
            
            // Bollinger Bands Benchmark
            results.Add(BenchmarkIndicator("Bollinger", size, () => SimulateBollinger(prices, 20, 2)));
            
            // Stochastic Benchmark
            results.Add(BenchmarkIndicator("Stochastic", size, () => SimulateStochastic(hlcData, 14, 3)));
            
            // MACD Benchmark
            results.Add(BenchmarkIndicator("MACD", size, () => SimulateMACD(prices, 12, 26, 9)));
        }
        
        // Generate summary report
        GenerateReport(results);
    }
    
    static BenchmarkResult BenchmarkIndicator(string name, int dataSize, Action calculation)
    {
        // Warm-up
        calculation();
        
        // Actual benchmark
        var sw = Stopwatch.StartNew();
        const int iterations = 10;
        
        for (int i = 0; i < iterations; i++)
        {
            calculation();
        }
        
        sw.Stop();
        
        var avgMs = sw.Elapsed.TotalMilliseconds / iterations;
        var throughput = dataSize / (avgMs / 1000.0); // points per second
        
        Console.WriteLine($"  {name,-12} {avgMs,8:F3} ms   {throughput/1000000,8:F2} M pts/sec");
        
        return new BenchmarkResult
        {
            Indicator = name,
            DataSize = dataSize,
            TimeMs = avgMs,
            ThroughputMpps = throughput / 1000000
        };
    }
    
    static double[] GenerateTestData(int size)
    {
        var data = new double[size];
        var random = new Random(42);
        double price = 100.0;
        
        for (int i = 0; i < size; i++)
        {
            price += (random.NextDouble() - 0.5) * 2;
            data[i] = price;
        }
        
        return data;
    }
    
    static (double[] high, double[] low, double[] close) GenerateHLCData(int size)
    {
        var high = new double[size];
        var low = new double[size];
        var close = new double[size];
        var random = new Random(42);
        double basePrice = 100.0;
        
        for (int i = 0; i < size; i++)
        {
            basePrice += (random.NextDouble() - 0.5) * 2;
            var range = random.NextDouble() * 2 + 0.5;
            high[i] = basePrice + range;
            low[i] = basePrice - range;
            close[i] = basePrice + (random.NextDouble() - 0.5) * range;
        }
        
        return (high, low, close);
    }
    
    // Simplified indicator simulations
    static double[] SimulateSMA(double[] prices, int period)
    {
        var result = new double[prices.Length];
        double sum = 0;
        
        for (int i = 0; i < prices.Length; i++)
        {
            if (i < period)
            {
                sum += prices[i];
                result[i] = sum / (i + 1);
            }
            else
            {
                sum = sum - prices[i - period] + prices[i];
                result[i] = sum / period;
            }
        }
        
        return result;
    }
    
    static double[] SimulateEMA(double[] prices, int period)
    {
        var result = new double[prices.Length];
        double multiplier = 2.0 / (period + 1);
        result[0] = prices[0];
        
        for (int i = 1; i < prices.Length; i++)
        {
            result[i] = (prices[i] - result[i - 1]) * multiplier + result[i - 1];
        }
        
        return result;
    }
    
    static double[] SimulateRSI(double[] prices, int period)
    {
        var result = new double[prices.Length];
        double avgGain = 0, avgLoss = 0;
        
        for (int i = 1; i < prices.Length; i++)
        {
            double change = prices[i] - prices[i - 1];
            
            if (i <= period)
            {
                if (change > 0) avgGain += change;
                else avgLoss -= change;
                
                if (i == period)
                {
                    avgGain /= period;
                    avgLoss /= period;
                }
            }
            else
            {
                double gain = change > 0 ? change : 0;
                double loss = change < 0 ? -change : 0;
                
                avgGain = (avgGain * (period - 1) + gain) / period;
                avgLoss = (avgLoss * (period - 1) + loss) / period;
            }
            
            double rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
            result[i] = 100 - (100 / (1 + rs));
        }
        
        return result;
    }
    
    static (double[] upper, double[] middle, double[] lower) SimulateBollinger(double[] prices, int period, double stdDev)
    {
        var sma = SimulateSMA(prices, period);
        var upper = new double[prices.Length];
        var lower = new double[prices.Length];
        
        for (int i = period - 1; i < prices.Length; i++)
        {
            double sum = 0;
            for (int j = 0; j < period; j++)
            {
                double diff = prices[i - j] - sma[i];
                sum += diff * diff;
            }
            double std = Math.Sqrt(sum / period);
            
            upper[i] = sma[i] + stdDev * std;
            lower[i] = sma[i] - stdDev * std;
        }
        
        return (upper, sma, lower);
    }
    
    static (double[] k, double[] d) SimulateStochastic((double[] high, double[] low, double[] close) data, int period, int smoothing)
    {
        var k = new double[data.close.Length];
        var d = new double[data.close.Length];
        
        for (int i = period - 1; i < data.close.Length; i++)
        {
            double highest = data.high[i];
            double lowest = data.low[i];
            
            for (int j = 1; j < period; j++)
            {
                if (data.high[i - j] > highest) highest = data.high[i - j];
                if (data.low[i - j] < lowest) lowest = data.low[i - j];
            }
            
            k[i] = (data.close[i] - lowest) / (highest - lowest) * 100;
        }
        
        // Calculate %D as SMA of %K
        d = SimulateSMA(k, smoothing);
        
        return (k, d);
    }
    
    static (double[] macd, double[] signal, double[] histogram) SimulateMACD(double[] prices, int fast, int slow, int signal)
    {
        var fastEma = SimulateEMA(prices, fast);
        var slowEma = SimulateEMA(prices, slow);
        var macd = new double[prices.Length];
        
        for (int i = 0; i < prices.Length; i++)
        {
            macd[i] = fastEma[i] - slowEma[i];
        }
        
        var signalLine = SimulateEMA(macd, signal);
        var histogram = new double[prices.Length];
        
        for (int i = 0; i < prices.Length; i++)
        {
            histogram[i] = macd[i] - signalLine[i];
        }
        
        return (macd, signalLine, histogram);
    }
    
    static void GenerateReport(List<BenchmarkResult> results)
    {
        Console.WriteLine("\n\n===========================================");
        Console.WriteLine("           PERFORMANCE SUMMARY");
        Console.WriteLine("===========================================");
        
        var indicators = results.Select(r => r.Indicator).Distinct().ToList();
        var dataSizes = results.Select(r => r.DataSize).Distinct().OrderBy(s => s).ToList();
        
        // Performance comparison table
        Console.WriteLine("\n📈 Execution Time (milliseconds):");
        Console.WriteLine("-------------------------------------------");
        Console.Write($"{"Indicator",-12}");
        foreach (var size in dataSizes)
        {
            Console.Write($"{size/1000 + "K",12}");
        }
        Console.WriteLine();
        Console.WriteLine(new string('-', 12 + dataSizes.Count * 12));
        
        foreach (var indicator in indicators)
        {
            Console.Write($"{indicator,-12}");
            foreach (var size in dataSizes)
            {
                var result = results.FirstOrDefault(r => r.Indicator == indicator && r.DataSize == size);
                if (result != null)
                {
                    Console.Write($"{result.TimeMs,12:F3}");
                }
                else
                {
                    Console.Write($"{"N/A",12}");
                }
            }
            Console.WriteLine();
        }
        
        // Throughput table
        Console.WriteLine("\n⚡ Throughput (Million points/second):");
        Console.WriteLine("-------------------------------------------");
        Console.Write($"{"Indicator",-12}");
        foreach (var size in dataSizes)
        {
            Console.Write($"{size/1000 + "K",12}");
        }
        Console.WriteLine();
        Console.WriteLine(new string('-', 12 + dataSizes.Count * 12));
        
        foreach (var indicator in indicators)
        {
            Console.Write($"{indicator,-12}");
            foreach (var size in dataSizes)
            {
                var result = results.FirstOrDefault(r => r.Indicator == indicator && r.DataSize == size);
                if (result != null)
                {
                    Console.Write($"{result.ThroughputMpps,12:F2}");
                }
                else
                {
                    Console.Write($"{"N/A",12}");
                }
            }
            Console.WriteLine();
        }
        
        // Performance rankings
        Console.WriteLine("\n🏆 Performance Rankings (by throughput at 100K points):");
        Console.WriteLine("-------------------------------------------");
        var rankings = results
            .Where(r => r.DataSize == 100000)
            .OrderByDescending(r => r.ThroughputMpps)
            .ToList();
        
        for (int i = 0; i < rankings.Count; i++)
        {
            var r = rankings[i];
            Console.WriteLine($"{i + 1}. {r.Indicator,-12} {r.ThroughputMpps,8:F2} M pts/sec");
        }
        
        // Scaling analysis
        Console.WriteLine("\n📊 Scaling Analysis:");
        Console.WriteLine("-------------------------------------------");
        foreach (var indicator in indicators)
        {
            var indicatorResults = results.Where(r => r.Indicator == indicator).OrderBy(r => r.DataSize).ToList();
            if (indicatorResults.Count >= 2)
            {
                var small = indicatorResults.First();
                var large = indicatorResults.Last();
                var scalingFactor = (large.TimeMs / small.TimeMs) / (large.DataSize / (double)small.DataSize);
                
                string complexity = scalingFactor < 1.1 ? "O(n) Linear" :
                                   scalingFactor < 1.5 ? "O(n log n)" :
                                   scalingFactor < 2.5 ? "O(n²) Quadratic" : "O(n²+) Poor";
                
                Console.WriteLine($"{indicator,-12} Scaling: {scalingFactor,6:F2}x - {complexity}");
            }
        }
        
        Console.WriteLine("\n===========================================");
        Console.WriteLine("            RECOMMENDATIONS");
        Console.WriteLine("===========================================");
        Console.WriteLine();
        Console.WriteLine("Based on the benchmark results:");
        Console.WriteLine();
        Console.WriteLine("✅ EXCELLENT Performance (>10M pts/sec):");
        foreach (var r in rankings.Where(r => r.ThroughputMpps > 10))
        {
            Console.WriteLine($"   - {r.Indicator}");
        }
        
        Console.WriteLine("\n⚠️  GOOD Performance (1-10M pts/sec):");
        foreach (var r in rankings.Where(r => r.ThroughputMpps >= 1 && r.ThroughputMpps <= 10))
        {
            Console.WriteLine($"   - {r.Indicator}");
        }
        
        Console.WriteLine("\n❌ NEEDS OPTIMIZATION (<1M pts/sec):");
        foreach (var r in rankings.Where(r => r.ThroughputMpps < 1))
        {
            Console.WriteLine($"   - {r.Indicator}");
        }
        
        Console.WriteLine("\n📝 Conclusion:");
        Console.WriteLine("-------------------------------------------");
        var avgThroughput = rankings.Average(r => r.ThroughputMpps);
        Console.WriteLine($"Average throughput: {avgThroughput:F2} M pts/sec");
        
        if (avgThroughput > 5)
        {
            Console.WriteLine("✅ Overall performance is EXCELLENT.");
            Console.WriteLine("   The indicators are suitable for high-frequency trading.");
        }
        else if (avgThroughput > 1)
        {
            Console.WriteLine("⚠️  Overall performance is GOOD.");
            Console.WriteLine("   The indicators are suitable for most trading scenarios.");
        }
        else
        {
            Console.WriteLine("❌ Overall performance needs improvement.");
            Console.WriteLine("   Consider optimizing critical indicators.");
        }
        
        Console.WriteLine("\n🔍 Note: This is a simplified benchmark.");
        Console.WriteLine("   For production use, consider:");
        Console.WriteLine("   - Memory allocation patterns");
        Console.WriteLine("   - GC pressure under load");
        Console.WriteLine("   - Concurrent execution scenarios");
        Console.WriteLine("   - Real-time streaming performance");
    }
    
    class BenchmarkResult
    {
        public string Indicator { get; set; }
        public int DataSize { get; set; }
        public double TimeMs { get; set; }
        public double ThroughputMpps { get; set; }
    }
}

// Run the benchmark
IndicatorBenchmark.Main();