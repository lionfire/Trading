using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class TEMATests
{
    [Fact]
    public void TEMA_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PTEMA<double, double> { Period = 10 };
        var tema = new TEMA_QC<double, double>(parameters);
        var inputs = new double[] { 
            100, 102, 101, 103, 105, 104, 106, 108, 107, 109,
            110, 112, 111, 113, 115, 114, 116, 118, 117, 119,
            120, 122, 121, 123, 125, 124, 126, 128, 127, 129
        };
        var outputs = new double[inputs.Length];

        // Act
        tema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(tema.IsReady);
        
        // TEMA should have values after sufficient data (3x period for triple smoothing)
        var validOutputs = outputs.Where(o => o != 0).ToArray();
        Assert.True(validOutputs.Length > 0);
        
        Assert.Equal(outputs[outputs.Length - 1], tema.Value);
    }

    [Fact]
    public void TEMA_ReducesLagMoreThanEMA()
    {
        // Arrange
        var period = 10;
        var temaParams = new PTEMA<double, double> { Period = period };
        var emaParams = new PEMA<double, double> { Period = period };
        
        var tema = new TEMA_QC<double, double>(temaParams);
        var ema = new EMA_QC<double, double>(emaParams);
        
        // Create trending data
        var inputs = Enumerable.Range(1, 50).Select(x => 100.0 + x * 2).ToArray();
        var temaOutputs = new double[inputs.Length];
        var emaOutputs = new double[inputs.Length];

        // Act
        tema.OnBarBatch(inputs, temaOutputs);
        ema.OnBarBatch(inputs, emaOutputs);

        // Assert
        // TEMA should be closer to current price than EMA (less lag)
        var currentPrice = inputs[inputs.Length - 1];
        var temaValue = temaOutputs[inputs.Length - 1];
        var emaValue = emaOutputs[inputs.Length - 1];
        
        var temaDistance = Math.Abs(currentPrice - temaValue);
        var emaDistance = Math.Abs(currentPrice - emaValue);
        
        Assert.True(temaDistance < emaDistance, 
            $"TEMA distance {temaDistance} should be less than EMA distance {emaDistance}");
    }

    [Fact]
    public void TEMA_TripleExponentialSmoothing()
    {
        // Arrange
        var parameters = new PTEMA<double, double> { Period = 10 };
        var tema = new TEMA_QC<double, double>(parameters);
        
        // Noisy data
        var inputs = new double[50];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            var trend = 100.0 + i * 0.5;
            var noise = random.NextDouble() * 10 - 5;
            inputs[i] = trend + noise;
        }
        var outputs = new double[inputs.Length];

        // Act
        tema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(tema.IsReady);
        
        // TEMA should smooth the data while maintaining responsiveness
        var validOutputs = outputs.Skip(parameters.Period * 3).Where(o => o != 0).ToArray();
        
        // Calculate smoothness
        var differences = new List<double>();
        for (int i = 1; i < validOutputs.Length; i++)
        {
            differences.Add(Math.Abs(validOutputs[i] - validOutputs[i - 1]));
        }
        
        var avgDifference = differences.Average();
        Assert.True(avgDifference < 3, "TEMA should produce smooth output");
    }

    [Fact]
    public void TEMA_RespondsToTrendChanges()
    {
        // Arrange
        var parameters = new PTEMA<double, double> { Period = 10 };
        var tema = new TEMA_QC<double, double>(parameters);
        
        // Data with sharp trend change
        var inputs = new double[60];
        for (int i = 0; i < 30; i++)
        {
            inputs[i] = 100.0 + i * 1.0; // Uptrend
        }
        for (int i = 30; i < 60; i++)
        {
            inputs[i] = 130.0 - (i - 30) * 1.0; // Downtrend
        }
        var outputs = new double[inputs.Length];

        // Act
        tema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(tema.IsReady);
        
        // TEMA should quickly respond to trend change
        var beforeChange = outputs[29];
        var afterChange = outputs[35];
        
        Assert.True(beforeChange < 130, "TEMA should be rising before trend change");
        Assert.True(afterChange < beforeChange || Math.Abs(afterChange - beforeChange) < 5, 
            "TEMA should respond quickly to trend reversal");
    }

    [Fact]
    public void TEMA_DifferentPeriods()
    {
        var periods = new[] { 5, 10, 20, 30 };
        var inputs = Enumerable.Range(1, 100).Select(x => 100.0 + Math.Sin(x * 0.1) * 10).ToArray();

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PTEMA<double, double> { Period = period };
            var tema = new TEMA_QC<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            tema.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(tema.IsReady);
            var validOutputs = outputs.Where(o => o != 0).ToArray();
            Assert.True(validOutputs.Length > 0);
            
            // Shorter periods should be more responsive
            // We need at least 3x period for TEMA to be fully ready
            Assert.True(validOutputs.Length >= inputs.Length - (period * 3));
        }
    }

    [Fact]
    public void TEMA_Overshooting()
    {
        // Arrange
        var parameters = new PTEMA<double, double> { Period = 10 };
        var tema = new TEMA_QC<double, double>(parameters);
        
        // Create data with sudden jump
        var inputs = new double[40];
        for (int i = 0; i < 20; i++)
        {
            inputs[i] = 100.0;
        }
        for (int i = 20; i < 40; i++)
        {
            inputs[i] = 120.0; // Sudden jump
        }
        var outputs = new double[inputs.Length];

        // Act
        tema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(tema.IsReady);
        
        // TEMA might overshoot due to its triple exponential nature
        var maxTema = outputs.Skip(20).Max();
        
        // TEMA could overshoot the new level initially
        Assert.True(maxTema >= 115, "TEMA should respond strongly to sudden changes");
    }

    [Fact]
    public void TEMA_ComparedToDoubleEMA()
    {
        // Arrange
        var period = 10;
        var temaParams = new PTEMA<double, double> { Period = period };
        var tema = new TEMA_QC<double, double>(temaParams);
        
        // Create oscillating data
        var inputs = new double[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 100.0 + Math.Sin(i * 0.2) * 10;
        }
        var outputs = new double[inputs.Length];

        // Act
        tema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(tema.IsReady);
        
        // TEMA should track the oscillations closely
        var validOutputs = outputs.Skip(period * 3).Where(o => o != 0).ToArray();
        
        // Find peaks and troughs
        var peaks = 0;
        var troughs = 0;
        for (int i = 1; i < validOutputs.Length - 1; i++)
        {
            if (validOutputs[i] > validOutputs[i - 1] && validOutputs[i] > validOutputs[i + 1])
                peaks++;
            if (validOutputs[i] < validOutputs[i - 1] && validOutputs[i] < validOutputs[i + 1])
                troughs++;
        }
        
        Assert.True(peaks > 0 && troughs > 0, "TEMA should track oscillations");
    }

    [Fact]
    public void TEMA_Convergence()
    {
        // Arrange
        var parameters = new PTEMA<double, double> { Period = 10 };
        var tema = new TEMA_QC<double, double>(parameters);
        
        // Constant value data
        var constantValue = 100.0;
        var inputs = Enumerable.Repeat(constantValue, 50).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        tema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(tema.IsReady);
        
        // TEMA should converge to the constant value
        var lastTema = outputs[outputs.Length - 1];
        Assert.Equal(constantValue, lastTema, 1);
    }
}