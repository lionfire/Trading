using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class OBVTests
{
    [Fact]
    public void OBV_BasicCalculation()
    {
        // Arrange
        var parameters = new POBV<OHLCV, double>();
        var obv = new OBV_FP<OHLCV, double>(parameters);

        // Test data: Price goes up, down, same
        var testData = new OHLCV[]
        {
            new() { Open = 10.0, High = 10.5, Low = 9.5, Close = 10.0, Volume = 1000 }, // First bar: OBV = 1000
            new() { Open = 10.0, High = 11.5, Low = 10.0, Close = 11.0, Volume = 1500 }, // Price up: OBV = 1000 + 1500 = 2500
            new() { Open = 11.0, High = 11.0, Low = 8.5, Close = 9.0, Volume = 800 },   // Price down: OBV = 2500 - 800 = 1700
            new() { Open = 9.0, High = 9.5, Low = 8.5, Close = 9.0, Volume = 1200 },    // Price same: OBV = 1700 (unchanged)
            new() { Open = 9.0, High = 12.5, Low = 9.0, Close = 12.0, Volume = 2000 }   // Price up: OBV = 1700 + 2000 = 3700
        };

        var expected = new double[] { 1000, 2500, 1700, 1700, 3700 };
        var results = new double[testData.Length];

        // Act
        obv.OnBarBatch(testData, results);

        // Assert
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.True(Math.Abs(expected[i] - results[i]) < 0.01,
                $"OBV value at index {i} should be {expected[i]}, but was {results[i]}");
        }

        // Verify final state
        Assert.True(Math.Abs(3700 - obv.CurrentValue) < 0.01);
        Assert.True(Math.Abs(2000 - obv.LastChange) < 0.01); // Last change should be +2000
    }

    [Fact]
    public void OBV_SellingPressure_ShouldDecrease()
    {
        // Arrange
        var parameters = new POBV<OHLCV, double>();
        var obv = new OBV_FP<OHLCV, double>(parameters);

        var testData = new OHLCV[]
        {
            new() { Open = 50.0, High = 51.0, Low = 49.0, Close = 50.0, Volume = 1000 }, // Initial: OBV = 1000
            new() { Open = 50.0, High = 50.0, Low = 47.0, Close = 48.0, Volume = 1200 }, // Price down: OBV = 1000 - 1200 = -200
            new() { Open = 48.0, High = 48.0, Low = 44.0, Close = 45.0, Volume = 800 },  // Price down: OBV = -200 - 800 = -1000
        };

        var results = new double[testData.Length];

        // Act
        obv.OnBarBatch(testData, results);

        // Assert
        Assert.Equal(1000, results[0]);
        Assert.Equal(-200, results[1]);
        Assert.Equal(-1000, results[2]);
        Assert.Equal(-800, obv.LastChange); // Last change was -800
    }

    [Fact]
    public void OBV_WithHLCV_ShouldWorkCorrectly()
    {
        // Arrange
        var parameters = new POBV<HLCV, double>();
        var obv = new OBV_FP<HLCV, double>(parameters);

        var testData = new HLCV[]
        {
            new() { High = 100.5, Low = 99.5, Close = 100, Volume = 500 },
            new() { High = 103, Low = 101, Close = 102, Volume = 750 },
        };

        var results = new double[testData.Length];

        // Act
        obv.OnBarBatch(testData, results);

        // Assert
        Assert.Equal(500, results[0]); // First bar sets initial OBV
        Assert.Equal(1250, results[1]); // Price up, so add volume: 500 + 750 = 1250
        Assert.True(obv.IsReady);
    }

    [Fact]
    public void OBV_Uptrend_ShouldIncrease()
    {
        // Arrange
        var parameters = new POBV<OHLCV, double>();
        var obv = new OBV_FP<OHLCV, double>(parameters);

        // Create strong uptrend data
        var testData = new OHLCV[20];
        for (int i = 0; i < testData.Length; i++)
        {
            var price = 100.0 + i * 2;
            testData[i] = new OHLCV
            {
                Open = price - 1,
                High = price + 1,
                Low = price - 1,
                Close = price,
                Volume = 1000 + i * 100
            };
        }

        var results = new double[testData.Length];

        // Act
        obv.OnBarBatch(testData, results);

        // Assert
        Assert.True(obv.IsReady);
        // In uptrend, OBV should increase
        Assert.True(results[^1] > results[0], "OBV should increase in uptrend");
        Assert.True(obv.CurrentValue > 0, "Final OBV should be positive in uptrend");
    }

    [Fact]
    public void OBV_Downtrend_ShouldDecrease()
    {
        // Arrange
        var parameters = new POBV<OHLCV, double>();
        var obv = new OBV_FP<OHLCV, double>(parameters);

        // Create strong downtrend data
        var testData = new OHLCV[20];
        for (int i = 0; i < testData.Length; i++)
        {
            var price = 100.0 - i * 2;
            testData[i] = new OHLCV
            {
                Open = price + 1,
                High = price + 1,
                Low = price - 1,
                Close = price,
                Volume = 1000 + i * 100
            };
        }

        var results = new double[testData.Length];

        // Act
        obv.OnBarBatch(testData, results);

        // Assert
        Assert.True(obv.IsReady);
        // In downtrend, OBV should decrease
        Assert.True(results[^1] < results[0], "OBV should decrease in downtrend");
    }

    [Fact]
    public void OBV_Clear_ShouldResetState()
    {
        // Arrange
        var parameters = new POBV<OHLCV, double>();
        var obv = new OBV_FP<OHLCV, double>(parameters);

        var testData = new OHLCV[]
        {
            new() { Open = 10, High = 11, Low = 9, Close = 10.0, Volume = 1000 },
            new() { Open = 10, High = 13, Low = 10, Close = 12.0, Volume = 1500 },
        };

        obv.OnBarBatch(testData, new double[testData.Length]);

        // Verify it has data
        Assert.True(obv.IsReady);
        Assert.Equal(2500, obv.CurrentValue);

        // Act
        obv.Clear();

        // Assert
        Assert.False(obv.IsReady);
        Assert.Equal(0, obv.CurrentValue);
        Assert.Equal(0, obv.LastChange);
    }

    [Fact]
    public void OBV_PriceUnchanged_ShouldNotChangeOBV()
    {
        // Arrange
        var parameters = new POBV<OHLCV, double>();
        var obv = new OBV_FP<OHLCV, double>(parameters);

        var testData = new OHLCV[]
        {
            new() { Open = 100, High = 101, Low = 99, Close = 100.0, Volume = 1000 },
            new() { Open = 100, High = 101, Low = 99, Close = 100.0, Volume = 2000 }, // Same close
            new() { Open = 100, High = 101, Low = 99, Close = 100.0, Volume = 3000 }, // Same close
        };

        var results = new double[testData.Length];

        // Act
        obv.OnBarBatch(testData, results);

        // Assert
        Assert.Equal(1000, results[0]); // Initial OBV
        Assert.Equal(1000, results[1]); // No change (price same)
        Assert.Equal(1000, results[2]); // No change (price same)
        Assert.Equal(0, obv.LastChange); // Last change should be 0
    }
}
