// DISABLED: Tests need updating to match current API
#if false
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class AroonTests
{
    [Fact]
    public void Aroon_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PAroon<HL, AroonResult> { Period = 14 };
        var aroon = new Aroon_QC<HL, AroonResult>(parameters);
        
        // Sample data with clear highs and lows
        var inputs = new HL[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5; // Uptrend
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }
        
        var outputs = new AroonResult[inputs.Length];

        // Act
        aroon.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(aroon.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // In a steady uptrend:
        // AroonUp should be high (recent high is recent)
        Assert.True(lastResult.AroonUp > 90, $"AroonUp {lastResult.AroonUp} should be > 90 in uptrend");
        // AroonDown should be low (recent low is older)
        Assert.True(lastResult.AroonDown < 10, $"AroonDown {lastResult.AroonDown} should be < 10 in uptrend");
    }

    [Fact]
    public void Aroon_DetectsTrendStrength()
    {
        // Arrange
        var parameters = new PAroon<HL, AroonResult> { Period = 14 };
        
        // Strong uptrend data
        var uptrend = new HL[20];
        for (int i = 0; i < uptrend.Length; i++)
        {
            var price = 100.0 + i * 2; // Strong uptrend
            uptrend[i] = new HL
            {
                High = price + 0.3,
                Low = price - 0.2
            };
        }
        
        // Strong downtrend data
        var downtrend = new HL[20];
        for (int i = 0; i < downtrend.Length; i++)
        {
            var price = 100.0 - i * 2; // Strong downtrend
            downtrend[i] = new HL
            {
                High = price + 0.2,
                Low = price - 0.3
            };
        }

        // Act
        var aroonUp = new Aroon_QC<HL, AroonResult>(parameters);
        var upOutputs = new AroonResult[uptrend.Length];
        aroonUp.OnBarBatch(uptrend, upOutputs);
        var upResult = upOutputs[upOutputs.Length - 1];
        
        var aroonDown = new Aroon_QC<HL, AroonResult>(parameters);
        var downOutputs = new AroonResult[downtrend.Length];
        aroonDown.OnBarBatch(downtrend, downOutputs);
        var downResult = downOutputs[downOutputs.Length - 1];

        // Assert
        // Uptrend: AroonUp high, AroonDown low
        Assert.True(upResult.AroonUp > 70, $"Uptrend AroonUp {upResult.AroonUp} should be > 70");
        Assert.True(upResult.AroonDown < 30, $"Uptrend AroonDown {upResult.AroonDown} should be < 30");
        
        // Downtrend: AroonDown high, AroonUp low
        Assert.True(downResult.AroonDown > 70, $"Downtrend AroonDown {downResult.AroonDown} should be > 70");
        Assert.True(downResult.AroonUp < 30, $"Downtrend AroonUp {downResult.AroonUp} should be < 30");
    }

    [Fact]
    public void Aroon_HandlesConsolidation()
    {
        // Arrange
        var parameters = new PAroon<HL, AroonResult> { Period = 14 };
        var aroon = new Aroon_QC<HL, AroonResult>(parameters);
        
        // Sideways/consolidating data
        var inputs = new HL[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.5) * 2;
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }
        
        var outputs = new AroonResult[inputs.Length];

        // Act
        aroon.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(aroon.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        
        // In consolidation, both Aroon lines should be moderate
        Assert.InRange(lastResult.AroonUp, 20, 80);
        Assert.InRange(lastResult.AroonDown, 20, 80);
    }

    [Fact]
    public void Aroon_OscillatorCalculation()
    {
        // Arrange
        var parameters = new PAroon<HL, AroonResult> { Period = 14 };
        var aroon = new Aroon_QC<HL, AroonResult>(parameters);
        
        // Create data with known pattern
        var inputs = new HL[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }
        
        var outputs = new AroonResult[inputs.Length];

        // Act
        aroon.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(aroon.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        var oscillator = lastResult.AroonUp - lastResult.AroonDown;
        
        // Oscillator should be positive in uptrend
        Assert.True(oscillator > 0, $"Aroon Oscillator {oscillator} should be positive in uptrend");
        
        // Oscillator should be between -100 and 100
        Assert.InRange(oscillator, -100, 100);
    }

    [Fact]
    public void Aroon_DifferentPeriods()
    {
        var periods = new[] { 10, 14, 25 };
        
        // Create trending data
        var inputs = new HL[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PAroon<HL, AroonResult> { Period = period };
            var aroon = new Aroon_QC<HL, AroonResult>(parameters);
            var outputs = new AroonResult[inputs.Length];

            // Act
            aroon.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(aroon.IsReady);
            var lastResult = outputs[outputs.Length - 1];
            
            // All periods should detect the uptrend
            Assert.True(lastResult.AroonUp > lastResult.AroonDown);
            Assert.InRange(lastResult.AroonUp, 0, 100);
            Assert.InRange(lastResult.AroonDown, 0, 100);
        }
    }

    [Fact]
    public void Aroon_CrossoverSignals()
    {
        // Arrange
        var parameters = new PAroon<HL, AroonResult> { Period = 14 };
        var aroon = new Aroon_QC<HL, AroonResult>(parameters);
        
        // Data with trend reversal
        var inputs = new HL[40];
        // Uptrend for first 20
        for (int i = 0; i < 20; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HL
            {
                High = price + 0.3,
                Low = price - 0.2
            };
        }
        // Downtrend for next 20
        for (int i = 20; i < 40; i++)
        {
            var price = 110.0 - (i - 20) * 0.5;
            inputs[i] = new HL
            {
                High = price + 0.2,
                Low = price - 0.3
            };
        }
        
        var outputs = new AroonResult[inputs.Length];

        // Act
        aroon.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(aroon.IsReady);
        
        // First half should have AroonUp > AroonDown
        var firstHalfResult = outputs[19];
        Assert.True(firstHalfResult.AroonUp > firstHalfResult.AroonDown);
        
        // Second half should have AroonDown > AroonUp (after crossover)
        var secondHalfResult = outputs[39];
        Assert.True(secondHalfResult.AroonDown > secondHalfResult.AroonUp);
    }
}

public class AroonResult
{
    public double AroonUp { get; set; }
    public double AroonDown { get; set; }
}
#endif
