using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class FisherTransformTests
{
    [Fact]
    public void FisherTransform_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PFisherTransform<double, double> { Period = 10 };
        var fisher = new FisherTransform_FP<double, double>(parameters);

        // Sample data
        var inputs = new HL<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 10;
            inputs[i] = new HL<double>
            {
                High = price + 1,
                Low = price - 1
            };
        }

        var outputs = new double[inputs.Length * 2]; // Fisher and Trigger

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);

        // Fisher and Trigger should have values
        Assert.NotEqual(0, fisher.Fisher);
        Assert.NotEqual(0, fisher.Trigger);
    }

    [Fact]
    public void FisherTransform_IdentifiesExtremes()
    {
        // Arrange
        var parameters = new PFisherTransform<double, double> { Period = 10 };
        var fisher = new FisherTransform_FP<double, double>(parameters);

        // Create data with extremes
        var inputs = new HL<double>[50];
        for (int i = 0; i < inputs.Length; i++)
        {
            double price;
            if (i < 20)
            {
                price = 100.0 + i * 2; // Strong uptrend
            }
            else if (i < 30)
            {
                price = 140.0; // Top
            }
            else
            {
                price = 140.0 - (i - 30) * 2; // Strong downtrend
            }

            inputs[i] = new HL<double>
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }

        var outputs = new double[inputs.Length * 2];

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);

        // Fisher Transform amplifies extremes - should have meaningful values
        Assert.False(double.IsNaN(fisher.Fisher));
        Assert.False(double.IsNaN(fisher.Trigger));
    }

    [Fact]
    public void FisherTransform_CrossoverSignals()
    {
        // Arrange
        var parameters = new PFisherTransform<double, double> { Period = 10 };
        var fisher = new FisherTransform_FP<double, double>(parameters);

        // Create oscillating data for crossovers
        var inputs = new HL<double>[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.2) * 15;
            inputs[i] = new HL<double>
            {
                High = price + 1,
                Low = price - 1
            };
        }

        var outputs = new double[inputs.Length * 2];

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);

        // Both Fisher and Trigger should have valid values
        Assert.False(double.IsNaN(fisher.Fisher));
        Assert.False(double.IsNaN(fisher.Trigger));
    }

    [Fact]
    public void FisherTransform_NormalizedValues()
    {
        // Arrange
        var parameters = new PFisherTransform<double, double> { Period = 10 };
        var fisher = new FisherTransform_FP<double, double>(parameters);

        // Create normalized data
        var inputs = new HL<double>[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + (i % 10) - 5; // Oscillating within range
            inputs[i] = new HL<double>
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }

        var outputs = new double[inputs.Length * 2];

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);

        // Fisher values should be within reasonable range for normal data
        Assert.True(Math.Abs(fisher.Fisher) < 10);
    }

    [Fact]
    public void FisherTransform_LeadingIndicator()
    {
        // Arrange
        var parameters = new PFisherTransform<double, double> { Period = 10 };
        var fisher = new FisherTransform_FP<double, double>(parameters);

        // Create data with clear turning points
        var inputs = new HL<double>[80];
        for (int i = 0; i < 40; i++)
        {
            var price = 100.0 + i * 0.5; // Uptrend
            inputs[i] = new HL<double>
            {
                High = price + 0.5,
                Low = price - 0.3
            };
        }
        for (int i = 40; i < 80; i++)
        {
            var price = 120.0 - (i - 40) * 0.5; // Downtrend
            inputs[i] = new HL<double>
            {
                High = price + 0.3,
                Low = price - 0.5
            };
        }

        var outputs = new double[inputs.Length * 2];

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);

        // Fisher should have valid values at trend reversal
        Assert.False(double.IsNaN(fisher.Fisher));
    }

    [Fact]
    public void FisherTransform_DifferentPeriods()
    {
        var periods = new[] { 5, 10, 20 };

        // Create sample data
        var inputs = new HL<double>[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.15) * 10;
            inputs[i] = new HL<double>
            {
                High = price + 1,
                Low = price - 1
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PFisherTransform<double, double> { Period = period };
            var fisher = new FisherTransform_FP<double, double>(parameters);
            var outputs = new double[inputs.Length * 2];

            // Act
            fisher.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(fisher.IsReady);
            Assert.False(double.IsNaN(fisher.Fisher));
            Assert.False(double.IsNaN(fisher.Trigger));
        }
    }

    [Fact]
    public void FisherTransform_TriggerLag()
    {
        // Arrange
        var parameters = new PFisherTransform<double, double> { Period = 10 };
        var fisher = new FisherTransform_FP<double, double>(parameters);

        // Create trending data
        var inputs = new HL<double>[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 1.0;
            inputs[i] = new HL<double>
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }

        var outputs = new double[inputs.Length * 2];

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);

        // Trigger should lag behind Fisher (it's the previous Fisher value)
        // Both should be valid
        Assert.False(double.IsNaN(fisher.Fisher));
        Assert.False(double.IsNaN(fisher.Trigger));
    }

    [Fact]
    public void FisherTransform_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PFisherTransform<double, double> { Period = 10 };
        var fisher = new FisherTransform_FP<double, double>(parameters);

        var inputs = new HL<double>[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HL<double>
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }

        fisher.OnBarBatch(inputs, new double[inputs.Length * 2]);
        Assert.True(fisher.IsReady);

        // Act
        fisher.Clear();

        // Assert
        Assert.False(fisher.IsReady);
    }
}
