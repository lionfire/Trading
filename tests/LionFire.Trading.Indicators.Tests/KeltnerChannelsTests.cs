using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class KeltnerChannelsTests
{
    [Fact]
    public void KeltnerChannels_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PKeltnerChannels<HLC<double>, double>
        {
            Period = 20,
            AtrMultiplier = 2.0,
            AtrPeriod = 10
        };
        var kc = new KeltnerChannelsHLC_FP<double, double>(parameters);

        // Sample data
        var inputs = new HLC<double>[30];
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

        var outputs = new double[inputs.Length * 3]; // Upper, Middle, Lower

        // Act
        kc.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(kc.IsReady);

        // Upper band should be above middle
        Assert.True(kc.UpperBand > kc.MiddleLine);
        // Lower band should be below middle
        Assert.True(kc.LowerBand < kc.MiddleLine);
    }

    [Fact]
    public void KeltnerChannels_ExpandsWithVolatility()
    {
        // Arrange
        var parameters = new PKeltnerChannels<HLC<double>, double>
        {
            Period = 20,
            AtrMultiplier = 2.0,
            AtrPeriod = 10
        };

        // Low volatility data
        var lowVolData = new HLC<double>[30];
        for (int i = 0; i < lowVolData.Length; i++)
        {
            var price = 100.0 + i * 0.1; // Small changes
            lowVolData[i] = new HLC<double>
            {
                High = price + 0.2,
                Low = price - 0.2,
                Close = price
            };
        }

        // High volatility data
        var highVolData = new HLC<double>[30];
        for (int i = 0; i < highVolData.Length; i++)
        {
            var price = 100.0 + (i % 2 == 0 ? 5 : -5); // Large swings
            highVolData[i] = new HLC<double>
            {
                High = price + 2,
                Low = price - 2,
                Close = price
            };
        }

        // Act
        var kcLow = new KeltnerChannelsHLC_FP<double, double>(parameters);
        kcLow.OnBarBatch(lowVolData, new double[lowVolData.Length * 3]);
        var lowVolWidth = kcLow.UpperBand - kcLow.LowerBand;

        var kcHigh = new KeltnerChannelsHLC_FP<double, double>(parameters);
        kcHigh.OnBarBatch(highVolData, new double[highVolData.Length * 3]);
        var highVolWidth = kcHigh.UpperBand - kcHigh.LowerBand;

        // Assert
        Assert.True(highVolWidth > lowVolWidth,
            $"High volatility width {highVolWidth} should be greater than low volatility width {lowVolWidth}");
    }

    [Fact]
    public void KeltnerChannels_PriceBreakouts()
    {
        // Arrange
        var parameters = new PKeltnerChannels<HLC<double>, double>
        {
            Period = 20,
            AtrMultiplier = 2.0,
            AtrPeriod = 10
        };
        var kc = new KeltnerChannelsHLC_FP<double, double>(parameters);

        // Data with trend and breakout
        var inputs = new HLC<double>[40];
        for (int i = 0; i < 30; i++)
        {
            // Normal range
            var price = 100.0 + Math.Sin(i * 0.3) * 2;
            inputs[i] = new HLC<double>
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
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.4
            };
        }

        // Act
        kc.OnBarBatch(inputs, new double[inputs.Length * 3]);

        // Assert
        Assert.True(kc.IsReady);

        // Price should break above upper band during breakout
        Assert.True(inputs[^1].Close > kc.UpperBand || inputs[^1].High > kc.UpperBand);
    }

    [Fact]
    public void KeltnerChannels_DifferentMultipliers()
    {
        var multipliers = new[] { 1.0, 2.0, 3.0 };

        // Create sample data
        var inputs = new HLC<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 3;
            inputs[i] = new HLC<double>
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
            var parameters = new PKeltnerChannels<HLC<double>, double>
            {
                Period = 20,
                AtrMultiplier = multiplier,
                AtrPeriod = 10
            };
            var kc = new KeltnerChannelsHLC_FP<double, double>(parameters);

            // Act
            kc.OnBarBatch(inputs, new double[inputs.Length * 3]);

            // Assert
            Assert.True(kc.IsReady);
            bandWidths[multiplier] = kc.UpperBand - kc.LowerBand;
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
        var inputs = new HLC<double>[50];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5 + Math.Sin(i * 0.3) * 2;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PKeltnerChannels<HLC<double>, double>
            {
                Period = period,
                AtrMultiplier = 2.0,
                AtrPeriod = Math.Min(10, period)
            };
            var kc = new KeltnerChannelsHLC_FP<double, double>(parameters);

            // Act
            kc.OnBarBatch(inputs, new double[inputs.Length * 3]);

            // Assert
            Assert.True(kc.IsReady);

            // All components should be valid
            Assert.False(double.IsNaN(kc.UpperBand));
            Assert.False(double.IsNaN(kc.MiddleLine));
            Assert.False(double.IsNaN(kc.LowerBand));
            Assert.True(kc.UpperBand > kc.LowerBand);
        }
    }

    [Fact]
    public void KeltnerChannels_MiddleBandAsEMA()
    {
        // Arrange
        var parameters = new PKeltnerChannels<HLC<double>, double>
        {
            Period = 20,
            AtrMultiplier = 2.0,
            AtrPeriod = 10
        };
        var kc = new KeltnerChannelsHLC_FP<double, double>(parameters);

        // Create data
        var inputs = new HLC<double>[30];
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

        // Act
        kc.OnBarBatch(inputs, new double[inputs.Length * 3]);

        // Assert
        Assert.True(kc.IsReady);

        // Middle band should act as moving average
        Assert.True(kc.MiddleLine > 100); // Should be above starting price in uptrend
        Assert.True(kc.MiddleLine < inputs[^1].Close + 5); // But not too far
    }

    [Fact]
    public void KeltnerChannels_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PKeltnerChannels<HLC<double>, double>
        {
            Period = 10,
            AtrMultiplier = 2.0,
            AtrPeriod = 10
        };
        var kc = new KeltnerChannelsHLC_FP<double, double>(parameters);

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

        kc.OnBarBatch(inputs, new double[inputs.Length * 3]);
        Assert.True(kc.IsReady);

        // Act
        kc.Clear();

        // Assert
        Assert.False(kc.IsReady);
    }
}
