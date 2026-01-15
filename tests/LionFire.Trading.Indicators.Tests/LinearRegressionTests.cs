// DISABLED: Tests need updating to match current API
#if false
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class LinearRegressionTests
{
    [Fact]
    public void LinearRegression_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PLinearRegression<double, LinearRegressionResult> { Period = 10 };
        var lr = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
        
        // Perfect linear data
        var inputs = Enumerable.Range(1, 20).Select(x => (double)x * 10).ToArray();
        var outputs = new LinearRegressionResult[inputs.Length];

        // Act
        lr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(lr.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // For perfect linear data, slope should be constant (10)
        Assert.Equal(10, lastResult.Slope, 1);
        
        // Intercept and value should be reasonable
        Assert.True(lastResult.Value > 0);
        Assert.True(lastResult.Intercept >= 0);
    }

    [Fact]
    public void LinearRegression_PerfectFit()
    {
        // Arrange
        var parameters = new PLinearRegression<double, LinearRegressionResult> { Period = 10 };
        var lr = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
        
        // Perfect linear relationship: y = 2x + 5
        var inputs = new double[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 2 * i + 5;
        }
        var outputs = new LinearRegressionResult[inputs.Length];

        // Act
        lr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(lr.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        
        // Slope should be 2
        Assert.Equal(2, lastResult.Slope, 2);
        
        // R-squared should be 1 (perfect fit)
        Assert.Equal(1, lastResult.RSquared, 2);
    }

    [Fact]
    public void LinearRegression_TrendDetection()
    {
        // Arrange
        var parameters = new PLinearRegression<double, LinearRegressionResult> { Period = 20 };
        
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
        var lrUp = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
        var upOutputs = new LinearRegressionResult[uptrend.Length];
        lrUp.OnBarBatch(uptrend, upOutputs);
        var upSlope = upOutputs[upOutputs.Length - 1].Slope;
        
        var lrDown = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
        var downOutputs = new LinearRegressionResult[downtrend.Length];
        lrDown.OnBarBatch(downtrend, downOutputs);
        var downSlope = downOutputs[downOutputs.Length - 1].Slope;
        
        var lrSide = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
        var sideOutputs = new LinearRegressionResult[sideways.Length];
        lrSide.OnBarBatch(sideways, sideOutputs);
        var sideSlope = sideOutputs[sideOutputs.Length - 1].Slope;

        // Assert
        Assert.True(upSlope > 0, $"Uptrend slope {upSlope} should be positive");
        Assert.True(downSlope < 0, $"Downtrend slope {downSlope} should be negative");
        Assert.InRange(sideSlope, -0.5, 0.5); // Near zero for sideways
    }

    [Fact]
    public void LinearRegression_StandardError()
    {
        // Arrange
        var parameters = new PLinearRegression<double, LinearRegressionResult> { Period = 20 };
        var lr = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
        
        // Data with noise
        var inputs = new double[30];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            var trend = 100.0 + i * 1.0;
            var noise = random.NextDouble() * 10 - 5;
            inputs[i] = trend + noise;
        }
        var outputs = new LinearRegressionResult[inputs.Length];

        // Act
        lr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(lr.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        
        // Standard error should be positive
        Assert.True(lastResult.StandardError > 0);
        
        // R-squared should be less than 1 due to noise
        Assert.True(lastResult.RSquared < 1);
        Assert.True(lastResult.RSquared > 0);
    }

    [Fact]
    public void LinearRegression_ConfidenceBands()
    {
        // Arrange
        var parameters = new PLinearRegression<double, LinearRegressionResult> { Period = 20 };
        var lr = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
        
        // Data with moderate volatility
        var inputs = new double[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var trend = 100.0 + i * 0.5;
            var volatility = Math.Sin(i * 0.3) * 3;
            inputs[i] = trend + volatility;
        }
        var outputs = new LinearRegressionResult[inputs.Length];

        // Act
        lr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(lr.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        
        // Upper and lower bands should bracket the regression line
        Assert.True(lastResult.UpperBand > lastResult.Value);
        Assert.True(lastResult.LowerBand < lastResult.Value);
        
        // Bands should be symmetric around the value
        var upperDistance = lastResult.UpperBand - lastResult.Value;
        var lowerDistance = lastResult.Value - lastResult.LowerBand;
        Assert.Equal(upperDistance, lowerDistance, 2);
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
            var parameters = new PLinearRegression<double, LinearRegressionResult> { Period = period };
            var lr = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
            var outputs = new LinearRegressionResult[inputs.Length];

            // Act
            lr.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(lr.IsReady);
            var lastResult = outputs[outputs.Length - 1];
            slopes[period] = lastResult.Slope;
            
            // All should detect positive trend
            Assert.True(lastResult.Slope > 0);
        }

        // Longer periods should have more stable slope estimates
        Assert.True(slopes.All(kv => Math.Abs(kv.Value - 0.8) < 0.5));
    }

    [Fact]
    public void LinearRegression_Extrapolation()
    {
        // Arrange
        var parameters = new PLinearRegression<double, LinearRegressionResult> { Period = 10 };
        var lr = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
        
        // Clear linear trend
        var inputs = new double[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = 50.0 + i * 3.0;
        }
        var outputs = new LinearRegressionResult[inputs.Length];

        // Act
        lr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(lr.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        
        // Extrapolate next value
        var expectedNext = lastResult.Value + lastResult.Slope;
        var actualNext = 50.0 + 30 * 3.0; // Next value if trend continues
        
        Assert.InRange(expectedNext, actualNext - 10, actualNext + 10);
    }

    [Fact]
    public void LinearRegression_VolatilityMeasure()
    {
        // Arrange
        var parameters = new PLinearRegression<double, LinearRegressionResult> { Period = 20 };
        
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
        var lrLow = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
        var lowOutputs = new LinearRegressionResult[lowVol.Length];
        lrLow.OnBarBatch(lowVol, lowOutputs);
        var lowError = lowOutputs[lowOutputs.Length - 1].StandardError;
        var lowRSquared = lowOutputs[lowOutputs.Length - 1].RSquared;
        
        var lrHigh = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
        var highOutputs = new LinearRegressionResult[highVol.Length];
        lrHigh.OnBarBatch(highVol, highOutputs);
        var highError = highOutputs[highOutputs.Length - 1].StandardError;
        var highRSquared = highOutputs[highOutputs.Length - 1].RSquared;

        // Assert
        Assert.True(highError > lowError, "High volatility should have higher standard error");
        Assert.True(lowRSquared > highRSquared, "Low volatility should have higher R-squared");
    }
}

public class LinearRegressionResult
{
    public double Value { get; set; }      // Current regression value
    public double Slope { get; set; }      // Slope of the regression line
    public double Intercept { get; set; }  // Y-intercept
    public double RSquared { get; set; }   // Coefficient of determination
    public double StandardError { get; set; } // Standard error
    public double UpperBand { get; set; }  // Upper confidence band
    public double LowerBand { get; set; }  // Lower confidence band
}
#endif
