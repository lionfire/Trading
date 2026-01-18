using LionFire.Trading.Indicators.QuantConnect_;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class ATRTests
{
    [Fact]
    public void ATR_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PAverageTrueRange<double, double> { Period = 14 };
        var atr = new AverageTrueRange<double, double>(parameters);

        // Sample OHLC data
        var inputs = new HLC<double>[]
        {
            new() { High = 48.70, Low = 47.79, Close = 48.16 },
            new() { High = 48.72, Low = 48.14, Close = 48.61 },
            new() { High = 48.90, Low = 48.39, Close = 48.75 },
            new() { High = 48.87, Low = 48.37, Close = 48.63 },
            new() { High = 48.82, Low = 48.24, Close = 48.74 },
            new() { High = 49.05, Low = 48.64, Close = 49.03 },
            new() { High = 49.20, Low = 48.94, Close = 49.07 },
            new() { High = 49.35, Low = 48.86, Close = 49.32 },
            new() { High = 49.92, Low = 49.50, Close = 49.91 },
            new() { High = 50.19, Low = 49.87, Close = 50.13 },
            new() { High = 50.12, Low = 49.20, Close = 49.53 },
            new() { High = 49.66, Low = 48.90, Close = 49.50 },
            new() { High = 49.88, Low = 49.43, Close = 49.75 },
            new() { High = 50.19, Low = 49.73, Close = 50.03 },
            new() { High = 50.36, Low = 49.26, Close = 50.31 },
            new() { High = 50.57, Low = 50.09, Close = 50.52 },
            new() { High = 50.65, Low = 50.30, Close = 50.41 },
            new() { High = 50.43, Low = 49.21, Close = 49.34 },
            new() { High = 49.63, Low = 48.98, Close = 49.37 },
            new() { High = 50.33, Low = 49.61, Close = 50.23 }
        };

        var outputs = new double[inputs.Length];

        // Act
        atr.OnBarBatch(inputs, outputs);

        // Assert
        // ATR should be positive for valid outputs
        for (int i = parameters.Period - 1; i < outputs.Length; i++)
        {
            Assert.True(outputs[i] > 0, $"ATR at index {i} should be positive, but was {outputs[i]}");
        }
    }

    [Fact]
    public void ATR_HandlesHighVolatility()
    {
        // Arrange
        var parameters = new PAverageTrueRange<double, double> { Period = 14 };
        var atr = new AverageTrueRange<double, double>(parameters);

        // High volatility data
        var inputs = new HLC<double>[20];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            var basePrice = 100.0 + i;
            var volatility = random.NextDouble() * 10; // 0-10 volatility
            inputs[i] = new HLC<double>
            {
                High = basePrice + volatility,
                Low = basePrice - volatility,
                Close = basePrice + (random.NextDouble() - 0.5) * volatility
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        atr.OnBarBatch(inputs, outputs);

        // Assert
        var lastATR = outputs[^1];
        Assert.True(lastATR > 0);
        Assert.True(lastATR < 20); // Should be reasonable given our volatility range
    }

    [Fact]
    public void ATR_HandlesLowVolatility()
    {
        // Arrange
        var parameters = new PAverageTrueRange<double, double> { Period = 14 };
        var atr = new AverageTrueRange<double, double>(parameters);

        // Low volatility data
        var inputs = new HLC<double>[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.1;
            inputs[i] = new HLC<double>
            {
                High = price + 0.05,
                Low = price - 0.05,
                Close = price
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        atr.OnBarBatch(inputs, outputs);

        // Assert
        var lastATR = outputs[^1];
        Assert.True(lastATR < 1); // Low volatility should produce low ATR
    }

    [Fact]
    public void ATR_DifferentPeriods()
    {
        var periods = new[] { 7, 14, 21 };

        // Create sample data
        var inputs = new HLC<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.5) * 5;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        var atrValues = new Dictionary<int, double>();

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PAverageTrueRange<double, double> { Period = period };
            var atr = new AverageTrueRange<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            atr.OnBarBatch(inputs, outputs);

            // Assert
            atrValues[period] = outputs[^1];
        }

        // Longer periods should produce smoother (potentially different) ATR values
        Assert.True(atrValues.All(kv => kv.Value > 0));
    }

    [Fact]
    public void ATR_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PAverageTrueRange<double, double> { Period = 7 };
        var atr = new AverageTrueRange<double, double>(parameters);

        var inputs = new HLC<double>[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        var outputs = new double[inputs.Length];
        atr.OnBarBatch(inputs, outputs);
        var firstValue = outputs[^1];

        // Act
        atr.Clear();

        // Process again
        var outputs2 = new double[inputs.Length];
        atr.OnBarBatch(inputs, outputs2);
        var secondValue = outputs2[^1];

        // Assert - should get same result after clear
        Assert.Equal(firstValue, secondValue, 6);
    }
}
