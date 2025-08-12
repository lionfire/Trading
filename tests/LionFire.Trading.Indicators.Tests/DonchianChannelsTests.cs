using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class DonchianChannelsTests
{
    [Fact]
    public void DonchianChannels_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PDonchianChannels<HL, DonchianChannelsResult> 
        { 
            UpperPeriod = 20,
            LowerPeriod = 20
        };
        var dc = new DonchianChannels_QC<HL, DonchianChannelsResult>(parameters);
        
        // Sample data with known highs and lows
        var inputs = new HL[]
        {
            new() { High = 10, Low = 5 },
            new() { High = 12, Low = 6 },
            new() { High = 15, Low = 8 },  // Highest high = 15
            new() { High = 11, Low = 4 },   // Lowest low = 4
            new() { High = 13, Low = 7 },
            new() { High = 14, Low = 9 },
            new() { High = 10, Low = 6 },
            new() { High = 11, Low = 5 },
            new() { High = 12, Low = 7 },
            new() { High = 13, Low = 8 },
        };
        
        var outputs = new DonchianChannelsResult[inputs.Length];

        // Act
        dc.OnBarBatch(inputs, outputs);

        // Assert
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // Upper band should be the highest high
        Assert.Equal(15, lastResult.UpperBand);
        // Lower band should be the lowest low
        Assert.Equal(4, lastResult.LowerBand);
        // Middle band should be average of upper and lower
        Assert.Equal(9.5, lastResult.MiddleBand);
    }

    [Fact]
    public void DonchianChannels_TracksBreakouts()
    {
        // Arrange
        var parameters = new PDonchianChannels<HL, DonchianChannelsResult> 
        { 
            UpperPeriod = 10,
            LowerPeriod = 10
        };
        var dc = new DonchianChannels_QC<HL, DonchianChannelsResult>(parameters);
        
        // Create data with consolidation then breakout
        var inputs = new HL[30];
        for (int i = 0; i < 20; i++)
        {
            // Consolidation between 95-105
            var price = 100.0 + Math.Sin(i * 0.5) * 5;
            inputs[i] = new HL
            {
                High = price + 1,
                Low = price - 1
            };
        }
        for (int i = 20; i < 30; i++)
        {
            // Breakout above 105
            var price = 106.0 + (i - 20) * 2;
            inputs[i] = new HL
            {
                High = price + 1,
                Low = price - 0.5
            };
        }
        
        var outputs = new DonchianChannelsResult[inputs.Length];

        // Act
        dc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(dc.IsReady);
        
        // During consolidation, bands should be relatively tight
        var consolidationResult = outputs[19];
        var consolidationWidth = consolidationResult.UpperBand - consolidationResult.LowerBand;
        
        // After breakout, upper band should expand
        var breakoutResult = outputs[outputs.Length - 1];
        Assert.True(breakoutResult.UpperBand > consolidationResult.UpperBand);
        
        // Price should be near upper band during breakout
        Assert.True(inputs[inputs.Length - 1].High >= breakoutResult.UpperBand - 2);
    }

    [Fact]
    public void DonchianChannels_DifferentPeriods()
    {
        var upperPeriods = new[] { 10, 20, 30 };
        var lowerPeriods = new[] { 10, 20, 30 };
        
        // Create volatile data
        var inputs = new HL[40];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            var basePrice = 100.0 + Math.Sin(i * 0.2) * 10;
            var volatility = random.NextDouble() * 5;
            inputs[i] = new HL
            {
                High = basePrice + volatility,
                Low = basePrice - volatility
            };
        }

        foreach (var period in upperPeriods)
        {
            // Arrange
            var parameters = new PDonchianChannels<HL, DonchianChannelsResult> 
            { 
                UpperPeriod = period,
                LowerPeriod = period
            };
            var dc = new DonchianChannels_QC<HL, DonchianChannelsResult>(parameters);
            var outputs = new DonchianChannelsResult[inputs.Length];

            // Act
            dc.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(dc.IsReady);
            var lastResult = outputs[outputs.Length - 1];
            
            // Bands should be valid
            Assert.True(lastResult.UpperBand > lastResult.LowerBand);
            Assert.Equal((lastResult.UpperBand + lastResult.LowerBand) / 2, lastResult.MiddleBand, 2);
        }
    }

    [Fact]
    public void DonchianChannels_AsymmetricPeriods()
    {
        // Arrange - Different periods for upper and lower bands
        var parameters = new PDonchianChannels<HL, DonchianChannelsResult> 
        { 
            UpperPeriod = 20,
            LowerPeriod = 10
        };
        var dc = new DonchianChannels_QC<HL, DonchianChannelsResult>(parameters);
        
        // Create trending data
        var inputs = new HL[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5; // Uptrend
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }
        
        var outputs = new DonchianChannelsResult[inputs.Length];

        // Act
        dc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(dc.IsReady);
        var lastResult = outputs[outputs.Length - 1];
        
        // With asymmetric periods, middle band won't be exactly centered
        Assert.True(lastResult.UpperBand > lastResult.LowerBand);
        Assert.True(lastResult.MiddleBand > 0);
    }

    [Fact]
    public void DonchianChannels_TrendDetection()
    {
        // Arrange
        var parameters = new PDonchianChannels<HL, DonchianChannelsResult> 
        { 
            UpperPeriod = 20,
            LowerPeriod = 20
        };
        
        // Uptrend data
        var uptrend = new HL[30];
        for (int i = 0; i < uptrend.Length; i++)
        {
            var price = 100.0 + i * 1.0;
            uptrend[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.3
            };
        }
        
        // Downtrend data
        var downtrend = new HL[30];
        for (int i = 0; i < downtrend.Length; i++)
        {
            var price = 100.0 - i * 1.0;
            downtrend[i] = new HL
            {
                High = price + 0.3,
                Low = price - 0.5
            };
        }

        // Act
        var dcUp = new DonchianChannels_QC<HL, DonchianChannelsResult>(parameters);
        var upOutputs = new DonchianChannelsResult[uptrend.Length];
        dcUp.OnBarBatch(uptrend, upOutputs);
        
        var dcDown = new DonchianChannels_QC<HL, DonchianChannelsResult>(parameters);
        var downOutputs = new DonchianChannelsResult[downtrend.Length];
        dcDown.OnBarBatch(downtrend, downOutputs);

        // Assert
        // In uptrend, bands should be rising
        var upResult1 = upOutputs[20];
        var upResult2 = upOutputs[29];
        Assert.True(upResult2.UpperBand > upResult1.UpperBand);
        Assert.True(upResult2.LowerBand > upResult1.LowerBand);
        
        // In downtrend, bands should be falling
        var downResult1 = downOutputs[20];
        var downResult2 = downOutputs[29];
        Assert.True(downResult2.UpperBand < downResult1.UpperBand);
        Assert.True(downResult2.LowerBand < downResult1.LowerBand);
    }

    [Fact]
    public void DonchianChannels_RangeMarket()
    {
        // Arrange
        var parameters = new PDonchianChannels<HL, DonchianChannelsResult> 
        { 
            UpperPeriod = 20,
            LowerPeriod = 20
        };
        var dc = new DonchianChannels_QC<HL, DonchianChannelsResult>(parameters);
        
        // Range-bound data
        var inputs = new HL[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            // Oscillating between 95 and 105
            var price = 100.0 + Math.Sin(i * 0.3) * 5;
            inputs[i] = new HL
            {
                High = price + 0.5,
                Low = price - 0.5
            };
        }
        
        var outputs = new DonchianChannelsResult[inputs.Length];

        // Act
        dc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(dc.IsReady);
        
        // In range market, bands should stabilize
        var result1 = outputs[25];
        var result2 = outputs[35];
        
        // Bands should be relatively stable
        Assert.True(Math.Abs(result1.UpperBand - result2.UpperBand) < 3);
        Assert.True(Math.Abs(result1.LowerBand - result2.LowerBand) < 3);
        
        // Width should be consistent
        var width1 = result1.UpperBand - result1.LowerBand;
        var width2 = result2.UpperBand - result2.LowerBand;
        Assert.True(Math.Abs(width1 - width2) < 3);
    }
}

public class DonchianChannelsResult
{
    public double UpperBand { get; set; }
    public double MiddleBand { get; set; }
    public double LowerBand { get; set; }
}