using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class IchimokuCloudTests
{
    [Fact]
    public void IchimokuCloud_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PIchimokuCloud<double, double>
        {
            ConversionLinePeriod = 9,
            BaseLinePeriod = 26,
            LeadingSpanBPeriod = 52,
            Displacement = 26
        };
        var ichimoku = new IchimokuCloud_FP<double, double>(parameters);

        // Create sample HLC data
        var inputs = new HLC<double>[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.1) * 10;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        var outputs = new double[inputs.Length];

        // Act
        ichimoku.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(ichimoku.IsReady);

        // Tenkan-sen should be calculated after ConversionLinePeriod
        Assert.NotEqual(0, ichimoku.TenkanSen);

        // Kijun-sen should be calculated after BaseLinePeriod
        Assert.NotEqual(0, ichimoku.KijunSen);
    }

    [Fact]
    public void IchimokuCloud_TenkanKijunCross()
    {
        // Arrange
        var parameters = new PIchimokuCloud<double, double>
        {
            ConversionLinePeriod = 9,
            BaseLinePeriod = 26,
            LeadingSpanBPeriod = 52,
            Displacement = 26
        };

        // Strong uptrend data
        var uptrend = new HLC<double>[60];
        for (int i = 0; i < uptrend.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            uptrend[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.2
            };
        }

        // Act
        var ichimokuUp = new IchimokuCloud_FP<double, double>(parameters);
        ichimokuUp.OnBarBatch(uptrend, new double[uptrend.Length]);

        // Assert
        Assert.True(ichimokuUp.IsReady);

        // In uptrend, Tenkan-sen (fast) should be above Kijun-sen (slow)
        Assert.True(ichimokuUp.TenkanSen >= ichimokuUp.KijunSen,
            $"Tenkan-sen ({ichimokuUp.TenkanSen}) should be >= Kijun-sen ({ichimokuUp.KijunSen}) in uptrend");
    }

    [Fact]
    public void IchimokuCloud_CloudFormation()
    {
        // Arrange
        var parameters = new PIchimokuCloud<double, double>
        {
            ConversionLinePeriod = 9,
            BaseLinePeriod = 26,
            LeadingSpanBPeriod = 52,
            Displacement = 26
        };
        var ichimoku = new IchimokuCloud_FP<double, double>(parameters);

        // Trending data
        var inputs = new HLC<double>[80];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.3;
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.1
            };
        }

        // Act
        ichimoku.OnBarBatch(inputs, new double[inputs.Length]);

        // Assert
        Assert.True(ichimoku.IsReady);

        // Cloud is formed by Senkou Span A and B
        // Both should have values
        Assert.NotEqual(0, ichimoku.SenkouSpanA);
        Assert.NotEqual(0, ichimoku.SenkouSpanB);
    }

    [Fact]
    public void IchimokuCloud_ChikouSpan()
    {
        // Arrange
        var parameters = new PIchimokuCloud<double, double>
        {
            ConversionLinePeriod = 9,
            BaseLinePeriod = 26,
            LeadingSpanBPeriod = 52,
            Displacement = 26
        };
        var ichimoku = new IchimokuCloud_FP<double, double>(parameters);

        // Create data
        var inputs = new HLC<double>[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price
            };
        }

        // Act
        ichimoku.OnBarBatch(inputs, new double[inputs.Length]);

        // Assert
        Assert.True(ichimoku.IsReady);

        // Chikou Span should equal the current close (plotted 26 periods back)
        Assert.NotEqual(0, ichimoku.ChikouSpan);
    }

    [Fact]
    public void IchimokuCloud_DifferentPeriods()
    {
        var periodSets = new[]
        {
            (9, 26, 52, 26),    // Standard
            (7, 22, 44, 22),    // Crypto-optimized
            (9, 30, 60, 30)     // Extended
        };

        // Create sample data
        var inputs = new HLC<double>[80];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.1) * 10;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        foreach (var (tenkan, kijun, senkouB, displacement) in periodSets)
        {
            // Arrange
            var parameters = new PIchimokuCloud<double, double>
            {
                ConversionLinePeriod = tenkan,
                BaseLinePeriod = kijun,
                LeadingSpanBPeriod = senkouB,
                Displacement = displacement
            };
            var ichimoku = new IchimokuCloud_FP<double, double>(parameters);

            // Act
            ichimoku.OnBarBatch(inputs, new double[inputs.Length]);

            // Assert
            Assert.True(ichimoku.IsReady);
            Assert.NotEqual(0, ichimoku.TenkanSen);
            Assert.NotEqual(0, ichimoku.KijunSen);
        }
    }

    [Fact]
    public void IchimokuCloud_TrendIdentification()
    {
        // Arrange
        var parameters = new PIchimokuCloud<double, double>
        {
            ConversionLinePeriod = 9,
            BaseLinePeriod = 26,
            LeadingSpanBPeriod = 52,
            Displacement = 26
        };

        // Strong uptrend
        var uptrendInputs = new HLC<double>[80];
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

        // Strong downtrend
        var downtrendInputs = new HLC<double>[80];
        for (int i = 0; i < downtrendInputs.Length; i++)
        {
            var price = 200.0 - i * 1.0;
            downtrendInputs[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.5,
                Close = price - 0.2
            };
        }

        // Act
        var ichimokuUp = new IchimokuCloud_FP<double, double>(parameters);
        ichimokuUp.OnBarBatch(uptrendInputs, new double[uptrendInputs.Length]);
        var upTenkan = ichimokuUp.TenkanSen;
        var upKijun = ichimokuUp.KijunSen;

        var ichimokuDown = new IchimokuCloud_FP<double, double>(parameters);
        ichimokuDown.OnBarBatch(downtrendInputs, new double[downtrendInputs.Length]);
        var downTenkan = ichimokuDown.TenkanSen;
        var downKijun = ichimokuDown.KijunSen;

        // Assert
        // In uptrend, Tenkan should be above or equal to Kijun
        Assert.True(upTenkan >= upKijun,
            $"Uptrend: Tenkan ({upTenkan}) should be >= Kijun ({upKijun})");

        // In downtrend, Tenkan should be below or equal to Kijun
        Assert.True(downTenkan <= downKijun,
            $"Downtrend: Tenkan ({downTenkan}) should be <= Kijun ({downKijun})");
    }

    [Fact]
    public void IchimokuCloud_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PIchimokuCloud<double, double>
        {
            ConversionLinePeriod = 9,
            BaseLinePeriod = 26,
            LeadingSpanBPeriod = 52,
            Displacement = 26
        };
        var ichimoku = new IchimokuCloud_FP<double, double>(parameters);

        var inputs = new HLC<double>[60];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price
            };
        }

        ichimoku.OnBarBatch(inputs, new double[inputs.Length]);
        Assert.True(ichimoku.IsReady);

        // Act
        ichimoku.Clear();

        // Assert
        Assert.False(ichimoku.IsReady);
        Assert.Equal(0, ichimoku.TenkanSen);
        Assert.Equal(0, ichimoku.KijunSen);
    }
}
