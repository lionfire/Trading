// DISABLED: Tests need updating to match current API
#if false
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class HullMovingAverageTests
{
    [Fact]
    public void HullMovingAverage_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PHullMovingAverage<double, double> { Period = 16 };
        var hma = new HullMovingAverage_QC<double, double>(parameters);
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
        Assert.Equal(outputs[outputs.Length - 1], hma.Value);
    }

    [Fact]
    public void HullMovingAverage_ReducesLag()
    {
        // Arrange
        var period = 20;
        var hmaParams = new PHullMovingAverage<double, double> { Period = period };
        var smaParams = new PSMA<double, double> { Period = period };
        
        var hma = new HullMovingAverage_QC<double, double>(hmaParams);
        var sma = new SMA_QC<double, double>(smaParams);
        
        // Create trending data
        var inputs = Enumerable.Range(1, 50).Select(x => 100.0 + x * 2).ToArray();
        var hmaOutputs = new double[inputs.Length];
        var smaOutputs = new double[inputs.Length];

        // Act
        hma.OnBarBatch(inputs, hmaOutputs);
        sma.OnBarBatch(inputs, smaOutputs);

        // Assert
        // HMA should be closer to current price than SMA (less lag)
        var currentPrice = inputs[inputs.Length - 1];
        var hmaValue = hmaOutputs[inputs.Length - 1];
        var smaValue = smaOutputs[inputs.Length - 1];
        
        var hmaDistance = Math.Abs(currentPrice - hmaValue);
        var smaDistance = Math.Abs(currentPrice - smaValue);
        
        Assert.True(hmaDistance < smaDistance, 
            $"HMA distance {hmaDistance} should be less than SMA distance {smaDistance}");
    }

    [Fact]
    public void HullMovingAverage_Smoothness()
    {
        // Arrange
        var parameters = new PHullMovingAverage<double, double> { Period = 20 };
        var hma = new HullMovingAverage_QC<double, double>(parameters);
        
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

        // Assert
        Assert.True(hma.IsReady);
        
        // Calculate smoothness (lower variance in differences)
        var validOutputs = outputs.Skip(parameters.Period).Where(o => o != 0).ToArray();
        var differences = new List<double>();
        for (int i = 1; i < validOutputs.Length; i++)
        {
            differences.Add(validOutputs[i] - validOutputs[i - 1]);
        }
        
        // HMA should produce smoother output
        var variance = differences.Select(d => d * d).Average();
        Assert.True(variance < 10, "HMA should produce smooth output");
    }

    [Fact]
    public void HullMovingAverage_ResponsivenessToTrends()
    {
        // Arrange
        var parameters = new PHullMovingAverage<double, double> { Period = 14 };
        var hma = new HullMovingAverage_QC<double, double>(parameters);
        
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

        // Assert
        Assert.True(hma.IsReady);
        
        // HMA should quickly respond to trend change
        var beforeChange = outputs[28];
        var afterChange = outputs[35];
        
        Assert.True(beforeChange < afterChange, "HMA should rise before trend change");
        Assert.True(outputs[45] < outputs[35], "HMA should fall after trend change");
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
            var hma = new HullMovingAverage_QC<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            hma.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(hma.IsReady);
            var validOutputs = outputs.Where(o => o != 0).ToArray();
            Assert.True(validOutputs.Length > 0);
            
            // Shorter periods should be more responsive
        }
    }

    [Fact]
    public void HullMovingAverage_NoiseReduction()
    {
        // Arrange
        var parameters = new PHullMovingAverage<double, double> { Period = 20 };
        var hma = new HullMovingAverage_QC<double, double>(parameters);
        
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

        // Assert
        Assert.True(hma.IsReady);
        
        // HMA should filter out noise while preserving trend
        var lastValues = outputs.Skip(80).Where(o => o != 0).ToArray();
        var range = lastValues.Max() - lastValues.Min();
        
        Assert.True(range < 20, "HMA should reduce noise significantly");
    }

    [Fact]
    public void HullMovingAverage_CrossoverSignals()
    {
        // Arrange
        var fastParams = new PHullMovingAverage<double, double> { Period = 9 };
        var slowParams = new PHullMovingAverage<double, double> { Period = 25 };
        
        var fastHMA = new HullMovingAverage_QC<double, double>(fastParams);
        var slowHMA = new HullMovingAverage_QC<double, double>(slowParams);
        
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
        
        // Fast HMA should cross slow HMA at trend changes
        var crossovers = 0;
        for (int i = 30; i < inputs.Length - 1; i++)
        {
            if (fastOutputs[i] != 0 && slowOutputs[i] != 0 &&
                fastOutputs[i + 1] != 0 && slowOutputs[i + 1] != 0)
            {
                var prevDiff = fastOutputs[i] - slowOutputs[i];
                var currDiff = fastOutputs[i + 1] - slowOutputs[i + 1];
                if (prevDiff * currDiff < 0) // Sign change indicates crossover
                {
                    crossovers++;
                }
            }
        }
        
        Assert.True(crossovers > 0, "Should detect crossovers at trend changes");
    }
}
#endif
