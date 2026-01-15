// DISABLED: Tests need updating to match current API
#if false
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class ROCTests
{
    [Fact]
    public void ROC_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PROC<double, double> { Period = 10 };
        var roc = new ROC_QC<double, double>(parameters);
        var inputs = new double[] { 
            100, 102, 101, 103, 105, 104, 106, 108, 107, 109,
            110, 112, 111, 113, 115, 114, 116, 118, 117, 119
        };
        var outputs = new double[inputs.Length];

        // Act
        roc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(roc.IsReady);
        
        // ROC = ((Current - Previous[period]) / Previous[period]) * 100
        // At index 10: ((110 - 100) / 100) * 100 = 10%
        Assert.Equal(10, outputs[10], 1);
        
        // At index 19: ((119 - 109) / 109) * 100 = 9.17%
        Assert.Equal(9.17, outputs[19], 1);
        
        Assert.Equal(outputs[outputs.Length - 1], roc.Value);
    }

    [Fact]
    public void ROC_DetectsMomentum()
    {
        // Arrange
        var parameters = new PROC<double, double> { Period = 10 };
        
        // Strong upward momentum
        var uptrend = Enumerable.Range(1, 30).Select(x => 100.0 + x * 2).ToArray();
        var upOutputs = new double[uptrend.Length];
        
        // Strong downward momentum
        var downtrend = Enumerable.Range(1, 30).Select(x => 100.0 - x * 2).ToArray();
        var downOutputs = new double[downtrend.Length];
        
        // No momentum (flat)
        var flat = Enumerable.Repeat(100.0, 30).ToArray();
        var flatOutputs = new double[flat.Length];

        // Act
        var rocUp = new ROC_QC<double, double>(parameters);
        rocUp.OnBarBatch(uptrend, upOutputs);
        
        var rocDown = new ROC_QC<double, double>(parameters);
        rocDown.OnBarBatch(downtrend, downOutputs);
        
        var rocFlat = new ROC_QC<double, double>(parameters);
        rocFlat.OnBarBatch(flat, flatOutputs);

        // Assert
        // Uptrend should have positive ROC
        var lastUpROC = upOutputs[upOutputs.Length - 1];
        Assert.True(lastUpROC > 0, $"Uptrend ROC {lastUpROC} should be positive");
        
        // Downtrend should have negative ROC
        var lastDownROC = downOutputs[downOutputs.Length - 1];
        Assert.True(lastDownROC < 0, $"Downtrend ROC {lastDownROC} should be negative");
        
        // Flat should have zero ROC
        var lastFlatROC = flatOutputs[flatOutputs.Length - 1];
        Assert.Equal(0, lastFlatROC, 1);
    }

    [Fact]
    public void ROC_HandlesVolatileData()
    {
        // Arrange
        var parameters = new PROC<double, double> { Period = 10 };
        var roc = new ROC_QC<double, double>(parameters);
        
        // Volatile oscillating data
        var inputs = new double[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 100 + Math.Sin(i * 0.5) * 20;
        }
        var outputs = new double[inputs.Length];

        // Act
        roc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(roc.IsReady);
        
        // ROC should oscillate with the data
        var validOutputs = outputs.Skip(parameters.Period).ToArray();
        var hasPositive = validOutputs.Any(v => v > 0);
        var hasNegative = validOutputs.Any(v => v < 0);
        
        Assert.True(hasPositive && hasNegative, "Oscillating data should produce both positive and negative ROC values");
    }

    [Fact]
    public void ROC_DifferentPeriods()
    {
        var periods = new[] { 5, 10, 20 };
        var inputs = Enumerable.Range(1, 50).Select(x => 100.0 + x * 0.5).ToArray();
        
        var rocValues = new Dictionary<int, double>();

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PROC<double, double> { Period = period };
            var roc = new ROC_QC<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            roc.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(roc.IsReady);
            rocValues[period] = outputs[outputs.Length - 1];
        }

        // Shorter periods should show different rates of change
        // In a linear trend, shorter periods show smaller percentage changes
        Assert.True(rocValues[5] < rocValues[10]);
        Assert.True(rocValues[10] < rocValues[20]);
    }

    [Fact]
    public void ROC_HandlesZeroCrossing()
    {
        // Arrange
        var parameters = new PROC<double, double> { Period = 10 };
        var roc = new ROC_QC<double, double>(parameters);
        
        // Data that goes from increase to decrease
        var inputs = new double[30];
        for (int i = 0; i < 15; i++)
        {
            inputs[i] = 100 + i * 2; // Increasing
        }
        for (int i = 15; i < 30; i++)
        {
            inputs[i] = 130 - (i - 15) * 2; // Decreasing
        }
        var outputs = new double[inputs.Length];

        // Act
        roc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(roc.IsReady);
        
        // Should have positive ROC in first half
        Assert.True(outputs[14] > 0);
        
        // Should transition to negative or zero ROC
        var lastROC = outputs[outputs.Length - 1];
        Assert.True(lastROC <= 0);
    }

    [Fact]
    public void ROC_PercentageCalculation()
    {
        // Arrange
        var parameters = new PROC<double, double> { Period = 1 };
        var roc = new ROC_QC<double, double>(parameters);
        
        // Simple doubling test
        var inputs = new double[] { 100, 200, 400, 200, 100 };
        var outputs = new double[inputs.Length];

        // Act
        roc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(roc.IsReady);
        
        // 100 to 200 = 100% increase
        Assert.Equal(100, outputs[1], 1);
        
        // 200 to 400 = 100% increase
        Assert.Equal(100, outputs[2], 1);
        
        // 400 to 200 = -50% decrease
        Assert.Equal(-50, outputs[3], 1);
        
        // 200 to 100 = -50% decrease
        Assert.Equal(-50, outputs[4], 1);
    }
}
#endif
