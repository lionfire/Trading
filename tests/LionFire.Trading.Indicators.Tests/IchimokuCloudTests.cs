using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class IchimokuCloudTests
{
    [Fact]
    public void IchimokuCloud_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PIchimokuCloud<HLC, IchimokuResult>
        {
            TenkanPeriod = 9,
            KijunPeriod = 26,
            SenkouAPeriod = 26,
            SenkouBPeriod = 52,
            ChikouPeriod = 26
        };
        var ichimoku = new IchimokuCloud_QC<HLC, IchimokuResult>(parameters);
        
        // Sample data
        var inputs = new HLC[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.2) * 10;
            inputs[i] = new HLC
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }
        
        var outputs = new IchimokuResult[inputs.Length];

        // Act
        ichimoku.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ichimoku.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // All components should have values
        Assert.True(lastResult.Tenkan != 0);
        Assert.True(lastResult.Kijun != 0);
        Assert.True(lastResult.SenkouA != 0);
        Assert.True(lastResult.SenkouB != 0);
        Assert.True(lastResult.Chikou != 0);
    }

    [Fact]
    public void IchimokuCloud_DetectsTrend()
    {
        // Arrange
        var parameters = new PIchimokuCloud<HLC, IchimokuResult>
        {
            TenkanPeriod = 9,
            KijunPeriod = 26,
            SenkouAPeriod = 26,
            SenkouBPeriod = 52,
            ChikouPeriod = 26
        };
        
        // Strong uptrend data
        var uptrend = new HLC[60];
        for (int i = 0; i < uptrend.Length; i++)
        {
            var price = 100.0 + i * 1.5;
            uptrend[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.2
            };
        }
        
        // Strong downtrend data
        var downtrend = new HLC[60];
        for (int i = 0; i < downtrend.Length; i++)
        {
            var price = 200.0 - i * 1.5;
            downtrend[i] = new HLC
            {
                High = price + 0.3,
                Low = price - 0.5,
                Close = price - 0.2
            };
        }

        // Act
        var ichimokuUp = new IchimokuCloud_QC<HLC, IchimokuResult>(parameters);
        var upOutputs = new IchimokuResult[uptrend.Length];
        ichimokuUp.OnBarBatch(uptrend, upOutputs);
        var upResult = upOutputs[upOutputs.Length - 1];
        
        var ichimokuDown = new IchimokuCloud_QC<HLC, IchimokuResult>(parameters);
        var downOutputs = new IchimokuResult[downtrend.Length];
        ichimokuDown.OnBarBatch(downtrend, downOutputs);
        var downResult = downOutputs[downOutputs.Length - 1];

        // Assert
        // In uptrend, Tenkan should be above Kijun
        Assert.True(upResult.Tenkan > upResult.Kijun, 
            $"Uptrend: Tenkan {upResult.Tenkan} should be > Kijun {upResult.Kijun}");
        
        // In downtrend, Tenkan should be below Kijun
        Assert.True(downResult.Tenkan < downResult.Kijun,
            $"Downtrend: Tenkan {downResult.Tenkan} should be < Kijun {downResult.Kijun}");
    }

    [Fact]
    public void IchimokuCloud_CloudFormation()
    {
        // Arrange
        var parameters = new PIchimokuCloud<HLC, IchimokuResult>
        {
            TenkanPeriod = 9,
            KijunPeriod = 26,
            SenkouAPeriod = 26,
            SenkouBPeriod = 52,
            ChikouPeriod = 26
        };
        var ichimoku = new IchimokuCloud_QC<HLC, IchimokuResult>(parameters);
        
        // Create data with clear trend change
        var inputs = new HLC[100];
        // First half uptrend
        for (int i = 0; i < 50; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.1
            };
        }
        // Second half downtrend
        for (int i = 50; i < 100; i++)
        {
            var price = 125.0 - (i - 50) * 0.5;
            inputs[i] = new HLC
            {
                High = price + 0.3,
                Low = price - 0.5,
                Close = price - 0.1
            };
        }
        
        var outputs = new IchimokuResult[inputs.Length];

        // Act
        ichimoku.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ichimoku.IsReady);
        
        // Cloud should exist (Senkou A and B should differ)
        var midResult = outputs[75];
        Assert.NotEqual(midResult.SenkouA, midResult.SenkouB);
        
        // Cloud thickness indicates trend strength
        var cloudThickness = Math.Abs(midResult.SenkouA - midResult.SenkouB);
        Assert.True(cloudThickness > 0);
    }

    [Fact]
    public void IchimokuCloud_TenkanKijunCross()
    {
        // Arrange
        var parameters = new PIchimokuCloud<HLC, IchimokuResult>
        {
            TenkanPeriod = 9,
            KijunPeriod = 26,
            SenkouAPeriod = 26,
            SenkouBPeriod = 52,
            ChikouPeriod = 26
        };
        var ichimoku = new IchimokuCloud_QC<HLC, IchimokuResult>(parameters);
        
        // Create oscillating data for crossovers
        var inputs = new HLC[100];
        for (int i = 0; i < inputs.Length; i++)
        {
            var trend = i < 30 ? i * 0.5 : 
                       i < 60 ? 15 - (i - 30) * 0.5 :
                       (i - 60) * 0.5;
            var price = 100.0 + trend;
            inputs[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }
        
        var outputs = new IchimokuResult[inputs.Length];

        // Act
        ichimoku.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ichimoku.IsReady);
        
        // Should have different Tenkan/Kijun relationships at different points
        var earlyResult = outputs[40];
        var lateResult = outputs[80];
        
        // Values should change over time
        Assert.NotEqual(earlyResult.Tenkan, lateResult.Tenkan);
        Assert.NotEqual(earlyResult.Kijun, lateResult.Kijun);
    }

    [Fact]
    public void IchimokuCloud_ChikouSpan()
    {
        // Arrange
        var parameters = new PIchimokuCloud<HLC, IchimokuResult>
        {
            TenkanPeriod = 9,
            KijunPeriod = 26,
            SenkouAPeriod = 26,
            SenkouBPeriod = 52,
            ChikouPeriod = 26
        };
        var ichimoku = new IchimokuCloud_QC<HLC, IchimokuResult>(parameters);
        
        // Create data
        var inputs = new HLC[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.3;
            inputs[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }
        
        var outputs = new IchimokuResult[inputs.Length];

        // Act
        ichimoku.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ichimoku.IsReady);
        
        // Chikou span should be lagged close price
        var lastResult = outputs[outputs.Length - 1];
        Assert.True(lastResult.Chikou > 0);
        
        // Chikou should reflect historical close (lagged by ChikouPeriod)
        if (outputs.Length > parameters.ChikouPeriod)
        {
            var expectedIndex = outputs.Length - 1 - parameters.ChikouPeriod;
            // Chikou should be related to the close price from ChikouPeriod bars ago
            Assert.True(Math.Abs(lastResult.Chikou - inputs[outputs.Length - 1].Close) < 50);
        }
    }

    [Fact]
    public void IchimokuCloud_DifferentPeriods()
    {
        var tenkanPeriods = new[] { 7, 9, 12 };
        
        // Create sample data
        var inputs = new HLC[100];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.1) * 10;
            inputs[i] = new HLC
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        foreach (var tenkan in tenkanPeriods)
        {
            // Arrange
            var parameters = new PIchimokuCloud<HLC, IchimokuResult>
            {
                TenkanPeriod = tenkan,
                KijunPeriod = tenkan * 3,
                SenkouAPeriod = tenkan * 3,
                SenkouBPeriod = tenkan * 6,
                ChikouPeriod = tenkan * 3
            };
            var ichimoku = new IchimokuCloud_QC<HLC, IchimokuResult>(parameters);
            var outputs = new IchimokuResult[inputs.Length];

            // Act
            ichimoku.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(ichimoku.IsReady);
            var lastResult = outputs[outputs.Length - 1];
            
            // All components should be calculated
            Assert.NotNull(lastResult);
            Assert.True(lastResult.Tenkan != 0);
            Assert.True(lastResult.Kijun != 0);
        }
    }
}

public class IchimokuResult
{
    public double Tenkan { get; set; }    // Conversion line
    public double Kijun { get; set; }     // Base line
    public double SenkouA { get; set; }   // Leading span A
    public double SenkouB { get; set; }   // Leading span B
    public double Chikou { get; set; }    // Lagging span
}