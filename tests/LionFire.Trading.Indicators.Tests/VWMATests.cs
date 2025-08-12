using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class VWMATests
{
    [Fact]
    public void VWMA_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PVWMA<PriceVolume, double> { Period = 10 };
        var vwma = new VWMA_QC<PriceVolume, double>(parameters);
        
        // Sample price and volume data
        var inputs = new PriceVolume[]
        {
            new() { Price = 100, Volume = 1000 },
            new() { Price = 102, Volume = 1500 },
            new() { Price = 101, Volume = 1200 },
            new() { Price = 103, Volume = 2000 },
            new() { Price = 105, Volume = 1800 },
            new() { Price = 104, Volume = 1100 },
            new() { Price = 106, Volume = 1600 },
            new() { Price = 108, Volume = 2200 },
            new() { Price = 107, Volume = 1400 },
            new() { Price = 109, Volume = 1900 },
            new() { Price = 110, Volume = 2500 },
            new() { Price = 112, Volume = 2100 },
        };
        
        var outputs = new double[inputs.Length];

        // Act
        vwma.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwma.IsReady);
        
        // VWMA should weight prices by volume
        var lastVWMA = outputs[outputs.Length - 1];
        Assert.True(lastVWMA > 0);
        Assert.Equal(lastVWMA, vwma.Value);
    }

    [Fact]
    public void VWMA_VolumeWeighting()
    {
        // Arrange
        var parameters = new PVWMA<PriceVolume, double> { Period = 5 };
        var vwma = new VWMA_QC<PriceVolume, double>(parameters);
        var smaParams = new PSMA<double, double> { Period = 5 };
        var sma = new SMA_QC<double, double>(smaParams);
        
        // Data where high prices have high volume
        var inputs = new PriceVolume[]
        {
            new() { Price = 100, Volume = 100 },   // Low price, low volume
            new() { Price = 110, Volume = 1000 },  // High price, high volume
            new() { Price = 95, Volume = 50 },     // Low price, low volume
            new() { Price = 115, Volume = 2000 },  // High price, high volume
            new() { Price = 90, Volume = 100 },    // Low price, low volume
            new() { Price = 120, Volume = 3000 },  // High price, high volume
            new() { Price = 105, Volume = 500 },
        };
        
        var vwmaOutputs = new double[inputs.Length];
        var prices = inputs.Select(i => i.Price).ToArray();
        var smaOutputs = new double[prices.Length];

        // Act
        vwma.OnBarBatch(inputs, vwmaOutputs);
        sma.OnBarBatch(prices, smaOutputs);

        // Assert
        var lastVWMA = vwmaOutputs[vwmaOutputs.Length - 1];
        var lastSMA = smaOutputs[smaOutputs.Length - 1];
        
        // VWMA should be higher than SMA because high prices have more volume
        Assert.True(lastVWMA > lastSMA, 
            $"VWMA {lastVWMA} should be > SMA {lastSMA} when high prices have high volume");
    }

    [Fact]
    public void VWMA_ConstantVolume()
    {
        // Arrange
        var parameters = new PVWMA<PriceVolume, double> { Period = 10 };
        var vwma = new VWMA_QC<PriceVolume, double>(parameters);
        var smaParams = new PSMA<double, double> { Period = 10 };
        var sma = new SMA_QC<double, double>(smaParams);
        
        // Data with constant volume
        var inputs = new PriceVolume[20];
        var prices = new double[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new PriceVolume { Price = price, Volume = 1000 };
            prices[i] = price;
        }
        
        var vwmaOutputs = new double[inputs.Length];
        var smaOutputs = new double[prices.Length];

        // Act
        vwma.OnBarBatch(inputs, vwmaOutputs);
        sma.OnBarBatch(prices, smaOutputs);

        // Assert
        // With constant volume, VWMA should equal SMA
        var lastVWMA = vwmaOutputs[vwmaOutputs.Length - 1];
        var lastSMA = smaOutputs[smaOutputs.Length - 1];
        
        Assert.Equal(lastSMA, lastVWMA, 2);
    }

    [Fact]
    public void VWMA_ZeroVolume()
    {
        // Arrange
        var parameters = new PVWMA<PriceVolume, double> { Period = 5 };
        var vwma = new VWMA_QC<PriceVolume, double>(parameters);
        
        // Data with some zero volume
        var inputs = new PriceVolume[]
        {
            new() { Price = 100, Volume = 1000 },
            new() { Price = 102, Volume = 0 },    // Zero volume
            new() { Price = 101, Volume = 1500 },
            new() { Price = 103, Volume = 0 },    // Zero volume
            new() { Price = 105, Volume = 2000 },
            new() { Price = 104, Volume = 1800 },
        };
        
        var outputs = new double[inputs.Length];

        // Act
        vwma.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwma.IsReady);
        
        // VWMA should handle zero volume gracefully
        var lastVWMA = outputs[outputs.Length - 1];
        Assert.True(lastVWMA > 0);
        
        // Zero volume bars should not contribute to the average
        // The VWMA should be closer to the prices with volume
    }

    [Fact]
    public void VWMA_TrendFollowing()
    {
        // Arrange
        var parameters = new PVWMA<PriceVolume, double> { Period = 10 };
        var vwma = new VWMA_QC<PriceVolume, double>(parameters);
        
        // Uptrending data with increasing volume
        var inputs = new PriceVolume[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = new PriceVolume
            {
                Price = 100.0 + i * 1.5,
                Volume = 1000 + i * 100 // Volume increases with trend
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        vwma.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwma.IsReady);
        
        // VWMA should follow the trend
        Assert.True(outputs[15] < outputs[20]);
        Assert.True(outputs[20] < outputs[25]);
        Assert.True(outputs[25] < outputs[29]);
    }

    [Fact]
    public void VWMA_DifferentPeriods()
    {
        var periods = new[] { 5, 10, 20 };
        
        // Create sample data
        var inputs = new PriceVolume[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = new PriceVolume
            {
                Price = 100.0 + Math.Sin(i * 0.2) * 10,
                Volume = 1000 + Math.Sin(i * 0.3) * 500
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PVWMA<PriceVolume, double> { Period = period };
            var vwma = new VWMA_QC<PriceVolume, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            vwma.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(vwma.IsReady);
            var lastValue = outputs[outputs.Length - 1];
            Assert.True(lastValue > 0);
            
            // Shorter periods should be more responsive
        }
    }

    [Fact]
    public void VWMA_VolumeSpikes()
    {
        // Arrange
        var parameters = new PVWMA<PriceVolume, double> { Period = 10 };
        var vwma = new VWMA_QC<PriceVolume, double>(parameters);
        
        // Normal data with volume spike
        var inputs = new PriceVolume[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 5;
            var volume = 1000.0;
            
            // Volume spike at specific points
            if (i == 10)
            {
                volume = 10000; // 10x normal volume
                price = 110.0;  // Price spike
            }
            
            inputs[i] = new PriceVolume { Price = price, Volume = volume };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        vwma.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(vwma.IsReady);
        
        // VWMA should be pulled toward the volume spike price
        var beforeSpike = outputs[9];
        var afterSpike = outputs[11];
        
        Assert.True(afterSpike > beforeSpike, 
            "VWMA should increase after high-volume price spike");
    }

    [Fact]
    public void VWMA_ComparisonWithEMA()
    {
        // Arrange
        var period = 10;
        var vwmaParams = new PVWMA<PriceVolume, double> { Period = period };
        var emaParams = new PEMA<double, double> { Period = period };
        
        var vwma = new VWMA_QC<PriceVolume, double>(vwmaParams);
        var ema = new EMA_QC<double, double>(emaParams);
        
        // Create data where recent prices have higher volume
        var inputs = new PriceVolume[30];
        var prices = new double[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            var volume = 500 + i * 50; // Increasing volume over time
            inputs[i] = new PriceVolume { Price = price, Volume = volume };
            prices[i] = price;
        }
        
        var vwmaOutputs = new double[inputs.Length];
        var emaOutputs = new double[prices.Length];

        // Act
        vwma.OnBarBatch(inputs, vwmaOutputs);
        ema.OnBarBatch(prices, emaOutputs);

        // Assert
        var lastVWMA = vwmaOutputs[vwmaOutputs.Length - 1];
        var lastEMA = emaOutputs[emaOutputs.Length - 1];
        
        // Both should be positive and reasonable
        Assert.True(lastVWMA > 100);
        Assert.True(lastEMA > 100);
        
        // VWMA weights by volume, EMA weights by time
        // They should be different but in the same ballpark
        Assert.InRange(Math.Abs(lastVWMA - lastEMA), 0, 10);
    }
}

public class PriceVolume
{
    public double Price { get; set; }
    public double Volume { get; set; }
}