using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;

namespace LionFire.Trading.Indicators.Tests;

public class ChandelierExitTests
{
    #region Test Data

    private static HLC<double>[] GetSampleData() =>
    [
        new() { High = 48.70, Low = 47.79, Close = 48.16 },
        new() { High = 48.72, Low = 48.14, Close = 48.61 },
        new() { High = 48.90, Low = 48.39, Close = 48.75 },
        new() { High = 48.87, Low = 48.37, Close = 48.63 },
        new() { High = 48.82, Low = 48.24, Close = 48.74 },
        new() { High = 49.05, Low = 48.64, Close = 49.03 },
        new() { High = 49.20, Low = 48.94, Close = 49.07 },
        new() { High = 49.35, Low = 48.86, Close = 49.32 },
        new() { High = 49.92, Low = 49.50, Close = 49.91 },
        new() { High = 50.19, Low = 49.87, Close = 50.13 },
        new() { High = 50.12, Low = 49.20, Close = 49.53 },
        new() { High = 49.66, Low = 48.90, Close = 49.50 },
        new() { High = 49.88, Low = 49.43, Close = 49.75 },
        new() { High = 50.19, Low = 49.73, Close = 50.03 },
        new() { High = 50.36, Low = 49.26, Close = 50.31 },
        new() { High = 50.57, Low = 50.09, Close = 50.52 },
        new() { High = 50.65, Low = 50.30, Close = 50.41 },
        new() { High = 50.43, Low = 49.21, Close = 49.34 },
        new() { High = 49.63, Low = 48.98, Close = 49.37 },
        new() { High = 50.33, Low = 49.61, Close = 50.23 },
        new() { High = 50.29, Low = 49.20, Close = 49.24 },
        new() { High = 50.17, Low = 49.43, Close = 49.93 },
        new() { High = 49.32, Low = 48.08, Close = 48.43 },
        new() { High = 48.50, Low = 47.64, Close = 48.18 },
        new() { High = 48.32, Low = 41.55, Close = 46.57 },
        new() { High = 46.80, Low = 44.28, Close = 45.41 },
        new() { High = 47.80, Low = 47.31, Close = 47.77 },
        new() { High = 48.39, Low = 47.20, Close = 47.72 },
        new() { High = 48.66, Low = 47.90, Close = 48.62 },
        new() { High = 48.79, Low = 47.73, Close = 47.85 }
    ];

    #endregion

    #region Native (FP) Implementation Tests

    [Fact]
    public void ChandelierExit_FP_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PChandelierExit<double, double> { Period = 22, AtrMultiplier = 3.0 };
        var indicator = ChandelierExit_FP<double, double>.Create(parameters);
        var inputs = GetSampleData();
        var outputs = new double[inputs.Length];

        // Act
        indicator.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(indicator.IsReady);
        Assert.True(indicator.ExitLong > 0, "ExitLong should be positive");
        Assert.True(indicator.ExitShort > 0, "ExitShort should be positive");
        Assert.True(indicator.CurrentATR > 0, "ATR should be positive");
        Assert.True(indicator.HighestHigh > indicator.LowestLow, "HighestHigh should be greater than LowestLow");
    }

    [Fact]
    public void ChandelierExit_FP_ExitLongBelowHighest()
    {
        // Arrange
        var parameters = new PChandelierExit<double, double> { Period = 10, AtrMultiplier = 2.0 };
        var indicator = ChandelierExit_FP<double, double>.Create(parameters);
        var inputs = GetSampleData();
        var outputs = new double[inputs.Length];

        // Act
        indicator.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(indicator.IsReady);
        // Exit Long should always be below the highest high (by ATR * multiplier)
        Assert.True(indicator.ExitLong < indicator.HighestHigh,
            $"ExitLong ({indicator.ExitLong}) should be less than HighestHigh ({indicator.HighestHigh})");
    }

    [Fact]
    public void ChandelierExit_FP_ExitShortAboveLowest()
    {
        // Arrange
        var parameters = new PChandelierExit<double, double> { Period = 10, AtrMultiplier = 2.0 };
        var indicator = ChandelierExit_FP<double, double>.Create(parameters);
        var inputs = GetSampleData();
        var outputs = new double[inputs.Length];

        // Act
        indicator.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(indicator.IsReady);
        // Exit Short should always be above the lowest low (by ATR * multiplier)
        Assert.True(indicator.ExitShort > indicator.LowestLow,
            $"ExitShort ({indicator.ExitShort}) should be greater than LowestLow ({indicator.LowestLow})");
    }

    [Fact]
    public void ChandelierExit_FP_DifferentMultipliers()
    {
        // Arrange
        var inputs = GetSampleData();
        var parameters1 = new PChandelierExit<double, double> { Period = 14, AtrMultiplier = 2.0 };
        var parameters2 = new PChandelierExit<double, double> { Period = 14, AtrMultiplier = 3.0 };
        var parameters3 = new PChandelierExit<double, double> { Period = 14, AtrMultiplier = 4.0 };

        var indicator1 = ChandelierExit_FP<double, double>.Create(parameters1);
        var indicator2 = ChandelierExit_FP<double, double>.Create(parameters2);
        var indicator3 = ChandelierExit_FP<double, double>.Create(parameters3);

        var outputs1 = new double[inputs.Length];
        var outputs2 = new double[inputs.Length];
        var outputs3 = new double[inputs.Length];

        // Act
        indicator1.OnBarBatch(inputs, outputs1);
        indicator2.OnBarBatch(inputs, outputs2);
        indicator3.OnBarBatch(inputs, outputs3);

        // Assert - Higher multiplier = wider stops
        // Exit Long: Higher multiplier means lower exit (further from high)
        Assert.True(indicator1.ExitLong > indicator2.ExitLong,
            "Higher multiplier should produce lower ExitLong");
        Assert.True(indicator2.ExitLong > indicator3.ExitLong,
            "Higher multiplier should produce lower ExitLong");

        // Exit Short: Higher multiplier means higher exit (further from low)
        Assert.True(indicator1.ExitShort < indicator2.ExitShort,
            "Higher multiplier should produce higher ExitShort");
        Assert.True(indicator2.ExitShort < indicator3.ExitShort,
            "Higher multiplier should produce higher ExitShort");
    }

    [Fact]
    public void ChandelierExit_FP_ClearResetsState()
    {
        // Arrange
        var parameters = new PChandelierExit<double, double> { Period = 10, AtrMultiplier = 3.0 };
        var indicator = ChandelierExit_FP<double, double>.Create(parameters);
        var inputs = GetSampleData();
        var outputs = new double[inputs.Length];

        indicator.OnBarBatch(inputs, outputs);
        Assert.True(indicator.IsReady);

        // Act
        indicator.Clear();

        // Assert
        Assert.False(indicator.IsReady);
    }

    #endregion

    #region QuantConnect (QC) Implementation Tests

    [Fact]
    public void ChandelierExit_QC_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PChandelierExit<double, double> { Period = 22, AtrMultiplier = 3.0 };
        var indicator = ChandelierExit_QC<double, double>.Create(parameters);
        var inputs = GetSampleData();
        var outputs = new double[inputs.Length];

        // Act
        indicator.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(indicator.IsReady);
        Assert.True(indicator.ExitLong > 0, "ExitLong should be positive");
        Assert.True(indicator.ExitShort > 0, "ExitShort should be positive");
        Assert.True(indicator.CurrentATR > 0, "ATR should be positive");
    }

    [Fact]
    public void ChandelierExit_QC_ClearResetsState()
    {
        // Arrange
        var parameters = new PChandelierExit<double, double> { Period = 10, AtrMultiplier = 3.0 };
        var indicator = ChandelierExit_QC<double, double>.Create(parameters);
        var inputs = GetSampleData();
        var outputs = new double[inputs.Length];

        indicator.OnBarBatch(inputs, outputs);
        Assert.True(indicator.IsReady);

        // Act
        indicator.Clear();

        // Assert
        Assert.False(indicator.IsReady);
    }

    #endregion

    #region Cross-Implementation Comparison Tests

    [Fact]
    public void ChandelierExit_FP_QC_ProduceSimilarResults()
    {
        // Arrange
        var parameters = new PChandelierExit<double, double> { Period = 14, AtrMultiplier = 3.0 };
        var fpIndicator = ChandelierExit_FP<double, double>.Create(parameters);
        var qcIndicator = ChandelierExit_QC<double, double>.Create(parameters);
        var inputs = GetSampleData();
        var fpOutputs = new double[inputs.Length];
        var qcOutputs = new double[inputs.Length];

        // Act
        fpIndicator.OnBarBatch(inputs, fpOutputs);
        qcIndicator.OnBarBatch(inputs, qcOutputs);

        // Assert
        Assert.True(fpIndicator.IsReady);
        Assert.True(qcIndicator.IsReady);

        // Both implementations should produce similar ATR values (within tolerance)
        var atrDifference = Math.Abs(fpIndicator.CurrentATR - qcIndicator.CurrentATR);
        Assert.True(atrDifference < 0.1,
            $"ATR values should be similar. FP: {fpIndicator.CurrentATR}, QC: {qcIndicator.CurrentATR}, Diff: {atrDifference}");

        // Exit values should also be similar
        var exitLongDiff = Math.Abs(fpIndicator.ExitLong - qcIndicator.ExitLong);
        Assert.True(exitLongDiff < 0.5,
            $"ExitLong values should be similar. FP: {fpIndicator.ExitLong}, QC: {qcIndicator.ExitLong}, Diff: {exitLongDiff}");

        var exitShortDiff = Math.Abs(fpIndicator.ExitShort - qcIndicator.ExitShort);
        Assert.True(exitShortDiff < 0.5,
            $"ExitShort values should be similar. FP: {fpIndicator.ExitShort}, QC: {qcIndicator.ExitShort}, Diff: {exitShortDiff}");
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void ChandelierExit_NotReadyWithInsufficientData()
    {
        // Arrange
        var parameters = new PChandelierExit<double, double> { Period = 22, AtrMultiplier = 3.0 };
        var indicator = ChandelierExit_FP<double, double>.Create(parameters);
        var inputs = new HLC<double>[10]; // Less than period
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = new HLC<double> { High = 100 + i, Low = 99 + i, Close = 99.5 + i };
        }
        var outputs = new double[inputs.Length];

        // Act
        indicator.OnBarBatch(inputs, outputs);

        // Assert
        Assert.False(indicator.IsReady);
    }

    [Fact]
    public void ChandelierExit_ParameterValidation()
    {
        // Period must be at least 2
        Assert.Throws<ArgumentException>(() =>
        {
            var parameters = new PChandelierExit<double, double> { Period = 1, AtrMultiplier = 3.0 };
            ChandelierExit_FP<double, double>.Create(parameters);
        });

        // Multiplier must be positive
        Assert.Throws<ArgumentException>(() =>
        {
            var parameters = new PChandelierExit<double, double> { Period = 14, AtrMultiplier = 0 };
            ChandelierExit_FP<double, double>.Create(parameters);
        });

        Assert.Throws<ArgumentException>(() =>
        {
            var parameters = new PChandelierExit<double, double> { Period = 14, AtrMultiplier = -1 };
            ChandelierExit_FP<double, double>.Create(parameters);
        });
    }

    [Fact]
    public void ChandelierExit_FlatMarket()
    {
        // Arrange - flat market with minimal volatility
        var parameters = new PChandelierExit<double, double> { Period = 10, AtrMultiplier = 3.0 };
        var indicator = ChandelierExit_FP<double, double>.Create(parameters);
        var inputs = new HLC<double>[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = new HLC<double>
            {
                High = 100.05,
                Low = 99.95,
                Close = 100.0
            };
        }
        var outputs = new double[inputs.Length];

        // Act
        indicator.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(indicator.IsReady);
        // In a flat market, ATR should be very low
        Assert.True(indicator.CurrentATR < 0.5, "ATR should be low in flat market");
        // Exit lines should be close together
        var exitSpread = indicator.HighestHigh - indicator.ExitLong;
        Assert.True(exitSpread < 1, "Exit spread should be small in flat market");
    }

    [Fact]
    public void ChandelierExit_HighVolatility()
    {
        // Arrange - high volatility market
        var parameters = new PChandelierExit<double, double> { Period = 10, AtrMultiplier = 3.0 };
        var indicator = ChandelierExit_FP<double, double>.Create(parameters);
        var inputs = new HLC<double>[20];
        var random = new Random(42);
        for (int i = 0; i < inputs.Length; i++)
        {
            var basePrice = 100.0 + i * 2;
            var volatility = random.NextDouble() * 10;
            inputs[i] = new HLC<double>
            {
                High = basePrice + volatility,
                Low = basePrice - volatility,
                Close = basePrice + (random.NextDouble() - 0.5) * volatility * 2
            };
        }
        var outputs = new double[inputs.Length];

        // Act
        indicator.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(indicator.IsReady);
        // In a volatile market, ATR should be higher
        Assert.True(indicator.CurrentATR > 1, "ATR should be higher in volatile market");
    }

    #endregion
}
