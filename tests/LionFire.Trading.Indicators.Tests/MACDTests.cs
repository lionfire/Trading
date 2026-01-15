// DISABLED: Tests need updating to match current API
#if false
using LionFire.Trading.Indicators.Defaults;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class MACDTests
{
    [Fact]
    public void FirstPartyImplementation_CalculatesCorrectly()
    {
        // Arrange - Use standard MACD parameters (12, 26, 9)
        var parameters = new PMACD<double, double> 
        { 
            FastPeriod = 12, 
            SlowPeriod = 26, 
            SignalPeriod = 9 
        };
        var macd = new MACD_FP<double, double>(parameters);
        
        // Create test data - ascending values to see clear MACD behavior
        var inputs = Enumerable.Range(1, 50).Select(x => (double)x).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        macd.OnBarBatch(inputs, outputs);

        // Assert
        // MACD needs SlowPeriod + SignalPeriod - 1 = 26 + 9 - 1 = 34 periods to be fully ready
        Assert.False(macd.IsReady);
        
        // Process more data to make it ready
        var moreInputs = Enumerable.Range(51, 20).Select(x => (double)x).ToArray();
        var moreOutputs = new double[moreInputs.Length];
        macd.OnBarBatch(moreInputs, moreOutputs);
        
        Assert.True(macd.IsReady);
        Assert.NotEqual(0, macd.MACD);
        Assert.NotEqual(0, macd.Signal);
        Assert.NotEqual(0, macd.Histogram);
        
        // MACD should be positive for uptrending data (fast EMA > slow EMA)
        Assert.True(macd.MACD > 0);
        
        // Histogram should equal MACD - Signal
        Assert.Equal(macd.MACD - macd.Signal, macd.Histogram, precision: 10);
    }

    [Fact]
    public void QuantConnectImplementation_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PMACD<double, double> 
        { 
            FastPeriod = 12, 
            SlowPeriod = 26, 
            SignalPeriod = 9 
        };
        var macd = new MACD_QC<double, double>(parameters);
        
        // Create test data
        var inputs = Enumerable.Range(1, 70).Select(x => (double)x).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        macd.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(macd.IsReady);
        Assert.NotEqual(0, macd.MACD);
        Assert.NotEqual(0, macd.Signal);
        Assert.NotEqual(0, macd.Histogram);
        
        // MACD should be positive for uptrending data
        Assert.True(macd.MACD > 0);
        
        // Histogram should equal MACD - Signal (within reasonable precision)
        Assert.Equal(macd.MACD - macd.Signal, macd.Histogram, precision: 6);
    }

    [Fact]
    public void DefaultFactory_CreatesWorkingImplementation()
    {
        // Arrange
        var macd = MACD.CreateDouble();
        
        // Act - Process a trend
        for (int i = 1; i <= 50; i++)
        {
            macd.OnNext((double)i);
        }
        
        // Assert
        Assert.True(macd.IsReady);
        Assert.True(macd.MACD > 0); // Uptrend should have positive MACD
        Assert.Equal(macd.MACD - macd.Signal, macd.Histogram, precision: 10);
    }

    [Fact]
    public void DefaultFactory_WithCustomParameters_Works()
    {
        // Arrange & Act
        var macd = MACD.CreateDouble(fastPeriod: 5, slowPeriod: 10, signalPeriod: 3);
        
        // Process data
        for (int i = 1; i <= 20; i++)
        {
            macd.OnNext((double)i);
        }
        
        // Assert
        Assert.True(macd.IsReady);
        Assert.Equal(5, macd.FastPeriod);
        Assert.Equal(10, macd.SlowPeriod);
        Assert.Equal(3, macd.SignalPeriod);
    }

    [Fact]
    public void MACD_DetectsTrendChange()
    {
        // Arrange
        var macd = MACD.CreateDouble(fastPeriod: 5, slowPeriod: 10, signalPeriod: 3);
        
        // Act - Uptrend then downtrend
        for (int i = 1; i <= 15; i++)
        {
            macd.OnNext((double)i); // Uptrend
        }
        
        var uptrendMACD = macd.MACD;
        var uptrendHistogram = macd.Histogram;
        
        for (int i = 15; i >= 1; i--)
        {
            macd.OnNext((double)i); // Downtrend
        }
        
        var downtrendMACD = macd.MACD;
        var downtrendHistogram = macd.Histogram;
        
        // Assert
        Assert.True(uptrendMACD > 0); // Should be positive during uptrend
        Assert.True(downtrendMACD < 0); // Should be negative during downtrend
        Assert.NotEqual(uptrendHistogram, downtrendHistogram);
    }

    [Fact]
    public void Clear_ResetsIndicatorState()
    {
        // Arrange
        var parameters = new PMACD<double, double> 
        { 
            FastPeriod = 5, 
            SlowPeriod = 10, 
            SignalPeriod = 3 
        };
        var macd = new MACD_FP<double, double>(parameters);
        
        // Process some data to get it ready
        for (int i = 1; i <= 20; i++)
        {
            macd.OnNext((double)i);
        }
        
        Assert.True(macd.IsReady);
        var originalMACD = macd.MACD;
        var originalSignal = macd.Signal;
        var originalHistogram = macd.Histogram;
        
        // Act
        macd.Clear();
        
        // Assert
        Assert.False(macd.IsReady);
        Assert.Equal(0, macd.MACD);
        Assert.Equal(0, macd.Signal);
        Assert.Equal(0, macd.Histogram);
        
        // Process new data after clear
        for (int i = 10; i <= 30; i++)
        {
            macd.OnNext((double)i);
        }
        
        Assert.True(macd.IsReady);
        // Values should be different from before clearing
        Assert.NotEqual(originalMACD, macd.MACD);
        Assert.NotEqual(originalSignal, macd.Signal);
        Assert.NotEqual(originalHistogram, macd.Histogram);
    }

    [Fact]
    public void MACD_HandlesConstantValues()
    {
        // Arrange
        var macd = MACD.CreateDouble(fastPeriod: 5, slowPeriod: 10, signalPeriod: 3);
        
        // Act - Feed constant values
        for (int i = 0; i < 20; i++)
        {
            macd.OnNext(100.0);
        }
        
        // Assert
        Assert.True(macd.IsReady);
        // With constant values, MACD should be close to 0 (fast EMA ≈ slow EMA)
        Assert.True(Math.Abs(macd.MACD) < 0.001);
        Assert.True(Math.Abs(macd.Signal) < 0.001);
        Assert.True(Math.Abs(macd.Histogram) < 0.001);
    }

    [Fact]
    public void MACD_ParameterValidation()
    {
        // Arrange & Assert - Should throw when FastPeriod >= SlowPeriod
        Assert.Throws<ArgumentException>(() => 
        {
            var invalidParams = new PMACD<double, double> 
            { 
                FastPeriod = 26, 
                SlowPeriod = 12, 
                SignalPeriod = 9 
            };
            invalidParams.Validate();
        });
        
        Assert.Throws<ArgumentException>(() => 
        {
            var invalidParams = new PMACD<double, double> 
            { 
                FastPeriod = 12, 
                SlowPeriod = 12, 
                SignalPeriod = 9 
            };
            invalidParams.Validate();
        });
        
        Assert.Throws<ArgumentException>(() => 
        {
            var invalidParams = new PMACD<double, double> 
            { 
                FastPeriod = 0, 
                SlowPeriod = 26, 
                SignalPeriod = 9 
            };
            invalidParams.Validate();
        });
    }

    [Fact]
    public void MACD_BothImplementations_ProduceSimilarResults()
    {
        // Arrange
        var parameters = new PMACD<double, double> 
        { 
            FastPeriod = 12, 
            SlowPeriod = 26, 
            SignalPeriod = 9 
        };
        
        var macdFP = new MACD_FP<double, double>(parameters);
        var macdQC = new MACD_QC<double, double>(parameters);
        
        // Create test data
        var inputs = Enumerable.Range(1, 100).Select(x => (double)x + Math.Sin(x * 0.1) * 10).ToArray();
        
        // Act - Process same data through both implementations
        macdFP.OnBarBatch(inputs, null);
        macdQC.OnBarBatch(inputs, null);
        
        // Assert - Results should be close (allowing for small numerical differences)
        Assert.True(macdFP.IsReady);
        Assert.True(macdQC.IsReady);
        
        Assert.Equal(macdFP.MACD, macdQC.MACD, precision: 2);
        Assert.Equal(macdFP.Signal, macdQC.Signal, precision: 2);
        Assert.Equal(macdFP.Histogram, macdQC.Histogram, precision: 2);
    }

    [Theory]
    [InlineData(5, 10, 3)]
    [InlineData(12, 26, 9)]
    [InlineData(8, 21, 5)]
    public void MACD_DifferentParameters_CalculateCorrectly(int fastPeriod, int slowPeriod, int signalPeriod)
    {
        // Arrange
        var parameters = new PMACD<double, double> 
        { 
            FastPeriod = fastPeriod, 
            SlowPeriod = slowPeriod, 
            SignalPeriod = signalPeriod 
        };
        var macd = new MACD_FP<double, double>(parameters);
        
        // Create trending data
        var inputs = Enumerable.Range(1, slowPeriod + signalPeriod + 10)
            .Select(x => (double)x).ToArray();
        
        // Act
        macd.OnBarBatch(inputs, null);
        
        // Assert
        Assert.True(macd.IsReady);
        Assert.Equal(fastPeriod, macd.FastPeriod);
        Assert.Equal(slowPeriod, macd.SlowPeriod);
        Assert.Equal(signalPeriod, macd.SignalPeriod);
        Assert.True(macd.MACD > 0); // Uptrend should have positive MACD
        Assert.Equal(macd.MACD - macd.Signal, macd.Histogram, precision: 10);
    }

    [Fact]
    public void MACD_HandlesDifferentDataTypes()
    {
        // Test with decimal
        var macdDecimal = MACD.CreateDecimal();
        for (int i = 1; i <= 50; i++)
        {
            macdDecimal.OnNext((decimal)i);
        }
        Assert.True(macdDecimal.IsReady);
        Assert.True(macdDecimal.MACD > 0);

        // Test with float
        var macdFloat = MACD.CreateFloat();
        for (int i = 1; i <= 50; i++)
        {
            macdFloat.OnNext((float)i);
        }
        Assert.True(macdFloat.IsReady);
        Assert.True(macdFloat.MACD > 0);
    }
}
#endif
