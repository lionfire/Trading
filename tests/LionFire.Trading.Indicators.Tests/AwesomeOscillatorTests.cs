using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class AwesomeOscillatorTests
{
    [Fact]
    public void AwesomeOscillator_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<double, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        var ao = new AwesomeOscillator_FP<double, double>(parameters);

        // Sample data
        var inputs = new HLC<double>[50];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.2) * 10;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        ao.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ao.IsReady);

        // AO should have values after slow period
        Assert.NotEqual(0, ao.Value);
        Assert.Equal(outputs[outputs.Length - 1], ao.Value);
    }

    [Fact]
    public void AwesomeOscillator_DetectsMomentum()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<double, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };

        // Strong upward momentum
        var uptrend = new HLC<double>[50];
        for (int i = 0; i < uptrend.Length; i++)
        {
            var price = 100.0 + i * 2.0;
            uptrend[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price
            };
        }

        // Strong downward momentum
        var downtrend = new HLC<double>[50];
        for (int i = 0; i < downtrend.Length; i++)
        {
            var price = 200.0 - i * 2.0;
            downtrend[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.5,
                Close = price
            };
        }

        // Act
        var aoUp = new AwesomeOscillator_FP<double, double>(parameters);
        var upOutputs = new double[uptrend.Length];
        aoUp.OnBarBatch(uptrend, upOutputs);
        var upAO = aoUp.Value;

        var aoDown = new AwesomeOscillator_FP<double, double>(parameters);
        var downOutputs = new double[downtrend.Length];
        aoDown.OnBarBatch(downtrend, downOutputs);
        var downAO = aoDown.Value;

        // Assert
        Assert.True(upAO > 0, $"Uptrend AO {upAO} should be positive");
        Assert.True(downAO < 0, $"Downtrend AO {downAO} should be negative");
    }

    [Fact]
    public void AwesomeOscillator_ZeroCrossing()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<double, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        var ao = new AwesomeOscillator_FP<double, double>(parameters);

        // Data that transitions from uptrend to downtrend
        var inputs = new HLC<double>[80];
        for (int i = 0; i < 40; i++)
        {
            var price = 100.0 + i * 1.0; // Uptrend
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price
            };
        }
        for (int i = 40; i < 80; i++)
        {
            var price = 140.0 - (i - 40) * 1.0; // Downtrend
            inputs[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.5,
                Close = price
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        ao.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ao.IsReady);

        // Should transition to negative in downtrend
        Assert.True(ao.Value < 0);
    }

    [Fact]
    public void AwesomeOscillator_SMAComponents()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<double, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        var ao = new AwesomeOscillator_FP<double, double>(parameters);

        // Create uptrend data
        var inputs = new HLC<double>[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        ao.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ao.IsReady);

        // Fast SMA should be greater than Slow SMA in uptrend
        Assert.True(ao.FastSMA > ao.SlowSMA,
            $"Fast SMA ({ao.FastSMA}) should be greater than Slow SMA ({ao.SlowSMA}) in uptrend");

        // AO = Fast SMA - Slow SMA
        Assert.Equal(ao.FastSMA - ao.SlowSMA, ao.Value, 5);
    }

    [Fact]
    public void AwesomeOscillator_ColorCoding()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<double, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        var ao = new AwesomeOscillator_FP<double, double>(parameters);

        // Create oscillating data
        var inputs = new HLC<double>[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 10;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        ao.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ao.IsReady);

        // Check for increasing and decreasing values (would be green/red in visual)
        var increases = 0;
        var decreases = 0;
        for (int i = parameters.SlowPeriod; i < outputs.Length; i++)
        {
            if (outputs[i] > outputs[i - 1])
                increases++;
            else if (outputs[i] < outputs[i - 1])
                decreases++;
        }

        // Should have both increases and decreases in oscillating data
        Assert.True(increases > 0, "Should have increasing values");
        Assert.True(decreases > 0, "Should have decreasing values");
    }

    [Fact]
    public void AwesomeOscillator_DifferentPeriods()
    {
        var fastPeriods = new[] { 3, 5, 8 };
        var slowPeriods = new[] { 21, 34, 55 };

        // Create sample data
        var inputs = new HLC<double>[80];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5 + Math.Sin(i * 0.2) * 5;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        foreach (var fast in fastPeriods)
        {
            foreach (var slow in slowPeriods.Where(s => s > fast))
            {
                // Arrange
                var parameters = new PAwesomeOscillator<double, double>
                {
                    FastPeriod = fast,
                    SlowPeriod = slow
                };
                var ao = new AwesomeOscillator_FP<double, double>(parameters);
                var outputs = new double[inputs.Length];

                // Act
                ao.OnBarBatch(inputs, outputs);

                // Assert
                Assert.True(ao.IsReady);
                Assert.NotEqual(0, ao.Value);
            }
        }
    }

    [Fact]
    public void AwesomeOscillator_TwinPeaks()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<double, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        var ao = new AwesomeOscillator_FP<double, double>(parameters);

        // Create data with twin peaks pattern
        var inputs = new HLC<double>[100];
        for (int i = 0; i < inputs.Length; i++)
        {
            double price;
            if (i < 30)
            {
                price = 100.0 + i * 0.5; // First rise
            }
            else if (i < 50)
            {
                price = 115.0 - (i - 30) * 0.5; // Pullback
            }
            else if (i < 70)
            {
                price = 105.0 + (i - 50) * 0.6; // Second rise (higher high)
            }
            else
            {
                price = 117.0 - (i - 70) * 0.4; // Final decline
            }

            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        ao.OnBarBatch(inputs, outputs);

        // Assert - verify AO produces output for varying price patterns
        Assert.True(ao.IsReady);
        Assert.False(double.IsNaN(ao.Value));

        // Verify we get some valid outputs
        var validOutputs = outputs.Where(o => !double.IsNaN(o)).ToArray();
        Assert.True(validOutputs.Length > 0, "AO should produce valid outputs");
    }

    [Fact]
    public void AwesomeOscillator_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<double, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        var ao = new AwesomeOscillator_FP<double, double>(parameters);

        var inputs = new HLC<double>[50];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }

        ao.OnBarBatch(inputs, new double[inputs.Length]);
        Assert.True(ao.IsReady);

        // Act
        ao.Clear();

        // Assert
        Assert.False(ao.IsReady);
    }
}
