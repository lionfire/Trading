using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class FisherTransformTests
{
    [Fact]
    public void FisherTransform_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PFisherTransform<HL, FisherTransformResult> { Period = 10 };
        var fisher = new FisherTransform_QC<HL, FisherTransformResult>(parameters);
        
        // Sample data
        var inputs = new HL[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 10;
            inputs[i] = new HL
            {
                High = price + 1,
                Low = price - 1
            };
        }
        
        var outputs = new FisherTransformResult[inputs.Length];

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // Fisher and Trigger should have values
        Assert.NotEqual(0, lastResult.Fisher);
        Assert.NotEqual(0, lastResult.Trigger);
    }

    [Fact]
    public void FisherTransform_IdentifiesExtremes()
    {
        // Arrange
        var parameters = new PFisherTransform<HL, FisherTransformResult> { Period = 10 };
        var fisher = new FisherTransform_QC<HL, FisherTransformResult>(parameters);
        
        // Create data with extremes
        var inputs = new HL[50];
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
            
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }
        
        var outputs = new FisherTransformResult[inputs.Length];

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);
        
        // Fisher Transform amplifies extremes
        var maxFisher = outputs.Where(o => o != null).Max(o => o.Fisher);
        var minFisher = outputs.Where(o => o != null).Min(o => o.Fisher);
        
        Assert.True(maxFisher > 1.5, $"Max Fisher {maxFisher} should be > 1.5 at extremes");
        Assert.True(minFisher < -1.5, $"Min Fisher {minFisher} should be < -1.5 at extremes");
    }

    [Fact]
    public void FisherTransform_CrossoverSignals()
    {
        // Arrange
        var parameters = new PFisherTransform<HL, FisherTransformResult> { Period = 10 };
        var fisher = new FisherTransform_QC<HL, FisherTransformResult>(parameters);
        
        // Create oscillating data for crossovers
        var inputs = new HL[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.2) * 15;
            inputs[i] = new HL
            {
                High = price + 1,
                Low = price - 1
            };
        }
        
        var outputs = new FisherTransformResult[inputs.Length];

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);
        
        // Count Fisher/Trigger crossovers
        var crossovers = 0;
        for (int i = parameters.Period + 1; i < outputs.Length; i++)
        {
            if (outputs[i - 1] != null && outputs[i] != null)
            {
                var prevDiff = outputs[i - 1].Fisher - outputs[i - 1].Trigger;
                var currDiff = outputs[i].Fisher - outputs[i].Trigger;
                
                if (prevDiff * currDiff < 0) // Sign change indicates crossover
                {
                    crossovers++;
                }
            }
        }
        
        Assert.True(crossovers > 0, "Should detect Fisher/Trigger crossovers");
    }

    [Fact]
    public void FisherTransform_NormalizedValues()
    {
        // Arrange
        var parameters = new PFisherTransform<HL, FisherTransformResult> { Period = 10 };
        var fisher = new FisherTransform_QC<HL, FisherTransformResult>(parameters);
        
        // Create normalized data
        var inputs = new HL[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + (i % 10) - 5; // Oscillating within range
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }
        
        var outputs = new FisherTransformResult[inputs.Length];

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);
        
        // Fisher values should mostly be within typical range
        var validOutputs = outputs.Where(o => o != null).ToArray();
        var withinRange = validOutputs.Count(o => o.Fisher >= -3 && o.Fisher <= 3);
        var percentage = (double)withinRange / validOutputs.Length;
        
        Assert.True(percentage > 0.8, "Most Fisher values should be within -3 to 3 range");
    }

    [Fact]
    public void FisherTransform_LeadingIndicator()
    {
        // Arrange
        var parameters = new PFisherTransform<HL, FisherTransformResult> { Period = 10 };
        var fisher = new FisherTransform_QC<HL, FisherTransformResult>(parameters);
        
        // Create data with clear turning points
        var inputs = new HL[80];
        for (int i = 0; i < 40; i++)
        {
            var price = 100.0 + i * 0.5; // Uptrend
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.3
            };
        }
        for (int i = 40; i < 80; i++)
        {
            var price = 120.0 - (i - 40) * 0.5; // Downtrend
            inputs[i] = new HL
            {
                High = price + 0.3,
                Low = price - 0.5
            };
        }
        
        var outputs = new FisherTransformResult[inputs.Length];

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);
        
        // Fisher should peak before price reversal
        var fisherPeakIndex = -1;
        double maxFisher = double.MinValue;
        
        for (int i = 30; i < 50; i++)
        {
            if (outputs[i] != null && outputs[i].Fisher > maxFisher)
            {
                maxFisher = outputs[i].Fisher;
                fisherPeakIndex = i;
            }
        }
        
        // Fisher peak should occur near the price peak (around index 40)
        Assert.InRange(fisherPeakIndex, 35, 45);
    }

    [Fact]
    public void FisherTransform_DifferentPeriods()
    {
        var periods = new[] { 5, 10, 20 };
        
        // Create sample data
        var inputs = new HL[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.15) * 10;
            inputs[i] = new HL
            {
                High = price + 1,
                Low = price - 1
            };
        }

        var fisherRanges = new Dictionary<int, double>();

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PFisherTransform<HL, FisherTransformResult> { Period = period };
            var fisher = new FisherTransform_QC<HL, FisherTransformResult>(parameters);
            var outputs = new FisherTransformResult[inputs.Length];

            // Act
            fisher.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(fisher.IsReady);
            
            var validOutputs = outputs.Where(o => o != null).ToArray();
            var range = validOutputs.Max(o => o.Fisher) - validOutputs.Min(o => o.Fisher);
            fisherRanges[period] = range;
        }

        // Shorter periods should be more sensitive (larger range)
        Assert.True(fisherRanges[5] > fisherRanges[20]);
    }

    [Fact]
    public void FisherTransform_TriggerLag()
    {
        // Arrange
        var parameters = new PFisherTransform<HL, FisherTransformResult> { Period = 10 };
        var fisher = new FisherTransform_QC<HL, FisherTransformResult>(parameters);
        
        // Create trending data
        var inputs = new HL[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 1.0;
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }
        
        var outputs = new FisherTransformResult[inputs.Length];

        // Act
        fisher.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(fisher.IsReady);
        
        // Trigger should lag behind Fisher (it's the previous Fisher value)
        for (int i = parameters.Period + 1; i < outputs.Length; i++)
        {
            if (outputs[i - 1] != null && outputs[i] != null)
            {
                // Trigger at current bar should equal Fisher from previous bar
                Assert.Equal(outputs[i - 1].Fisher, outputs[i].Trigger, 5);
            }
        }
    }
}

public class FisherTransformResult
{
    public double Fisher { get; set; }
    public double Trigger { get; set; }
}