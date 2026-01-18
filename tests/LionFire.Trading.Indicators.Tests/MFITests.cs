using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class MFITests
{
    [Fact]
    public void MFI_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PMFI<OHLCV, double> { Period = 14 };
        var mfi = new MFI_FP<OHLCV, double>(parameters);

        // Sample HLCV data
        var inputs = new OHLCV[]
        {
            new() { High = 24.63, Low = 24.20, Close = 24.28, Volume = 18730 },
            new() { High = 24.69, Low = 24.21, Close = 24.51, Volume = 12272 },
            new() { High = 24.99, Low = 24.44, Close = 24.68, Volume = 24691 },
            new() { High = 25.36, Low = 24.64, Close = 25.21, Volume = 18358 },
            new() { High = 25.19, Low = 24.82, Close = 24.87, Volume = 22964 },
            new() { High = 25.17, Low = 24.71, Close = 25.07, Volume = 15919 },
            new() { High = 25.01, Low = 24.62, Close = 24.77, Volume = 16067 },
            new() { High = 24.88, Low = 24.48, Close = 24.56, Volume = 16568 },
            new() { High = 25.24, Low = 24.49, Close = 25.17, Volume = 16019 },
            new() { High = 25.72, Low = 25.13, Close = 25.60, Volume = 9774 },
            new() { High = 26.22, Low = 25.49, Close = 26.13, Volume = 22573 },
            new() { High = 26.48, Low = 25.87, Close = 25.97, Volume = 15472 },
            new() { High = 26.18, Low = 25.60, Close = 25.71, Volume = 15843 },
            new() { High = 26.09, Low = 25.35, Close = 26.02, Volume = 19089 },
            new() { High = 26.25, Low = 25.71, Close = 25.91, Volume = 18924 },
            new() { High = 26.24, Low = 25.37, Close = 25.53, Volume = 17851 },
            new() { High = 26.25, Low = 25.42, Close = 26.17, Volume = 18372 },
            new() { High = 26.61, Low = 26.02, Close = 26.47, Volume = 20676 },
            new() { High = 26.95, Low = 26.39, Close = 26.69, Volume = 26202 },
            new() { High = 27.09, Low = 26.46, Close = 26.78, Volume = 32064 }
        };

        var outputs = new double[inputs.Length];

        // Act
        mfi.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(mfi.IsReady);

        // MFI should be between 0 and 100
        for (int i = parameters.Period; i < outputs.Length; i++)
        {
            if (!double.IsNaN(outputs[i]))
            {
                Assert.InRange(outputs[i], 0, 100);
            }
        }

        Assert.Equal(outputs[^1], mfi.CurrentValue);
    }

    [Fact]
    public void MFI_DetectsOverboughtOversold()
    {
        // Arrange
        var parameters = new PMFI<OHLCV, double> { Period = 14 };

        // Create data with high buying pressure (should be overbought)
        var buyingPressure = new OHLCV[30];
        for (int i = 0; i < buyingPressure.Length; i++)
        {
            var price = 100.0 + i * 2; // Strong uptrend
            var volume = 10000 + i * 500; // Increasing volume
            buyingPressure[i] = new OHLCV
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.4, // Close near high
                Volume = volume
            };
        }

        // Create data with high selling pressure (should be oversold)
        var sellingPressure = new OHLCV[30];
        for (int i = 0; i < sellingPressure.Length; i++)
        {
            var price = 100.0 - i * 2; // Strong downtrend
            var volume = 10000 + i * 500; // Increasing volume
            sellingPressure[i] = new OHLCV
            {
                High = price + 0.3,
                Low = price - 0.5,
                Close = price - 0.4, // Close near low
                Volume = volume
            };
        }

        // Act
        var mfiBuy = new MFI_FP<OHLCV, double>(parameters);
        var buyOutputs = new double[buyingPressure.Length];
        mfiBuy.OnBarBatch(buyingPressure, buyOutputs);
        var buyMFI = buyOutputs[^1];

        var mfiSell = new MFI_FP<OHLCV, double>(parameters);
        var sellOutputs = new double[sellingPressure.Length];
        mfiSell.OnBarBatch(sellingPressure, sellOutputs);
        var sellMFI = sellOutputs[^1];

        // Assert
        Assert.True(buyMFI > 70, $"Buying pressure MFI {buyMFI} should be > 70 (overbought)");
        Assert.True(sellMFI < 30, $"Selling pressure MFI {sellMFI} should be < 30 (oversold)");
    }

    [Fact]
    public void MFI_HandlesVolumeChanges()
    {
        // Arrange
        var parameters = new PMFI<OHLCV, double> { Period = 14 };
        var mfi = new MFI_FP<OHLCV, double>(parameters);

        // Data with varying volume
        var inputs = new OHLCV[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 5;
            var volume = i < 15 ? 1000 : 10000; // Volume spike in second half
            inputs[i] = new OHLCV
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price,
                Volume = volume
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        mfi.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(mfi.IsReady);

        // MFI should respond to volume changes
        var lowVolumeMFI = outputs[14]; // Before volume spike
        var highVolumeMFI = outputs[^1]; // After volume spike

        // Values should be different (volume matters)
        Assert.NotEqual(lowVolumeMFI, highVolumeMFI);
    }

    [Fact]
    public void MFI_MoneyFlowProperties()
    {
        // Arrange
        var parameters = new PMFI<OHLCV, double> { Period = 14 };
        var mfi = new MFI_FP<OHLCV, double>(parameters);

        // Data with constant volume
        var inputs = new OHLCV[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 5;
            inputs[i] = new OHLCV
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price,
                Volume = 10000 // Constant volume
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        mfi.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(mfi.IsReady);

        // MFI should be between 0 and 100
        var lastMFI = outputs[^1];
        Assert.InRange(lastMFI, 0, 100);

        // Positive and negative money flows should be tracked
        Assert.True(mfi.PositiveMoneyFlow >= 0);
        Assert.True(mfi.NegativeMoneyFlow >= 0);
    }

    [Fact]
    public void MFI_DifferentPeriods()
    {
        var periods = new[] { 10, 14, 20 };

        // Create sample data
        var inputs = new OHLCV[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new OHLCV
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price + 0.2,
                Volume = 10000 + i * 100
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PMFI<OHLCV, double> { Period = period };
            var mfi = new MFI_FP<OHLCV, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            mfi.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(mfi.IsReady);
            var lastValue = outputs[^1];
            Assert.InRange(lastValue, 0, 100);

            // Shorter periods should be more sensitive
        }
    }

    [Fact]
    public void MFI_HandlesZeroVolume()
    {
        // Arrange
        var parameters = new PMFI<OHLCV, double> { Period = 14 };
        var mfi = new MFI_FP<OHLCV, double>(parameters);

        // Data with some zero volume bars
        var inputs = new OHLCV[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            var volume = i % 5 == 0 ? 0 : 10000; // Every 5th bar has zero volume
            inputs[i] = new OHLCV
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price,
                Volume = volume
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        mfi.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(mfi.IsReady);

        // MFI should handle zero volume gracefully
        var lastMFI = outputs[^1];
        Assert.InRange(lastMFI, 0, 100);
    }

    [Fact]
    public void MFI_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PMFI<OHLCV, double> { Period = 10 };
        var mfi = new MFI_FP<OHLCV, double>(parameters);

        var inputs = new OHLCV[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new OHLCV
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price,
                Volume = 10000
            };
        }

        mfi.OnBarBatch(inputs, new double[inputs.Length]);
        Assert.True(mfi.IsReady);

        // Act
        mfi.Clear();

        // Assert
        Assert.False(mfi.IsReady);
    }
}
