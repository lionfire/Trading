using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class StandardDeviationTests
{
    [Fact]
    public void StandardDeviation_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PStandardDeviation<double, double> { Period = 10 };
        var stdDev = new StandardDeviation_FP<double, double>(parameters);
        var inputs = new double[] {
            10, 12, 11, 13, 15, 14, 16, 18, 17, 19,
            20, 22, 21, 23, 25, 24, 26, 28, 27, 29
        };
        var outputs = new double[inputs.Length];

        // Act
        stdDev.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(stdDev.IsReady);

        // Standard deviation should be positive
        for (int i = parameters.Period - 1; i < outputs.Length; i++)
        {
            Assert.True(outputs[i] > 0, $"StdDev at index {i} should be positive");
        }

        Assert.Equal(outputs[^1], stdDev.Value);
    }

    [Fact]
    public void StandardDeviation_ConstantValues()
    {
        // Arrange
        var parameters = new PStandardDeviation<double, double> { Period = 10 };
        var stdDev = new StandardDeviation_FP<double, double>(parameters);
        var inputs = Enumerable.Repeat(100.0, 30).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        stdDev.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(stdDev.IsReady);

        // Standard deviation of constant values should be 0
        var lastStdDev = stdDev.Value;
        Assert.Equal(0, lastStdDev, 5);
    }

    [Fact]
    public void StandardDeviation_HighVolatility()
    {
        // Arrange
        var parameters = new PStandardDeviation<double, double> { Period = 10 };

        // Low volatility data
        var lowVol = new double[30];
        for (int i = 0; i < lowVol.Length; i++)
        {
            lowVol[i] = 100.0 + (i % 2) * 0.5; // Small changes
        }

        // High volatility data
        var highVol = new double[30];
        for (int i = 0; i < highVol.Length; i++)
        {
            highVol[i] = 100.0 + (i % 2) * 10; // Large changes
        }

        // Act
        var stdDevLow = new StandardDeviation_FP<double, double>(parameters);
        var lowOutputs = new double[lowVol.Length];
        stdDevLow.OnBarBatch(lowVol, lowOutputs);
        var lowStdDevValue = stdDevLow.Value;

        var stdDevHigh = new StandardDeviation_FP<double, double>(parameters);
        var highOutputs = new double[highVol.Length];
        stdDevHigh.OnBarBatch(highVol, highOutputs);
        var highStdDevValue = stdDevHigh.Value;

        // Assert
        Assert.True(highStdDevValue > lowStdDevValue,
            $"High volatility StdDev {highStdDevValue} should be greater than low volatility {lowStdDevValue}");
    }

    [Fact]
    public void StandardDeviation_NormalDistribution()
    {
        // Arrange
        var parameters = new PStandardDeviation<double, double> { Period = 100 };
        var stdDev = new StandardDeviation_FP<double, double>(parameters);

        // Create normally distributed data
        var random = new Random(42);
        var inputs = new double[200];
        var mean = 100.0;
        var targetStdDev = 10.0;

        for (int i = 0; i < inputs.Length; i++)
        {
            // Box-Muller transform for normal distribution
            var u1 = random.NextDouble();
            var u2 = random.NextDouble();
            var normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            inputs[i] = mean + normal * targetStdDev;
        }

        var outputs = new double[inputs.Length];

        // Act
        stdDev.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(stdDev.IsReady);

        // Calculated StdDev should be close to target
        var calculatedStdDev = stdDev.Value;
        Assert.InRange(calculatedStdDev, targetStdDev * 0.5, targetStdDev * 1.5);
    }

    [Fact]
    public void StandardDeviation_DifferentPeriods()
    {
        var periods = new[] { 10, 20, 50 };
        var inputs = new double[100];
        var random = new Random(42);

        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 100.0 + random.NextDouble() * 20 - 10;
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PStandardDeviation<double, double> { Period = period };
            var stdDev = new StandardDeviation_FP<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            stdDev.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(stdDev.IsReady);
            var lastValue = stdDev.Value;
            Assert.True(lastValue > 0);
        }
    }

    [Fact]
    public void StandardDeviation_TrendingData()
    {
        // Arrange
        var parameters = new PStandardDeviation<double, double> { Period = 20 };
        var stdDev = new StandardDeviation_FP<double, double>(parameters);

        // Linear trend with noise
        var inputs = new double[50];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            var trend = 100.0 + i * 1.0;
            var noise = random.NextDouble() * 2 - 1;
            inputs[i] = trend + noise;
        }
        var outputs = new double[inputs.Length];

        // Act
        stdDev.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(stdDev.IsReady);

        // StdDev should capture dispersion around the mean
        var lastStdDev = stdDev.Value;
        Assert.True(lastStdDev > 0);
    }

    [Fact]
    public void StandardDeviation_OutlierImpact()
    {
        // Arrange
        var parameters = new PStandardDeviation<double, double> { Period = 10 };

        // Normal data
        var normal = Enumerable.Repeat(100.0, 20).ToArray();

        // Data with outlier
        var withOutlier = Enumerable.Repeat(100.0, 20).ToArray();
        withOutlier[15] = 150.0; // Add outlier

        // Act
        var stdDevNormal = new StandardDeviation_FP<double, double>(parameters);
        var normalOutputs = new double[normal.Length];
        stdDevNormal.OnBarBatch(normal, normalOutputs);

        var stdDevOutlier = new StandardDeviation_FP<double, double>(parameters);
        var outlierOutputs = new double[withOutlier.Length];
        stdDevOutlier.OnBarBatch(withOutlier, outlierOutputs);

        // Assert
        // StdDev should increase when outlier is in the window
        var normalStdDevValue = stdDevNormal.Value;
        var outlierStdDevValue = stdDevOutlier.Value;

        Assert.True(outlierStdDevValue > normalStdDevValue,
            "StdDev should increase when outlier is present");
    }

    [Fact]
    public void StandardDeviation_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PStandardDeviation<double, double> { Period = 5 };
        var stdDev = new StandardDeviation_FP<double, double>(parameters);
        var inputs = Enumerable.Range(1, 20).Select(x => (double)x * 10).ToArray();
        var outputs = new double[inputs.Length];

        // First run
        stdDev.OnBarBatch(inputs, outputs);
        Assert.True(stdDev.IsReady);
        var firstValue = stdDev.Value;

        // Clear
        stdDev.Clear();
        Assert.False(stdDev.IsReady);

        // Process again
        var outputs2 = new double[inputs.Length];
        stdDev.OnBarBatch(inputs, outputs2);
        Assert.True(stdDev.IsReady);
        Assert.Equal(firstValue, stdDev.Value, 6);
    }
}
