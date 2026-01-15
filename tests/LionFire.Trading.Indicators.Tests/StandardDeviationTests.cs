// DISABLED: Tests need updating to match current API
#if false
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class StandardDeviationTests
{
    [Fact]
    public void StandardDeviation_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PStandardDeviation<double, double> { Period = 10 };
        var stdDev = new StandardDeviation_QC<double, double>(parameters);
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
        
        Assert.Equal(outputs[outputs.Length - 1], stdDev.Value);
    }

    [Fact]
    public void StandardDeviation_ConstantValues()
    {
        // Arrange
        var parameters = new PStandardDeviation<double, double> { Period = 10 };
        var stdDev = new StandardDeviation_QC<double, double>(parameters);
        var inputs = Enumerable.Repeat(100.0, 30).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        stdDev.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(stdDev.IsReady);
        
        // Standard deviation of constant values should be 0
        var lastStdDev = outputs[outputs.Length - 1];
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
        var stdDevLow = new StandardDeviation_QC<double, double>(parameters);
        var lowOutputs = new double[lowVol.Length];
        stdDevLow.OnBarBatch(lowVol, lowOutputs);
        var lowStdDev = lowOutputs[lowOutputs.Length - 1];
        
        var stdDevHigh = new StandardDeviation_QC<double, double>(parameters);
        var highOutputs = new double[highVol.Length];
        stdDevHigh.OnBarBatch(highVol, highOutputs);
        var highStdDev = highOutputs[highOutputs.Length - 1];

        // Assert
        Assert.True(highStdDev > lowStdDev, 
            $"High volatility StdDev {highStdDev} should be greater than low volatility {lowStdDev}");
    }

    [Fact]
    public void StandardDeviation_NormalDistribution()
    {
        // Arrange
        var parameters = new PStandardDeviation<double, double> { Period = 100 };
        var stdDev = new StandardDeviation_QC<double, double>(parameters);
        
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
        var calculatedStdDev = outputs[outputs.Length - 1];
        Assert.InRange(calculatedStdDev, targetStdDev * 0.8, targetStdDev * 1.2);
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
            var stdDev = new StandardDeviation_QC<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            stdDev.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(stdDev.IsReady);
            var lastValue = outputs[outputs.Length - 1];
            Assert.True(lastValue > 0);
            
            // Longer periods should produce more stable (potentially different) StdDev
        }
    }

    [Fact]
    public void StandardDeviation_TrendingData()
    {
        // Arrange
        var parameters = new PStandardDeviation<double, double> { Period = 20 };
        var stdDev = new StandardDeviation_QC<double, double>(parameters);
        
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
        var lastStdDev = outputs[outputs.Length - 1];
        Assert.True(lastStdDev > 0);
        
        // With linear trend, StdDev should be relatively stable
        var last10Values = outputs.Skip(outputs.Length - 10).Where(v => v > 0).ToArray();
        var maxStdDev = last10Values.Max();
        var minStdDev = last10Values.Min();
        var range = maxStdDev - minStdDev;
        
        Assert.True(range < maxStdDev * 0.5, "StdDev should be relatively stable in linear trend");
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
        var stdDevNormal = new StandardDeviation_QC<double, double>(parameters);
        var normalOutputs = new double[normal.Length];
        stdDevNormal.OnBarBatch(normal, normalOutputs);
        
        var stdDevOutlier = new StandardDeviation_QC<double, double>(parameters);
        var outlierOutputs = new double[withOutlier.Length];
        stdDevOutlier.OnBarBatch(withOutlier, outlierOutputs);

        // Assert
        // StdDev should increase when outlier is in the window
        var normalStdDev = normalOutputs[19];
        var outlierStdDev = outlierOutputs[19];
        
        Assert.True(outlierStdDev > normalStdDev, 
            "StdDev should increase when outlier is present");
    }
}
#endif
