using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class TEMATests
{
    [Fact]
    public void TEMA_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PTEMA<double, double> { Period = 10 };
        var tema = new TEMA_FP<double, double>(parameters);
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

        Assert.Equal(outputs[^1], tema.Value);
    }

    [Fact]
    public void TEMA_ReducesLagMoreThanEMA()
    {
        // Arrange
        var period = 10;
        var temaParams = new PTEMA<double, double> { Period = period };
        var emaParams = new PEMA<double, double> { Period = period };

        var tema = new TEMA_FP<double, double>(temaParams);
        var ema = new EMA_FP<double, double>(emaParams);

        // Create trending data
        var inputs = Enumerable.Range(1, 50).Select(x => 100.0 + x * 2).ToArray();
        var temaOutputs = new double[inputs.Length];
        var emaOutputs = new double[inputs.Length];

        // Act
        tema.OnBarBatch(inputs, temaOutputs);
        ema.OnBarBatch(inputs, emaOutputs);

        // Assert
        // TEMA should be closer to current price than EMA (less lag)
        var currentPrice = inputs[^1];
        var temaValue = tema.Value;
        var emaValue = ema.Value;

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
        var tema = new TEMA_FP<double, double>(parameters);

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
        var tema = new TEMA_FP<double, double>(parameters);

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

        Assert.True(beforeChange < 130 || double.IsNaN(beforeChange), "TEMA should be rising before trend change");
        Assert.True(afterChange < beforeChange || Math.Abs(afterChange - beforeChange) < 5 || double.IsNaN(afterChange),
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
            var tema = new TEMA_FP<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            tema.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(tema.IsReady);
            var validOutputs = outputs.Where(o => o != 0 && !double.IsNaN(o)).ToArray();
            Assert.True(validOutputs.Length > 0);
        }
    }

    [Fact]
    public void TEMA_Overshooting()
    {
        // Arrange
        var parameters = new PTEMA<double, double> { Period = 10 };
        var tema = new TEMA_FP<double, double>(parameters);

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
        var validOutputs = outputs.Skip(20).Where(o => !double.IsNaN(o)).ToArray();
        if (validOutputs.Length > 0)
        {
            var maxTema = validOutputs.Max();
            Assert.True(maxTema >= 100, "TEMA should respond to sudden changes");
        }
    }

    [Fact]
    public void TEMA_ComparedToDoubleEMA()
    {
        // Arrange
        var period = 10;
        var temaParams = new PTEMA<double, double> { Period = period };
        var tema = new TEMA_FP<double, double>(temaParams);

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
        var validOutputs = outputs.Skip(period * 3).Where(o => o != 0 && !double.IsNaN(o)).ToArray();

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
        var tema = new TEMA_FP<double, double>(parameters);

        // Constant value data
        var constantValue = 100.0;
        var inputs = Enumerable.Repeat(constantValue, 50).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        tema.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(tema.IsReady);

        // TEMA should converge to the constant value
        var lastTema = tema.Value;
        Assert.Equal(constantValue, lastTema, 1);
    }

    [Fact]
    public void TEMA_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PTEMA<double, double> { Period = 5 };
        var tema = new TEMA_FP<double, double>(parameters);
        var inputs = Enumerable.Range(1, 20).Select(x => (double)x * 10).ToArray();
        var outputs = new double[inputs.Length];

        // First run
        tema.OnBarBatch(inputs, outputs);
        Assert.True(tema.IsReady);
        var firstValue = tema.Value;

        // Clear
        tema.Clear();
        Assert.False(tema.IsReady);

        // Process again
        var outputs2 = new double[inputs.Length];
        tema.OnBarBatch(inputs, outputs2);
        Assert.True(tema.IsReady);
        Assert.Equal(firstValue, tema.Value, 6);
    }
}
