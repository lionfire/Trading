using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class DonchianChannelsTests
{
    [Fact]
    public void DonchianChannels_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PDonchianChannels<double, double> { Period = 10 };
        var dc = new DonchianChannels_FP<double, double>(parameters);

        // Sample data with known highs and lows
        var inputs = new HLC<double>[]
        {
            new() { High = 10, Low = 5, Close = 8 },
            new() { High = 12, Low = 6, Close = 10 },
            new() { High = 15, Low = 8, Close = 12 },  // Highest high = 15
            new() { High = 11, Low = 4, Close = 7 },   // Lowest low = 4
            new() { High = 13, Low = 7, Close = 11 },
            new() { High = 14, Low = 9, Close = 12 },
            new() { High = 10, Low = 6, Close = 8 },
            new() { High = 11, Low = 5, Close = 9 },
            new() { High = 12, Low = 7, Close = 10 },
            new() { High = 13, Low = 8, Close = 11 },
        };

        var outputs = new double[inputs.Length * 3]; // Upper, Lower, Middle

        // Act
        dc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(dc.IsReady);

        // Upper band should be the highest high
        Assert.Equal(15, dc.UpperChannel);
        // Lower band should be the lowest low
        Assert.Equal(4, dc.LowerChannel);
        // Middle band should be average of upper and lower
        Assert.Equal(9.5, dc.MiddleChannel);
    }

    [Fact]
    public void DonchianChannels_TracksBreakouts()
    {
        // Arrange
        var parameters = new PDonchianChannels<double, double> { Period = 10 };
        var dc = new DonchianChannels_FP<double, double>(parameters);

        // Create data with consolidation then breakout
        var inputs = new HLC<double>[30];
        for (int i = 0; i < 20; i++)
        {
            // Consolidation between 95-105
            var price = 100.0 + Math.Sin(i * 0.5) * 5;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }
        for (int i = 20; i < 30; i++)
        {
            // Breakout above 105
            var price = 106.0 + (i - 20) * 2;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 0.5,
                Close = price
            };
        }

        // Act
        dc.OnBarBatch(inputs, new double[inputs.Length * 3]);

        // Assert
        Assert.True(dc.IsReady);

        // After breakout, upper band should have expanded
        Assert.True(dc.UpperChannel > 110);

        // Price should be near upper band during breakout
        Assert.True(inputs[^1].High >= dc.UpperChannel - 2);
    }

    [Fact]
    public void DonchianChannels_DifferentPeriods()
    {
        var periods = new[] { 10, 20, 30 };

        // Create volatile data
        var inputs = new HLC<double>[40];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            var basePrice = 100.0 + Math.Sin(i * 0.2) * 10;
            var volatility = random.NextDouble() * 5;
            inputs[i] = new HLC<double>
            {
                High = basePrice + volatility,
                Low = basePrice - volatility,
                Close = basePrice
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PDonchianChannels<double, double> { Period = period };
            var dc = new DonchianChannels_FP<double, double>(parameters);

            // Act
            dc.OnBarBatch(inputs, new double[inputs.Length * 3]);

            // Assert
            Assert.True(dc.IsReady);

            // Bands should be valid
            Assert.True(dc.UpperChannel > dc.LowerChannel);
            Assert.Equal((dc.UpperChannel + dc.LowerChannel) / 2, dc.MiddleChannel, 2);
        }
    }

    [Fact]
    public void DonchianChannels_TrendDetection()
    {
        // Arrange
        var parameters = new PDonchianChannels<double, double> { Period = 10 };

        // Uptrend data
        var uptrend = new HLC<double>[30];
        for (int i = 0; i < uptrend.Length; i++)
        {
            var price = 100.0 + i * 1.0;
            uptrend[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price
            };
        }

        // Downtrend data
        var downtrend = new HLC<double>[30];
        for (int i = 0; i < downtrend.Length; i++)
        {
            var price = 100.0 - i * 1.0;
            downtrend[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.5,
                Close = price
            };
        }

        // Act
        var dcUp = new DonchianChannels_FP<double, double>(parameters);
        dcUp.OnBarBatch(uptrend, new double[uptrend.Length * 3]);
        var upUpperFinal = dcUp.UpperChannel;

        var dcDown = new DonchianChannels_FP<double, double>(parameters);
        dcDown.OnBarBatch(downtrend, new double[downtrend.Length * 3]);
        var downLowerFinal = dcDown.LowerChannel;

        // Assert
        // In uptrend, upper band should be near the latest highs
        Assert.True(upUpperFinal > 120);

        // In downtrend, lower band should be near the latest lows
        Assert.True(downLowerFinal < 80);
    }

    [Fact]
    public void DonchianChannels_RangeMarket()
    {
        // Arrange
        var parameters = new PDonchianChannels<double, double> { Period = 20 };
        var dc = new DonchianChannels_FP<double, double>(parameters);

        // Range-bound data
        var inputs = new HLC<double>[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            // Oscillating between 95 and 105
            var price = 100.0 + Math.Sin(i * 0.3) * 5;
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }

        // Act
        dc.OnBarBatch(inputs, new double[inputs.Length * 3]);

        // Assert
        Assert.True(dc.IsReady);

        // In range market, bands should contain the oscillation
        Assert.True(dc.UpperChannel >= 104);
        Assert.True(dc.LowerChannel <= 96);

        // Width should be reasonable
        var width = dc.UpperChannel - dc.LowerChannel;
        Assert.True(width > 8 && width < 15);
    }

    [Fact]
    public void DonchianChannels_ChannelWidth()
    {
        // Arrange
        var parameters = new PDonchianChannels<double, double> { Period = 10 };
        var dc = new DonchianChannels_FP<double, double>(parameters);

        var inputs = new HLC<double>[20];
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

        // Act
        dc.OnBarBatch(inputs, new double[inputs.Length * 3]);

        // Assert
        Assert.True(dc.IsReady);

        // Channel width should equal upper - lower
        Assert.Equal(dc.UpperChannel - dc.LowerChannel, dc.ChannelWidth);
    }

    [Fact]
    public void DonchianChannels_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PDonchianChannels<double, double> { Period = 10 };
        var dc = new DonchianChannels_FP<double, double>(parameters);

        var inputs = new HLC<double>[15];
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

        dc.OnBarBatch(inputs, new double[inputs.Length * 3]);
        Assert.True(dc.IsReady);

        // Act
        dc.Clear();

        // Assert
        Assert.False(dc.IsReady);
    }
}
