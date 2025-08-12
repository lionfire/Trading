using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Exporters;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;
using System.Globalization;
using System.Collections.Generic;

namespace LionFire.Trading.Indicators.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("LionFire Trading Indicators Benchmark Suite");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Check for simple runner mode (fallback when BenchmarkDotNet has issues)
        if (args.Length > 0 && args[0] == "--simple")
        {
            SimpleRunner.RunSimpleBenchmarks(args);
            return;
        }

        // Check for quick test mode (minimal dependencies)
        if (args.Length > 0 && args[0] == "--quicktest")
        {
            QuickTestRunner.RunQuickTests();
            return;
        }

        var config = DefaultConfig.Instance
            .WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .AddExporter(HtmlExporter.Default)
            .AddExporter(CsvExporter.Default)
            .AddExporter(MarkdownExporter.GitHub)
            .AddExporter(new CustomSummaryExporter());

        if (args.Length > 0 && args[0] == "--all")
        {
            RunAllBenchmarks(config);
        }
        else if (args.Length > 0 && args[0] == "--quick")
        {
            RunQuickBenchmarks();
        }
        else
        {
            ShowMenu(config);
        }
    }

    private static void ShowMenu(IConfig config)
    {
        Console.WriteLine("Select benchmark option:");
        Console.WriteLine("1. Run all benchmarks");
        Console.WriteLine("2. Run specific indicator benchmark");
        Console.WriteLine("3. Run quick benchmarks (smaller data sizes)");
        Console.WriteLine("4. Run memory stress tests");
        Console.WriteLine("5. Generate comparison report");
        Console.WriteLine("6. Run simple benchmarks (no BenchmarkDotNet)");
        Console.WriteLine("0. Exit");
        Console.WriteLine();
        Console.Write("Enter option: ");

        var option = Console.ReadLine();

        switch (option)
        {
            case "1":
                RunAllBenchmarks(config);
                break;
            case "2":
                RunSpecificBenchmark(config);
                break;
            case "3":
                RunQuickBenchmarks();
                break;
            case "4":
                RunMemoryStressTests();
                break;
            case "5":
                GenerateComparisonReport();
                break;
            case "6":
                SimpleRunner.RunSimpleBenchmarks(new string[] { });
                break;
            case "0":
                return;
            default:
                Console.WriteLine("Invalid option. Please try again.");
                ShowMenu(config);
                break;
        }
    }

    private static void RunAllBenchmarks(IConfig config)
    {
        Console.WriteLine("Running all benchmarks...");
        Console.WriteLine("This may take several minutes to complete.");
        Console.WriteLine();

        var assembly = Assembly.GetExecutingAssembly();
        var benchmarkTypes = assembly.GetTypes()
            .Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<BenchmarkDotNet.Attributes.BenchmarkAttribute>() != null))
            .ToArray();

        var summaries = new List<Summary>();

        foreach (var type in benchmarkTypes)
        {
            Console.WriteLine($"Running {type.Name}...");
            var summary = BenchmarkRunner.Run(type, config);
            summaries.Add(summary);
            Console.WriteLine();
        }

        GenerateCombinedReport(summaries);
    }

    private static void RunSpecificBenchmark(IConfig config)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var benchmarkTypes = assembly.GetTypes()
            .Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<BenchmarkDotNet.Attributes.BenchmarkAttribute>() != null))
            .ToArray();

        Console.WriteLine("Available benchmarks:");
        for (int i = 0; i < benchmarkTypes.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {benchmarkTypes[i].Name}");
        }

        Console.Write("Enter benchmark number: ");
        if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= benchmarkTypes.Length)
        {
            var type = benchmarkTypes[selection - 1];
            Console.WriteLine($"Running {type.Name}...");
            BenchmarkRunner.Run(type, config);
        }
        else
        {
            Console.WriteLine("Invalid selection.");
        }
    }

    private static void RunQuickBenchmarks()
    {
        Console.WriteLine("Running quick benchmarks with reduced data sizes...");
        
        var quickConfig = DefaultConfig.Instance
            .WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .AddJob(BenchmarkDotNet.Jobs.Job.Dry)
            .AddExporter(MarkdownExporter.Console);

        var assembly = Assembly.GetExecutingAssembly();
        var benchmarkTypes = assembly.GetTypes()
            .Where(t => t.GetMethods().Any(m => m.GetCustomAttribute<BenchmarkDotNet.Attributes.BenchmarkAttribute>() != null))
            .ToArray();

        foreach (var type in benchmarkTypes)
        {
            Console.WriteLine($"Quick run of {type.Name}...");
            BenchmarkRunner.Run(type, quickConfig);
        }
    }

    private static void RunMemoryStressTests()
    {
        Console.WriteLine("Running memory stress tests...");
        Console.WriteLine("Monitoring memory usage and garbage collection...");
        
        var generator = new TestDataGenerator();
        var testSizes = new[] { 10_000, 100_000, 500_000, 1_000_000 };

        foreach (var size in testSizes)
        {
            Console.WriteLine($"\nTesting with {size:N0} data points:");
            
            // Force clean state
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var memBefore = GC.GetTotalMemory(false);
            var gen0Before = GC.CollectionCount(0);
            var gen1Before = GC.CollectionCount(1);
            var gen2Before = GC.CollectionCount(2);
            
            // Generate and process data
            var data = generator.GenerateSimplePriceData(size);
            
            // Simulate indicator processing (placeholder - would use actual indicators)
            var result = new decimal[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = data[i] * 1.01m; // Simple transformation
            }
            
            var memAfter = GC.GetTotalMemory(false);
            var gen0After = GC.CollectionCount(0);
            var gen1After = GC.CollectionCount(1);
            var gen2After = GC.CollectionCount(2);
            
            Console.WriteLine($"  Memory used: {(memAfter - memBefore) / 1024.0 / 1024.0:F2} MB");
            Console.WriteLine($"  Gen 0 collections: {gen0After - gen0Before}");
            Console.WriteLine($"  Gen 1 collections: {gen1After - gen1Before}");
            Console.WriteLine($"  Gen 2 collections: {gen2After - gen2Before}");
        }
    }

    private static void GenerateComparisonReport()
    {
        Console.WriteLine("Generating comparison report...");
        
        var resultsDir = Path.Combine(Directory.GetCurrentDirectory(), "BenchmarkDotNet.Artifacts", "results");
        if (!Directory.Exists(resultsDir))
        {
            Console.WriteLine("No benchmark results found. Please run benchmarks first.");
            return;
        }

        var csvFiles = Directory.GetFiles(resultsDir, "*.csv");
        if (csvFiles.Length == 0)
        {
            Console.WriteLine("No CSV result files found.");
            return;
        }

        var reportPath = Path.Combine(resultsDir, $"ComparisonReport_{DateTime.Now:yyyyMMdd_HHmmss}.html");
        GenerateHtmlComparisonReport(csvFiles, reportPath);
        
        Console.WriteLine($"Comparison report generated: {reportPath}");
    }

    private static void GenerateCombinedReport(List<Summary> summaries)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var reportDir = Path.Combine(Directory.GetCurrentDirectory(), "BenchmarkDotNet.Artifacts", "combined");
        Directory.CreateDirectory(reportDir);

        var reportPath = Path.Combine(reportDir, $"CombinedReport_{timestamp}.md");
        
        using var writer = new StreamWriter(reportPath);
        writer.WriteLine("# Combined Benchmark Report");
        writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        writer.WriteLine();

        foreach (var summary in summaries)
        {
            writer.WriteLine($"## {summary.Title}");
            writer.WriteLine();
            
            var exporter = MarkdownExporter.GitHub;
            exporter.ExportToLog(summary, new StreamLogger(writer));
            
            writer.WriteLine();
            writer.WriteLine("---");
            writer.WriteLine();
        }

        Console.WriteLine($"Combined report saved to: {reportPath}");
    }

    private static void GenerateHtmlComparisonReport(string[] csvFiles, string outputPath)
    {
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Indicator Benchmark Comparison</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        table { border-collapse: collapse; width: 100%; margin: 20px 0; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #4CAF50; color: white; }
        tr:nth-child(even) { background-color: #f2f2f2; }
        .faster { color: green; font-weight: bold; }
        .slower { color: red; }
        h2 { color: #333; }
    </style>
</head>
<body>
    <h1>Trading Indicators Performance Comparison</h1>
    <p>Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</p>
    
    <h2>Summary</h2>
    <p>This report compares the performance of QuantConnect vs FinancialPython indicator implementations.</p>
    
    <div id='content'>
        <!-- Benchmark results will be inserted here -->
    </div>
    
    <script>
        // Add interactive features if needed
    </script>
</body>
</html>";

        File.WriteAllText(outputPath, html);
    }

    private class StreamLogger : BenchmarkDotNet.Loggers.ILogger
    {
        private readonly StreamWriter _writer;
        
        public StreamLogger(StreamWriter writer) => _writer = writer;
        
        public string Id => nameof(StreamLogger);
        public int Priority => 0;

        public void Write(BenchmarkDotNet.Loggers.LogKind logKind, string text) => _writer.Write(text);
        public void WriteLine() => _writer.WriteLine();
        public void WriteLine(BenchmarkDotNet.Loggers.LogKind logKind, string text) => _writer.WriteLine(text);
        public void Flush() => _writer.Flush();
    }

    private class CustomSummaryExporter : IExporter
    {
        public string Name => "CustomSummary";
        public void ExportToLog(Summary summary, BenchmarkDotNet.Loggers.ILogger logger) { }
        
        public IEnumerable<string> ExportToFiles(Summary summary, BenchmarkDotNet.Loggers.ILogger consoleLogger)
        {
            var reportDir = Path.Combine(Directory.GetCurrentDirectory(), "BenchmarkDotNet.Artifacts", "custom");
            Directory.CreateDirectory(reportDir);
            
            var fileName = $"{summary.Title}_{DateTime.Now:yyyyMMdd_HHmmss}_summary.txt";
            var filePath = Path.Combine(reportDir, fileName);
            
            using var writer = new StreamWriter(filePath);
            writer.WriteLine($"Benchmark: {summary.Title}");
            writer.WriteLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine();
            writer.WriteLine("Key Metrics:");
            
            foreach (var report in summary.Reports)
            {
                if (report.Success)
                {
                    writer.WriteLine($"  {report.BenchmarkCase.DisplayInfo}:");
                    writer.WriteLine($"    Mean: {report.ResultStatistics?.Mean / 1_000_000:F2} ms");
                    writer.WriteLine($"    StdDev: {report.ResultStatistics?.StandardDeviation / 1_000_000:F2} ms");
                    writer.WriteLine($"    Allocated: {report.GcStats.GetBytesAllocatedPerOperation(report.BenchmarkCase) / 1024:F2} KB");
                }
            }
            
            return new[] { filePath };
        }
    }
}