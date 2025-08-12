using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class AccumulationDistributionLineTests
{
    [Fact]
    public void AccumulationDistributionLine_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PAccumulationDistributionLine<HLCV, double>();
        var adl = new AccumulationDistributionLine_QC<HLCV, double>(parameters);
        
        // Sample HLCV data
        var inputs = new HLCV[]
        {
            new() { High = 62.34, Low = 61.37, Close = 62.15, Volume = 7849 },
            new() { High = 62.05, Low = 60.69, Close = 60.81, Volume = 11692 },
            new() { High = 62.27, Low = 60.10, Close = 60.45, Volume = 10575 },
            new() { High = 60.79, Low = 58.61, Close = 59.18, Volume = 13059 },
            new() { High = 59.93, Low = 58.71, Close = 59.24, Volume = 20734 },
            new() { High = 61.75, Low = 59.86, Close = 60.20, Volume = 29630 },
            new() { High = 60.00, Low = 57.97, Close = 58.48, Volume = 17705 },
            new() { High = 59.00, Low = 58.02, Close = 58.24, Volume = 7259 },
            new() { High = 59.07, Low = 57.48, Close = 58.69, Volume = 10475 },
            new() { High = 59.22, Low = 58.30, Close = 58.65, Volume = 5204 },
            new() { High = 58.75, Low = 57.83, Close = 58.47, Volume = 3423 },
            new() { High = 58.65, Low = 57.86, Close = 58.02, Volume = 3962 },
            new() { High = 58.47, Low = 57.91, Close = 58.17, Volume = 4095 },
            new() { High = 58.25, Low = 57.83, Close = 58.07, Volume = 3766 },
            new() { High = 58.35, Low = 57.53, Close = 58.13, Volume = 4239 },
            new() { High = 59.86, Low = 58.58, Close = 58.94, Volume = 8040 },
            new() { High = 59.53, Low = 58.30, Close = 59.10, Volume = 6956 },
            new() { High = 62.10, Low = 58.53, Close = 61.92, Volume = 18171 },
            new() { High = 62.16, Low = 59.80, Close = 61.37, Volume = 22226 },
            new() { High = 62.67, Low = 60.93, Close = 61.68, Volume = 14613 }
        };
        
        var outputs = new double[inputs.Length];

        // Act
        adl.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(adl.IsReady);
        
        // ADL should accumulate over time
        var lastADL = outputs[outputs.Length - 1];
        Assert.NotEqual(0, lastADL);
        Assert.Equal(lastADL, adl.Value);
    }

    [Fact]
    public void AccumulationDistributionLine_MoneyFlowMultiplier()
    {
        // Arrange
        var parameters = new PAccumulationDistributionLine<HLCV, double>();
        var adl = new AccumulationDistributionLine_QC<HLCV, double>(parameters);
        
        // Test specific money flow multiplier scenarios
        var inputs = new HLCV[]
        {
            // Close at high (MFM = 1)
            new() { High = 100, Low = 90, Close = 100, Volume = 1000 },
            // Close at low (MFM = -1)
            new() { High = 100, Low = 90, Close = 90, Volume = 1000 },
            // Close at midpoint (MFM = 0)
            new() { High = 100, Low = 90, Close = 95, Volume = 1000 },
            // Close above midpoint (MFM > 0)
            new() { High = 100, Low = 90, Close = 98, Volume = 1000 },
            // Close below midpoint (MFM < 0)
            new() { High = 100, Low = 90, Close = 92, Volume = 1000 },
        };
        
        var outputs = new double[inputs.Length];

        // Act
        adl.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(adl.IsReady);
        
        // First bar: Close at high, ADL should be positive
        Assert.True(outputs[0] > 0);
        
        // Second bar: Close at low, ADL should decrease
        Assert.True(outputs[1] < outputs[0]);
    }

    [Fact]
    public void AccumulationDistributionLine_AccumulationVsDistribution()
    {
        // Arrange
        var parameters = new PAccumulationDistributionLine<HLCV, double>();
        
        // Accumulation scenario: prices rising, close near highs
        var accumulation = new HLCV[20];
        for (int i = 0; i < accumulation.Length; i++)
        {
            var price = 100.0 + i * 1.0;
            accumulation[i] = new HLCV
            {
                High = price + 1,
                Low = price - 1,
                Close = price + 0.8, // Close near high
                Volume = 10000 + i * 100
            };
        }
        
        // Distribution scenario: prices falling, close near lows
        var distribution = new HLCV[20];
        for (int i = 0; i < distribution.Length; i++)
        {
            var price = 100.0 - i * 1.0;
            distribution[i] = new HLCV
            {
                High = price + 1,
                Low = price - 1,
                Close = price - 0.8, // Close near low
                Volume = 10000 + i * 100
            };
        }

        // Act
        var adlAccum = new AccumulationDistributionLine_QC<HLCV, double>(parameters);
        var accumOutputs = new double[accumulation.Length];
        adlAccum.OnBarBatch(accumulation, accumOutputs);
        var accumADL = accumOutputs[accumOutputs.Length - 1];
        
        var adlDist = new AccumulationDistributionLine_QC<HLCV, double>(parameters);
        var distOutputs = new double[distribution.Length];
        adlDist.OnBarBatch(distribution, distOutputs);
        var distADL = distOutputs[distOutputs.Length - 1];

        // Assert
        Assert.True(accumADL > 0, $"Accumulation ADL {accumADL} should be positive");
        Assert.True(distADL < 0, $"Distribution ADL {distADL} should be negative");
    }

    [Fact]
    public void AccumulationDistributionLine_VolumeImpact()
    {
        // Arrange
        var parameters = new PAccumulationDistributionLine<HLCV, double>();
        
        // Same price action, different volumes
        var lowVolume = new HLCV[]
        {
            new() { High = 102, Low = 98, Close = 101, Volume = 100 },
            new() { High = 103, Low = 99, Close = 102, Volume = 100 },
            new() { High = 104, Low = 100, Close = 103, Volume = 100 },
        };
        
        var highVolume = new HLCV[]
        {
            new() { High = 102, Low = 98, Close = 101, Volume = 10000 },
            new() { High = 103, Low = 99, Close = 102, Volume = 10000 },
            new() { High = 104, Low = 100, Close = 103, Volume = 10000 },
        };

        // Act
        var adlLow = new AccumulationDistributionLine_QC<HLCV, double>(parameters);
        var lowOutputs = new double[lowVolume.Length];
        adlLow.OnBarBatch(lowVolume, lowOutputs);
        var lowADL = Math.Abs(lowOutputs[lowOutputs.Length - 1]);
        
        var adlHigh = new AccumulationDistributionLine_QC<HLCV, double>(parameters);
        var highOutputs = new double[highVolume.Length];
        adlHigh.OnBarBatch(highVolume, highOutputs);
        var highADL = Math.Abs(highOutputs[highOutputs.Length - 1]);

        // Assert
        Assert.True(highADL > lowADL, 
            $"High volume ADL {highADL} should be greater than low volume {lowADL}");
    }

    [Fact]
    public void AccumulationDistributionLine_Divergence()
    {
        // Arrange
        var parameters = new PAccumulationDistributionLine<HLCV, double>();
        var adl = new AccumulationDistributionLine_QC<HLCV, double>(parameters);
        
        // Price rising but closing near lows (bearish divergence)
        var inputs = new HLCV[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5; // Price rising
            inputs[i] = new HLCV
            {
                High = price + 1,
                Low = price - 1,
                Close = price - 0.7, // But closing near lows
                Volume = 10000 - i * 200 // Decreasing volume
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        adl.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(adl.IsReady);
        
        // Despite rising prices, ADL should be declining (distribution)
        var midADL = outputs[10];
        var lastADL = outputs[outputs.Length - 1];
        
        Assert.True(lastADL < midADL, 
            "ADL should decline despite rising prices when closing near lows");
    }

    [Fact]
    public void AccumulationDistributionLine_CumulativeNature()
    {
        // Arrange
        var parameters = new PAccumulationDistributionLine<HLCV, double>();
        var adl = new AccumulationDistributionLine_QC<HLCV, double>(parameters);
        
        // Mixed data
        var inputs = new HLCV[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 10;
            inputs[i] = new HLCV
            {
                High = price + 1,
                Low = price - 1,
                Close = price + Math.Sin(i * 0.5) * 0.5,
                Volume = 10000 + Math.Sin(i * 0.4) * 5000
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        adl.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(adl.IsReady);
        
        // ADL is cumulative, each value depends on previous
        for (int i = 1; i < outputs.Length; i++)
        {
            // Current ADL = Previous ADL + Current Money Flow Volume
            // So the difference should be the current period's money flow volume
            var diff = outputs[i] - outputs[i - 1];
            
            // Money flow volume calculation
            var mfm = ((inputs[i].Close - inputs[i].Low) - (inputs[i].High - inputs[i].Close)) /
                     (inputs[i].High - inputs[i].Low);
            if (double.IsNaN(mfm)) mfm = 0;
            var expectedMFV = mfm * inputs[i].Volume;
            
            Assert.Equal(expectedMFV, diff, 2);
        }
    }

    [Fact]
    public void AccumulationDistributionLine_ZeroVolume()
    {
        // Arrange
        var parameters = new PAccumulationDistributionLine<HLCV, double>();
        var adl = new AccumulationDistributionLine_QC<HLCV, double>(parameters);
        
        // Data with zero volume bars
        var inputs = new HLCV[]
        {
            new() { High = 100, Low = 98, Close = 99, Volume = 1000 },
            new() { High = 101, Low = 99, Close = 100, Volume = 0 }, // Zero volume
            new() { High = 102, Low = 100, Close = 101, Volume = 1500 },
            new() { High = 103, Low = 101, Close = 102, Volume = 0 }, // Zero volume
            new() { High = 104, Low = 102, Close = 103, Volume = 2000 },
        };
        
        var outputs = new double[inputs.Length];

        // Act
        adl.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(adl.IsReady);
        
        // Zero volume bars should not change ADL
        Assert.Equal(outputs[0], outputs[1]); // No change on zero volume
        Assert.Equal(outputs[2], outputs[3]); // No change on zero volume
    }

    [Fact]
    public void AccumulationDistributionLine_TrendConfirmation()
    {
        // Arrange
        var parameters = new PAccumulationDistributionLine<HLCV, double>();
        var adl = new AccumulationDistributionLine_QC<HLCV, double>(parameters);
        
        // Strong uptrend with accumulation
        var inputs = new HLCV[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 2.0;
            inputs[i] = new HLCV
            {
                High = price + 1,
                Low = price - 0.5,
                Close = price + 0.7, // Consistently closing near highs
                Volume = 10000 + i * 500 // Increasing volume
            };
        }
        
        var outputs = new double[inputs.Length];

        // Act
        adl.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(adl.IsReady);
        
        // ADL should consistently increase in strong uptrend with accumulation
        for (int i = 1; i < outputs.Length; i++)
        {
            Assert.True(outputs[i] > outputs[i - 1], 
                $"ADL should increase at index {i}: {outputs[i]} > {outputs[i-1]}");
        }
    }
}