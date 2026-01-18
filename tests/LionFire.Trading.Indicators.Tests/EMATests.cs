using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class EMATests
{
    [Fact]
    public void EMA_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PEMA<double, double> { Period = 3 };
        var ema = new EMA_FP<double, double>(parameters);
        var inputs = new double[] { 2, 4, 6, 8, 12, 14, 16, 18, 20 };
        var outputs = new double[inputs.Length];

        // Act
        ema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ema.IsReady);

        // EMA calculation with smoothing = 2/(period+1) = 0.5 for period 3
        // First value is the average of first 3: (2+4+6)/3 = 4
        Assert.True(double.IsNaN(outputs[0])); // Not ready (FP returns NaN)
        Assert.True(double.IsNaN(outputs[1])); // Not ready
        Assert.Equal(4, outputs[2], 2); // Initial SMA
        Assert.Equal(6, outputs[3], 2); // 4 + 0.5 * (8 - 4)
        Assert.Equal(9, outputs[4], 2); // 6 + 0.5 * (12 - 6)
        Assert.Equal(11.5, outputs[5], 2); // 9 + 0.5 * (14 - 9)
        Assert.Equal(13.75, outputs[6], 2); // 11.5 + 0.5 * (16 - 11.5)
        Assert.Equal(15.875, outputs[7], 2); // 13.75 + 0.5 * (18 - 13.75)
        Assert.Equal(17.9375, outputs[8], 2); // 15.875 + 0.5 * (20 - 15.875)
    }

    [Fact]
    public void EMA_HandlesVariousPeriods()
    {
        // Test with different periods
        var periods = new[] { 5, 10, 20, 50 };
        var inputs = Enumerable.Range(1, 100).Select(x => (double)x).ToArray();

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PEMA<double, double> { Period = period };
            var ema = new EMA_FP<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            ema.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(ema.IsReady);
            Assert.False(double.IsNaN(outputs[period - 1])); // Should have output at period
            Assert.True(outputs[inputs.Length - 1] > outputs[period - 1]); // Should be trending up
            Assert.Equal(outputs[inputs.Length - 1], ema.Value);
        }
    }

    [Fact]
    public void EMA_HandlesSingleValue()
    {
        // Arrange
        var parameters = new PEMA<double, double> { Period = 10 };
        var ema = new EMA_FP<double, double>(parameters);
        var inputs = new double[] { 100 };
        var outputs = new double[1];

        // Act
        ema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.False(ema.IsReady); // Not ready with single value for period 10
    }

    [Fact]
    public void EMA_HandlesVolatileData()
    {
        // Arrange
        var parameters = new PEMA<double, double> { Period = 5 };
        var ema = new EMA_FP<double, double>(parameters);
        var inputs = new double[] { 100, 50, 150, 25, 175, 10, 200, 5, 195 };
        var outputs = new double[inputs.Length];

        // Act
        ema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ema.IsReady);
        // EMA should smooth out volatility - check non-NaN values are positive
        for (int i = parameters.Period - 1; i < outputs.Length; i++)
        {
            Assert.False(double.IsNaN(outputs[i]));
            Assert.True(outputs[i] > 0);
        }
    }

    [Fact]
    public void EMA_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PEMA<double, double> { Period = 5 };
        var ema = new EMA_FP<double, double>(parameters);
        var inputs = Enumerable.Range(1, 20).Select(x => (double)x).ToArray();
        var outputs = new double[inputs.Length];

        // First run
        ema.OnBarBatch(inputs, outputs);
        Assert.True(ema.IsReady);
        var firstValue = ema.Value;

        // Act - Clear and verify reset
        ema.Clear();
        Assert.False(ema.IsReady);

        // Process again - should get same result
        var outputs2 = new double[inputs.Length];
        ema.OnBarBatch(inputs, outputs2);
        Assert.True(ema.IsReady);
        Assert.Equal(firstValue, ema.Value, 10);
    }

    [Fact]
    public void EMA_PropertyMatchesLastOutput()
    {
        // Arrange
        var parameters = new PEMA<double, double> { Period = 5 };
        var ema = new EMA_FP<double, double>(parameters);
        var inputs = Enumerable.Range(1, 20).Select(x => (double)x).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        ema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ema.IsReady);
        Assert.Equal(ema.Value, outputs[^1]);
        Assert.Equal(parameters.Period, ema.Period);
    }
}
