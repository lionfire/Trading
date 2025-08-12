using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class KeltnerChannelsTests
{
    [Fact]
    public void KeltnerChannels_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PKeltnerChannels<HLC, KeltnerChannelsResult> 
        { 
            Period = 20,
            Multiplier = 2.0,
            AtrPeriod = 10
        };
        var kc = new KeltnerChannels_QC<HLC, KeltnerChannelsResult>(parameters);
        
        // Sample data
        var inputs = new HLC[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 5;
            inputs[i] = new HLC
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }
        
        var outputs = new KeltnerChannelsResult[inputs.Length];

        // Act
        kc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(kc.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // Upper band should be above middle
        Assert.True(lastResult.UpperBand > lastResult.MiddleBand);
        // Lower band should be below middle
        Assert.True(lastResult.LowerBand < lastResult.MiddleBand);
        // Bands should be symmetric around middle
        var upperDistance = lastResult.UpperBand - lastResult.MiddleBand;
        var lowerDistance = lastResult.MiddleBand - lastResult.LowerBand;
        Assert.Equal(upperDistance, lowerDistance, 2);
    }

    [Fact]
    public void KeltnerChannels_ExpandsWithVolatility()
    {
        // Arrange
        var parameters = new PKeltnerChannels<HLC, KeltnerChannelsResult> 
        { 
            Period = 20,
            Multiplier = 2.0,
            AtrPeriod = 10
        };
        
        // Low volatility data
        var lowVolData = new HLC[30];
        for (int i = 0; i < lowVolData.Length; i++)
        {
            var price = 100.0 + i * 0.1; // Small changes
            lowVolData[i] = new HLC
            {
                High = price + 0.2,
                Low = price - 0.2,
                Close = price
            };
        }
        
        // High volatility data
        var highVolData = new HLC[30];
        for (int i = 0; i < highVolData.Length; i++)
        {
            var price = 100.0 + (i % 2 == 0 ? 5 : -5); // Large swings
            highVolData[i] = new HLC
            {
                High = price + 2,
                Low = price - 2,
                Close = price
            };
        }

        // Act
        var kcLow = new KeltnerChannels_QC<HLC, KeltnerChannelsResult>(parameters);
        var lowOutputs = new KeltnerChannelsResult[lowVolData.Length];
        kcLow.OnBarBatch(lowVolData, lowOutputs);
        var lowVolWidth = lowOutputs[lowOutputs.Length - 1].UpperBand - 
                          lowOutputs[lowOutputs.Length - 1].LowerBand;
        
        var kcHigh = new KeltnerChannels_QC<HLC, KeltnerChannelsResult>(parameters);
        var highOutputs = new KeltnerChannelsResult[highVolData.Length];
        kcHigh.OnBarBatch(highVolData, highOutputs);
        var highVolWidth = highOutputs[highOutputs.Length - 1].UpperBand - 
                           highOutputs[highOutputs.Length - 1].LowerBand;

        // Assert
        Assert.True(highVolWidth > lowVolWidth, 
            $"High volatility width {highVolWidth} should be greater than low volatility width {lowVolWidth}");
    }

    [Fact]
    public void KeltnerChannels_PriceBreakouts()
    {
        // Arrange
        var parameters = new PKeltnerChannels<HLC, KeltnerChannelsResult> 
        { 
            Period = 20,
            Multiplier = 2.0,
            AtrPeriod = 10
        };
        var kc = new KeltnerChannels_QC<HLC, KeltnerChannelsResult>(parameters);
        
        // Data with trend and breakout
        var inputs = new HLC[40];
        for (int i = 0; i < 30; i++)
        {
            // Normal range
            var price = 100.0 + Math.Sin(i * 0.3) * 2;
            inputs[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }
        for (int i = 30; i < 40; i++)
        {
            // Breakout
            var price = 105.0 + (i - 30) * 2;
            inputs[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.4
            };
        }
        
        var outputs = new KeltnerChannelsResult[inputs.Length];

        // Act
        kc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(kc.IsReady);
        
        // Price should be within bands during consolidation
        var consolidationResult = outputs[25];
        Assert.True(inputs[25].Close >= consolidationResult.LowerBand);
        Assert.True(inputs[25].Close <= consolidationResult.UpperBand);
        
        // Price should break above upper band during breakout
        var breakoutResult = outputs[outputs.Length - 1];
        Assert.True(inputs[inputs.Length - 1].Close > breakoutResult.UpperBand || 
                   inputs[inputs.Length - 1].High > breakoutResult.UpperBand);
    }

    [Fact]
    public void KeltnerChannels_DifferentMultipliers()
    {
        var multipliers = new[] { 1.0, 2.0, 3.0 };
        
        // Create sample data
        var inputs = new HLC[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 3;
            inputs[i] = new HLC
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        var bandWidths = new Dictionary<double, double>();

        foreach (var multiplier in multipliers)
        {
            // Arrange
            var parameters = new PKeltnerChannels<HLC, KeltnerChannelsResult> 
            { 
                Period = 20,
                Multiplier = multiplier,
                AtrPeriod = 10
            };
            var kc = new KeltnerChannels_QC<HLC, KeltnerChannelsResult>(parameters);
            var outputs = new KeltnerChannelsResult[inputs.Length];

            // Act
            kc.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(kc.IsReady);
            var lastResult = outputs[outputs.Length - 1];
            bandWidths[multiplier] = lastResult.UpperBand - lastResult.LowerBand;
        }

        // Larger multipliers should create wider bands
        Assert.True(bandWidths[1.0] < bandWidths[2.0]);
        Assert.True(bandWidths[2.0] < bandWidths[3.0]);
    }

    [Fact]
    public void KeltnerChannels_DifferentPeriods()
    {
        var periods = new[] { 10, 20, 30 };
        
        // Create trending data
        var inputs = new HLC[50];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5 + Math.Sin(i * 0.3) * 2;
            inputs[i] = new HLC
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PKeltnerChannels<HLC, KeltnerChannelsResult> 
            { 
                Period = period,
                Multiplier = 2.0,
                AtrPeriod = Math.Min(10, period)
            };
            var kc = new KeltnerChannels_QC<HLC, KeltnerChannelsResult>(parameters);
            var outputs = new KeltnerChannelsResult[inputs.Length];

            // Act
            kc.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(kc.IsReady);
            var lastResult = outputs[outputs.Length - 1];
            
            // All components should be valid
            Assert.True(lastResult.UpperBand > 0);
            Assert.True(lastResult.MiddleBand > 0);
            Assert.True(lastResult.LowerBand > 0);
            Assert.True(lastResult.UpperBand > lastResult.LowerBand);
        }
    }

    [Fact]
    public void KeltnerChannels_MiddleBandAsEMA()
    {
        // Arrange
        var parameters = new PKeltnerChannels<HLC, KeltnerChannelsResult> 
        { 
            Period = 20,
            Multiplier = 2.0,
            AtrPeriod = 10
        };
        var kc = new KeltnerChannels_QC<HLC, KeltnerChannelsResult>(parameters);
        
        // Create data
        var inputs = new HLC[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }
        
        var outputs = new KeltnerChannelsResult[inputs.Length];

        // Act
        kc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(kc.IsReady);
        
        // Middle band should act as moving average
        var lastResult = outputs[outputs.Length - 1];
        Assert.True(lastResult.MiddleBand > 100); // Should be above starting price in uptrend
        Assert.True(lastResult.MiddleBand < inputs[inputs.Length - 1].Close + 5); // But not too far
    }
}

public class KeltnerChannelsResult
{
    public double UpperBand { get; set; }
    public double MiddleBand { get; set; }
    public double LowerBand { get; set; }
}