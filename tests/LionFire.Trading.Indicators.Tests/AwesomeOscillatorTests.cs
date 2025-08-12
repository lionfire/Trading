using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class AwesomeOscillatorTests
{
    [Fact]
    public void AwesomeOscillator_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<HL, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        var ao = new AwesomeOscillator_QC<HL, double>(parameters);
        
        // Sample data
        var inputs = new HL[50];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.2) * 10;
            inputs[i] = new HL
            {
                High = price + 1,
                Low = price - 1
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        ao.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ao.IsReady);
        
        // AO should have values after slow period
        for (int i = parameters.SlowPeriod - 1; i < outputs.Length; i++)
        {
            Assert.NotEqual(0, outputs[i]);
        }
        
        Assert.Equal(outputs[outputs.Length - 1], ao.Value);
    }

    [Fact]
    public void AwesomeOscillator_DetectsMomentum()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<HL, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        
        // Strong upward momentum
        var uptrend = new HL[50];
        for (int i = 0; i < uptrend.Length; i++)
        {
            var price = 100.0 + i * 2.0;
            uptrend[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.3
            };
        }
        
        // Strong downward momentum
        var downtrend = new HL[50];
        for (int i = 0; i < downtrend.Length; i++)
        {
            var price = 200.0 - i * 2.0;
            downtrend[i] = new HL
            {
                High = price + 0.3,
                Low = price - 0.5
            };
        }

        // Act
        var aoUp = new AwesomeOscillator_QC<HL, double>(parameters);
        var upOutputs = new double[uptrend.Length];
        aoUp.OnBarBatch(uptrend, upOutputs);
        var upAO = upOutputs[upOutputs.Length - 1];
        
        var aoDown = new AwesomeOscillator_QC<HL, double>(parameters);
        var downOutputs = new double[downtrend.Length];
        aoDown.OnBarBatch(downtrend, downOutputs);
        var downAO = downOutputs[downOutputs.Length - 1];

        // Assert
        Assert.True(upAO > 0, $"Uptrend AO {upAO} should be positive");
        Assert.True(downAO < 0, $"Downtrend AO {downAO} should be negative");
    }

    [Fact]
    public void AwesomeOscillator_ZeroCrossing()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<HL, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        var ao = new AwesomeOscillator_QC<HL, double>(parameters);
        
        // Data that transitions from uptrend to downtrend
        var inputs = new HL[80];
        for (int i = 0; i < 40; i++)
        {
            var price = 100.0 + i * 1.0; // Uptrend
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.3
            };
        }
        for (int i = 40; i < 80; i++)
        {
            var price = 140.0 - (i - 40) * 1.0; // Downtrend
            inputs[i] = new HL
            {
                High = price + 0.3,
                Low = price - 0.5
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        ao.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ao.IsReady);
        
        // Should have positive values in uptrend
        Assert.True(outputs[39] > 0);
        
        // Should transition to negative in downtrend
        Assert.True(outputs[outputs.Length - 1] < 0);
    }

    [Fact]
    public void AwesomeOscillator_SaucerSignals()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<HL, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        var ao = new AwesomeOscillator_QC<HL, double>(parameters);
        
        // Create data for saucer pattern (three consecutive bars)
        var inputs = new HL[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            double price;
            if (i < 35)
            {
                price = 100.0 + i * 0.5; // Initial uptrend
            }
            else if (i < 40)
            {
                price = 117.5 - (i - 35) * 0.3; // Small pullback
            }
            else
            {
                price = 116.0 + (i - 40) * 0.4; // Resume uptrend
            }
            
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        ao.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ao.IsReady);
        
        // Check for consecutive bar patterns
        var hasPattern = false;
        for (int i = parameters.SlowPeriod + 2; i < outputs.Length; i++)
        {
            // Look for three consecutive bars forming specific patterns
            if (outputs[i - 2] != 0 && outputs[i - 1] != 0 && outputs[i] != 0)
            {
                hasPattern = true;
                break;
            }
        }
        
        Assert.True(hasPattern, "Should have valid consecutive bar patterns");
    }

    [Fact]
    public void AwesomeOscillator_TwinPeaks()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<HL, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        var ao = new AwesomeOscillator_QC<HL, double>(parameters);
        
        // Create data with twin peaks pattern
        var inputs = new HL[100];
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
            
            inputs[i] = new HL
            {
                High = price + 1,
                Low = price - 1
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        ao.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ao.IsReady);
        
        // Find peaks in AO
        var peaks = new List<int>();
        for (int i = parameters.SlowPeriod + 1; i < outputs.Length - 1; i++)
        {
            if (outputs[i] > outputs[i - 1] && outputs[i] > outputs[i + 1])
            {
                peaks.Add(i);
            }
        }
        
        // Should have at least two peaks
        Assert.True(peaks.Count >= 2, "Should detect at least two peaks for twin peaks pattern");
    }

    [Fact]
    public void AwesomeOscillator_ColorCoding()
    {
        // Arrange
        var parameters = new PAwesomeOscillator<HL, double>
        {
            FastPeriod = 5,
            SlowPeriod = 34
        };
        var ao = new AwesomeOscillator_QC<HL, double>(parameters);
        
        // Create oscillating data
        var inputs = new HL[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 10;
            inputs[i] = new HL
            {
                High = price + 1,
                Low = price - 1
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
        var inputs = new HL[80];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5 + Math.Sin(i * 0.2) * 5;
            inputs[i] = new HL
            {
                High = price + 1,
                Low = price - 1
            };
        }

        foreach (var fast in fastPeriods)
        {
            foreach (var slow in slowPeriods.Where(s => s > fast))
            {
                // Arrange
                var parameters = new PAwesomeOscillator<HL, double>
                {
                    FastPeriod = fast,
                    SlowPeriod = slow
                };
                var ao = new AwesomeOscillator_QC<HL, double>(parameters);
                var outputs = new double[inputs.Length];

                // Act
                ao.OnBarBatch(inputs, outputs);

                // Assert
                Assert.True(ao.IsReady);
                var lastValue = outputs[outputs.Length - 1];
                Assert.NotEqual(0, lastValue);
            }
        }
    }
}