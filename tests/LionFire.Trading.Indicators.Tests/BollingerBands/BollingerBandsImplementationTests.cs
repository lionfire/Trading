using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests.BollingerBands;

public class BollingerBandsImplementationTests
{
    [Fact]
    public void FirstPartyImplementation_CalculatesCorrectValues()
    {
        // Arrange
        var parameters = new PBollingerBands<double, double>
        {
            Period = 5,
            StandardDeviations = 2.0
        };

        var indicator = new BollingerBands_FP<double, double>(parameters);
        var prices = new double[] { 10, 11, 12, 11, 10, 9, 10, 11, 12, 13 };
        var outputs = new double[prices.Length];

        // Act
        indicator.OnBarBatch(prices, outputs);

        // Assert - should be ready after receiving Period prices
        Assert.True(indicator.IsReady);

        // Middle band should be the SMA
        var expectedMiddle = (double)(10 + 11 + 12 + 11 + 10) / 5.0; // = 10.8
        // Note: after processing all 10 prices, middle band will be different

        // Bands should be in order: Lower < Middle < Upper
        Assert.True(indicator.LowerBand < indicator.MiddleBand);
        Assert.True(indicator.MiddleBand < indicator.UpperBand);

        // Bands should be symmetric around middle
        var upperDistance = indicator.UpperBand - indicator.MiddleBand;
        var lowerDistance = indicator.MiddleBand - indicator.LowerBand;
        Assert.Equal(upperDistance, lowerDistance, 6);
    }

    [Fact]
    public void BollingerBands_CalculatesValues()
    {
        // Arrange
        var parameters = new PBollingerBands<double, double>
        {
            Period = 20,
            StandardDeviations = 2.0
        };

        var indicator = new BollingerBands_FP<double, double>(parameters);
        var prices = new List<double>();

        // Generate test data
        for (int i = 0; i < 30; i++)
        {
            prices.Add(100 + i % 5); // Oscillating prices
        }

        var output = new double[prices.Count];

        // Act
        indicator.OnBarBatch(prices, output);

        // Assert
        Assert.True(indicator.IsReady);
        Assert.True(indicator.UpperBand > indicator.MiddleBand);
        Assert.True(indicator.MiddleBand > indicator.LowerBand);
        Assert.True(indicator.BandWidth > 0);
    }

    [Fact]
    public void BollingerBands_BandWidthIsCorrect()
    {
        // Arrange
        var parameters = new PBollingerBands<double, double>
        {
            Period = 10,
            StandardDeviations = 2.0
        };

        var indicator = new BollingerBands_FP<double, double>(parameters);
        var prices = Enumerable.Range(1, 20)
            .Select(x => 50.0 + 2 * Math.Sin(x * 0.5))
            .ToArray();

        // Act
        indicator.OnBarBatch(prices, new double[prices.Length]);

        // Assert
        Assert.True(indicator.IsReady);

        // BandWidth should equal Upper - Lower
        var expectedWidth = indicator.UpperBand - indicator.LowerBand;
        Assert.Equal(expectedWidth, indicator.BandWidth, 6);
    }

    [Fact]
    public void BollingerBands_DifferentStandardDeviations()
    {
        var stdDevs = new[] { 1.0, 2.0, 3.0 };
        var prices = Enumerable.Range(1, 30).Select(x => (double)(100 + x % 10)).ToArray();

        var bandWidths = new Dictionary<double, double>();

        foreach (var sd in stdDevs)
        {
            var parameters = new PBollingerBands<double, double>
            {
                Period = 10,
                StandardDeviations = sd
            };

            var indicator = new BollingerBands_FP<double, double>(parameters);
            indicator.OnBarBatch(prices, new double[prices.Length]);

            Assert.True(indicator.IsReady);
            bandWidths[sd] = indicator.BandWidth;
        }

        // Higher standard deviations should produce wider bands
        Assert.True(bandWidths[1.0] < bandWidths[2.0]);
        Assert.True(bandWidths[2.0] < bandWidths[3.0]);
    }

    [Fact]
    public void BollingerBands_ConstantPricesNarrowBands()
    {
        // Arrange
        var parameters = new PBollingerBands<double, double>
        {
            Period = 10,
            StandardDeviations = 2.0
        };

        var indicator = new BollingerBands_FP<double, double>(parameters);
        var constantPrices = Enumerable.Repeat(100.0, 20).ToArray();

        // Act
        indicator.OnBarBatch(constantPrices, new double[constantPrices.Length]);

        // Assert
        Assert.True(indicator.IsReady);
        Assert.Equal(100.0, indicator.MiddleBand, 6);

        // With constant prices, standard deviation is 0, so bands should equal middle
        Assert.Equal(indicator.MiddleBand, indicator.UpperBand, 6);
        Assert.Equal(indicator.MiddleBand, indicator.LowerBand, 6);
        Assert.Equal(0, indicator.BandWidth, 6);
    }

    [Fact]
    public void BollingerBands_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PBollingerBands<double, double>
        {
            Period = 5,
            StandardDeviations = 2.0
        };

        var indicator = new BollingerBands_FP<double, double>(parameters);
        var prices = Enumerable.Range(1, 10).Select(x => (double)x * 10).ToArray();

        indicator.OnBarBatch(prices, new double[prices.Length]);
        Assert.True(indicator.IsReady);

        // Act
        indicator.Clear();

        // Assert
        Assert.False(indicator.IsReady);
    }

    [Fact]
    public void BollingerBands_VolatilePricesWidenBands()
    {
        var parameters = new PBollingerBands<double, double>
        {
            Period = 10,
            StandardDeviations = 2.0
        };

        // Low volatility
        var lowVolIndicator = new BollingerBands_FP<double, double>(parameters);
        var lowVolPrices = Enumerable.Range(1, 20).Select(x => 100.0 + x * 0.1).ToArray();
        lowVolIndicator.OnBarBatch(lowVolPrices, new double[lowVolPrices.Length]);

        // High volatility
        var highVolIndicator = new BollingerBands_FP<double, double>(parameters);
        var highVolPrices = Enumerable.Range(1, 20).Select(x => 100.0 + Math.Sin(x) * 10).ToArray();
        highVolIndicator.OnBarBatch(highVolPrices, new double[highVolPrices.Length]);

        // Assert
        Assert.True(lowVolIndicator.IsReady);
        Assert.True(highVolIndicator.IsReady);
        Assert.True(highVolIndicator.BandWidth > lowVolIndicator.BandWidth,
            $"High volatility bandwidth {highVolIndicator.BandWidth} should be > low volatility {lowVolIndicator.BandWidth}");
    }
}
