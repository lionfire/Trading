using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class CCITests
{
    [Fact]
    public void CCI_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PCCI<double, double> { Period = 20 };
        var cci = new CCI_FP<double, double>(parameters);

        // Sample HLC data
        var inputs = new HLC<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 5;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        cci.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(cci.IsReady);

        // CCI typically ranges from -200 to +200, but can exceed
        var lastCCI = cci.Value;
        Assert.Equal(outputs[^1], lastCCI);

        // Check that we have non-NaN outputs after the period
        for (int i = parameters.Period - 1; i < outputs.Length; i++)
        {
            Assert.False(double.IsNaN(outputs[i]));
        }
    }

    [Fact]
    public void CCI_DetectsOverboughtOversold()
    {
        // Arrange
        var parameters = new PCCI<double, double> { Period = 20 };

        // Create strong uptrend data (should show overbought)
        var uptrend = new HLC<double>[40];
        for (int i = 0; i < uptrend.Length; i++)
        {
            var price = 100.0 + i * 2; // Strong uptrend
            uptrend[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.4
            };
        }

        // Create strong downtrend data (should show oversold)
        var downtrend = new HLC<double>[40];
        for (int i = 0; i < downtrend.Length; i++)
        {
            var price = 100.0 - i * 2; // Strong downtrend
            downtrend[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.5,
                Close = price - 0.4
            };
        }

        // Act
        var cciUp = new CCI_FP<double, double>(parameters);
        var upOutputs = new double[uptrend.Length];
        cciUp.OnBarBatch(uptrend, upOutputs);
        var uptrendCCI = cciUp.Value;

        var cciDown = new CCI_FP<double, double>(parameters);
        var downOutputs = new double[downtrend.Length];
        cciDown.OnBarBatch(downtrend, downOutputs);
        var downtrendCCI = cciDown.Value;

        // Assert
        Assert.True(uptrendCCI > 100, $"Uptrend CCI {uptrendCCI} should be > 100 (overbought)");
        Assert.True(downtrendCCI < -100, $"Downtrend CCI {downtrendCCI} should be < -100 (oversold)");
    }

    [Fact]
    public void CCI_HandlesConstantPrices()
    {
        // Arrange
        var parameters = new PCCI<double, double> { Period = 20 };
        var cci = new CCI_FP<double, double>(parameters);

        // Constant price data
        var inputs = new HLC<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = new HLC<double>
            {
                High = 100.5,
                Low = 99.5,
                Close = 100
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        cci.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(cci.IsReady);
        var lastCCI = cci.Value;

        // With constant prices, CCI should be near 0
        // Note: May be NaN if std dev is 0
        if (!double.IsNaN(lastCCI))
        {
            Assert.InRange(lastCCI, -10, 10);
        }
    }

    [Fact]
    public void CCI_DifferentPeriods()
    {
        var periods = new[] { 14, 20, 30 };

        // Create oscillating data
        var inputs = new HLC<double>[50];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.2) * 10;
            inputs[i] = new HLC<double>
            {
                High = price + 1.5,
                Low = price - 1.5,
                Close = price
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PCCI<double, double> { Period = period };
            var cci = new CCI_FP<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            cci.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(cci.IsReady);

            // Should have valid outputs after period
            var values = outputs.Skip(period).Where(v => !double.IsNaN(v)).ToArray();
            Assert.True(values.Length > 0);
        }
    }

    [Fact]
    public void CCI_HandlesVolatileData()
    {
        // Arrange
        var parameters = new PCCI<double, double> { Period = 20 };
        var cci = new CCI_FP<double, double>(parameters);

        // Volatile data
        var inputs = new HLC<double>[40];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            var basePrice = 100.0;
            var volatility = random.NextDouble() * 20 - 10; // -10 to +10
            inputs[i] = new HLC<double>
            {
                High = basePrice + volatility + 2,
                Low = basePrice + volatility - 2,
                Close = basePrice + volatility
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        cci.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(cci.IsReady);

        // Check that CCI responds to volatility
        var values = outputs.Skip(parameters.Period).Where(v => !double.IsNaN(v) && v != 0).ToArray();
        if (values.Length >= 2)
        {
            var maxCCI = values.Max();
            var minCCI = values.Min();
            Assert.True(Math.Abs(maxCCI - minCCI) > 10, "CCI should show some range in volatile data");
        }
    }

    [Fact]
    public void CCI_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PCCI<double, double> { Period = 10 };
        var cci = new CCI_FP<double, double>(parameters);

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
        cci.OnBarBatch(inputs, outputs);
        Assert.True(cci.IsReady);

        // Act
        cci.Clear();

        // Assert
        Assert.False(cci.IsReady);
    }
}
