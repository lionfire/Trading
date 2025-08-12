using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class EMATests
{
    [Fact]
    public void EMA_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PEMA<double, double> { Period = 3 };
        var ema = new EMA_QC<double, double>(parameters);
        var inputs = new double[] { 2, 4, 6, 8, 12, 14, 16, 18, 20 };
        var outputs = new double[inputs.Length];

        // Act
        ema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ema.IsReady);
        
        // EMA calculation with smoothing = 2/(period+1) = 0.5 for period 3
        // First value is the average of first 3: (2+4+6)/3 = 4
        Assert.Equal(0, outputs[0]); // Not ready
        Assert.Equal(0, outputs[1]); // Not ready
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
            var ema = new EMA_QC<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            ema.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(ema.IsReady);
            Assert.True(outputs[period - 1] > 0); // Should have output at period
            Assert.True(outputs[inputs.Length - 1] > outputs[period - 1]); // Should be trending up
            Assert.Equal(outputs[inputs.Length - 1], ema.Value);
        }
    }

    [Fact]
    public void EMA_HandlesSingleValue()
    {
        // Arrange
        var parameters = new PEMA<double, double> { Period = 10 };
        var ema = new EMA_QC<double, double>(parameters);

        // Act
        ema.OnBar(100);

        // Assert
        Assert.False(ema.IsReady); // Not ready with single value for period 10
        Assert.Equal(100, ema.Value); // But value should be set
    }

    [Fact]
    public void EMA_HandlesVolatileData()
    {
        // Arrange
        var parameters = new PEMA<double, double> { Period = 5 };
        var ema = new EMA_QC<double, double>(parameters);
        var inputs = new double[] { 100, 50, 150, 25, 175, 10, 200, 5, 195 };
        var outputs = new double[inputs.Length];

        // Act
        ema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ema.IsReady);
        // EMA should smooth out volatility
        for (int i = parameters.Period; i < outputs.Length; i++)
        {
            Assert.True(outputs[i] > 0);
        }
    }
}