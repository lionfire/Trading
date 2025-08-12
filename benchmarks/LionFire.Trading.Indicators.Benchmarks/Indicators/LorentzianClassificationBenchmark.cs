using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Exporters;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.Benchmarks;
using LionFire.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

namespace LionFire.Trading.Indicators.Benchmarks.Indicators;

/// <summary>
/// Comprehensive benchmarks for the Lorentzian Classification indicator.
/// Tests performance across different parameter configurations, data sizes, and market conditions.
/// Measures initialization, single updates, batch processing, feature extraction, k-NN search, and memory usage.
/// </summary>
[MemoryDiagnoser]
[ThreadingDiagnoser]
[EventPipeProfiler(EventPipeProfile.CpuSampling)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[Config(typeof(LorentzianBenchmarkConfig))]
public class LorentzianClassificationBenchmark : IndicatorBenchmarkBase
{
    #region Configuration

    public class LorentzianBenchmarkConfig : ManualConfig
    {
        public LorentzianBenchmarkConfig()
        {
            AddJob(Job.Default
                .WithWarmupCount(2)
                .WithIterationCount(5)
                .WithLaunchCount(1)
                .WithUnrollFactor(1)
                .WithGcServer(true)
                .WithGcConcurrent(true));

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

    #endregion

    #region Parameters

    /// <summary>
    /// Different K values for k-NN classification
    /// </summary>
    [Params(4, 8, 16, 32)]
    public int KValue { get; set; }

    /// <summary>
    /// Different lookback periods for historical pattern storage
    /// </summary>
    [Params(50, 100, 500)]
    public int LookbackPeriod { get; set; }

    /// <summary>
    /// Different normalization window sizes
    /// </summary>
    [Params(10, 20, 50)]
    public int NormalizationWindow { get; set; }

    /// <summary>
    /// Different numeric types for performance comparison
    /// </summary>
    [Params(NumericType.Double, NumericType.Float, NumericType.Decimal)]
    public NumericType NumericType { get; set; }

    // Override base class parameters for LC-specific values
    [Params(1_000, 10_000, 100_000)]
    public new int DataSize { get; set; }

    #endregion

    #region Fields

    private LorentzianClassification_FP<decimal, double>? _indicatorDouble;
    private LorentzianClassification_FP<float, float>? _indicatorFloat;
    private LorentzianClassification_FP<decimal, decimal>? _indicatorDecimal;
    
    private OHLC<decimal>[] _ohlcDataDecimal = null!;
    private OHLC<float>[] _ohlcDataFloat = null!;
    
    private PLorentzianClassification<decimal, double> _parametersDouble = null!;
    private PLorentzianClassification<float, float> _parametersFloat = null!;
    private PLorentzianClassification<decimal, decimal> _parametersDecimal = null!;

    // Batch test data
    private List<OHLC<decimal>> _batchDataSmall = null!;
    private List<OHLC<decimal>> _batchDataMedium = null!;
    private List<OHLC<decimal>> _batchDataLarge = null!;

    #endregion

    #region Setup

    [GlobalSetup]
    public override void GlobalSetup()
    {
        base.GlobalSetup();
        
        // Generate OHLC data from price data
        _ohlcDataDecimal = GenerateOHLCDataDecimal(DataSize);
        _ohlcDataFloat = GenerateOHLCDataFloat(DataSize);
        
        // Create parameter sets for different numeric types
        _parametersDouble = new PLorentzianClassification<decimal, double>
        {
            NeighborsCount = KValue,
            LookbackPeriod = LookbackPeriod,
            NormalizationWindow = NormalizationWindow,
            RSIPeriod = Period / 2, // Use half of period for RSI
            CCIPeriod = Period,
            ADXPeriod = Period / 2,
            MinConfidence = 0.6,
            LabelLookahead = 5,
            LabelThreshold = 0.01
        };

        _parametersFloat = new PLorentzianClassification<float, float>
        {
            NeighborsCount = KValue,
            LookbackPeriod = LookbackPeriod,
            NormalizationWindow = NormalizationWindow,
            RSIPeriod = Period / 2,
            CCIPeriod = Period,
            ADXPeriod = Period / 2,
            MinConfidence = 0.6,
            LabelLookahead = 5,
            LabelThreshold = 0.01
        };

        _parametersDecimal = new PLorentzianClassification<decimal, decimal>
        {
            NeighborsCount = KValue,
            LookbackPeriod = LookbackPeriod,
            NormalizationWindow = NormalizationWindow,
            RSIPeriod = Period / 2,
            CCIPeriod = Period,
            ADXPeriod = Period / 2,
            MinConfidence = 0.6m,
            LabelLookahead = 5,
            LabelThreshold = 0.01
        };

        // Initialize indicators
        _indicatorDouble = new LorentzianClassification_FP<decimal, double>(_parametersDouble);
        _indicatorFloat = new LorentzianClassification_FP<float, float>(_parametersFloat);
        _indicatorDecimal = new LorentzianClassification_FP<decimal, decimal>(_parametersDecimal);

        // Create batch test data
        _batchDataSmall = _ohlcDataDecimal.Take(1_000).ToList();
        _batchDataMedium = _ohlcDataDecimal.Take(10_000).ToList();
        _batchDataLarge = _ohlcDataDecimal.Take(100_000).ToList();
    }

    #endregion

    #region Initialization Benchmarks

    [Benchmark]
    [BenchmarkCategory("Initialization", "Double")]
    public LorentzianClassification_FP<decimal, double> InitializeDouble()
    {
        var parameters = new PLorentzianClassification<decimal, double>
        {
            NeighborsCount = KValue,
            LookbackPeriod = LookbackPeriod,
            NormalizationWindow = NormalizationWindow,
            RSIPeriod = 14,
            CCIPeriod = 20,
            ADXPeriod = 14
        };
        
        var indicator = new LorentzianClassification_FP<decimal, double>(parameters);
        return indicator;
    }

    [Benchmark]
    [BenchmarkCategory("Initialization", "Float")]
    public LorentzianClassification_FP<float, float> InitializeFloat()
    {
        var parameters = new PLorentzianClassification<float, float>
        {
            NeighborsCount = KValue,
            LookbackPeriod = LookbackPeriod,
            NormalizationWindow = NormalizationWindow,
            RSIPeriod = 14,
            CCIPeriod = 20,
            ADXPeriod = 14
        };
        
        var indicator = new LorentzianClassification_FP<float, float>(parameters);
        return indicator;
    }

    [Benchmark]
    [BenchmarkCategory("Initialization", "Decimal")]
    public LorentzianClassification_FP<decimal, decimal> InitializeDecimal()
    {
        var parameters = new PLorentzianClassification<decimal, decimal>
        {
            NeighborsCount = KValue,
            LookbackPeriod = LookbackPeriod,
            NormalizationWindow = NormalizationWindow,
            RSIPeriod = 14,
            CCIPeriod = 20,
            ADXPeriod = 14
        };
        
        var indicator = new LorentzianClassification_FP<decimal, decimal>(parameters);
        return indicator;
    }

    #endregion

    #region Single Update Benchmarks

    [Benchmark]
    [BenchmarkCategory("SingleUpdate", "Double")]
    [OperationsPerInvoke(1)]
    public void SingleUpdateDouble()
    {
        _indicatorDouble!.Clear();
        
        // Warm up with initial data
        int warmUpSize = Math.Min(50, _ohlcDataDecimal.Length);
        for (int i = 0; i < warmUpSize; i++)
        {
            _indicatorDouble.OnBarBatch([_ohlcDataDecimal[i]], null);
        }
        
        // Measure single update performance
        _indicatorDouble.OnBarBatch([_ohlcDataDecimal[warmUpSize]], null);
    }

    [Benchmark]
    [BenchmarkCategory("SingleUpdate", "Float")]
    [OperationsPerInvoke(1)]
    public void SingleUpdateFloat()
    {
        _indicatorFloat!.Clear();
        
        // Warm up with initial data
        int warmUpSize = Math.Min(50, _ohlcDataFloat.Length);
        for (int i = 0; i < warmUpSize; i++)
        {
            _indicatorFloat.OnBarBatch([_ohlcDataFloat[i]], null);
        }
        
        // Measure single update performance
        _indicatorFloat.OnBarBatch([_ohlcDataFloat[warmUpSize]], null);
    }

    [Benchmark]
    [BenchmarkCategory("SingleUpdate", "Decimal")]
    [OperationsPerInvoke(1)]
    public void SingleUpdateDecimal()
    {
        _indicatorDecimal!.Clear();
        
        // Warm up with initial data
        int warmUpSize = Math.Min(50, _ohlcDataDecimal.Length);
        for (int i = 0; i < warmUpSize; i++)
        {
            _indicatorDecimal.OnBarBatch([_ohlcDataDecimal[i]], null);
        }
        
        // Measure single update performance
        _indicatorDecimal.OnBarBatch([_ohlcDataDecimal[warmUpSize]], null);
    }

    #endregion

    #region Batch Processing Benchmarks

    [Benchmark]
    [BenchmarkCategory("BatchProcessing", "Small")]
    [OperationsPerInvoke(1000)]
    public (double signal, double confidence) BatchProcessingSmall()
    {
        _indicatorDouble!.Clear();
        _indicatorDouble.OnBarBatch(_batchDataSmall, null);
        return (_indicatorDouble.Signal, _indicatorDouble.Confidence);
    }

    [Benchmark]
    [BenchmarkCategory("BatchProcessing", "Medium")]
    [OperationsPerInvoke(10000)]
    public (double signal, double confidence) BatchProcessingMedium()
    {
        _indicatorDouble!.Clear();
        _indicatorDouble.OnBarBatch(_batchDataMedium, null);
        return (_indicatorDouble.Signal, _indicatorDouble.Confidence);
    }

    [Benchmark]
    [BenchmarkCategory("BatchProcessing", "Large")]
    [OperationsPerInvoke(100000)]
    public (double signal, double confidence) BatchProcessingLarge()
    {
        _indicatorDouble!.Clear();
        _indicatorDouble.OnBarBatch(_batchDataLarge, null);
        return (_indicatorDouble.Signal, _indicatorDouble.Confidence);
    }

    #endregion

    #region Feature Extraction Benchmarks

    [Benchmark]
    [BenchmarkCategory("FeatureExtraction", "Overhead")]
    public double[] FeatureExtractionOverhead()
    {
        _indicatorDouble!.Clear();
        
        // Process enough data to get features
        var warmUpData = _ohlcDataDecimal.Take(Math.Max(50, Period * 2)).ToList();
        _indicatorDouble.OnBarBatch(warmUpData, null);
        
        // Get current features (this triggers feature extraction)
        return _indicatorDouble.CurrentFeatures;
    }

    #endregion

    #region k-NN Search Performance

    [Benchmark]
    [BenchmarkCategory("KNN", "SearchPerformance")]
    public (double signal, double confidence) KNNSearchPerformance()
    {
        _indicatorDouble!.Clear();
        
        // Fill up historical patterns to test k-NN search
        var data = _ohlcDataDecimal.Take(LookbackPeriod + 100).ToList();
        _indicatorDouble.OnBarBatch(data, null);
        
        // The search happens during the last update
        return (_indicatorDouble.Signal, _indicatorDouble.Confidence);
    }

    #endregion

    #region Memory Usage Benchmarks

    [Benchmark]
    [BenchmarkCategory("Memory", "AllocationsPerUpdate")]
    public long MemoryAllocationsPerUpdate()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new LorentzianClassification_FP<decimal, double>(_parametersDouble);
            
            // Process a moderate amount of data
            var testData = _ohlcDataDecimal.Take(200).ToList();
            indicator.OnBarBatch(testData, null);
            
            indicator.Clear();
        });
    }

    [Benchmark]
    [BenchmarkCategory("Memory", "FullCycleAllocation")]
    public long MemoryFullCycleAllocation()
    {
        return MeasureAllocations(() =>
        {
            var indicator = new LorentzianClassification_FP<decimal, double>(_parametersDouble);
            
            // Full processing cycle
            indicator.OnBarBatch(_ohlcDataDecimal, null);
            
            // Get results
            var signal = indicator.Signal;
            var confidence = indicator.Confidence;
            var features = indicator.CurrentFeatures;
            var patternCount = indicator.HistoricalPatternsCount;
            
            indicator.Clear();
        });
    }

    #endregion

    #region Market Condition Performance

    [Benchmark]
    [BenchmarkCategory("MarketCondition", "Trending")]
    public (double signal, double confidence) TrendingMarketPerformance()
    {
        _indicatorDouble!.Clear();
        
        // Generate trending market data
        var generator = new TestDataGenerator(42); // Fixed seed for consistency
        var trendingData = generator.GenerateTrendingData(DataSize / 10, bullish: true);
        var ohlcData = ConvertToOHLC(trendingData);
        
        _indicatorDouble.OnBarBatch(ohlcData, null);
        return (_indicatorDouble.Signal, _indicatorDouble.Confidence);
    }

    [Benchmark]
    [BenchmarkCategory("MarketCondition", "Sideways")]
    public (double signal, double confidence) SidewaysMarketPerformance()
    {
        _indicatorDouble!.Clear();
        
        // Generate sideways market data
        var generator = new TestDataGenerator(42);
        var sidewaysData = generator.GenerateSidewaysData(DataSize / 10);
        var ohlcData = ConvertToOHLC(sidewaysData);
        
        _indicatorDouble.OnBarBatch(ohlcData, null);
        return (_indicatorDouble.Signal, _indicatorDouble.Confidence);
    }

    [Benchmark]
    [BenchmarkCategory("MarketCondition", "Volatile")]
    public (double signal, double confidence) VolatileMarketPerformance()
    {
        _indicatorDouble!.Clear();
        
        // Generate volatile market data
        var generator = new TestDataGenerator(42);
        var volatileData = generator.GenerateVolatileData(DataSize / 10);
        var ohlcData = ConvertToOHLC(volatileData);
        
        _indicatorDouble.OnBarBatch(ohlcData, null);
        return (_indicatorDouble.Signal, _indicatorDouble.Confidence);
    }

    #endregion

    #region Streaming vs Backtesting Comparison

    [Benchmark]
    [BenchmarkCategory("Usage", "StreamingSimulation")]
    public int StreamingSimulation()
    {
        _indicatorDouble!.Clear();
        int signalChanges = 0;
        double previousSignal = 0;
        
        // Simulate real-time streaming: process data one by one
        foreach (var bar in _ohlcDataDecimal.Take(1000))
        {
            _indicatorDouble.OnBarBatch([bar], null);
            
            if (Math.Abs(_indicatorDouble.Signal - previousSignal) > 0.1)
            {
                signalChanges++;
                previousSignal = _indicatorDouble.Signal;
            }
        }
        
        return signalChanges;
    }

    [Benchmark]
    [BenchmarkCategory("Usage", "BacktestingSimulation")]
    public int BacktestingSimulation()
    {
        _indicatorDouble!.Clear();
        int signalChanges = 0;
        double previousSignal = 0;
        
        // Simulate backtesting: process data in batches
        var batchSize = 100;
        var data = _ohlcDataDecimal.Take(1000).ToList();
        
        for (int i = 0; i < data.Count; i += batchSize)
        {
            var batch = data.Skip(i).Take(batchSize).ToList();
            _indicatorDouble.OnBarBatch(batch, null);
            
            if (Math.Abs(_indicatorDouble.Signal - previousSignal) > 0.1)
            {
                signalChanges++;
                previousSignal = _indicatorDouble.Signal;
            }
        }
        
        return signalChanges;
    }

    #endregion

    #region Helper Methods

    private OHLC<decimal>[] GenerateOHLCDataDecimal(int count)
    {
        var generator = new TestDataGenerator();
        var marketData = Condition switch
        {
            MarketCondition.Trending => generator.GenerateTrendingData(count),
            MarketCondition.Sideways => generator.GenerateSidewaysData(count),
            MarketCondition.Volatile => generator.GenerateVolatileData(count),
            _ => generator.GenerateRealisticData(count)
        };

        return ConvertToOHLC(marketData);
    }

    private OHLC<float>[] GenerateOHLCDataFloat(int count)
    {
        var decimalData = GenerateOHLCDataDecimal(count);
        return decimalData.Select(d => new OHLC<float>
        {
            Open = (float)d.Open,
            High = (float)d.High,
            Low = (float)d.Low,
            Close = (float)d.Close
        }).ToArray();
    }

    private static OHLC<decimal>[] ConvertToOHLC(List<TestDataGenerator.MarketDataPoint> marketData)
    {
        return marketData.Select(d => new OHLC<decimal>
        {
            Open = d.Open,
            High = d.High,
            Low = d.Low,
            Close = d.Close
        }).ToArray();
    }

    #endregion

    #region Cleanup

    [GlobalCleanup]
    public override void GlobalCleanup()
    {
        _indicatorDouble?.Clear();
        _indicatorFloat?.Clear();
        _indicatorDecimal?.Clear();
        
        _indicatorDouble = null;
        _indicatorFloat = null;
        _indicatorDecimal = null;
        
        _ohlcDataDecimal = null!;
        _ohlcDataFloat = null!;
        _batchDataSmall = null!;
        _batchDataMedium = null!;
        _batchDataLarge = null!;
        
        base.GlobalCleanup();
    }

    #endregion
}

/// <summary>
/// Enumeration for different numeric types to test performance
/// </summary>
public enum NumericType
{
    Double,
    Float,
    Decimal
}