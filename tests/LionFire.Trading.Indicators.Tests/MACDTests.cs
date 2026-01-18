using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
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

        // Assert - MACD should be ready after processing sufficient data
        Assert.True(macd.IsReady);
        Assert.NotEqual(0.0, macd.MACD);
        Assert.NotEqual(0.0, macd.Signal);
        Assert.NotEqual(0.0, macd.Histogram);

        // MACD should be positive for uptrending data (fast EMA > slow EMA)
        Assert.True(macd.MACD > 0);

        // Histogram should equal MACD - Signal
        Assert.Equal(macd.MACD - macd.Signal, macd.Histogram, 10);
    }

    [Fact]
    public void MACD_DetectsTrendChange()
    {
        // Arrange
        var parameters = new PMACD<double, double>
        {
            FastPeriod = 5,
            SlowPeriod = 10,
            SignalPeriod = 3
        };
        var macd = new MACD_FP<double, double>(parameters);

        // Act - Uptrend
        var uptrendInputs = Enumerable.Range(1, 15).Select(x => (double)x).ToArray();
        macd.OnBarBatch(uptrendInputs, new double[uptrendInputs.Length]);

        var uptrendMACD = macd.MACD;
        var uptrendHistogram = macd.Histogram;

        // Downtrend
        var downtrendInputs = Enumerable.Range(1, 15).Select(x => (double)(15 - x)).ToArray();
        macd.OnBarBatch(downtrendInputs, new double[downtrendInputs.Length]);

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
        var inputs = Enumerable.Range(1, 20).Select(x => (double)x).ToArray();
        macd.OnBarBatch(inputs, new double[inputs.Length]);

        Assert.True(macd.IsReady);
        var originalMACD = macd.MACD;

        // Act
        macd.Clear();

        // Assert
        Assert.False(macd.IsReady);

        // Process new data after clear
        var newInputs = Enumerable.Range(10, 21).Select(x => (double)x).ToArray();
        macd.OnBarBatch(newInputs, new double[newInputs.Length]);

        Assert.True(macd.IsReady);
        // Values should be different from before clearing (different starting data)
        Assert.NotEqual(originalMACD, macd.MACD);
    }

    [Fact]
    public void MACD_HandlesConstantValues()
    {
        // Arrange
        var parameters = new PMACD<double, double>
        {
            FastPeriod = 5,
            SlowPeriod = 10,
            SignalPeriod = 3
        };
        var macd = new MACD_FP<double, double>(parameters);

        // Act - Feed constant values
        var inputs = Enumerable.Repeat(100.0, 20).ToArray();
        macd.OnBarBatch(inputs, new double[inputs.Length]);

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
        Assert.Equal(macd.MACD - macd.Signal, macd.Histogram, 10);
    }

    [Fact]
    public void MACD_HistogramIsMACD_MinusSignal()
    {
        // Arrange
        var parameters = new PMACD<double, double>
        {
            FastPeriod = 5,
            SlowPeriod = 10,
            SignalPeriod = 3
        };
        var macd = new MACD_FP<double, double>(parameters);

        // Volatile data
        var inputs = Enumerable.Range(1, 30)
            .Select(x => 100.0 + Math.Sin(x * 0.3) * 10)
            .ToArray();

        // Act
        macd.OnBarBatch(inputs, new double[inputs.Length]);

        // Assert
        Assert.True(macd.IsReady);
        Assert.Equal(macd.MACD - macd.Signal, macd.Histogram, 10);
    }
}
