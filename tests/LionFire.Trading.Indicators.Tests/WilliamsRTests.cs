using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class WilliamsRTests
{
    [Fact]
    public void WilliamsR_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PWilliamsR<HLC, double> { Period = 14 };
        var williamsR = new WilliamsR_QC<HLC, double>(parameters);
        
        // Sample OHLC data
        var inputs = new HLC[]
        {
            new() { High = 127.01, Low = 125.36, Close = 125.36 },
            new() { High = 127.62, Low = 126.16, Close = 126.96 },
            new() { High = 126.59, Low = 124.93, Close = 126.01 },
            new() { High = 127.35, Low = 126.09, Close = 127.29 },
            new() { High = 128.17, Low = 126.82, Close = 127.72 },
            new() { High = 128.43, Low = 126.48, Close = 127.79 },
            new() { High = 127.37, Low = 126.03, Close = 127.17 },
            new() { High = 126.42, Low = 124.83, Close = 124.92 },
            new() { High = 126.90, Low = 126.39, Close = 126.85 },
            new() { High = 126.85, Low = 125.72, Close = 125.92 },
            new() { High = 125.65, Low = 124.56, Close = 124.62 },
            new() { High = 125.72, Low = 124.57, Close = 125.07 },
            new() { High = 127.16, Low = 125.07, Close = 127.12 },
            new() { High = 127.72, Low = 126.86, Close = 127.49 },
            new() { High = 127.69, Low = 126.63, Close = 127.40 },
            new() { High = 128.22, Low = 126.80, Close = 128.10 },
            new() { High = 128.27, Low = 126.71, Close = 127.00 },
            new() { High = 127.74, Low = 125.79, Close = 126.90 },
            new() { High = 128.77, Low = 126.60, Close = 128.58 },
            new() { High = 129.29, Low = 127.87, Close = 128.60 }
        };
        
        var outputs = new double[inputs.Length];

        // Act
        williamsR.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(williamsR.IsReady);
        
        // Williams %R should be between -100 and 0
        for (int i = parameters.Period - 1; i < outputs.Length; i++)
        {
            Assert.InRange(outputs[i], -100, 0);
        }
        
        Assert.Equal(outputs[outputs.Length - 1], williamsR.Value);
    }

    [Fact]
    public void WilliamsR_DetectsOverboughtOversold()
    {
        // Arrange
        var parameters = new PWilliamsR<HLC, double> { Period = 14 };
        
        // Create data near highs (should show overbought, near 0)
        var nearHighs = new HLC[20];
        for (int i = 0; i < nearHighs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            nearHighs[i] = new HLC
            {
                High = price + 0.2,
                Low = price - 0.5,
                Close = price + 0.1 // Close near high
            };
        }
        
        // Create data near lows (should show oversold, near -100)
        var nearLows = new HLC[20];
        for (int i = 0; i < nearLows.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            nearLows[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.2,
                Close = price - 0.1 // Close near low
            };
        }

        // Act
        var wrHigh = new WilliamsR_QC<HLC, double>(parameters);
        var highOutputs = new double[nearHighs.Length];
        wrHigh.OnBarBatch(nearHighs, highOutputs);
        var highWR = highOutputs[highOutputs.Length - 1];
        
        var wrLow = new WilliamsR_QC<HLC, double>(parameters);
        var lowOutputs = new double[nearLows.Length];
        wrLow.OnBarBatch(nearLows, lowOutputs);
        var lowWR = lowOutputs[lowOutputs.Length - 1];

        // Assert
        Assert.True(highWR > -20, $"Near highs Williams %R {highWR} should be > -20 (overbought)");
        Assert.True(lowWR < -80, $"Near lows Williams %R {lowWR} should be < -80 (oversold)");
    }

    [Fact]
    public void WilliamsR_HandlesMiddleRange()
    {
        // Arrange
        var parameters = new PWilliamsR<HLC, double> { Period = 14 };
        var williamsR = new WilliamsR_QC<HLC, double>(parameters);
        
        // Data with closes in middle of range
        var inputs = new HLC[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 2;
            inputs[i] = new HLC
            {
                High = price + 2,
                Low = price - 2,
                Close = price // Close in middle
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        williamsR.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(williamsR.IsReady);
        var lastWR = outputs[outputs.Length - 1];
        
        // Middle range closes should produce Williams %R around -50
        Assert.InRange(lastWR, -70, -30);
    }

    [Fact]
    public void WilliamsR_DifferentPeriods()
    {
        var periods = new[] { 7, 14, 21 };
        
        // Create sample data
        var inputs = new HLC[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.2) * 5;
            inputs[i] = new HLC
            {
                High = price + 1,
                Low = price - 1,
                Close = price + Math.Sin(i * 0.3) * 0.5
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PWilliamsR<HLC, double> { Period = period };
            var williamsR = new WilliamsR_QC<HLC, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            williamsR.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(williamsR.IsReady);
            var lastValue = outputs[outputs.Length - 1];
            Assert.InRange(lastValue, -100, 0);
            
            // Shorter periods should be more sensitive
            var values = outputs.Skip(period).ToArray();
            Assert.True(values.All(v => v >= -100 && v <= 0));
        }
    }

    [Fact]
    public void WilliamsR_InverseOfStochastic()
    {
        // Williams %R is essentially the inverse of Stochastic %K
        // %R = -100 * (Highest High - Close) / (Highest High - Lowest Low)
        // %K = 100 * (Close - Lowest Low) / (Highest High - Lowest Low)
        // So %R = %K - 100
        
        // Arrange
        var parameters = new PWilliamsR<HLC, double> { Period = 14 };
        var williamsR = new WilliamsR_QC<HLC, double>(parameters);
        
        var inputs = new HLC[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.3 + Math.Sin(i * 0.5) * 2;
            inputs[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        williamsR.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(williamsR.IsReady);
        
        // All values should be negative
        var validOutputs = outputs.Skip(parameters.Period - 1).ToArray();
        Assert.True(validOutputs.All(v => v <= 0 && v >= -100));
    }
}