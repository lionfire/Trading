// DISABLED: Tests need updating to match current API
#if false
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using System.Collections.Concurrent;
using Xunit;
using Xunit.Abstractions;

namespace LionFire.Trading.Indicators.Tests;

public class ConcurrencyTests
{
    private readonly ITestOutputHelper _output;

    public ConcurrencyTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Concurrency_MultipleThreadsProcessingDifferentIndicators()
    {
        // Arrange
        var dataSize = 10000;
        var inputs = GenerateTestData(dataSize);
        var threadCount = Environment.ProcessorCount;
        var results = new ConcurrentBag<(string Name, double Value, bool IsReady)>();
        
        var indicators = new[]
        {
            ("SMA", (Func<double[], (double, bool)>)(data => ProcessSMA(data, 20))),
            ("EMA", (Func<double[], (double, bool)>)(data => ProcessEMA(data, 20))),
            ("RSI", (Func<double[], (double, bool)>)(data => ProcessRSI(data, 14))),
            ("MACD", (Func<double[], (double, bool)>)(data => ProcessMACD(data))),
            ("BollingerBands", (Func<double[], (double, bool)>)(data => ProcessBollingerBands(data))),
        };

        // Act
        Parallel.ForEach(indicators, indicator =>
        {
            var (name, processor) = indicator;
            for (int i = 0; i < 100; i++) // Process multiple times per indicator
            {
                var (value, isReady) = processor(inputs);
                results.Add((name, value, isReady));
            }
        });

        // Assert
        var resultsList = results.ToList();
        Assert.Equal(500, resultsList.Count); // 5 indicators × 100 iterations
        
        // Group by indicator and verify consistency
        var groupedResults = resultsList.GroupBy(r => r.Name).ToList();
        foreach (var group in groupedResults)
        {
            var values = group.Select(g => g.Value).Distinct().ToList();
            var readyStates = group.Select(g => g.IsReady).Distinct().ToList();
            
            // All results for the same indicator should be identical
            Assert.Single(values); // Only one unique value
            Assert.Single(readyStates); // Only one ready state
            
            _output.WriteLine($"{group.Key}: {values[0]:F4} (Ready: {readyStates[0]})");
        }
    }

    [Fact]
    public void Concurrency_ThreadSafetyOfSingleIndicator()
    {
        // Test if a single indicator can be safely accessed from multiple threads
        var parameters = new PSMA<double, double> { Period = 20 };
        var sma = new SMA_QC<double, double>(parameters);
        var inputs = GenerateTestData(1000);
        var outputs = new double[inputs.Length];
        
        // Pre-populate the indicator
        sma.OnBarBatch(inputs, outputs);
        var expectedValue = sma.Value;
        var expectedIsReady = sma.IsReady;
        
        var results = new ConcurrentBag<(double Value, bool IsReady)>();
        var threadCount = 50;
        
        // Act - Multiple threads reading from the same indicator
        Parallel.For(0, threadCount, i =>
        {
            // Each thread reads the indicator state multiple times
            for (int j = 0; j < 100; j++)
            {
                var value = sma.Value;
                var isReady = sma.IsReady;
                results.Add((value, isReady));
                
                Thread.Sleep(1); // Small delay to increase chance of race conditions
            }
        });

        // Assert
        var resultsList = results.ToList();
        Assert.Equal(threadCount * 100, resultsList.Count);
        
        // All reads should return consistent values
        var uniqueValues = resultsList.Select(r => r.Value).Distinct().ToList();
        var uniqueReadyStates = resultsList.Select(r => r.IsReady).Distinct().ToList();
        
        Assert.Single(uniqueValues);
        Assert.Single(uniqueReadyStates);
        Assert.Equal(expectedValue, uniqueValues[0]);
        Assert.Equal(expectedIsReady, uniqueReadyStates[0]);
    }

    [Fact]
    public void Concurrency_ParallelProcessingDifferentDatasets()
    {
        // Test processing different datasets in parallel with same indicator type
        var datasets = new[]
        {
            GenerateTestData(5000, seed: 1),
            GenerateTestData(5000, seed: 2),
            GenerateTestData(5000, seed: 3),
            GenerateTestData(5000, seed: 4),
            GenerateTestData(5000, seed: 5)
        };
        
        var results = new ConcurrentDictionary<int, (double SMA, double EMA, double RSI)>();
        
        // Act
        Parallel.For(0, datasets.Length, i =>
        {
            var data = datasets[i];
            
            // Process with different indicators
            var (smaValue, _) = ProcessSMA(data, 20);
            var (emaValue, _) = ProcessEMA(data, 20);
            var (rsiValue, _) = ProcessRSI(data, 14);
            
            results[i] = (smaValue, emaValue, rsiValue);
        });

        // Assert
        Assert.Equal(datasets.Length, results.Count);
        
        // Results should be different for different datasets
        var smaValues = results.Values.Select(v => v.SMA).Distinct().ToList();
        var emaValues = results.Values.Select(v => v.EMA).Distinct().ToList();
        var rsiValues = results.Values.Select(v => v.RSI).Distinct().ToList();
        
        Assert.True(smaValues.Count > 1, "Different datasets should produce different SMA values");
        Assert.True(emaValues.Count > 1, "Different datasets should produce different EMA values");
        Assert.True(rsiValues.Count > 1, "Different datasets should produce different RSI values");
        
        foreach (var result in results)
        {
            _output.WriteLine($"Dataset {result.Key}: SMA={result.Value.SMA:F2}, EMA={result.Value.EMA:F2}, RSI={result.Value.RSI:F2}");
        }
    }

    [Fact]
    public void Concurrency_ResetOperationsThreadSafety()
    {
        // Test thread safety of reset operations
        var parameters = new PSMA<double, double> { Period = 10 };
        var inputs = GenerateTestData(100);
        var resetCount = new int[1];
        var processCount = new int[1];
        var errors = new ConcurrentBag<Exception>();
        
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        
        // Act - One thread continuously resets, others process data
        var resetTask = Task.Run(() =>
        {
            while (!cancellationToken.Token.IsCancellationRequested)
            {
                try
                {
                    var sma = new SMA_QC<double, double>(parameters);
                    sma.Reset();
                    Interlocked.Increment(ref resetCount[0]);
                    Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
        });
        
        var processTasks = Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
        {
            while (!cancellationToken.Token.IsCancellationRequested)
            {
                try
                {
                    var sma = new SMA_QC<double, double>(parameters);
                    var outputs = new double[inputs.Length];
                    sma.OnBarBatch(inputs, outputs);
                    Interlocked.Increment(ref processCount[0]);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
        })).ToArray();
        
        Task.WaitAll(new[] { resetTask }.Concat(processTasks).ToArray());
        
        // Assert
        Assert.Empty(errors);
        Assert.True(resetCount[0] > 0, "Reset operations should have occurred");
        Assert.True(processCount[0] > 0, "Process operations should have occurred");
        
        _output.WriteLine($"Reset operations: {resetCount[0]}, Process operations: {processCount[0]}");
    }

    [Fact]
    public void Concurrency_HighFrequencyUpdates()
    {
        // Simulate high-frequency trading scenario
        var parameters = new PEMA<double, double> { Period = 10 };
        var ema = new EMA_QC<double, double>(parameters);
        var random = new Random(42);
        var updateCount = 0;
        var errors = new ConcurrentBag<Exception>();
        
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        
        // Act - Multiple threads rapidly updating the indicator
        var updateTasks = Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
        {
            while (!cancellationToken.Token.IsCancellationRequested)
            {
                try
                {
                    var price = 100 + random.NextDouble() * 20;
                    ema.OnBar(price);
                    Interlocked.Increment(ref updateCount);
                    
                    // High frequency - minimal delay
                    if (updateCount % 1000 == 0)
                        Thread.Yield();
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
        })).ToArray();
        
        Task.WaitAll(updateTasks);
        
        // Assert
        Assert.Empty(errors);
        Assert.True(updateCount > 10000, $"Should process many updates in high-frequency scenario: {updateCount}");
        Assert.True(ema.IsReady, "EMA should be ready after many updates");
        Assert.True(ema.Value > 0, "EMA should have valid value");
        
        _output.WriteLine($"Processed {updateCount} high-frequency updates without errors");
    }

    [Fact]
    public void Concurrency_MemoryStressTest()
    {
        // Test memory behavior under concurrent load
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(false);
        
        var taskCount = Environment.ProcessorCount * 2;
        var iterationsPerTask = 100;
        
        // Act
        var tasks = Enumerable.Range(0, taskCount).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < iterationsPerTask; i++)
            {
                var inputs = GenerateTestData(1000);
                var sma = new SMA_QC<double, double>(new PSMA<double, double> { Period = 20 });
                var rsi = new RSI_QC<double, double>(new PRSI<double, double> { Period = 14 });
                
                var smaOutputs = new double[inputs.Length];
                var rsiOutputs = new double[inputs.Length];
                
                sma.OnBarBatch(inputs, smaOutputs);
                rsi.OnBarBatch(inputs, rsiOutputs);
                
                // Force some GC pressure
                if (i % 10 == 0)
                {
                    GC.Collect(0, GCCollectionMode.Optimized);
                }
            }
        })).ToArray();
        
        Task.WaitAll(tasks);
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(false);
        
        // Assert
        var memoryIncrease = finalMemory - initialMemory;
        var memoryIncreaseKB = memoryIncrease / 1024.0;
        
        _output.WriteLine($"Memory increase: {memoryIncreaseKB:F2} KB");
        
        // Memory increase should be reasonable (less than 10MB for this test)
        Assert.True(memoryIncrease < 10_000_000, 
            $"Memory increase {memoryIncreaseKB:F2} KB should be reasonable");
    }

    private (double Value, bool IsReady) ProcessSMA(double[] data, int period)
    {
        var sma = new SMA_QC<double, double>(new PSMA<double, double> { Period = period });
        var outputs = new double[data.Length];
        sma.OnBarBatch(data, outputs);
        return (sma.Value, sma.IsReady);
    }

    private (double Value, bool IsReady) ProcessEMA(double[] data, int period)
    {
        var ema = new EMA_QC<double, double>(new PEMA<double, double> { Period = period });
        var outputs = new double[data.Length];
        ema.OnBarBatch(data, outputs);
        return (ema.Value, ema.IsReady);
    }

    private (double Value, bool IsReady) ProcessRSI(double[] data, int period)
    {
        var rsi = new RSI_QC<double, double>(new PRSI<double, double> { Period = period });
        var outputs = new double[data.Length];
        rsi.OnBarBatch(data, outputs);
        return (rsi.Value, rsi.IsReady);
    }

    private (double Value, bool IsReady) ProcessMACD(double[] data)
    {
        var macd = new MACD_QC<double, MACDResult>(new PMACD<double, MACDResult> 
        { 
            FastPeriod = 12, 
            SlowPeriod = 26, 
            SignalPeriod = 9 
        });
        var outputs = new MACDResult[data.Length];
        macd.OnBarBatch(data, outputs);
        var result = outputs[outputs.Length - 1];
        return (result?.MACD ?? 0, macd.IsReady);
    }

    private (double Value, bool IsReady) ProcessBollingerBands(double[] data)
    {
        var bb = new BollingerBands_QC<double, BollingerBandsResult>(
            new PBollingerBands<double, BollingerBandsResult> { Period = 20, StandardDeviations = 2 });
        var outputs = new BollingerBandsResult[data.Length];
        bb.OnBarBatch(data, outputs);
        var result = outputs[outputs.Length - 1];
        return (result?.MiddleBand ?? 0, bb.IsReady);
    }

    private static double[] GenerateTestData(int count, int seed = 42)
    {
        var data = new double[count];
        var random = new Random(seed);
        var price = 100.0;
        
        for (int i = 0; i < count; i++)
        {
            price += (random.NextDouble() - 0.5) * 2;
            data[i] = Math.Max(0.01, price);
        }
        
        return data;
    }
}
#endif
