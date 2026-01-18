using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class VWAPTests
{
    [Fact]
    public void VWAP_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double>();
        var vwap = new VWAP_FP<HLCV, double>(parameters);

        // Sample HLCV data
        var inputs = new HLCV[]
        {
            new() { High = 127.36, Low = 126.99, Close = 127.28, Volume = 89329 },
            new() { High = 127.31, Low = 126.48, Close = 127.18, Volume = 16137 },
            new() { High = 127.21, Low = 126.71, Close = 127.17, Volume = 23945 },
            new() { High = 127.21, Low = 126.64, Close = 127.01, Volume = 20679 },
            new() { High = 126.98, Low = 126.48, Close = 126.85, Volume = 27252 },
            new() { High = 126.75, Low = 126.22, Close = 126.39, Volume = 20915 },
            new() { High = 126.44, Low = 125.81, Close = 126.41, Volume = 31179 },
            new() { High = 126.82, Low = 126.01, Close = 126.70, Volume = 23492 },
            new() { High = 126.55, Low = 126.01, Close = 126.11, Volume = 15896 },
            new() { High = 126.30, Low = 125.91, Close = 125.93, Volume = 14398 },
            new() { High = 126.14, Low = 125.48, Close = 126.09, Volume = 18963 },
            new() { High = 126.49, Low = 125.87, Close = 126.35, Volume = 19148 },
            new() { High = 126.45, Low = 125.66, Close = 125.71, Volume = 17165 },
            new() { High = 125.78, Low = 125.36, Close = 125.69, Volume = 13912 },
            new() { High = 125.98, Low = 125.52, Close = 125.60, Volume = 13572 }
        };

        var outputs = new double[inputs.Length];

        // Act
        vwap.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwap.IsReady);

        // VWAP should be positive and reasonable
        for (int i = 0; i < outputs.Length; i++)
        {
            if (outputs[i] > 0)
            {
                // VWAP should be within the price range
                Assert.True(outputs[i] >= 125 && outputs[i] <= 128,
                    $"VWAP at {i} should be in price range, was {outputs[i]}");
            }
        }

        Assert.Equal(outputs[^1], vwap.Value);
    }

    [Fact]
    public void VWAP_WeightsVolumeCorrectly()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double>();
        var vwap = new VWAP_FP<HLCV, double>(parameters);

        // Simple test data with clear volume weighting
        var inputs = new HLCV[]
        {
            new() { High = 101, Low = 99, Close = 100, Volume = 1000 },  // Close = 100
            new() { High = 111, Low = 109, Close = 110, Volume = 2000 }, // Close = 110
            new() { High = 121, Low = 119, Close = 120, Volume = 3000 }, // Close = 120
        };

        var outputs = new double[inputs.Length];

        // Act
        vwap.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwap.IsReady);

        // VWAP calculation should weight by volume
        // (100*1000 + 110*2000 + 120*3000) / (1000+2000+3000) = 680000/6000 = 113.33
        var expectedVWAP = (100.0 * 1000 + 110.0 * 2000 + 120.0 * 3000) / (1000 + 2000 + 3000);
        Assert.Equal(expectedVWAP, outputs[2], 1);
    }

    [Fact]
    public void VWAP_HandlesHighVolume()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double>();
        var vwap = new VWAP_FP<HLCV, double>(parameters);

        // Data with varying volumes
        var inputs = new HLCV[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 5;
            var volume = i < 10 ? 1000 : 10000; // High volume in second half
            inputs[i] = new HLCV
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price,
                Volume = volume
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        vwap.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwap.IsReady);

        // VWAP should be influenced more by high volume periods
        var lastVWAP = outputs[^1];
        Assert.True(lastVWAP > 0);
    }

    [Fact]
    public void VWAP_CumulativeProperties()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double>();
        var vwap = new VWAP_FP<HLCV, double>(parameters);

        var inputs = new HLCV[]
        {
            new() { High = 102, Low = 98, Close = 100, Volume = 1000 },
            new() { High = 112, Low = 108, Close = 110, Volume = 2000 },
        };

        var outputs = new double[inputs.Length];

        // Act
        vwap.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwap.IsReady);
        Assert.True(vwap.CumulativePriceVolume > 0);
        Assert.True(vwap.CumulativeVolume > 0);
        Assert.Equal(3000, vwap.CumulativeVolume); // 1000 + 2000
    }

    [Fact]
    public void VWAP_ZeroVolume()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double>();
        var vwap = new VWAP_FP<HLCV, double>(parameters);

        // Data with some zero volume bars
        var inputs = new HLCV[]
        {
            new() { High = 101, Low = 99, Close = 100, Volume = 1000 },
            new() { High = 102, Low = 100, Close = 101, Volume = 0 }, // Zero volume
            new() { High = 103, Low = 101, Close = 102, Volume = 2000 },
            new() { High = 104, Low = 102, Close = 103, Volume = 0 }, // Zero volume
            new() { High = 105, Low = 103, Close = 104, Volume = 3000 },
        };

        var outputs = new double[inputs.Length];

        // Act
        vwap.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwap.IsReady);

        // VWAP should handle zero volume gracefully
        var lastVWAP = outputs[^1];
        Assert.True(lastVWAP > 0);
    }

    [Fact]
    public void VWAP_TrendingData()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double>();
        var vwap = new VWAP_FP<HLCV, double>(parameters);

        // Uptrending data
        var inputs = new HLCV[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 1.0;
            inputs[i] = new HLCV
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price,
                Volume = 1000
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        vwap.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwap.IsReady);

        // VWAP should lag behind the current price in an uptrend
        var lastPrice = inputs[^1].Close;
        Assert.True(vwap.Value < lastPrice, "VWAP should lag behind price in uptrend");
    }

    [Fact]
    public void VWAP_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double>();
        var vwap = new VWAP_FP<HLCV, double>(parameters);

        var inputs = new HLCV[]
        {
            new() { High = 102, Low = 98, Close = 100, Volume = 1000 },
            new() { High = 112, Low = 108, Close = 110, Volume = 2000 },
        };

        vwap.OnBarBatch(inputs, new double[inputs.Length]);
        Assert.True(vwap.IsReady);

        // Act
        vwap.Clear();

        // Assert
        Assert.False(vwap.IsReady);
        Assert.Equal(0, vwap.Value);
        Assert.Equal(0, vwap.CumulativePriceVolume);
        Assert.Equal(0, vwap.CumulativeVolume);
    }
}
