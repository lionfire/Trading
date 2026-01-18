using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class AroonTests
{
    [Fact]
    public void Aroon_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PAroon<double, double> { Period = 14 };
        var aroon = new Aroon_FP<double, double>(parameters);

        // Sample data with clear highs and lows
        var inputs = new HLC<double>[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5; // Uptrend
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }

        var outputs = new double[inputs.Length * 3]; // AroonUp, AroonDown, Oscillator

        // Act
        aroon.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(aroon.IsReady);

        // In a steady uptrend:
        // AroonUp should be high (recent high is recent)
        Assert.True(aroon.AroonUp > 70, $"AroonUp {aroon.AroonUp} should be > 70 in uptrend");
    }

    [Fact]
    public void Aroon_DetectsTrendStrength()
    {
        // Arrange
        var parameters = new PAroon<double, double> { Period = 14 };

        // Strong uptrend data
        var uptrend = new HLC<double>[20];
        for (int i = 0; i < uptrend.Length; i++)
        {
            var price = 100.0 + i * 2; // Strong uptrend
            uptrend[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.2,
                Close = price
            };
        }

        // Strong downtrend data
        var downtrend = new HLC<double>[20];
        for (int i = 0; i < downtrend.Length; i++)
        {
            var price = 100.0 - i * 2; // Strong downtrend
            downtrend[i] = new HLC<double>
            {
                High = price + 0.2,
                Low = price - 0.3,
                Close = price
            };
        }

        // Act
        var aroonUp = new Aroon_FP<double, double>(parameters);
        aroonUp.OnBarBatch(uptrend, new double[uptrend.Length * 3]);
        var upAroonUp = aroonUp.AroonUp;
        var upAroonDown = aroonUp.AroonDown;

        var aroonDown = new Aroon_FP<double, double>(parameters);
        aroonDown.OnBarBatch(downtrend, new double[downtrend.Length * 3]);
        var downAroonUp = aroonDown.AroonUp;
        var downAroonDown = aroonDown.AroonDown;

        // Assert
        // Uptrend: AroonUp high, AroonDown low
        Assert.True(upAroonUp > upAroonDown, $"Uptrend AroonUp ({upAroonUp}) should be > AroonDown ({upAroonDown})");

        // Downtrend: AroonDown high, AroonUp low
        Assert.True(downAroonDown > downAroonUp, $"Downtrend AroonDown ({downAroonDown}) should be > AroonUp ({downAroonUp})");
    }

    [Fact]
    public void Aroon_HandlesConsolidation()
    {
        // Arrange
        var parameters = new PAroon<double, double> { Period = 14 };
        var aroon = new Aroon_FP<double, double>(parameters);

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

        var outputs = new double[inputs.Length * 3];

        // Act
        aroon.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(aroon.IsReady);

        // Aroon values should be valid (0-100)
        Assert.InRange(aroon.AroonUp, 0, 100);
        Assert.InRange(aroon.AroonDown, 0, 100);
    }

    [Fact]
    public void Aroon_OscillatorCalculation()
    {
        // Arrange
        var parameters = new PAroon<double, double> { Period = 14 };
        var aroon = new Aroon_FP<double, double>(parameters);

        // Create data with known pattern
        var inputs = new HLC<double>[20];
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

        var outputs = new double[inputs.Length * 3];

        // Act
        aroon.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(aroon.IsReady);

        var oscillator = aroon.AroonOscillator;

        // Oscillator should be positive in uptrend (AroonUp > AroonDown)
        Assert.True(oscillator > 0, $"Aroon Oscillator {oscillator} should be positive in uptrend");

        // Oscillator should be between -100 and 100
        Assert.InRange(oscillator, -100, 100);
    }

    [Fact]
    public void Aroon_DifferentPeriods()
    {
        var periods = new[] { 10, 14, 25 };

        // Create trending data
        var inputs = new HLC<double>[40];
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

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PAroon<double, double> { Period = period };
            var aroon = new Aroon_FP<double, double>(parameters);
            var outputs = new double[inputs.Length * 3];

            // Act
            aroon.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(aroon.IsReady);

            // All periods should detect the uptrend
            Assert.True(aroon.AroonUp > aroon.AroonDown,
                $"Period {period}: AroonUp ({aroon.AroonUp}) should be > AroonDown ({aroon.AroonDown})");
            Assert.InRange(aroon.AroonUp, 0, 100);
            Assert.InRange(aroon.AroonDown, 0, 100);
        }
    }

    [Fact]
    public void Aroon_CrossoverSignals()
    {
        // Arrange
        var parameters = new PAroon<double, double> { Period = 14 };

        // Data with trend reversal
        var inputs = new HLC<double>[40];
        // Uptrend for first 20
        for (int i = 0; i < 20; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.2,
                Close = price
            };
        }
        // Downtrend for next 20
        for (int i = 20; i < 40; i++)
        {
            var price = 110.0 - (i - 20) * 0.5;
            inputs[i] = new HLC<double>
            {
                High = price + 0.2,
                Low = price - 0.3,
                Close = price
            };
        }

        // Act - First half (uptrend)
        var aroonFirst = new Aroon_FP<double, double>(parameters);
        var firstInputs = inputs.Take(20).ToArray();
        aroonFirst.OnBarBatch(firstInputs, new double[firstInputs.Length * 3]);
        var firstAroonUp = aroonFirst.AroonUp;
        var firstAroonDown = aroonFirst.AroonDown;

        // Act - Full data (trend reversal)
        var aroonFull = new Aroon_FP<double, double>(parameters);
        aroonFull.OnBarBatch(inputs, new double[inputs.Length * 3]);
        var lastAroonUp = aroonFull.AroonUp;
        var lastAroonDown = aroonFull.AroonDown;

        // Assert
        // First half should have AroonUp > AroonDown
        Assert.True(firstAroonUp > firstAroonDown,
            $"First half: AroonUp ({firstAroonUp}) should be > AroonDown ({firstAroonDown})");

        // After downtrend, AroonDown should be > AroonUp
        Assert.True(lastAroonDown > lastAroonUp,
            $"After downtrend: AroonDown ({lastAroonDown}) should be > AroonUp ({lastAroonUp})");
    }

    [Fact]
    public void Aroon_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PAroon<double, double> { Period = 10 };
        var aroon = new Aroon_FP<double, double>(parameters);

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

        aroon.OnBarBatch(inputs, new double[inputs.Length * 3]);
        Assert.True(aroon.IsReady);

        // Act
        aroon.Clear();

        // Assert
        Assert.False(aroon.IsReady);
    }
}
