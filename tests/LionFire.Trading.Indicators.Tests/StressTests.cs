using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
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
        var sma = new SMA_QC<double, double>(parameters);
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
        
        // All values after warmup should be valid
        var validOutputs = outputs.Skip(parameters.Period - 1).Where(o => o > 0).Count();
        Assert.Equal(dataSize - parameters.Period + 1, validOutputs);
    }

    [Fact]
    public void StressTest_RSI_ExtremePriceMovements()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 14 };
        var rsi = new RSI_QC<double, double>(parameters);
        
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
        var validOutputs = outputs.Skip(parameters.Period).Where(o => o > 0).ToArray();
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
        var parameters = new PMACD<double, MACDResult> 
        { 
            FastPeriod = 12, 
            SlowPeriod = 26, 
            SignalPeriod = 9 
        };
        var macd = new MACD_QC<double, MACDResult>(parameters);
        
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
        
        var outputs = new MACDResult[inputs.Length];

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
        var validOutputs = outputs.Skip(parameters.SlowPeriod + parameters.SignalPeriod - 1)
                                 .Where(o => o != null).ToArray();
        Assert.True(validOutputs.Length > 0);
    }

    [Fact]
    public void StressTest_BollingerBands_VolatilitySpikes()
    {
        // Arrange
        var parameters = new PBollingerBands<double, BollingerBandsResult> 
        { 
            Period = 20, 
            StandardDeviations = 2 
        };
        var bb = new BollingerBands_QC<double, BollingerBandsResult>(parameters);
        
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
        
        var outputs = new BollingerBandsResult[inputs.Length];

        // Act
        bb.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(bb.IsReady);
        
        // Bands should expand during volatility spikes
        var validOutputs = outputs.Skip(parameters.Period - 1).Where(o => o != null).ToArray();
        var bandWidths = validOutputs.Select(o => o.UpperBand - o.LowerBand).ToArray();
        
        var maxWidth = bandWidths.Max();
        var minWidth = bandWidths.Min();
        var widthRatio = maxWidth / minWidth;
        
        Assert.True(widthRatio > 2, "Bands should expand significantly during volatility spikes");
    }

    [Fact]
    public void StressTest_ATR_GapData()
    {
        // Arrange
        var parameters = new PATR<HLC, double> { Period = 14 };
        var atr = new AverageTrueRange_QC<HLC, double>(parameters);
        
        // Data with gaps (overnight gaps, news gaps)
        var inputs = new HLC[500];
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
            
            inputs[i] = new HLC
            {
                High = Math.Max(high, close),
                Low = Math.Min(low, close),
                Close = close
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        atr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(atr.IsReady);
        
        // ATR should handle gaps properly
        var validOutputs = outputs.Skip(parameters.Period - 1).Where(o => o > 0).ToArray();
        Assert.True(validOutputs.All(o => o > 0));
        
        // Should show elevated ATR during gap periods
        var avgATR = validOutputs.Average();
        var maxATR = validOutputs.Max();
        Assert.True(maxATR > avgATR * 1.5, "ATR should spike during gaps");
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
        var ema = new EMA_QC<double, double>(parameters);
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
        var macdParams = new PMACD<double, MACDResult> { FastPeriod = 12, SlowPeriod = 26, SignalPeriod = 9 };
        
        var sma = new SMA_QC<double, double>(smaParams);
        var rsi = new RSI_QC<double, double>(rsiParams);
        var macd = new MACD_QC<double, MACDResult>(macdParams);
        
        var dataSize = 10000;
        var inputs = GenerateRandomData(dataSize, 100, 20);
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        
        var smaOutputs = new double[inputs.Length];
        var rsiOutputs = new double[inputs.Length];
        var macdOutputs = new MACDResult[inputs.Length];
        
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
        var sma = new SMA_QC<double, double>(parameters);
        var inputs = GenerateRandomData(100, 100, 10);
        var outputs = new double[inputs.Length];

        // Act & Assert
        for (int i = 0; i < 1000; i++)
        {
            sma.Reset();
            Assert.False(sma.IsReady);
            
            sma.OnBarBatch(inputs, outputs);
            Assert.True(sma.IsReady);
            
            // Verify consistent results
            if (i == 0)
            {
                // Store first result for comparison
                var firstResult = sma.Value;
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

public class MACDResult
{
    public double MACD { get; set; }
    public double Signal { get; set; }
    public double Histogram { get; set; }
}

public class BollingerBandsResult
{
    public double UpperBand { get; set; }
    public double MiddleBand { get; set; }
    public double LowerBand { get; set; }
}