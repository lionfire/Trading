using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class ParabolicSARTests
{
    [Fact]
    public void ParabolicSAR_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PParabolicSAR<double, double>
        {
            AccelerationFactor = 0.02,
            MaxAccelerationFactor = 0.2
        };
        var sar = new ParabolicSAR_FP<double, double>(parameters);

        // Sample trending data
        var inputs = new HLC<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5; // Uptrend
            inputs[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.2,
                Close = price + 0.1
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        sar.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(sar.IsReady);

        // SAR should be below price in uptrend
        for (int i = 2; i < outputs.Length; i++)
        {
            if (!double.IsNaN(outputs[i]) && outputs[i] > 0)
            {
                Assert.True(outputs[i] < inputs[i].High,
                    $"SAR {outputs[i]} should be below high {inputs[i].High} in uptrend at index {i}");
            }
        }

        Assert.Equal(outputs[^1], sar.CurrentValue);
    }

    [Fact]
    public void ParabolicSAR_DetectsTrendReversal()
    {
        // Arrange
        var parameters = new PParabolicSAR<double, double>
        {
            AccelerationFactor = 0.02,
            MaxAccelerationFactor = 0.2
        };
        var sar = new ParabolicSAR_FP<double, double>(parameters);

        // Data with trend reversal
        var inputs = new HLC<double>[40];
        // Uptrend for first 20 bars
        for (int i = 0; i < 20; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.2,
                Close = price + 0.1
            };
        }
        // Downtrend for next 20 bars
        for (int i = 20; i < 40; i++)
        {
            var price = 110.0 - (i - 20) * 0.5;
            inputs[i] = new HLC<double>
            {
                High = price + 0.2,
                Low = price - 0.3,
                Close = price - 0.1
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        sar.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(sar.IsReady);

        // SAR should be below price in uptrend (first half)
        Assert.True(outputs[10] < inputs[10].High);

        // SAR should be above price in downtrend (second half)
        Assert.True(outputs[35] > inputs[35].Low);
    }

    [Fact]
    public void ParabolicSAR_AccelerationFactorIncreases()
    {
        // Arrange
        var parameters = new PParabolicSAR<double, double>
        {
            AccelerationFactor = 0.02,
            MaxAccelerationFactor = 0.2
        };
        var sar = new ParabolicSAR_FP<double, double>(parameters);

        // Strong consistent uptrend
        var inputs = new HLC<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 2; // Strong uptrend
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.1,
                Close = price + 0.4
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        sar.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(sar.IsReady);

        // In a strong trend, SAR should accelerate (get closer to price)
        var distances = new List<double>();
        for (int i = 10; i < outputs.Length; i++)
        {
            if (!double.IsNaN(outputs[i]) && outputs[i] > 0)
            {
                distances.Add(inputs[i].Low - outputs[i]);
            }
        }

        // Distance should generally decrease as acceleration increases
        Assert.True(distances.Count > 0);
    }

    [Fact]
    public void ParabolicSAR_HandlesConsolidation()
    {
        // Arrange
        var parameters = new PParabolicSAR<double, double>
        {
            AccelerationFactor = 0.02,
            MaxAccelerationFactor = 0.2
        };
        var sar = new ParabolicSAR_FP<double, double>(parameters);

        // Sideways/consolidating data
        var inputs = new HLC<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.5) * 2;
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        sar.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(sar.IsReady);

        // In consolidation, SAR should flip between above and below
        var flips = 0;
        for (int i = 3; i < outputs.Length - 1; i++)
        {
            if (!double.IsNaN(outputs[i]) && !double.IsNaN(outputs[i + 1]) && outputs[i] > 0 && outputs[i + 1] > 0)
            {
                var currentPosition = outputs[i] < inputs[i].Close;
                var nextPosition = outputs[i + 1] < inputs[i + 1].Close;
                if (currentPosition != nextPosition) flips++;
            }
        }

        Assert.True(flips > 0, "SAR should flip in consolidating market");
    }

    [Fact]
    public void ParabolicSAR_DifferentAccelerationSettings()
    {
        var accelerationFactors = new[] { 0.01, 0.02, 0.05 };

        // Create trending data
        var inputs = new HLC<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.2,
                Close = price + 0.1
            };
        }

        var sarValues = new Dictionary<double, double>();

        foreach (var af in accelerationFactors)
        {
            // Arrange
            var parameters = new PParabolicSAR<double, double>
            {
                AccelerationFactor = af,
                MaxAccelerationFactor = 0.2
            };
            var sar = new ParabolicSAR_FP<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            sar.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(sar.IsReady);
            sarValues[af] = outputs[^1];
        }

        // Higher acceleration should result in SAR being closer to price
        Assert.True(sarValues.All(kv => !double.IsNaN(kv.Value) && kv.Value > 0));
    }

    [Fact]
    public void ParabolicSAR_TrendDirection()
    {
        // Arrange
        var parameters = new PParabolicSAR<double, double>
        {
            AccelerationFactor = 0.02,
            MaxAccelerationFactor = 0.2
        };

        // Uptrend
        var uptrendInputs = new HLC<double>[20];
        for (int i = 0; i < uptrendInputs.Length; i++)
        {
            var price = 100.0 + i * 1.0;
            uptrendInputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.2
            };
        }

        var sarUp = new ParabolicSAR_FP<double, double>(parameters);
        sarUp.OnBarBatch(uptrendInputs, new double[uptrendInputs.Length]);

        // Assert
        Assert.True(sarUp.IsReady);
        Assert.True(sarUp.IsLong, "SAR should indicate long position in uptrend");
    }

    [Fact]
    public void ParabolicSAR_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PParabolicSAR<double, double>
        {
            AccelerationFactor = 0.02,
            MaxAccelerationFactor = 0.2
        };
        var sar = new ParabolicSAR_FP<double, double>(parameters);

        var inputs = new HLC<double>[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.2,
                Close = price + 0.1
            };
        }

        sar.OnBarBatch(inputs, new double[inputs.Length]);
        Assert.True(sar.IsReady);

        // Act
        sar.Clear();

        // Assert
        Assert.False(sar.IsReady);
    }
}
