using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Running;
using LionFire.Trading.Indicators;
using System.Collections.Generic;
using System.Linq;

namespace LionFire.Trading.Indicators.Benchmarks;

/// <summary>
/// Base class for all indicator benchmarks providing common configuration and data generation
/// </summary>
[MemoryDiagnoser]
[ThreadingDiagnoser]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[Config(typeof(BenchmarkConfig))]
public abstract class IndicatorBenchmarkBase
{
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddJob(Job.Default
                .WithWarmupCount(3)
                .WithIterationCount(10)
                .WithLaunchCount(1)
                .WithUnrollFactor(1));

            AddDiagnoser(MemoryDiagnoser.Default);
            AddColumn(StatisticColumn.Mean);
            AddColumn(StatisticColumn.Median);
            AddColumn(StatisticColumn.StdDev);
            AddColumn(StatisticColumn.Min);
            AddColumn(StatisticColumn.Max);
            AddColumn(new ThroughputColumn());
            
            AddExporter(HtmlExporter.Default);
            AddExporter(CsvExporter.Default);
            AddExporter(MarkdownExporter.GitHub);
            
            WithOptions(ConfigOptions.DisableOptimizationsValidator);
        }
    }

    /// <summary>
    /// Custom column to show throughput in data points per second
    /// </summary>
    public class ThroughputColumn : IColumn
    {
        public string Id => nameof(ThroughputColumn);
        public string ColumnName => "Throughput (pts/sec)";
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Custom;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Dimensionless;
        public string Legend => "Data points processed per second";

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            return GetValue(summary, benchmarkCase, SummaryStyle.Default);
        }

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        {
            var report = summary[benchmarkCase];
            if (report == null || !report.Success) return "N/A";

            var meanTime = report.ResultStatistics?.Mean ?? 0;
            if (meanTime <= 0) return "N/A";

            // Get data size from parameters
            var dataSizeParam = benchmarkCase.Parameters["DataSize"];
            if (dataSizeParam == null) return "N/A";

            var dataSize = (int)dataSizeParam.Value;
            var throughput = dataSize / (meanTime / 1_000_000_000); // Convert nanoseconds to seconds
            
            return $"{throughput:N0}";
        }

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;
        public bool IsAvailable(Summary summary) => true;
    }

    [Params(1_000, 10_000, 100_000, 1_000_000)]
    public int DataSize { get; set; }

    [Params(14, 20, 50, 200)]
    public int Period { get; set; }

    protected decimal[] PriceData = null!;
    protected HLCData[] HLCData = null!;
    protected DateTime[] Timestamps = null!;

    /// <summary>
    /// Market condition for data generation
    /// </summary>
    [Params(MarketCondition.Trending, MarketCondition.Sideways, MarketCondition.Volatile)]
    public MarketCondition Condition { get; set; }

    [GlobalSetup]
    public virtual void GlobalSetup()
    {
        var generator = new TestDataGenerator();
        
        // Generate data based on market condition
        var dataPoints = Condition switch
        {
            MarketCondition.Trending => generator.GenerateTrendingData(DataSize),
            MarketCondition.Sideways => generator.GenerateSidewaysData(DataSize),
            MarketCondition.Volatile => generator.GenerateVolatileData(DataSize),
            _ => generator.GenerateRealisticData(DataSize)
        };

        PriceData = dataPoints.Select(d => d.Close).ToArray();
        HLCData = dataPoints.Select(d => new HLCData 
        { 
            High = d.High, 
            Low = d.Low, 
            Close = d.Close 
        }).ToArray();
        Timestamps = dataPoints.Select(d => d.Timestamp).ToArray();
    }

    [GlobalCleanup]
    public virtual void GlobalCleanup()
    {
        PriceData = null!;
        HLCData = null!;
        Timestamps = null!;
        
        // Force garbage collection to clean up
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    /// <summary>
    /// Helper method to measure memory allocations for a specific operation
    /// </summary>
    protected static long MeasureAllocations(Action action)
    {
        var before = GC.GetTotalAllocatedBytes();
        action();
        var after = GC.GetTotalAllocatedBytes();
        return after - before;
    }

    /// <summary>
    /// Helper method to get memory usage in MB
    /// </summary>
    protected static double GetMemoryUsageMB()
    {
        return GC.GetTotalMemory(false) / (1024.0 * 1024.0);
    }

    /// <summary>
    /// Validates that two indicator outputs are equivalent within tolerance
    /// </summary>
    protected bool ValidateOutputs(IEnumerable<decimal?> output1, IEnumerable<decimal?> output2, decimal tolerance = 0.0001m)
    {
        var arr1 = output1.ToArray();
        var arr2 = output2.ToArray();

        if (arr1.Length != arr2.Length) return false;

        for (int i = 0; i < arr1.Length; i++)
        {
            if (arr1[i].HasValue != arr2[i].HasValue) return false;
            if (arr1[i].HasValue && Math.Abs(arr1[i]!.Value - arr2[i]!.Value) > tolerance) return false;
        }

        return true;
    }
}

public enum MarketCondition
{
    Trending,
    Sideways,
    Volatile,
    Realistic
}

public struct HLCData
{
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
}