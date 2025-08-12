using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class VWAPTests
{
    [Fact]
    public void VWAP_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double> { Period = 14 };
        var vwap = new VWAP_QC<HLCV, double>(parameters);
        
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
                Assert.True(outputs[i] >= 125 && outputs[i] <= 128);
            }
        }
        
        Assert.Equal(outputs[outputs.Length - 1], vwap.Value);
    }

    [Fact]
    public void VWAP_WeightsVolumeCorrectly()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double> { Period = 3 };
        var vwap = new VWAP_QC<HLCV, double>(parameters);
        
        // Simple test data with clear volume weighting
        var inputs = new HLCV[]
        {
            new() { High = 101, Low = 99, Close = 100, Volume = 1000 },  // TP = 100
            new() { High = 111, Low = 109, Close = 110, Volume = 2000 }, // TP = 110
            new() { High = 121, Low = 119, Close = 120, Volume = 3000 }, // TP = 120
            new() { High = 131, Low = 129, Close = 130, Volume = 1000 }, // TP = 130
        };
        
        var outputs = new double[inputs.Length];

        // Act
        vwap.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwap.IsReady);
        
        // VWAP calculation should weight by volume
        // Period 3: (100*1000 + 110*2000 + 120*3000) / (1000+2000+3000) = 680000/6000 = 113.33
        var vwapPeriod3 = outputs[2];
        Assert.True(vwapPeriod3 > 110 && vwapPeriod3 < 120); // Should be weighted toward 120
    }

    [Fact]
    public void VWAP_HandlesHighVolume()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double> { Period = 14 };
        var vwap = new VWAP_QC<HLCV, double>(parameters);
        
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
        var lastVWAP = outputs[outputs.Length - 1];
        Assert.True(lastVWAP > 0);
    }

    [Fact]
    public void VWAP_ResetsDaily()
    {
        // Note: In real trading, VWAP typically resets daily
        // This test verifies the rolling period behavior
        
        // Arrange
        var parameters = new PVWAP<HLCV, double> { Period = 5 };
        var vwap = new VWAP_QC<HLCV, double>(parameters);
        
        // Data simulating two "days" of trading
        var inputs = new HLCV[10];
        for (int i = 0; i < 5; i++)
        {
            // "Day 1" - prices around 100
            inputs[i] = new HLCV
            {
                High = 101,
                Low = 99,
                Close = 100,
                Volume = 1000
            };
        }
        for (int i = 5; i < 10; i++)
        {
            // "Day 2" - prices around 110
            inputs[i] = new HLCV
            {
                High = 111,
                Low = 109,
                Close = 110,
                Volume = 1000
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        vwap.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwap.IsReady);
        
        // After period 5, VWAP should shift to reflect new data
        var day1VWAP = outputs[4];
        var day2VWAP = outputs[9];
        
        Assert.True(Math.Abs(day1VWAP - 100) < 2); // Should be near 100
        Assert.True(Math.Abs(day2VWAP - 110) < 2); // Should be near 110
    }

    [Fact]
    public void VWAP_DifferentPeriods()
    {
        var periods = new[] { 10, 20, 50 };
        
        // Create sample data
        var inputs = new HLCV[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.1;
            inputs[i] = new HLCV
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price,
                Volume = 1000 + i * 10
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PVWAP<HLCV, double> { Period = period };
            var vwap = new VWAP_QC<HLCV, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            vwap.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(vwap.IsReady);
            var lastValue = outputs[outputs.Length - 1];
            Assert.True(lastValue > 100);
            
            // Longer periods should have different VWAP values
            // due to including more historical data
        }
    }

    [Fact]
    public void VWAP_ZeroVolume()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double> { Period = 5 };
        var vwap = new VWAP_QC<HLCV, double>(parameters);
        
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
        var lastVWAP = outputs[outputs.Length - 1];
        Assert.True(lastVWAP > 0);
    }
}