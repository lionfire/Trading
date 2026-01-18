using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class ChaikinMoneyFlowTests
{
    [Fact]
    public void ChaikinMoneyFlow_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PChaikinMoneyFlow<HLCV, double> { Period = 20 };
        var cmf = new ChaikinMoneyFlow_FP<HLCV, double>(parameters);

        // Sample HLCV data
        var inputs = new HLCV[]
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
            new() { High = 27.09, Low = 26.46, Close = 26.78, Volume = 32064 },
            new() { High = 26.87, Low = 26.23, Close = 26.35, Volume = 25551 },
            new() { High = 26.88, Low = 26.24, Close = 26.81, Volume = 19347 }
        };

        var outputs = new double[inputs.Length];

        // Act
        cmf.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(cmf.IsReady);

        // CMF should be between -1 and 1
        for (int i = parameters.Period - 1; i < outputs.Length; i++)
        {
            if (!double.IsNaN(outputs[i]))
            {
                Assert.InRange(outputs[i], -1, 1);
            }
        }

        Assert.Equal(outputs[^1], cmf.CurrentValue);
    }

    [Fact]
    public void ChaikinMoneyFlow_DetectsAccumulationDistribution()
    {
        // Arrange
        var parameters = new PChaikinMoneyFlow<HLCV, double> { Period = 20 };

        // Accumulation: price rising with close near high
        var accumulation = new HLCV[30];
        for (int i = 0; i < accumulation.Length; i++)
        {
            var price = 100.0 + i * 1.0;
            accumulation[i] = new HLCV
            {
                High = price + 1.0,
                Low = price - 0.5,
                Close = price + 0.8, // Close near high
                Volume = 10000 + i * 100
            };
        }

        // Distribution: price falling with close near low
        var distribution = new HLCV[30];
        for (int i = 0; i < distribution.Length; i++)
        {
            var price = 100.0 - i * 1.0;
            distribution[i] = new HLCV
            {
                High = price + 0.5,
                Low = price - 1.0,
                Close = price - 0.8, // Close near low
                Volume = 10000 + i * 100
            };
        }

        // Act
        var cmfAccum = new ChaikinMoneyFlow_FP<HLCV, double>(parameters);
        var accumOutputs = new double[accumulation.Length];
        cmfAccum.OnBarBatch(accumulation, accumOutputs);
        var accumCMF = accumOutputs[^1];

        var cmfDist = new ChaikinMoneyFlow_FP<HLCV, double>(parameters);
        var distOutputs = new double[distribution.Length];
        cmfDist.OnBarBatch(distribution, distOutputs);
        var distCMF = distOutputs[^1];

        // Assert
        Assert.True(accumCMF > 0, $"Accumulation CMF {accumCMF} should be positive");
        Assert.True(distCMF < 0, $"Distribution CMF {distCMF} should be negative");
    }

    [Fact]
    public void ChaikinMoneyFlow_HandlesZeroVolume()
    {
        // Arrange
        var parameters = new PChaikinMoneyFlow<HLCV, double> { Period = 20 };
        var cmf = new ChaikinMoneyFlow_FP<HLCV, double>(parameters);

        // Data with some zero volume bars
        var inputs = new HLCV[25];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 5;
            var volume = i % 5 == 0 ? 0 : 10000; // Every 5th bar has zero volume
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
        cmf.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(cmf.IsReady);

        // CMF should handle zero volume gracefully
        var lastCMF = outputs[^1];
        Assert.InRange(lastCMF, -1, 1);
    }

    [Fact]
    public void ChaikinMoneyFlow_NeutralMarket()
    {
        // Arrange
        var parameters = new PChaikinMoneyFlow<HLCV, double> { Period = 20 };
        var cmf = new ChaikinMoneyFlow_FP<HLCV, double>(parameters);

        // Neutral market with close at midpoint
        var inputs = new HLCV[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 2;
            inputs[i] = new HLCV
            {
                High = price + 1,
                Low = price - 1,
                Close = price, // Close at midpoint
                Volume = 10000
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        cmf.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(cmf.IsReady);

        // CMF should be near zero for neutral market
        var lastCMF = outputs[^1];
        Assert.InRange(lastCMF, -0.2, 0.2);
    }

    [Fact]
    public void ChaikinMoneyFlow_VolumeWeighting()
    {
        // Arrange
        var parameters = new PChaikinMoneyFlow<HLCV, double> { Period = 10 };
        var cmf = new ChaikinMoneyFlow_FP<HLCV, double>(parameters);

        // Data with varying volume emphasis
        var inputs = new HLCV[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            // High volume on accumulation days (close near high)
            // Low volume on distribution days
            var isAccumDay = i % 2 == 0;
            inputs[i] = new HLCV
            {
                High = price + 1,
                Low = price - 1,
                Close = isAccumDay ? price + 0.8 : price - 0.8,
                Volume = isAccumDay ? 50000 : 5000
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        cmf.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(cmf.IsReady);

        // CMF should be positive due to higher volume on accumulation days
        var lastCMF = outputs[^1];
        Assert.True(lastCMF > 0, "CMF should be positive when accumulation has higher volume");
    }

    [Fact]
    public void ChaikinMoneyFlow_DifferentPeriods()
    {
        var periods = new[] { 10, 20, 30 };

        // Create sample data
        var inputs = new HLCV[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.2) * 5;
            inputs[i] = new HLCV
            {
                High = price + 1,
                Low = price - 1,
                Close = price + Math.Sin(i * 0.3) * 0.5,
                Volume = 10000 + i * 100
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PChaikinMoneyFlow<HLCV, double> { Period = period };
            var cmf = new ChaikinMoneyFlow_FP<HLCV, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            cmf.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(cmf.IsReady);
            var lastValue = outputs[^1];
            Assert.InRange(lastValue, -1, 1);
        }
    }

    [Fact]
    public void ChaikinMoneyFlow_SumProperties()
    {
        // Arrange
        var parameters = new PChaikinMoneyFlow<HLCV, double> { Period = 10 };
        var cmf = new ChaikinMoneyFlow_FP<HLCV, double>(parameters);

        var inputs = new HLCV[15];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLCV
            {
                High = price + 1,
                Low = price - 1,
                Close = price + 0.5, // Close above midpoint
                Volume = 10000
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        cmf.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(cmf.IsReady);
        Assert.True(cmf.VolumeSum > 0, "Volume sum should be positive");
    }

    [Fact]
    public void ChaikinMoneyFlow_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PChaikinMoneyFlow<HLCV, double> { Period = 10 };
        var cmf = new ChaikinMoneyFlow_FP<HLCV, double>(parameters);

        var inputs = new HLCV[15];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLCV
            {
                High = price + 1,
                Low = price - 1,
                Close = price,
                Volume = 10000
            };
        }

        cmf.OnBarBatch(inputs, new double[inputs.Length]);
        Assert.True(cmf.IsReady);

        // Act
        cmf.Clear();

        // Assert
        Assert.False(cmf.IsReady);
        Assert.Equal(0, cmf.CurrentValue);
    }
}
