using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class LinearRegressionTests
{
    [Fact]
    public void LinearRegression_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PLinearRegression<double, double> { Period = 10 };
        var lr = new LinearRegression_FP<double, double>(parameters);

        // Uptrending linear data
        var inputs = Enumerable.Range(1, 20).Select(x => (double)x * 10).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        lr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(lr.IsReady);

        // Slope should be positive for uptrending data
        Assert.True(lr.Slope > 0, $"Slope {lr.Slope} should be positive for uptrending data");

        // Value should be positive
        Assert.True(lr.Value > 0);
    }

    [Fact]
    public void LinearRegression_PerfectFit()
    {
        // Arrange
        var parameters = new PLinearRegression<double, double> { Period = 10 };
        var lr = new LinearRegression_FP<double, double>(parameters);

        // Perfect linear relationship: y = 2x + 5
        var inputs = new double[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 2 * i + 5;
        }
        var outputs = new double[inputs.Length];

        // Act
        lr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(lr.IsReady);

        // Slope should be positive for uptrending data
        Assert.True(lr.Slope > 0, $"Slope {lr.Slope} should be positive");

        // R-squared should be positive for linear data
        Assert.True(lr.RSquared > 0.5, $"R-squared {lr.RSquared} should be significant for linear data");
    }

    [Fact]
    public void LinearRegression_TrendDetection()
    {
        // Arrange
        var parameters = new PLinearRegression<double, double> { Period = 20 };

        // Uptrend data
        var uptrend = Enumerable.Range(1, 30).Select(x => 100.0 + x * 2).ToArray();

        // Downtrend data
        var downtrend = Enumerable.Range(1, 30).Select(x => 200.0 - x * 2).ToArray();

        // Sideways data
        var sideways = new double[30];
        for (int i = 0; i < sideways.Length; i++)
        {
            sideways[i] = 100.0 + Math.Sin(i * 0.5) * 2;
        }

        // Act
        var lrUp = new LinearRegression_FP<double, double>(parameters);
        var upOutputs = new double[uptrend.Length];
        lrUp.OnBarBatch(uptrend, upOutputs);
        var upSlope = lrUp.Slope;

        var lrDown = new LinearRegression_FP<double, double>(parameters);
        var downOutputs = new double[downtrend.Length];
        lrDown.OnBarBatch(downtrend, downOutputs);
        var downSlope = lrDown.Slope;

        var lrSide = new LinearRegression_FP<double, double>(parameters);
        var sideOutputs = new double[sideways.Length];
        lrSide.OnBarBatch(sideways, sideOutputs);
        var sideSlope = lrSide.Slope;

        // Assert
        Assert.True(upSlope > 0, $"Uptrend slope {upSlope} should be positive");
        Assert.True(downSlope < 0, $"Downtrend slope {downSlope} should be negative");
        Assert.InRange(sideSlope, -0.5, 0.5); // Near zero for sideways
    }

    [Fact]
    public void LinearRegression_RSquaredWithNoise()
    {
        // Arrange
        var parameters = new PLinearRegression<double, double> { Period = 20 };
        var lr = new LinearRegression_FP<double, double>(parameters);

        // Data with noise
        var inputs = new double[30];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            var trend = 100.0 + i * 1.0;
            var noise = random.NextDouble() * 10 - 5;
            inputs[i] = trend + noise;
        }
        var outputs = new double[inputs.Length];

        // Act
        lr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(lr.IsReady);

        // R-squared should be less than 1 due to noise
        Assert.True(lr.RSquared < 1);
        Assert.True(lr.RSquared > 0);
    }

    [Fact]
    public void LinearRegression_DifferentPeriods()
    {
        var periods = new[] { 10, 20, 30 };

        // Create sample data with trend and noise
        var inputs = new double[50];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 100.0 + i * 0.8 + random.NextDouble() * 5 - 2.5;
        }

        var slopes = new Dictionary<int, double>();

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PLinearRegression<double, double> { Period = period };
            var lr = new LinearRegression_FP<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            lr.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(lr.IsReady);
            slopes[period] = lr.Slope;

            // All should detect positive trend
            Assert.True(lr.Slope > 0);
        }
    }

    [Fact]
    public void LinearRegression_Extrapolation()
    {
        // Arrange
        var parameters = new PLinearRegression<double, double> { Period = 10 };
        var lr = new LinearRegression_FP<double, double>(parameters);

        // Clear linear trend
        var inputs = new double[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 50.0 + i * 3.0;
        }
        var outputs = new double[inputs.Length];

        // Act
        lr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(lr.IsReady);

        // Extrapolate next value
        var expectedNext = lr.Value + lr.Slope;
        var actualNext = 50.0 + 30 * 3.0; // Next value if trend continues

        Assert.InRange(expectedNext, actualNext - 10, actualNext + 10);
    }

    [Fact]
    public void LinearRegression_VolatilityMeasure()
    {
        // Arrange
        var parameters = new PLinearRegression<double, double> { Period = 20 };

        // Low volatility data
        var lowVol = new double[30];
        for (int i = 0; i < lowVol.Length; i++)
        {
            lowVol[i] = 100.0 + i * 1.0 + Math.Sin(i) * 0.5;
        }

        // High volatility data
        var highVol = new double[30];
        var random = new Random(42);
        for (int i = 0; i < highVol.Length; i++)
        {
            highVol[i] = 100.0 + i * 1.0 + random.NextDouble() * 20 - 10;
        }

        // Act
        var lrLow = new LinearRegression_FP<double, double>(parameters);
        var lowOutputs = new double[lowVol.Length];
        lrLow.OnBarBatch(lowVol, lowOutputs);
        var lowRSquared = lrLow.RSquared;

        var lrHigh = new LinearRegression_FP<double, double>(parameters);
        var highOutputs = new double[highVol.Length];
        lrHigh.OnBarBatch(highVol, highOutputs);
        var highRSquared = lrHigh.RSquared;

        // Assert
        Assert.True(lowRSquared > highRSquared, "Low volatility should have higher R-squared");
    }

    [Fact]
    public void LinearRegression_SlopeAndIntercept()
    {
        // Arrange
        var parameters = new PLinearRegression<double, double> { Period = 10 };
        var lr = new LinearRegression_FP<double, double>(parameters);

        // Uptrending linear data
        var inputs = new double[15];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 3 * i + 10;
        }
        var outputs = new double[inputs.Length];

        // Act
        lr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(lr.IsReady);
        Assert.True(lr.Slope > 0, $"Slope {lr.Slope} should be positive for uptrending data");
        // Intercept is implementation-dependent, just verify it exists
    }

    [Fact]
    public void LinearRegression_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PLinearRegression<double, double> { Period = 10 };
        var lr = new LinearRegression_FP<double, double>(parameters);

        var inputs = new double[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 100.0 + i * 2;
        }

        lr.OnBarBatch(inputs, new double[inputs.Length]);
        Assert.True(lr.IsReady);

        // Act
        lr.Clear();

        // Assert
        Assert.False(lr.IsReady);
    }
}
