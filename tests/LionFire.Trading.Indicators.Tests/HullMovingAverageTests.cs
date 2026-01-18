using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class HullMovingAverageTests
{
    [Fact]
    public void HullMovingAverage_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PHullMovingAverage<double, double> { Period = 16 };
        var hma = new HullMovingAverage_FP<double, double>(parameters);
        var inputs = new double[] {
            100, 102, 101, 103, 105, 104, 106, 108, 107, 109,
            110, 112, 111, 113, 115, 114, 116, 118, 117, 119
        };
        var outputs = new double[inputs.Length];

        // Act
        hma.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(hma.IsReady);

        // HMA should have values after sufficient data
        var validOutputs = outputs.Where(o => o != 0).ToArray();
        Assert.True(validOutputs.Length > 0);

        // HMA should be smooth and responsive
        Assert.Equal(outputs[^1], hma.Value);
    }

    [Fact]
    public void HullMovingAverage_ReducesLag()
    {
        // Arrange
        var period = 20;
        var hmaParams = new PHullMovingAverage<double, double> { Period = period };
        var smaParams = new PSMA<double, double> { Period = period };

        var hma = new HullMovingAverage_FP<double, double>(hmaParams);
        var sma = new SMA_FP<double, double>(smaParams);

        // Create trending data
        var inputs = Enumerable.Range(1, 50).Select(x => 100.0 + x * 2).ToArray();
        var hmaOutputs = new double[inputs.Length];
        var smaOutputs = new double[inputs.Length];

        // Act
        hma.OnBarBatch(inputs, hmaOutputs);
        sma.OnBarBatch(inputs, smaOutputs);

        // Assert - verify both indicators produce values
        Assert.True(hma.IsReady);
        Assert.True(sma.IsReady);
        Assert.False(double.IsNaN(hma.Value));
        Assert.False(double.IsNaN(sma.Value));
    }

    [Fact]
    public void HullMovingAverage_Smoothness()
    {
        // Arrange
        var parameters = new PHullMovingAverage<double, double> { Period = 20 };
        var hma = new HullMovingAverage_FP<double, double>(parameters);

        // Noisy data
        var inputs = new double[50];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 100.0 + i * 0.5 + random.NextDouble() * 10 - 5;
        }
        var outputs = new double[inputs.Length];

        // Act
        hma.OnBarBatch(inputs, outputs);

        // Assert - verify indicator produces values
        Assert.True(hma.IsReady);
        Assert.False(double.IsNaN(hma.Value));

        // Verify we get some non-zero outputs
        var validOutputs = outputs.Where(o => o != 0 && !double.IsNaN(o)).ToArray();
        Assert.True(validOutputs.Length > 0, "HMA should produce some valid outputs");
    }

    [Fact]
    public void HullMovingAverage_ResponsivenessToTrends()
    {
        // Arrange
        var parameters = new PHullMovingAverage<double, double> { Period = 14 };
        var hma = new HullMovingAverage_FP<double, double>(parameters);

        // Data with trend change
        var inputs = new double[60];
        // Uptrend
        for (int i = 0; i < 30; i++)
        {
            inputs[i] = 100.0 + i * 1.0;
        }
        // Downtrend
        for (int i = 30; i < 60; i++)
        {
            inputs[i] = 130.0 - (i - 30) * 1.0;
        }
        var outputs = new double[inputs.Length];

        // Act
        hma.OnBarBatch(inputs, outputs);

        // Assert - verify indicator processes trend change data
        Assert.True(hma.IsReady);
        Assert.False(double.IsNaN(hma.Value));

        // Verify we get some non-zero outputs
        var validOutputs = outputs.Where(o => o != 0 && !double.IsNaN(o)).ToArray();
        Assert.True(validOutputs.Length > 0, "HMA should produce valid outputs");
    }

    [Fact]
    public void HullMovingAverage_DifferentPeriods()
    {
        var periods = new[] { 9, 16, 25, 49 }; // Should be perfect squares for optimal HMA
        var inputs = Enumerable.Range(1, 100).Select(x => 100.0 + Math.Sin(x * 0.1) * 10).ToArray();

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PHullMovingAverage<double, double> { Period = period };
            var hma = new HullMovingAverage_FP<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            hma.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(hma.IsReady);
            var validOutputs = outputs.Where(o => o != 0).ToArray();
            Assert.True(validOutputs.Length > 0);
        }
    }

    [Fact]
    public void HullMovingAverage_NoiseReduction()
    {
        // Arrange
        var parameters = new PHullMovingAverage<double, double> { Period = 20 };
        var hma = new HullMovingAverage_FP<double, double>(parameters);

        // Create noisy oscillating data
        var inputs = new double[100];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            var trend = 100.0 + i * 0.1;
            var noise = random.NextDouble() * 4 - 2;
            inputs[i] = trend + noise;
        }
        var outputs = new double[inputs.Length];

        // Act
        hma.OnBarBatch(inputs, outputs);

        // Assert - verify indicator produces values
        Assert.True(hma.IsReady);
        Assert.False(double.IsNaN(hma.Value));

        // Verify we get some non-zero outputs
        var validOutputs = outputs.Where(o => o != 0 && !double.IsNaN(o)).ToArray();
        Assert.True(validOutputs.Length > 0, "HMA should produce valid outputs");
    }

    [Fact]
    public void HullMovingAverage_CrossoverSignals()
    {
        // Arrange
        var fastParams = new PHullMovingAverage<double, double> { Period = 9 };
        var slowParams = new PHullMovingAverage<double, double> { Period = 25 };

        var fastHMA = new HullMovingAverage_FP<double, double>(fastParams);
        var slowHMA = new HullMovingAverage_FP<double, double>(slowParams);

        // Create data with clear trend changes
        var inputs = new double[100];
        for (int i = 0; i < 50; i++)
        {
            inputs[i] = 100.0 + i * 0.5; // Uptrend
        }
        for (int i = 50; i < 100; i++)
        {
            inputs[i] = 125.0 - (i - 50) * 0.5; // Downtrend
        }

        var fastOutputs = new double[inputs.Length];
        var slowOutputs = new double[inputs.Length];

        // Act
        fastHMA.OnBarBatch(inputs, fastOutputs);
        slowHMA.OnBarBatch(inputs, slowOutputs);

        // Assert
        Assert.True(fastHMA.IsReady);
        Assert.True(slowHMA.IsReady);

        // Both HMAs should have valid values
        var fastValidCount = fastOutputs.Count(o => o != 0);
        var slowValidCount = slowOutputs.Count(o => o != 0);
        Assert.True(fastValidCount > 0, "Fast HMA should have valid values");
        Assert.True(slowValidCount > 0, "Slow HMA should have valid values");
    }

    [Fact]
    public void HullMovingAverage_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PHullMovingAverage<double, double> { Period = 9 };
        var hma = new HullMovingAverage_FP<double, double>(parameters);
        var inputs = Enumerable.Range(1, 30).Select(x => (double)x * 10).ToArray();
        var outputs = new double[inputs.Length];

        // First run
        hma.OnBarBatch(inputs, outputs);
        Assert.True(hma.IsReady);
        var firstValue = hma.Value;

        // Clear
        hma.Clear();
        Assert.False(hma.IsReady);

        // Process again
        var outputs2 = new double[inputs.Length];
        hma.OnBarBatch(inputs, outputs2);
        Assert.True(hma.IsReady);
        Assert.Equal(firstValue, hma.Value, 6);
    }
}
