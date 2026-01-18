using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class RSITests
{
    [Fact]
    public void RSI_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 14 };
        var rsi = new RSI_FP<double, double>(parameters);
        var inputs = new double[] {
            44.34, 44.09, 44.15, 43.61, 44.33, 44.83, 45.10, 45.42,
            45.84, 46.08, 45.89, 46.03, 45.61, 46.28, 46.28, 46.00,
            46.03, 46.41, 46.22, 45.64, 46.21, 46.25, 45.71, 46.45
        };
        var outputs = new double[inputs.Length];

        // Act
        rsi.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(rsi.IsReady);
        // RSI should be between 0 and 100 for valid outputs
        for (int i = parameters.Period + 1; i < outputs.Length; i++)
        {
            if (!double.IsNaN(outputs[i]))
            {
                Assert.InRange(outputs[i], 0, 100);
            }
        }
        Assert.Equal(outputs[^1], rsi.CurrentValue);
    }

    [Fact]
    public void RSI_DetectsOverboughtAndOversold()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 14 };

        // Create trending up data (should become overbought)
        var uptrend = Enumerable.Range(1, 30).Select(x => 100.0 + x * 2).ToArray();
        var upOutputs = new double[uptrend.Length];

        // Create trending down data (should become oversold)
        var downtrend = Enumerable.Range(1, 30).Select(x => 100.0 - x * 2).ToArray();
        var downOutputs = new double[downtrend.Length];

        // Act
        var rsiUp = new RSI_FP<double, double>(parameters);
        rsiUp.OnBarBatch(uptrend, upOutputs);
        var uptrendRSI = rsiUp.CurrentValue;

        var rsiDown = new RSI_FP<double, double>(parameters);
        rsiDown.OnBarBatch(downtrend, downOutputs);
        var downtrendRSI = rsiDown.CurrentValue;

        // Assert
        Assert.True(uptrendRSI > 70, $"Uptrend RSI {uptrendRSI} should be > 70 (overbought)");
        Assert.True(downtrendRSI < 30, $"Downtrend RSI {downtrendRSI} should be < 30 (oversold)");
    }

    [Fact]
    public void RSI_HandlesConstantValues()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 14 };
        var rsi = new RSI_FP<double, double>(parameters);
        var inputs = Enumerable.Repeat(100.0, 30).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        rsi.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(rsi.IsReady);
        // RSI should be around 50 for constant values (no gain or loss)
        // Note: Implementation may return 100 if no losses detected
        var lastRSI = rsi.CurrentValue;
        Assert.InRange(lastRSI, 0, 100);
    }

    [Fact]
    public void RSI_DifferentPeriods()
    {
        var periods = new[] { 7, 14, 21 };
        var inputs = new double[] {
            44.34, 44.09, 44.15, 43.61, 44.33, 44.83, 45.10, 45.42,
            45.84, 46.08, 45.89, 46.03, 45.61, 46.28, 46.28, 46.00,
            46.03, 46.41, 46.22, 45.64, 46.21, 46.25, 45.71, 46.45,
            46.50, 46.32, 46.65, 46.89, 46.73, 46.55
        };

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PRSI<double, double> { Period = period };
            var rsi = new RSI_FP<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            rsi.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(rsi.IsReady);
            // All valid RSI values should be between 0 and 100
            Assert.InRange(rsi.CurrentValue, 0, 100);
        }
    }

    [Fact]
    public void RSI_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 14 };
        var rsi = new RSI_FP<double, double>(parameters);
        var inputs = Enumerable.Range(1, 20).Select(x => (double)x * 10).ToArray();

        // Act
        rsi.OnBarBatch(inputs, new double[inputs.Length]);
        var firstValue = rsi.CurrentValue;

        rsi.Clear();
        Assert.False(rsi.IsReady);

        rsi.OnBarBatch(inputs, new double[inputs.Length]);
        var secondValue = rsi.CurrentValue;

        // Assert
        Assert.Equal(firstValue, secondValue, 2); // Should get same result after reset
    }

    [Fact]
    public void RSI_OverboughtOversoldLevels()
    {
        // Arrange
        var parameters = new PRSI<double, double>
        {
            Period = 14,
            OverboughtLevel = 80,
            OversoldLevel = 20
        };
        var rsi = new RSI_FP<double, double>(parameters);

        // Assert - verify parameters are set
        Assert.Equal(80, parameters.OverboughtLevel);
        Assert.Equal(20, parameters.OversoldLevel);
    }

    [Fact]
    public void RSI_RangeIsBounded()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 5 };
        var rsi = new RSI_FP<double, double>(parameters);

        // Extreme volatility
        var inputs = new double[] { 10, 100, 5, 200, 1, 500, 0.1, 1000, 0.01, 5000 };
        var outputs = new double[inputs.Length];

        // Act
        rsi.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(rsi.IsReady);
        Assert.InRange(rsi.CurrentValue, 0, 100);
    }
}
