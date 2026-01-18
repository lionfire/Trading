using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.ValueTypes;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace LionFire.Trading.Indicators.Tests;

public class StressTests
{
    private readonly ITestOutputHelper _output;

    public StressTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void StressTest_SMA_LargeDataset()
    {
        // Arrange
        var parameters = new PSMA<double, double> { Period = 50 };
        var sma = new SMA_FP<double, double>(parameters);
        var dataSize = 100000;
        var inputs = GenerateRandomData(dataSize, 100, 50);
        var outputs = new double[inputs.Length];

        // Act
        var stopwatch = Stopwatch.StartNew();
        sma.OnBarBatch(inputs, outputs);
        stopwatch.Stop();

        // Assert
        Assert.True(sma.IsReady);
        _output.WriteLine($"SMA processed {dataSize} bars in {stopwatch.ElapsedMilliseconds}ms");

        // Should complete within reasonable time
        Assert.True(stopwatch.ElapsedMilliseconds < 5000);

        // All values after warmup should be valid (not NaN)
        var validOutputs = outputs.Skip(parameters.Period - 1).Where(o => !double.IsNaN(o) && o > 0).Count();
        Assert.Equal(dataSize - parameters.Period + 1, validOutputs);
    }

    [Fact]
    public void StressTest_RSI_ExtremePriceMovements()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 14 };
        var rsi = new RSI_FP<double, double>(parameters);

        // Extreme price movements
        var inputs = new double[1000];
        var random = new Random(42);
        var price = 100.0;

        for (int i = 0; i < inputs.Length; i++)
        {
            // Random walk with extreme jumps
            var change = random.NextDouble() < 0.05 ?
                         (random.NextDouble() - 0.5) * 50 : // 5% chance of extreme move
                         (random.NextDouble() - 0.5) * 2;   // Normal move
            price = Math.Max(0.01, price + change);
            inputs[i] = price;
        }

        var outputs = new double[inputs.Length];

        // Act
        rsi.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(rsi.IsReady);

        // RSI should stay within bounds despite extreme moves
        var validOutputs = outputs.Skip(parameters.Period).Where(o => !double.IsNaN(o)).ToArray();
        Assert.True(validOutputs.All(o => o >= 0 && o <= 100));

        // Should have extreme readings
        var hasOverbought = validOutputs.Any(o => o > 80);
        var hasOversold = validOutputs.Any(o => o < 20);
        Assert.True(hasOverbought, "Should detect overbought conditions");
        Assert.True(hasOversold, "Should detect oversold conditions");
    }

    [Fact]
    public void StressTest_MACD_HighFrequencyData()
    {
        // Arrange
        var parameters = new PMACD<double, double>
        {
            FastPeriod = 12,
            SlowPeriod = 26,
            SignalPeriod = 9
        };
        var macd = new MACD_FP<double, double>(parameters);

        // High frequency tick data simulation
        var dataSize = 50000;
        var inputs = new double[dataSize];
        var basePrice = 100.0;
        var random = new Random(42);

        for (int i = 0; i < dataSize; i++)
        {
            // Simulate high-frequency price movements (small increments)
            basePrice += (random.NextDouble() - 0.5) * 0.01;
            inputs[i] = Math.Max(0.01, basePrice);
        }

        var outputs = new double[inputs.Length * 3]; // MACD, Signal, Histogram

        // Act
        var stopwatch = Stopwatch.StartNew();
        macd.OnBarBatch(inputs, outputs);
        stopwatch.Stop();

        // Assert
        Assert.True(macd.IsReady);
        _output.WriteLine($"MACD processed {dataSize} high-freq bars in {stopwatch.ElapsedMilliseconds}ms");

        // Performance should be reasonable
        Assert.True(stopwatch.ElapsedMilliseconds < 10000);

        // Should produce valid signals
        Assert.False(double.IsNaN(macd.MACD));
        Assert.False(double.IsNaN(macd.Signal));
    }

    [Fact]
    public void StressTest_BollingerBands_VolatilitySpikes()
    {
        // Arrange
        var parameters = new PBollingerBands<double, double>
        {
            Period = 20,
            StandardDeviations = 2
        };
        var bb = new BollingerBands_FP<double, double>(parameters);

        // Create data with sudden volatility spikes
        var inputs = new double[1000];
        var price = 100.0;
        var random = new Random(42);

        for (int i = 0; i < inputs.Length; i++)
        {
            // Normal volatility most of the time
            var normalMove = (random.NextDouble() - 0.5) * 1.0;

            // Volatility spike every 50 bars
            var volatilitySpike = (i % 50 == 0) ? (random.NextDouble() - 0.5) * 20 : 0;

            price = Math.Max(0.01, price + normalMove + volatilitySpike);
            inputs[i] = price;
        }

        var outputs = new double[inputs.Length * 3]; // Upper, Middle, Lower

        // Act
        bb.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(bb.IsReady);

        // Collect band widths from outputs (every 3rd value is upper, middle, lower)
        var bandWidths = new List<double>();
        for (int i = parameters.Period - 1; i < inputs.Length; i++)
        {
            var upper = outputs[i * 3];
            var lower = outputs[i * 3 + 2];
            if (!double.IsNaN(upper) && !double.IsNaN(lower))
            {
                bandWidths.Add(upper - lower);
            }
        }

        var maxWidth = bandWidths.Max();
        var minWidth = bandWidths.Min();
        var widthRatio = maxWidth / minWidth;

        Assert.True(widthRatio > 2, "Bands should expand significantly during volatility spikes");
    }

    [Fact]
    public void StressTest_CCI_GapData()
    {
        // Arrange
        var parameters = new PCCI<double, double> { Period = 14 };
        var cci = new CCI_FP<double, double>(parameters);

        // Data with gaps (overnight gaps, news gaps)
        var inputs = new HLC<double>[500];
        var random = new Random(42);
        var close = 100.0;

        for (int i = 0; i < inputs.Length; i++)
        {
            var prevClose = close;

            // 10% chance of gap
            if (random.NextDouble() < 0.1)
            {
                var gapPercent = (random.NextDouble() - 0.5) * 0.1; // ±5% gap
                close = prevClose * (1 + gapPercent);
            }
            else
            {
                close += (random.NextDouble() - 0.5) * 2;
            }

            var high = close + random.NextDouble() * 2;
            var low = close - random.NextDouble() * 2;

            inputs[i] = new HLC<double>
            {
                High = Math.Max(high, close),
                Low = Math.Min(low, close),
                Close = close
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        cci.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(cci.IsReady);

        // CCI should handle gaps properly
        var validOutputs = outputs.Skip(parameters.Period - 1).Where(o => !double.IsNaN(o)).ToArray();
        Assert.True(validOutputs.Length > 0);

        // CCI should show extreme readings during volatile gap periods
        var hasExtremeReadings = validOutputs.Any(o => Math.Abs(o) > 100);
        Assert.True(hasExtremeReadings, "CCI should show extreme readings during gaps");
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(10000)]
    [InlineData(50000)]
    public void StressTest_MemoryUsage_LargeDatasets(int dataSize)
    {
        // Arrange
        var parameters = new PEMA<double, double> { Period = 50 };

        // Measure memory before
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var memoryBefore = GC.GetTotalMemory(false);

        // Act
        var ema = new EMA_FP<double, double>(parameters);
        var inputs = GenerateRandomData(dataSize, 100, 10);
        var outputs = new double[inputs.Length];

        ema.OnBarBatch(inputs, outputs);

        // Measure memory after
        var memoryAfter = GC.GetTotalMemory(false);
        var memoryUsed = memoryAfter - memoryBefore;

        // Assert
        Assert.True(ema.IsReady);
        _output.WriteLine($"EMA with {dataSize} bars used {memoryUsed / 1024}KB memory");

        // Memory usage should be reasonable (less than 10MB for 50k bars)
        Assert.True(memoryUsed < 10_000_000, $"Memory usage {memoryUsed} too high");
    }

    [Fact]
    public void StressTest_ChainedIndicators()
    {
        // Arrange - Chain multiple indicators together
        var smaParams = new PSMA<double, double> { Period = 20 };
        var rsiParams = new PRSI<double, double> { Period = 14 };
        var macdParams = new PMACD<double, double> { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 };

        var sma = new SMA_FP<double, double>(smaParams);
        var rsi = new RSI_FP<double, double>(rsiParams);
        var macd = new MACD_FP<double, double>(macdParams);

        var dataSize = 10000;
        var inputs = GenerateRandomData(dataSize, 100, 20);

        // Act
        var stopwatch = Stopwatch.StartNew();

        var smaOutputs = new double[inputs.Length];
        var rsiOutputs = new double[inputs.Length];
        var macdOutputs = new double[inputs.Length * 3];

        sma.OnBarBatch(inputs, smaOutputs);
        rsi.OnBarBatch(inputs, rsiOutputs);
        macd.OnBarBatch(inputs, macdOutputs);

        stopwatch.Stop();

        // Assert
        Assert.True(sma.IsReady);
        Assert.True(rsi.IsReady);
        Assert.True(macd.IsReady);

        _output.WriteLine($"3 indicators processed {dataSize} bars in {stopwatch.ElapsedMilliseconds}ms");
        Assert.True(stopwatch.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public void StressTest_RepeatedReset()
    {
        // Arrange
        var parameters = new PSMA<double, double> { Period = 20 };
        var sma = new SMA_FP<double, double>(parameters);
        var inputs = GenerateRandomData(100, 100, 10);
        var outputs = new double[inputs.Length];

        // Act & Assert
        double? firstResult = null;

        for (int i = 0; i < 1000; i++)
        {
            sma.Clear();
            Assert.False(sma.IsReady);

            sma.OnBarBatch(inputs, outputs);
            Assert.True(sma.IsReady);

            // Verify consistent results
            if (firstResult == null)
            {
                firstResult = sma.Value;
            }
            else
            {
                Assert.Equal(firstResult.Value, sma.Value, 5);
            }
        }

        _output.WriteLine("Successfully completed 1000 reset cycles");
    }

    private static double[] GenerateRandomData(int count, double startPrice, double volatility)
    {
        var data = new double[count];
        var random = new Random(42);
        var price = startPrice;

        for (int i = 0; i < count; i++)
        {
            price += (random.NextDouble() - 0.5) * volatility;
            data[i] = Math.Max(0.01, price);
        }

        return data;
    }
}
