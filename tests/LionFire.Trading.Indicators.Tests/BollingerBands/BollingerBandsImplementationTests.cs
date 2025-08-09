using Xunit;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.QuantConnect_;
using System.Collections.Generic;

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
        
        var indicator = new BollingerBandsFP<double, double>(parameters);
        var prices = new List<double> { 10, 11, 12, 11, 10, 9, 10, 11, 12, 13 };
        var output = new double[prices.Count * 3]; // 3 values per price: Upper, Middle, Lower
        
        // Act
        indicator.OnBarBatch(prices, output);
        
        // Assert
        Assert.False(indicator.IsReady); // Not ready after first 4 values
        
        // After 5 values, should be ready
        for (int i = 0; i < 5; i++)
        {
            indicator.OnNext(prices[i]);
        }
        Assert.True(indicator.IsReady);
        
        // Middle band should be the SMA
        var expectedMiddle = (10 + 11 + 12 + 11 + 10) / 5.0; // = 10.8
        Assert.Equal(10.8, indicator.MiddleBand, 1);
        
        // Bands should be symmetric around middle
        var bandwidth = indicator.UpperBand - indicator.LowerBand;
        Assert.True(bandwidth > 0);
    }
    
    [Fact]
    public void QuantConnectImplementation_CalculatesValues()
    {
        // Arrange
        var parameters = new PBollingerBands<double, double>
        {
            Period = 20,
            StandardDeviations = 2.0
        };
        
        var indicator = new BollingerBandsQC<double, double>(parameters);
        var prices = new List<double>();
        
        // Generate test data
        for (int i = 0; i < 30; i++)
        {
            prices.Add(100 + i % 5); // Oscillating prices
        }
        
        var output = new double[prices.Count * 3];
        
        // Act
        indicator.OnBarBatch(prices, output);
        
        // Assert
        Assert.True(indicator.IsReady); // Should be ready after 20 values
        Assert.True(indicator.UpperBand > indicator.MiddleBand);
        Assert.True(indicator.MiddleBand > indicator.LowerBand);
        Assert.True(indicator.BandWidth > 0);
    }
    
    [Fact]
    public void BothImplementations_ProduceSimilarResults()
    {
        // Arrange
        var parameters = new PBollingerBands<double, double>
        {
            Period = 10,
            StandardDeviations = 2.0
        };
        
        var fpIndicator = new BollingerBandsFP<double, double>(parameters);
        var qcIndicator = new BollingerBandsQC<double, double>(parameters);
        
        var prices = new List<double>();
        for (int i = 0; i < 20; i++)
        {
            prices.Add(50 + 2 * System.Math.Sin(i * 0.5)); // Sinusoidal prices
        }
        
        // Act
        foreach (var price in prices)
        {
            fpIndicator.OnNext(price);
            qcIndicator.OnNext(price);
        }
        
        // Assert - both should be ready
        Assert.True(fpIndicator.IsReady);
        Assert.True(qcIndicator.IsReady);
        
        // Middle bands should be very close (both use SMA)
        Assert.Equal(fpIndicator.MiddleBand, qcIndicator.MiddleBand, 0.1);
        
        // Band widths should be similar
        var fpBandWidth = fpIndicator.UpperBand - fpIndicator.LowerBand;
        var qcBandWidth = qcIndicator.UpperBand - qcIndicator.LowerBand;
        Assert.Equal(fpBandWidth, qcBandWidth, 0.5); // Allow some difference due to calculation methods
    }
}