using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class ADXTests
{
    [Fact]
    public void ADX_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PADX<double, double> { Period = 14 };
        var adx = new ADX_FP<double, double>(parameters);

        // Sample trending OHLC data
        var inputs = new HLC<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5; // Uptrend
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.2
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        adx.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(adx.IsReady);

        // ADX should be between 0 and 100
        var validOutputs = outputs.Skip(parameters.Period * 2).Where(o => !double.IsNaN(o) && o > 0);
        foreach (var val in validOutputs)
        {
            Assert.InRange(val, 0, 100);
        }

        var lastADX = adx.ADX;
        Assert.True(lastADX > 0, $"ADX should be positive, but was {lastADX}");
    }

    [Fact]
    public void ADX_DetectsTrendStrength()
    {
        // Arrange
        var parameters = new PADX<double, double> { Period = 14 };

        // Strong uptrend data
        var strongTrend = new HLC<double>[40];
        for (int i = 0; i < strongTrend.Length; i++)
        {
            var price = 100.0 + i * 2; // Strong uptrend
            strongTrend[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.2,
                Close = price + 0.3
            };
        }

        // Sideways/ranging data
        var sideways = new HLC<double>[40];
        for (int i = 0; i < sideways.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.5) * 2; // Oscillating
            sideways[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }

        // Act
        var adxStrong = new ADX_FP<double, double>(parameters);
        var strongOutputs = new double[strongTrend.Length];
        adxStrong.OnBarBatch(strongTrend, strongOutputs);
        var strongADX = adxStrong.ADX;

        var adxSideways = new ADX_FP<double, double>(parameters);
        var sidewaysOutputs = new double[sideways.Length];
        adxSideways.OnBarBatch(sideways, sidewaysOutputs);
        var sidewaysADX = adxSideways.ADX;

        // Assert
        Assert.True(adxStrong.IsReady);
        Assert.True(adxSideways.IsReady);
        Assert.True(strongADX > sidewaysADX, $"Strong trend ({strongADX}) should have higher ADX than sideways ({sidewaysADX})");
    }

    [Fact]
    public void ADX_HandlesDowntrend()
    {
        // Arrange
        var parameters = new PADX<double, double> { Period = 14 };
        var adx = new ADX_FP<double, double>(parameters);

        // Downtrend data
        var inputs = new HLC<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 - i * 0.5; // Downtrend
            inputs[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.5,
                Close = price - 0.2
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        adx.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(adx.IsReady);
        var lastADX = adx.ADX;

        // ADX measures trend strength regardless of direction
        Assert.True(lastADX > 0, $"Downtrend should produce positive ADX, but was {lastADX}");
    }

    [Fact]
    public void ADX_DifferentPeriods()
    {
        var periods = new[] { 7, 14, 21 };

        // Create trending data
        var inputs = new HLC<double>[50];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.8;
            inputs[i] = new HLC<double>
            {
                High = price + 0.6,
                Low = price - 0.4,
                Close = price + 0.1
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PADX<double, double> { Period = period };
            var adx = new ADX_FP<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            adx.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(adx.IsReady);
            var lastValue = adx.ADX;
            Assert.True(lastValue >= 0);
            Assert.True(lastValue <= 100);
        }
    }

    [Fact]
    public void ADX_DirectionalIndicators()
    {
        // Arrange
        var parameters = new PADX<double, double> { Period = 14 };
        var adx = new ADX_FP<double, double>(parameters);

        // Strong uptrend - should have +DI > -DI
        var uptrendInputs = new HLC<double>[40];
        for (int i = 0; i < uptrendInputs.Length; i++)
        {
            var price = 100.0 + i * 1.0;
            uptrendInputs[i] = new HLC<double>
            {
                High = price + 1.0,
                Low = price - 0.2,
                Close = price + 0.8
            };
        }

        var outputs = new double[uptrendInputs.Length];

        // Act
        adx.OnBarBatch(uptrendInputs, outputs);

        // Assert
        Assert.True(adx.IsReady);
        var plusDI = adx.PlusDI;
        var minusDI = adx.MinusDI;

        // In uptrend, +DI should be greater than -DI
        Assert.True(plusDI > minusDI, $"+DI ({plusDI}) should be > -DI ({minusDI}) in uptrend");
    }

    [Fact]
    public void ADX_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PADX<double, double> { Period = 7 };
        var adx = new ADX_FP<double, double>(parameters);

        var inputs = new HLC<double>[30];
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
        adx.OnBarBatch(inputs, outputs);
        Assert.True(adx.IsReady);

        // Act
        adx.Clear();

        // Assert
        Assert.False(adx.IsReady);
    }
}
