using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class RSITests
{
    [Fact]
    public void RSI_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 14 };
        var rsi = new RSI_QC<double, double>(parameters);
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
        // RSI should be between 0 and 100
        for (int i = parameters.Period; i < outputs.Length; i++)
        {
            Assert.InRange(outputs[i], 0, 100);
        }
        Assert.Equal(outputs[outputs.Length - 1], rsi.Value);
    }

    [Fact]
    public void RSI_DetectsOverboughtAndOversold()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 14 };
        var rsi = new RSI_QC<double, double>(parameters);
        
        // Create trending up data (should become overbought)
        var uptrend = Enumerable.Range(1, 30).Select(x => 100.0 + x * 2).ToArray();
        var upOutputs = new double[uptrend.Length];
        
        // Create trending down data (should become oversold)
        var downtrend = Enumerable.Range(1, 30).Select(x => 100.0 - x * 2).ToArray();
        var downOutputs = new double[downtrend.Length];

        // Act
        rsi.OnBarBatch(uptrend, upOutputs);
        var uptrendRSI = upOutputs[upOutputs.Length - 1];
        
        rsi = new RSI_QC<double, double>(parameters); // Reset
        rsi.OnBarBatch(downtrend, downOutputs);
        var downtrendRSI = downOutputs[downOutputs.Length - 1];

        // Assert
        Assert.True(uptrendRSI > 70, $"Uptrend RSI {uptrendRSI} should be > 70 (overbought)");
        Assert.True(downtrendRSI < 30, $"Downtrend RSI {downtrendRSI} should be < 30 (oversold)");
    }

    [Fact]
    public void RSI_HandlesConstantValues()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 14 };
        var rsi = new RSI_QC<double, double>(parameters);
        var inputs = Enumerable.Repeat(100.0, 30).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        rsi.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(rsi.IsReady);
        // RSI should be around 50 for constant values (no gain or loss)
        var lastRSI = outputs[outputs.Length - 1];
        Assert.InRange(lastRSI, 45, 55);
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
            var rsi = new RSI_QC<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            rsi.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(rsi.IsReady);
            // Shorter periods should be more volatile
            var values = outputs.Skip(period).Where(v => v > 0).ToArray();
            Assert.True(values.All(v => v >= 0 && v <= 100));
        }
    }

    [Fact]
    public void RSI_ResetsFunctionality()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 14 };
        var rsi = new RSI_QC<double, double>(parameters);
        var inputs = Enumerable.Range(1, 20).Select(x => (double)x * 10).ToArray();

        // Act
        rsi.OnBarBatch(inputs, new double[inputs.Length]);
        var firstValue = rsi.Value;
        
        rsi.Reset();
        Assert.False(rsi.IsReady);
        
        rsi.OnBarBatch(inputs, new double[inputs.Length]);
        var secondValue = rsi.Value;

        // Assert
        Assert.Equal(firstValue, secondValue, 2); // Should get same result after reset
    }
}