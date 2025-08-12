using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace LionFire.Trading.Indicators.Benchmarks;

/// <summary>
/// Generates consolidated performance reports from benchmark results
/// </summary>
public class ReportGenerator
{
    private readonly string _reportsPath;
    private readonly string _timestamp;
    private readonly Dictionary<string, List<BenchmarkResult>> _results;

    public ReportGenerator(string reportsPath, string timestamp)
    {
        _reportsPath = reportsPath;
        _timestamp = timestamp;
        _results = new Dictionary<string, List<BenchmarkResult>>();
    }

    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: ReportGenerator <reports-path> <timestamp>");
            Environment.Exit(1);
        }

        var generator = new ReportGenerator(args[0], args[1]);
        generator.Generate();
    }

    public void Generate()
    {
        Console.WriteLine($"Generating consolidated report for timestamp: {_timestamp}");
        
        // Load all CSV results
        LoadResults();
        
        if (_results.Count == 0)
        {
            Console.WriteLine("No benchmark results found to process.");
            return;
        }

        // Generate different report formats
        GenerateMarkdownReport();
        GenerateJsonReport();
        GenerateComparisonMatrix();
        GenerateSummaryStatistics();
        
        Console.WriteLine($"Report generation complete. Files saved to: {_reportsPath}");
    }

    private void LoadResults()
    {
        var csvFiles = Directory.GetFiles(_reportsPath, $"*{_timestamp}.csv", SearchOption.TopDirectoryOnly);
        
        foreach (var csvFile in csvFiles)
        {
            try
            {
                Console.WriteLine($"Loading: {Path.GetFileName(csvFile)}");
                var results = ParseCsvFile(csvFile);
                
                foreach (var result in results)
                {
                    if (!_results.ContainsKey(result.Benchmark))
                    {
                        _results[result.Benchmark] = new List<BenchmarkResult>();
                    }
                    _results[result.Benchmark].Add(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading {csvFile}: {ex.Message}");
            }
        }
        
        Console.WriteLine($"Loaded {_results.Count} benchmark categories");
    }

    private List<BenchmarkResult> ParseCsvFile(string csvFile)
    {
        var results = new List<BenchmarkResult>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        };

        using (var reader = new StreamReader(csvFile))
        using (var csv = new CsvReader(reader, config))
        {
            // Try to read with dynamic objects first
            var records = csv.GetRecords<dynamic>().ToList();
            
            foreach (var record in records)
            {
                var dict = record as IDictionary<string, object>;
                if (dict != null)
                {
                    results.Add(new BenchmarkResult
                    {
                        Benchmark = GetValue(dict, "Method", "Type") ?? "Unknown",
                        Mean = ParseDouble(GetValue(dict, "Mean", "Mean (ns)", "Mean (ms)")),
                        Error = ParseDouble(GetValue(dict, "Error", "StdErr")),
                        StdDev = ParseDouble(GetValue(dict, "StdDev", "StdDev (ns)", "StdDev (ms)")),
                        Median = ParseDouble(GetValue(dict, "Median", "Median (ns)", "Median (ms)")),
                        Min = ParseDouble(GetValue(dict, "Min", "Min (ns)", "Min (ms)")),
                        Max = ParseDouble(GetValue(dict, "Max", "Max (ns)", "Max (ms)")),
                        Allocated = ParseMemory(GetValue(dict, "Allocated", "Allocated (KB)", "Gen0"))
                    });
                }
            }
        }
        
        return results;
    }

    private string GetValue(IDictionary<string, object> dict, params string[] possibleKeys)
    {
        foreach (var key in possibleKeys)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key]?.ToString();
            }
        }
        return null;
    }

    private double ParseDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        // Remove units and parse
        value = Regex.Replace(value, @"[^\d.-]", "");
        
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
            
        return 0;
    }

    private long ParseMemory(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        // Parse memory values (handle KB, MB, etc.)
        var match = Regex.Match(value, @"([\d.]+)\s*([KMG]?B)?");
        if (match.Success && double.TryParse(match.Groups[1].Value, out var number))
        {
            var unit = match.Groups[2].Value.ToUpper();
            return unit switch
            {
                "KB" => (long)(number * 1024),
                "MB" => (long)(number * 1024 * 1024),
                "GB" => (long)(number * 1024 * 1024 * 1024),
                _ => (long)number
            };
        }
        
        return 0;
    }

    private void GenerateMarkdownReport()
    {
        var reportPath = Path.Combine(_reportsPath, $"PerformanceReport_{_timestamp}.md");
        var sb = new StringBuilder();
        
        sb.AppendLine("# Performance Benchmark Report");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        
        // Executive Summary
        sb.AppendLine("## Executive Summary");
        sb.AppendLine();
        
        var fpResults = _results.Where(r => r.Key.Contains("FP")).ToList();
        var qcResults = _results.Where(r => r.Key.Contains("QC")).ToList();
        
        if (fpResults.Any() && qcResults.Any())
        {
            var fpAvgTime = fpResults.SelectMany(r => r.Value).Average(r => r.Mean);
            var qcAvgTime = qcResults.SelectMany(r => r.Value).Average(r => r.Mean);
            var speedup = qcAvgTime / fpAvgTime;
            
            sb.AppendLine($"- **Financial Python Average Time**: {fpAvgTime:F2} ns");
            sb.AppendLine($"- **QuantConnect Average Time**: {qcAvgTime:F2} ns");
            sb.AppendLine($"- **Performance Ratio**: {speedup:F2}x (FP is {(speedup > 1 ? "faster" : "slower")})");
        }
        
        sb.AppendLine();
        sb.AppendLine("## Detailed Results");
        sb.AppendLine();
        
        // Group by indicator type
        var indicatorGroups = _results.GroupBy(r => ExtractIndicatorName(r.Key));
        
        foreach (var group in indicatorGroups.OrderBy(g => g.Key))
        {
            sb.AppendLine($"### {group.Key}");
            sb.AppendLine();
            sb.AppendLine("| Implementation | Mean (ns) | Error (ns) | StdDev (ns) | Median (ns) | Allocated (bytes) |");
            sb.AppendLine("|---------------|-----------|------------|-------------|-------------|-------------------|");
            
            foreach (var result in group.OrderBy(r => r.Key))
            {
                var stats = CalculateStatistics(result.Value);
                sb.AppendLine($"| {ExtractImplementation(result.Key)} | {stats.Mean:F2} | {stats.Error:F2} | {stats.StdDev:F2} | {stats.Median:F2} | {stats.Allocated:N0} |");
            }
            
            // Add comparison if both implementations exist
            var fpImpl = group.FirstOrDefault(r => r.Key.Contains("FP"));
            var qcImpl = group.FirstOrDefault(r => r.Key.Contains("QC"));
            
            if (fpImpl != null && qcImpl != null)
            {
                var fpStats = CalculateStatistics(fpImpl.Value);
                var qcStats = CalculateStatistics(qcImpl.Value);
                var ratio = qcStats.Mean / fpStats.Mean;
                var memRatio = (double)qcStats.Allocated / fpStats.Allocated;
                
                sb.AppendLine();
                sb.AppendLine($"**Performance Comparison:**");
                sb.AppendLine($"- Speed: FP is {ratio:F2}x {(ratio > 1 ? "faster" : "slower")} than QC");
                sb.AppendLine($"- Memory: FP uses {memRatio:F2}x {(memRatio > 1 ? "more" : "less")} memory than QC");
            }
            
            sb.AppendLine();
        }
        
        // Add recommendations
        sb.AppendLine("## Recommendations");
        sb.AppendLine();
        GenerateRecommendations(sb);
        
        File.WriteAllText(reportPath, sb.ToString());
        Console.WriteLine($"Generated Markdown report: {Path.GetFileName(reportPath)}");
    }

    private void GenerateJsonReport()
    {
        var reportPath = Path.Combine(_reportsPath, $"PerformanceReport_{_timestamp}.json");
        
        var jsonData = new
        {
            Timestamp = _timestamp,
            Generated = DateTime.Now,
            Results = _results.Select(r => new
            {
                Benchmark = r.Key,
                Indicator = ExtractIndicatorName(r.Key),
                Implementation = ExtractImplementation(r.Key),
                Statistics = CalculateStatistics(r.Value)
            }),
            Summary = GenerateSummary()
        };
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        
        var json = JsonSerializer.Serialize(jsonData, options);
        File.WriteAllText(reportPath, json);
        
        Console.WriteLine($"Generated JSON report: {Path.GetFileName(reportPath)}");
    }

    private void GenerateComparisonMatrix()
    {
        var reportPath = Path.Combine(_reportsPath, $"ComparisonMatrix_{_timestamp}.csv");
        
        using (var writer = new StreamWriter(reportPath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            // Write header
            csv.WriteField("Indicator");
            csv.WriteField("FP Mean (ns)");
            csv.WriteField("QC Mean (ns)");
            csv.WriteField("Speed Ratio");
            csv.WriteField("FP Memory (bytes)");
            csv.WriteField("QC Memory (bytes)");
            csv.WriteField("Memory Ratio");
            csv.WriteField("Winner");
            csv.NextRecord();
            
            var indicatorGroups = _results.GroupBy(r => ExtractIndicatorName(r.Key));
            
            foreach (var group in indicatorGroups.OrderBy(g => g.Key))
            {
                var fpImpl = group.FirstOrDefault(r => r.Key.Contains("FP"));
                var qcImpl = group.FirstOrDefault(r => r.Key.Contains("QC"));
                
                if (fpImpl != null && qcImpl != null)
                {
                    var fpStats = CalculateStatistics(fpImpl.Value);
                    var qcStats = CalculateStatistics(qcImpl.Value);
                    var speedRatio = qcStats.Mean / fpStats.Mean;
                    var memRatio = (double)qcStats.Allocated / fpStats.Allocated;
                    
                    csv.WriteField(group.Key);
                    csv.WriteField(fpStats.Mean);
                    csv.WriteField(qcStats.Mean);
                    csv.WriteField(speedRatio);
                    csv.WriteField(fpStats.Allocated);
                    csv.WriteField(qcStats.Allocated);
                    csv.WriteField(memRatio);
                    csv.WriteField(speedRatio > 1 ? "FP" : "QC");
                    csv.NextRecord();
                }
            }
        }
        
        Console.WriteLine($"Generated comparison matrix: {Path.GetFileName(reportPath)}");
    }

    private void GenerateSummaryStatistics()
    {
        var reportPath = Path.Combine(_reportsPath, $"SummaryStatistics_{_timestamp}.txt");
        var sb = new StringBuilder();
        
        sb.AppendLine("BENCHMARK SUMMARY STATISTICS");
        sb.AppendLine("=" + new string('=', 50));
        sb.AppendLine();
        
        var summary = GenerateSummary();
        
        sb.AppendLine($"Total Benchmarks Run: {summary.TotalBenchmarks}");
        sb.AppendLine($"Total Indicators Tested: {summary.IndicatorCount}");
        sb.AppendLine();
        
        if (summary.FpWins > 0 || summary.QcWins > 0)
        {
            sb.AppendLine("PERFORMANCE WINNERS:");
            sb.AppendLine($"  Financial Python: {summary.FpWins} indicators");
            sb.AppendLine($"  QuantConnect: {summary.QcWins} indicators");
            sb.AppendLine($"  Average Speed Advantage: {summary.AverageSpeedRatio:F2}x");
            sb.AppendLine();
        }
        
        sb.AppendLine("TOP PERFORMERS:");
        foreach (var top in summary.TopPerformers.Take(5))
        {
            sb.AppendLine($"  {top.Indicator}: {top.MeanTime:F2} ns ({top.Implementation})");
        }
        
        sb.AppendLine();
        sb.AppendLine("MEMORY USAGE:");
        sb.AppendLine($"  Average FP: {summary.AverageFpMemory:N0} bytes");
        sb.AppendLine($"  Average QC: {summary.AverageQcMemory:N0} bytes");
        
        File.WriteAllText(reportPath, sb.ToString());
        Console.WriteLine($"Generated summary statistics: {Path.GetFileName(reportPath)}");
    }

    private void GenerateRecommendations(StringBuilder sb)
    {
        var indicatorGroups = _results.GroupBy(r => ExtractIndicatorName(r.Key));
        var recommendations = new List<string>();
        
        foreach (var group in indicatorGroups)
        {
            var fpImpl = group.FirstOrDefault(r => r.Key.Contains("FP"));
            var qcImpl = group.FirstOrDefault(r => r.Key.Contains("QC"));
            
            if (fpImpl != null && qcImpl != null)
            {
                var fpStats = CalculateStatistics(fpImpl.Value);
                var qcStats = CalculateStatistics(qcImpl.Value);
                var speedRatio = qcStats.Mean / fpStats.Mean;
                
                if (speedRatio > 1.5)
                {
                    recommendations.Add($"- **{group.Key}**: Use Financial Python implementation (FP is {speedRatio:F1}x faster)");
                }
                else if (speedRatio < 0.67)
                {
                    recommendations.Add($"- **{group.Key}**: Use QuantConnect implementation (QC is {1/speedRatio:F1}x faster)");
                }
                else
                {
                    recommendations.Add($"- **{group.Key}**: Performance is comparable (ratio: {speedRatio:F2}), choose based on other factors");
                }
            }
        }
        
        foreach (var rec in recommendations.OrderBy(r => r))
        {
            sb.AppendLine(rec);
        }
        
        sb.AppendLine();
        sb.AppendLine("### General Observations");
        
        var overallFpAvg = _results.Where(r => r.Key.Contains("FP"))
            .SelectMany(r => r.Value)
            .Average(r => r.Mean);
        var overallQcAvg = _results.Where(r => r.Key.Contains("QC"))
            .SelectMany(r => r.Value)
            .Average(r => r.Mean);
        
        if (overallFpAvg < overallQcAvg)
        {
            sb.AppendLine("- Overall, Financial Python implementations show better performance");
            sb.AppendLine("- Consider using FP as the default implementation for new indicators");
        }
        else
        {
            sb.AppendLine("- Overall, QuantConnect implementations show better performance");
            sb.AppendLine("- Consider using QC as the default implementation for new indicators");
        }
    }

    private BenchmarkStatistics CalculateStatistics(List<BenchmarkResult> results)
    {
        if (results == null || results.Count == 0)
        {
            return new BenchmarkStatistics();
        }
        
        return new BenchmarkStatistics
        {
            Mean = results.Average(r => r.Mean),
            Error = results.Average(r => r.Error),
            StdDev = results.Average(r => r.StdDev),
            Median = results.Average(r => r.Median),
            Min = results.Min(r => r.Min),
            Max = results.Max(r => r.Max),
            Allocated = (long)results.Average(r => r.Allocated)
        };
    }

    private Summary GenerateSummary()
    {
        var summary = new Summary
        {
            TotalBenchmarks = _results.Count,
            IndicatorCount = _results.GroupBy(r => ExtractIndicatorName(r.Key)).Count(),
            TopPerformers = new List<TopPerformer>()
        };
        
        var indicatorGroups = _results.GroupBy(r => ExtractIndicatorName(r.Key));
        
        foreach (var group in indicatorGroups)
        {
            var fpImpl = group.FirstOrDefault(r => r.Key.Contains("FP"));
            var qcImpl = group.FirstOrDefault(r => r.Key.Contains("QC"));
            
            if (fpImpl != null)
            {
                var fpStats = CalculateStatistics(fpImpl.Value);
                summary.TopPerformers.Add(new TopPerformer
                {
                    Indicator = group.Key,
                    Implementation = "FP",
                    MeanTime = fpStats.Mean
                });
                summary.AverageFpMemory += fpStats.Allocated;
            }
            
            if (qcImpl != null)
            {
                var qcStats = CalculateStatistics(qcImpl.Value);
                summary.TopPerformers.Add(new TopPerformer
                {
                    Indicator = group.Key,
                    Implementation = "QC",
                    MeanTime = qcStats.Mean
                });
                summary.AverageQcMemory += qcStats.Allocated;
            }
            
            if (fpImpl != null && qcImpl != null)
            {
                var fpStats = CalculateStatistics(fpImpl.Value);
                var qcStats = CalculateStatistics(qcImpl.Value);
                
                if (fpStats.Mean < qcStats.Mean)
                {
                    summary.FpWins++;
                    summary.AverageSpeedRatio += qcStats.Mean / fpStats.Mean;
                }
                else
                {
                    summary.QcWins++;
                    summary.AverageSpeedRatio += fpStats.Mean / qcStats.Mean;
                }
            }
        }
        
        if (summary.FpWins + summary.QcWins > 0)
        {
            summary.AverageSpeedRatio /= (summary.FpWins + summary.QcWins);
        }
        
        var fpCount = _results.Count(r => r.Key.Contains("FP"));
        var qcCount = _results.Count(r => r.Key.Contains("QC"));
        
        if (fpCount > 0)
            summary.AverageFpMemory /= fpCount;
        if (qcCount > 0)
            summary.AverageQcMemory /= qcCount;
        
        summary.TopPerformers = summary.TopPerformers.OrderBy(t => t.MeanTime).ToList();
        
        return summary;
    }

    private string ExtractIndicatorName(string benchmarkName)
    {
        // Extract indicator name from benchmark name
        var match = Regex.Match(benchmarkName, @"(\w+)_(FP|QC)");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        
        // Alternative pattern
        match = Regex.Match(benchmarkName, @"(\w+)Benchmark");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        
        return benchmarkName;
    }

    private string ExtractImplementation(string benchmarkName)
    {
        if (benchmarkName.Contains("FP"))
            return "Financial Python";
        if (benchmarkName.Contains("QC"))
            return "QuantConnect";
        return "Unknown";
    }

    // Data classes
    private class BenchmarkResult
    {
        public string Benchmark { get; set; }
        public double Mean { get; set; }
        public double Error { get; set; }
        public double StdDev { get; set; }
        public double Median { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public long Allocated { get; set; }
    }

    private class BenchmarkStatistics
    {
        public double Mean { get; set; }
        public double Error { get; set; }
        public double StdDev { get; set; }
        public double Median { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public long Allocated { get; set; }
    }

    private class Summary
    {
        public int TotalBenchmarks { get; set; }
        public int IndicatorCount { get; set; }
        public int FpWins { get; set; }
        public int QcWins { get; set; }
        public double AverageSpeedRatio { get; set; }
        public long AverageFpMemory { get; set; }
        public long AverageQcMemory { get; set; }
        public List<TopPerformer> TopPerformers { get; set; }
    }

    private class TopPerformer
    {
        public string Indicator { get; set; }
        public string Implementation { get; set; }
        public double MeanTime { get; set; }
    }
}