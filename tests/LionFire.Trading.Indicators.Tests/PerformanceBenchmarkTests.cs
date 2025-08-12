using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace LionFire.Trading.Indicators.Tests;

public class PerformanceBenchmarkTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceBenchmarkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Benchmark_SMA_Performance()
    {
        // Arrange
        var periods = new[] { 10, 50, 200 };
        var dataSizes = new[] { 1000, 10000, 100000 };
        
        foreach (var period in periods)
        {
            foreach (var dataSize in dataSizes)
            {
                var parameters = new PSMA<double, double> { Period = period };
                var sma = new SMA_QC<double, double>(parameters);
                var inputs = GenerateTestData(dataSize);
                var outputs = new double[inputs.Length];

                // Act
                var stopwatch = Stopwatch.StartNew();
                sma.OnBarBatch(inputs, outputs);
                stopwatch.Stop();

                // Assert
                Assert.True(sma.IsReady);
                var throughput = dataSize / (stopwatch.ElapsedMilliseconds + 1) * 1000; // bars per second
                
                _output.WriteLine($"SMA({period}) - {dataSize} bars: {stopwatch.ElapsedMilliseconds}ms ({throughput:F0} bars/sec)");
                
                // Performance targets
                Assert.True(throughput > 10000, $"SMA throughput {throughput} should be > 10k bars/sec");
            }
        }
    }

    [Fact]
    public void Benchmark_ComplexIndicator_Performance()
    {
        // Test performance of complex indicators
        var testCases = new[]
        {
            ("RSI", (Func<double[], double>)TestRSIPerformance),
            ("MACD", (Func<double[], double>)TestMACDPerformance),
            ("BollingerBands", (Func<double[], double>)TestBollingerBandsPerformance),
            ("Stochastic", (Func<double[], double>)TestStochasticPerformance)
        };

        var dataSize = 50000;
        var inputs = GenerateTestData(dataSize);

        foreach (var (name, testFunc) in testCases)
        {
            var throughput = testFunc(inputs);
            _output.WriteLine($"{name} - {dataSize} bars: {throughput:F0} bars/sec");
            Assert.True(throughput > 5000, $"{name} throughput should be > 5k bars/sec");
        }
    }

    [Fact] 
    public void Benchmark_MemoryEfficiency()
    {
        // Test memory usage of indicators
        var dataSize = 100000;
        var inputs = GenerateTestData(dataSize);
        
        // Measure baseline memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var baselineMemory = GC.GetTotalMemory(false);

        // Test SMA memory usage
        var smaParams = new PSMA<double, double> { Period = 50 };
        var sma = new SMA_QC<double, double>(smaParams);
        var outputs = new double[inputs.Length];
        
        sma.OnBarBatch(inputs, outputs);
        
        var afterMemory = GC.GetTotalMemory(false);
        var memoryUsed = afterMemory - baselineMemory;
        var bytesPerBar = memoryUsed / (double)dataSize;
        
        _output.WriteLine($"Memory usage: {memoryUsed / 1024:F0}KB ({bytesPerBar:F2} bytes/bar)");
        
        // Should use reasonable memory (less than 50 bytes per bar)
        Assert.True(bytesPerBar < 50, $"Memory per bar {bytesPerBar} should be < 50 bytes");
    }

    [Fact]
    public void Benchmark_BatchVsIncremental()
    {
        // Compare batch processing vs incremental processing
        var dataSize = 10000;
        var inputs = GenerateTestData(dataSize);
        var parameters = new PEMA<double, double> { Period = 20 };
        
        // Batch processing
        var emaBatch = new EMA_QC<double, double>(parameters);
        var batchOutputs = new double[inputs.Length];
        
        var batchStopwatch = Stopwatch.StartNew();
        emaBatch.OnBarBatch(inputs, batchOutputs);
        batchStopwatch.Stop();
        
        // Incremental processing
        var emaIncremental = new EMA_QC<double, double>(parameters);
        var incrementalStopwatch = Stopwatch.StartNew();
        
        foreach (var input in inputs)
        {
            emaIncremental.OnBar(input);
        }
        incrementalStopwatch.Stop();
        
        // Results should be the same
        Assert.Equal(emaBatch.Value, emaIncremental.Value, 5);
        
        var batchThroughput = dataSize / (double)batchStopwatch.ElapsedMilliseconds * 1000;
        var incrementalThroughput = dataSize / (double)incrementalStopwatch.ElapsedMilliseconds * 1000;
        
        _output.WriteLine($"Batch: {batchThroughput:F0} bars/sec, Incremental: {incrementalThroughput:F0} bars/sec");
        
        // Batch should be faster or at least comparable
        Assert.True(batchThroughput >= incrementalThroughput * 0.5, "Batch processing should be competitive");
    }

    [Fact]
    public void Benchmark_LargeDatasetStability()
    {
        // Test with very large datasets to check for memory leaks or performance degradation
        var dataSizes = new[] { 100000, 500000, 1000000 };
        var parameters = new PSMA<double, double> { Period = 50 };
        
        var throughputs = new List<double>();
        
        foreach (var dataSize in dataSizes)
        {
            var inputs = GenerateTestData(dataSize);
            var sma = new SMA_QC<double, double>(parameters);
            var outputs = new double[inputs.Length];
            
            GC.Collect(); // Clean start
            
            var stopwatch = Stopwatch.StartNew();
            sma.OnBarBatch(inputs, outputs);
            stopwatch.Stop();
            
            var throughput = dataSize / (double)stopwatch.ElapsedMilliseconds * 1000;
            throughputs.Add(throughput);
            
            _output.WriteLine($"Dataset {dataSize}: {throughput:F0} bars/sec, Memory: {GC.GetTotalMemory(false) / 1024 / 1024}MB");
        }
        
        // Throughput should remain relatively stable (within 50% of first measurement)
        var baselineThroughput = throughputs[0];
        foreach (var throughput in throughputs.Skip(1))
        {
            Assert.True(throughput > baselineThroughput * 0.5, 
                $"Throughput {throughput} should not degrade significantly from baseline {baselineThroughput}");
        }
    }

    [Fact]
    public void Benchmark_ConcurrentIndicators()
    {
        // Test performance when running multiple indicators concurrently
        var dataSize = 50000;
        var inputs = GenerateTestData(dataSize);
        
        var indicators = new object[]
        {
            new SMA_QC<double, double>(new PSMA<double, double> { Period = 20 }),
            new EMA_QC<double, double>(new PEMA<double, double> { Period = 20 }),
            new RSI_QC<double, double>(new PRSI<double, double> { Period = 14 }),
        };
        
        var stopwatch = Stopwatch.StartNew();
        
        Parallel.ForEach(indicators, indicator =>
        {
            var outputs = new double[inputs.Length];
            switch (indicator)
            {
                case SMA_QC<double, double> sma:
                    sma.OnBarBatch(inputs, outputs);
                    break;
                case EMA_QC<double, double> ema:
                    ema.OnBarBatch(inputs, outputs);
                    break;
                case RSI_QC<double, double> rsi:
                    rsi.OnBarBatch(inputs, outputs);
                    break;
            }
        });
        
        stopwatch.Stop();
        
        var totalOperations = dataSize * indicators.Length;
        var throughput = totalOperations / (double)stopwatch.ElapsedMilliseconds * 1000;
        
        _output.WriteLine($"Concurrent indicators: {throughput:F0} total operations/sec");
        
        Assert.True(throughput > 50000, "Concurrent processing should be efficient");
    }

    private double TestRSIPerformance(double[] inputs)
    {
        var parameters = new PRSI<double, double> { Period = 14 };
        var rsi = new RSI_QC<double, double>(parameters);
        var outputs = new double[inputs.Length];
        
        var stopwatch = Stopwatch.StartNew();
        rsi.OnBarBatch(inputs, outputs);
        stopwatch.Stop();
        
        return inputs.Length / (double)stopwatch.ElapsedMilliseconds * 1000;
    }

    private double TestMACDPerformance(double[] inputs)
    {
        var parameters = new PMACD<double, MACDResult> { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 };
        var macd = new MACD_QC<double, MACDResult>(parameters);
        var outputs = new MACDResult[inputs.Length];
        
        var stopwatch = Stopwatch.StartNew();
        macd.OnBarBatch(inputs, outputs);
        stopwatch.Stop();
        
        return inputs.Length / (double)stopwatch.ElapsedMilliseconds * 1000;
    }

    private double TestBollingerBandsPerformance(double[] inputs)
    {
        var parameters = new PBollingerBands<double, BollingerBandsResult> { Period = 20, StandardDeviations = 2 };
        var bb = new BollingerBands_QC<double, BollingerBandsResult>(parameters);
        var outputs = new BollingerBandsResult[inputs.Length];
        
        var stopwatch = Stopwatch.StartNew();
        bb.OnBarBatch(inputs, outputs);
        stopwatch.Stop();
        
        return inputs.Length / (double)stopwatch.ElapsedMilliseconds * 1000;
    }

    private double TestStochasticPerformance(double[] inputs)
    {
        var parameters = new PStochastic<HLC, StochasticResult> { KPeriod = 14, DPeriod = 3 };
        var stoch = new Stochastic_QC<HLC, StochasticResult>(parameters);
        var hlcInputs = inputs.Select(p => new HLC { High = p + 1, Low = p - 1, Close = p }).ToArray();
        var outputs = new StochasticResult[inputs.Length];
        
        var stopwatch = Stopwatch.StartNew();
        stoch.OnBarBatch(hlcInputs, outputs);
        stopwatch.Stop();
        
        return inputs.Length / (double)stopwatch.ElapsedMilliseconds * 1000;
    }

    private static double[] GenerateTestData(int count)
    {
        var data = new double[count];
        var random = new Random(42);
        var price = 100.0;
        
        for (int i = 0; i < count; i++)
        {
            price += (random.NextDouble() - 0.5) * 2;
            data[i] = Math.Max(0.01, price);
        }
        
        return data;
    }
}

// BenchmarkDotNet benchmark class for more detailed performance testing
[MemoryDiagnoser]
[SimpleJob]
public class IndicatorBenchmarks
{
    private double[] _data1K;
    private double[] _data10K;
    private double[] _data100K;
    
    [GlobalSetup]
    public void Setup()
    {
        _data1K = GenerateTestData(1000);
        _data10K = GenerateTestData(10000);
        _data100K = GenerateTestData(100000);
    }
    
    [Benchmark]
    [Arguments(1000)]
    [Arguments(10000)]
    [Arguments(100000)]
    public void SMA_Benchmark(int dataSize)
    {
        var data = dataSize switch
        {
            1000 => _data1K,
            10000 => _data10K,
            100000 => _data100K,
            _ => GenerateTestData(dataSize)
        };
        
        var sma = new SMA_QC<double, double>(new PSMA<double, double> { Period = 20 });
        var outputs = new double[data.Length];
        sma.OnBarBatch(data, outputs);
    }
    
    [Benchmark]
    public void RSI_Benchmark()
    {
        var rsi = new RSI_QC<double, double>(new PRSI<double, double> { Period = 14 });
        var outputs = new double[_data10K.Length];
        rsi.OnBarBatch(_data10K, outputs);
    }
    
    [Benchmark]
    public void MACD_Benchmark()
    {
        var macd = new MACD_QC<double, MACDResult>(new PMACD<double, MACDResult> 
        { 
            FastPeriod = 12, 
            SlowPeriod = 26, 
            SignalPeriod = 9 
        });
        var outputs = new MACDResult[_data10K.Length];
        macd.OnBarBatch(_data10K, outputs);
    }
    
    private static double[] GenerateTestData(int count)
    {
        var data = new double[count];
        var random = new Random(42);
        var price = 100.0;
        
        for (int i = 0; i < count; i++)
        {
            price += (random.NextDouble() - 0.5) * 2;
            data[i] = Math.Max(0.01, price);
        }
        
        return data;
    }
}