using LionFire.Trading.Indicators;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators.Native;
using Xunit;

namespace LionFire.Trading.Tests.Indicators;

public class StochasticTests
{
    [Fact]
    public void StochasticQC_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PStochastic<double, double>
        {
            FastPeriod = 14,
            SlowKPeriod = 3,
            SlowDPeriod = 3,
            OverboughtLevel = 80,
            OversoldLevel = 20
        };

        var indicator = new StochasticQC<double, double>(parameters);
        
        // Create sample HLC data
        var testData = new List<HLC<double>>();
        
        // Generate test data with a clear trend
        for (int i = 0; i < 20; i++)
        {
            double basePrice = 100 + i * 2; // Uptrend
            testData.Add(new HLC<double>
            {
                High = basePrice + 2,
                Low = basePrice - 2,
                Close = basePrice
            });
        }
        
        // Act
        double[]? output = new double[testData.Count * 2]; // 2 outputs per input (%K and %D)
        indicator.OnBarBatch(testData, output);
        
        // Assert
        Assert.True(indicator.IsReady);
        Assert.True(indicator.PercentK >= 0 && indicator.PercentK <= 100);
        Assert.True(indicator.PercentD >= 0 && indicator.PercentD <= 100);
        
        // In an uptrend, the stochastic should be high
        Assert.True(indicator.PercentK > 50);
    }

    [Fact]
    public void StochasticFP_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PStochastic<double, double>
        {
            FastPeriod = 14,
            SlowKPeriod = 3,
            SlowDPeriod = 3,
            OverboughtLevel = 80,
            OversoldLevel = 20
        };

        var indicator = new StochasticFP<double, double>(parameters);
        
        // Create sample HLC data
        var testData = new List<HLC<double>>();
        
        // Generate test data with a clear downtrend
        for (int i = 0; i < 20; i++)
        {
            double basePrice = 100 - i * 2; // Downtrend
            testData.Add(new HLC<double>
            {
                High = basePrice + 2,
                Low = basePrice - 2,
                Close = basePrice
            });
        }
        
        // Act
        double[]? output = new double[testData.Count * 2]; // 2 outputs per input (%K and %D)
        indicator.OnBarBatch(testData, output);
        
        // Assert
        Assert.True(indicator.IsReady);
        Assert.True(indicator.PercentK >= 0 && indicator.PercentK <= 100);
        Assert.True(indicator.PercentD >= 0 && indicator.PercentD <= 100);
        
        // In a downtrend, the stochastic should be low
        Assert.True(indicator.PercentK < 50);
    }

    [Fact]
    public void Stochastic_DefaultAlias_Works()
    {
        // Arrange
        var parameters = new PStochastic<double, double>
        {
            FastPeriod = 5,
            SlowKPeriod = 3,
            SlowDPeriod = 3
        };

        // Default Stochastic should point to StochasticQC
        var indicator = new Stochastic<double, double>(parameters);
        
        // Create sample HLC data
        var testData = new List<HLC<double>>
        {
            new HLC<double> { High = 110, Low = 90, Close = 100 },
            new HLC<double> { High = 112, Low = 92, Close = 105 },
            new HLC<double> { High = 115, Low = 95, Close = 110 },
            new HLC<double> { High = 118, Low = 98, Close = 115 },
            new HLC<double> { High = 120, Low = 100, Close = 118 },
            new HLC<double> { High = 119, Low = 99, Close = 117 },
            new HLC<double> { High = 118, Low = 98, Close = 116 },
            new HLC<double> { High = 117, Low = 97, Close = 115 },
        };
        
        // Act
        double[]? output = new double[testData.Count * 2];
        indicator.OnBarBatch(testData, output);
        
        // Assert
        Assert.True(indicator.IsReady);
        Assert.NotEqual(0, indicator.PercentK);
        Assert.NotEqual(0, indicator.PercentD);
    }

    [Fact]
    public void Stochastic_OverboughtOversold_Detection()
    {
        // Arrange
        var parameters = new PStochastic<double, double>
        {
            FastPeriod = 5,
            SlowKPeriod = 1, // No smoothing for simplicity
            SlowDPeriod = 1,
            OverboughtLevel = 80,
            OversoldLevel = 20
        };

        var indicator = new StochasticQC<double, double>(parameters);
        
        // Create data that will result in overbought condition
        var overboughtData = new List<HLC<double>>();
        for (int i = 0; i < 10; i++)
        {
            overboughtData.Add(new HLC<double>
            {
                High = 100 + i,
                Low = 95 + i,
                Close = 99 + i // Close near the high
            });
        }
        
        // Act
        double[]? output = new double[overboughtData.Count * 2];
        indicator.OnBarBatch(overboughtData, output);
        
        // Assert - should be overbought
        Assert.True(indicator.IsReady);
        Assert.True(indicator.PercentK > 80, $"Expected %K > 80, but got {indicator.PercentK}");
        Assert.True(indicator.IsOverbought);
        Assert.False(indicator.IsOversold);
        
        // Now test oversold condition
        indicator.Clear();
        var oversoldData = new List<HLC<double>>();
        for (int i = 0; i < 10; i++)
        {
            oversoldData.Add(new HLC<double>
            {
                High = 100 - i,
                Low = 95 - i,
                Close = 96 - i // Close near the low
            });
        }
        
        output = new double[oversoldData.Count * 2];
        indicator.OnBarBatch(oversoldData, output);
        
        // Assert - should be oversold
        Assert.True(indicator.IsReady);
        Assert.True(indicator.PercentK < 20, $"Expected %K < 20, but got {indicator.PercentK}");
        Assert.False(indicator.IsOverbought);
        Assert.True(indicator.IsOversold);
    }
}