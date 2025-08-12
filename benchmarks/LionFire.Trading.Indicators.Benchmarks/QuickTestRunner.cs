using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LionFire.Trading.Indicators.Benchmarks;

/// <summary>
/// Quick test runner to generate initial performance numbers
/// This is a minimal implementation that doesn't require BenchmarkDotNet
/// </summary>
public class QuickTestRunner
{
    public static void RunQuickTests()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("   Quick Performance Test Runner");
        Console.WriteLine("   (Minimal implementation for initial testing)");
        Console.WriteLine("========================================");
        Console.WriteLine();

        var results = new StringBuilder();
        results.AppendLine("# Quick Performance Test Results");
        results.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        results.AppendLine();
        results.AppendLine("## Test Configuration");
        results.AppendLine("- Data Sizes: 100, 1000, 10000 points");
        results.AppendLine("- Iterations: 100 per test");
        results.AppendLine("- Warmup: 10 iterations");
        results.AppendLine();

        // Generate test data
        Console.WriteLine("Generating test data...");
        var smallData = GenerateTestData(100);
        var mediumData = GenerateTestData(1000);
        var largeData = GenerateTestData(10000);

        results.AppendLine("## Results");
        results.AppendLine();
        results.AppendLine("| Test | Data Size | Time (ms) | Ops/sec |");
        results.AppendLine("|------|-----------|-----------|---------|");

        // Test 1: Simple loop test
        Console.WriteLine("\nRunning Simple Loop Test...");
        var loopSmall = MeasurePerformance("Simple Loop", () => ProcessData(smallData), 100);
        var loopMedium = MeasurePerformance("Simple Loop", () => ProcessData(mediumData), 100);
        var loopLarge = MeasurePerformance("Simple Loop", () => ProcessData(largeData), 100);
        
        results.AppendLine($"| Simple Loop | 100 | {loopSmall:F3} | {1000.0/loopSmall:F0} |");
        results.AppendLine($"| Simple Loop | 1000 | {loopMedium:F3} | {1000.0/loopMedium:F0} |");
        results.AppendLine($"| Simple Loop | 10000 | {loopLarge:F3} | {1000.0/loopLarge:F0} |");

        // Test 2: Moving Average simulation
        Console.WriteLine("Running Moving Average Simulation...");
        var maSmall = MeasurePerformance("MA Sim", () => SimulateMovingAverage(smallData, 20), 100);
        var maMedium = MeasurePerformance("MA Sim", () => SimulateMovingAverage(mediumData, 20), 100);
        var maLarge = MeasurePerformance("MA Sim", () => SimulateMovingAverage(largeData, 20), 100);
        
        results.AppendLine($"| MA Simulation | 100 | {maSmall:F3} | {1000.0/maSmall:F0} |");
        results.AppendLine($"| MA Simulation | 1000 | {maMedium:F3} | {1000.0/maMedium:F0} |");
        results.AppendLine($"| MA Simulation | 10000 | {maLarge:F3} | {1000.0/maLarge:F0} |");

        // Test 3: Standard Deviation simulation
        Console.WriteLine("Running Standard Deviation Simulation...");
        var stdSmall = MeasurePerformance("StdDev Sim", () => SimulateStdDev(smallData, 20), 100);
        var stdMedium = MeasurePerformance("StdDev Sim", () => SimulateStdDev(mediumData, 20), 100);
        var stdLarge = MeasurePerformance("StdDev Sim", () => SimulateStdDev(largeData, 20), 100);
        
        results.AppendLine($"| StdDev Simulation | 100 | {stdSmall:F3} | {1000.0/stdSmall:F0} |");
        results.AppendLine($"| StdDev Simulation | 1000 | {stdMedium:F3} | {1000.0/stdMedium:F0} |");
        results.AppendLine($"| StdDev Simulation | 10000 | {stdLarge:F3} | {1000.0/stdLarge:F0} |");

        // Save results
        results.AppendLine();
        results.AppendLine("## Summary");
        results.AppendLine();
        results.AppendLine("### Scaling Analysis");
        results.AppendLine($"- Simple Loop: 10x data = {loopLarge/loopSmall:F1}x time");
        results.AppendLine($"- MA Simulation: 10x data = {maLarge/maSmall:F1}x time");
        results.AppendLine($"- StdDev Simulation: 10x data = {stdLarge/stdSmall:F1}x time");
        results.AppendLine();
        results.AppendLine("### Performance Characteristics");
        results.AppendLine($"- Average time for 1000 points: {(loopMedium + maMedium + stdMedium)/3:F3} ms");
        results.AppendLine($"- Throughput at 1000 points: {3000.0/(loopMedium + maMedium + stdMedium):F0} ops/sec");

        // Save to file
        var reportsDir = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
        if (!Directory.Exists(reportsDir))
        {
            Directory.CreateDirectory(reportsDir);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var reportPath = Path.Combine(reportsDir, $"QuickTest_{timestamp}.md");
        File.WriteAllText(reportPath, results.ToString());

        Console.WriteLine();
        Console.WriteLine("========================================");
        Console.WriteLine("Quick test complete!");
        Console.WriteLine($"Report saved to: {reportPath}");
        Console.WriteLine("========================================");
        
        // Display summary on console
        Console.WriteLine();
        Console.WriteLine("Performance Summary:");
        Console.WriteLine($"  Simple Loop (1000): {loopMedium:F3} ms");
        Console.WriteLine($"  MA Simulation (1000): {maMedium:F3} ms");
        Console.WriteLine($"  StdDev Simulation (1000): {stdMedium:F3} ms");
        Console.WriteLine($"  Average: {(loopMedium + maMedium + stdMedium)/3:F3} ms");
    }

    private static decimal[] GenerateTestData(int count)
    {
        var random = new Random(42); // Fixed seed for reproducibility
        var data = new decimal[count];
        decimal price = 100m;
        
        for (int i = 0; i < count; i++)
        {
            // Random walk
            price += (decimal)(random.NextDouble() - 0.5) * 2;
            data[i] = price;
        }
        
        return data;
    }

    private static double MeasurePerformance(string name, Action action, int iterations)
    {
        // Warmup
        for (int i = 0; i < 10; i++)
        {
            action();
        }

        // Measure
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            action();
        }
        sw.Stop();

        var avgMs = sw.Elapsed.TotalMilliseconds / iterations;
        Console.WriteLine($"  {name}: {avgMs:F3} ms per operation");
        return avgMs;
    }

    private static decimal ProcessData(decimal[] data)
    {
        decimal sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += data[i];
        }
        return sum / data.Length;
    }

    private static decimal SimulateMovingAverage(decimal[] data, int period)
    {
        if (data.Length < period)
            return 0;

        decimal sum = 0;
        for (int i = 0; i < period; i++)
        {
            sum += data[i];
        }

        decimal ma = sum / period;
        
        for (int i = period; i < data.Length; i++)
        {
            sum = sum - data[i - period] + data[i];
            ma = sum / period;
        }

        return ma;
    }

    private static decimal SimulateStdDev(decimal[] data, int period)
    {
        if (data.Length < period)
            return 0;

        decimal sum = 0;
        decimal sumSquares = 0;

        for (int i = 0; i < period; i++)
        {
            sum += data[i];
            sumSquares += data[i] * data[i];
        }

        decimal mean = sum / period;
        decimal variance = (sumSquares / period) - (mean * mean);
        decimal stdDev = (decimal)Math.Sqrt((double)variance);

        for (int i = period; i < data.Length; i++)
        {
            sum = sum - data[i - period] + data[i];
            sumSquares = sumSquares - (data[i - period] * data[i - period]) + (data[i] * data[i]);
            mean = sum / period;
            variance = (sumSquares / period) - (mean * mean);
            stdDev = (decimal)Math.Sqrt(Math.Max(0, (double)variance));
        }

        return stdDev;
    }
}