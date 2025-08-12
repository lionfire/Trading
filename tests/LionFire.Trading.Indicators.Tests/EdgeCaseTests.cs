using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class EdgeCaseTests
{
    [Fact]
    public void EdgeCase_SMA_NaNValues()
    {
        // Arrange
        var parameters = new PSMA<double, double> { Period = 5 };
        var sma = new SMA_QC<double, double>(parameters);
        var inputs = new double[] { 100, double.NaN, 102, 103, double.NaN, 105 };
        var outputs = new double[inputs.Length];

        // Act
        sma.OnBarBatch(inputs, outputs);

        // Assert
        // Indicator should handle NaN values gracefully
        Assert.True(sma.IsReady);
        
        // Valid outputs should not be NaN
        var validOutputs = outputs.Where(o => o != 0 && !double.IsNaN(o)).ToArray();
        Assert.True(validOutputs.Length > 0);
    }

    [Fact]
    public void EdgeCase_RSI_InfinityValues()
    {
        // Arrange
        var parameters = new PRSI<double, double> { Period = 14 };
        var rsi = new RSI_QC<double, double>(parameters);
        var inputs = new double[] 
        { 
            100, 101, 102, double.PositiveInfinity, 104, 105, 106, 107, 108, 109,
            110, 111, 112, 113, 114, 115, 116, 117, 118, 119
        };
        var outputs = new double[inputs.Length];

        // Act & Assert
        // Should not throw exception
        Assert.DoesNotThrow(() => rsi.OnBarBatch(inputs, outputs));
        
        // RSI values should still be within bounds
        var validOutputs = outputs.Skip(parameters.Period).Where(o => o > 0 && !double.IsInfinity(o)).ToArray();
        Assert.True(validOutputs.All(o => o >= 0 && o <= 100));
    }

    [Fact]
    public void EdgeCase_MACD_ZeroValues()
    {
        // Arrange
        var parameters = new PMACD<double, MACDResult> 
        { 
            FastPeriod = 12, 
            SlowPeriod = 26, 
            SignalPeriod = 9 
        };
        var macd = new MACD_QC<double, MACDResult>(parameters);
        var inputs = Enumerable.Repeat(0.0, 50).ToArray();
        var outputs = new MACDResult[inputs.Length];

        // Act
        macd.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(macd.IsReady);
        
        // With all zero values, MACD should be zero
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        Assert.Equal(0, lastResult.MACD, 5);
        Assert.Equal(0, lastResult.Signal, 5);
        Assert.Equal(0, lastResult.Histogram, 5);
    }

    [Fact]
    public void EdgeCase_BollingerBands_NegativeValues()
    {
        // Arrange
        var parameters = new PBollingerBands<double, BollingerBandsResult> 
        { 
            Period = 20, 
            StandardDeviations = 2 
        };
        var bb = new BollingerBands_QC<double, BollingerBandsResult>(parameters);
        var inputs = Enumerable.Range(-50, 30).Select(x => (double)x).ToArray();
        var outputs = new BollingerBandsResult[inputs.Length];

        // Act
        bb.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(bb.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // Bands should be properly ordered even with negative values
        Assert.True(lastResult.UpperBand > lastResult.MiddleBand);
        Assert.True(lastResult.MiddleBand > lastResult.LowerBand);
    }

    [Fact]
    public void EdgeCase_ATR_ZeroRange()
    {
        // Arrange
        var parameters = new PATR<HLC, double> { Period = 14 };
        var atr = new AverageTrueRange_QC<HLC, double>(parameters);
        
        // All bars have same high, low, close (zero range)
        var inputs = new HLC[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = new HLC { High = 100, Low = 100, Close = 100 };
        }
        var outputs = new double[inputs.Length];

        // Act
        atr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(atr.IsReady);
        
        // ATR should be zero for zero range data
        var lastATR = outputs[outputs.Length - 1];
        Assert.Equal(0, lastATR, 5);
    }

    [Fact]
    public void EdgeCase_VWAP_ZeroVolume()
    {
        // Arrange
        var parameters = new PVWAP<HLCV, double> { Period = 14 };
        var vwap = new VWAP_QC<HLCV, double>(parameters);
        
        // All zero volume
        var inputs = new HLCV[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = new HLCV 
            { 
                High = 100 + i, 
                Low = 99 + i, 
                Close = 99.5 + i, 
                Volume = 0 
            };
        }
        var outputs = new double[inputs.Length];

        // Act
        vwap.OnBarBatch(inputs, outputs);

        // Assert
        // Should handle zero volume gracefully without crashing
        Assert.True(vwap.IsReady);
    }

    [Fact]
    public void EdgeCase_MFI_ExtremeVolume()
    {
        // Arrange
        var parameters = new PMFI<HLCV, double> { Period = 14 };
        var mfi = new MFI_QC<HLCV, double>(parameters);
        
        // One bar with extremely high volume
        var inputs = new HLCV[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var volume = i == 10 ? double.MaxValue / 1000 : 1000; // Extreme volume on one bar
            inputs[i] = new HLCV 
            { 
                High = 101, 
                Low = 99, 
                Close = 100, 
                Volume = volume 
            };
        }
        var outputs = new double[inputs.Length];

        // Act & Assert
        Assert.DoesNotThrow(() => mfi.OnBarBatch(inputs, outputs));
        
        // MFI should still be between 0-100
        if (mfi.IsReady)
        {
            var lastMFI = outputs[outputs.Length - 1];
            Assert.InRange(lastMFI, 0, 100);
        }
    }

    [Fact]
    public void EdgeCase_Stochastic_IdenticalHighLow()
    {
        // Arrange
        var parameters = new PStochastic<HLC, StochasticResult> 
        { 
            KPeriod = 14, 
            DPeriod = 3 
        };
        var stoch = new Stochastic_QC<HLC, StochasticResult>(parameters);
        
        // All bars with identical high and low
        var inputs = new HLC[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = new HLC 
            { 
                High = 100, 
                Low = 100, 
                Close = 100 
            };
        }
        var outputs = new StochasticResult[inputs.Length];

        // Act
        stoch.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(stoch.IsReady);
        
        // With identical high/low, stochastic behavior depends on implementation
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(int.MaxValue / 1000000)] // Very large period
    public void EdgeCase_SMA_ExtremePeriods(int period)
    {
        // Arrange
        var parameters = new PSMA<double, double> { Period = period };
        var sma = new SMA_QC<double, double>(parameters);
        var dataSize = Math.Min(period * 2, 1000);
        var inputs = Enumerable.Range(1, dataSize).Select(x => (double)x).ToArray();
        var outputs = new double[inputs.Length];

        // Act & Assert
        if (period <= 0)
        {
            Assert.Throws<ArgumentException>(() => new SMA_QC<double, double>(parameters));
        }
        else if (period > inputs.Length)
        {
            sma.OnBarBatch(inputs, outputs);
            Assert.False(sma.IsReady); // Not enough data
        }
        else
        {
            sma.OnBarBatch(inputs, outputs);
            if (period <= inputs.Length)
            {
                Assert.True(sma.IsReady);
            }
        }
    }

    [Fact]
    public void EdgeCase_EMA_AlphaNearLimits()
    {
        // Arrange - Test EMA with very small and large periods
        var smallPeriod = new PEMA<double, double> { Period = 1 };
        var largePeriod = new PEMA<double, double> { Period = 1000000 };
        
        var emaSmall = new EMA_QC<double, double>(smallPeriod);
        var emaLarge = new EMA_QC<double, double>(largePeriod);
        
        var inputs = new double[] { 100, 110, 120, 130, 140 };
        var outputsSmall = new double[inputs.Length];
        var outputsLarge = new double[inputs.Length];

        // Act
        emaSmall.OnBarBatch(inputs, outputsSmall);
        emaLarge.OnBarBatch(inputs, outputsLarge);

        // Assert
        // Period 1 EMA should equal the input values
        if (emaSmall.IsReady)
        {
            Assert.Equal(inputs[inputs.Length - 1], outputsSmall[inputs.Length - 1], 1);
        }
        
        // Very large period EMA should change very slowly
        if (emaLarge.IsReady)
        {
            var change = Math.Abs(outputsLarge[inputs.Length - 1] - outputsLarge[1]);
            Assert.True(change < 1); // Should change very little
        }
    }

    [Fact]
    public void EdgeCase_ParabolicSAR_NoTrend()
    {
        // Arrange
        var parameters = new PParabolicSAR<HLC, double> 
        { 
            AccelerationFactor = 0.02,
            AccelerationStep = 0.02,
            MaxAccelerationFactor = 0.2
        };
        var sar = new ParabolicSAR_QC<HLC, double>(parameters);
        
        // Completely flat data (no trend)
        var inputs = new HLC[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = new HLC 
            { 
                High = 100.1, 
                Low = 99.9, 
                Close = 100 
            };
        }
        var outputs = new double[inputs.Length];

        // Act
        sar.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(sar.IsReady);
        
        // SAR should handle flat market without errors
        var validOutputs = outputs.Where(o => o > 0).ToArray();
        Assert.True(validOutputs.All(o => o > 99 && o < 101));
    }

    [Fact]
    public void EdgeCase_IchimokuCloud_InsufficientData()
    {
        // Arrange
        var parameters = new PIchimokuCloud<HLC, IchimokuResult>
        {
            TenkanPeriod = 9,
            KijunPeriod = 26,
            SenkouAPeriod = 26,
            SenkouBPeriod = 52,
            ChikouPeriod = 26
        };
        var ichimoku = new IchimokuCloud_QC<HLC, IchimokuResult>(parameters);
        
        // Only provide 10 bars (insufficient for full calculation)
        var inputs = new HLC[10];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = new HLC 
            { 
                High = 100 + i, 
                Low = 99 + i, 
                Close = 99.5 + i 
            };
        }
        var outputs = new IchimokuResult[inputs.Length];

        // Act
        ichimoku.OnBarBatch(inputs, outputs);

        // Assert
        // Should not be ready with insufficient data
        Assert.False(ichimoku.IsReady);
    }

    [Fact]
    public void EdgeCase_LinearRegression_PerfectLinearData()
    {
        // Arrange
        var parameters = new PLinearRegression<double, LinearRegressionResult> { Period = 10 };
        var lr = new LinearRegression_QC<double, LinearRegressionResult>(parameters);
        
        // Perfect linear progression
        var inputs = Enumerable.Range(1, 20).Select(x => x * 2.5).ToArray();
        var outputs = new LinearRegressionResult[inputs.Length];

        // Act
        lr.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(lr.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // Perfect linear data should have R² = 1
        Assert.Equal(1.0, lastResult.RSquared, 2);
        
        // Slope should be 2.5
        Assert.Equal(2.5, lastResult.Slope, 1);
        
        // Standard error should be very small
        Assert.True(lastResult.StandardError < 0.01);
    }
}

public class StochasticResult
{
    public double K { get; set; }
    public double D { get; set; }
}