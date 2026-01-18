using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class SupertrendTests
{
    #region Helpers

    /// <summary>
    /// Creates HLC test data with a trend pattern
    /// </summary>
    private static HLC<double>[] CreateTrendingData(int count, double startPrice, double priceIncrement)
    {
        var inputs = new HLC<double>[count];
        for (int i = 0; i < count; i++)
        {
            var price = startPrice + i * priceIncrement;
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.2
            };
        }
        return inputs;
    }

    /// <summary>
    /// Processes bars one at a time and records state after each bar.
    /// This is needed to test historical trend values since properties only reflect current state.
    /// </summary>
    private static (double[] values, bool[] isUptrend, int[] trendDirection) ProcessAndRecordHistory(
        Supertrend_FP<double, double> indicator,
        HLC<double>[] inputs)
    {
        var values = new double[inputs.Length];
        var isUptrend = new bool[inputs.Length];
        var trendDirection = new int[inputs.Length];

        for (int i = 0; i < inputs.Length; i++)
        {
            var singleInput = new[] { inputs[i] };
            var singleOutput = new double[1];
            indicator.OnBarBatch(singleInput, singleOutput);

            values[i] = singleOutput[0];
            isUptrend[i] = indicator.IsUptrend;
            trendDirection[i] = indicator.TrendDirection;
        }

        return (values, isUptrend, trendDirection);
    }

    #endregion

    [Fact]
    public void Supertrend_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PSupertrend<double, double>
        {
            AtrPeriod = 10,
            Multiplier = 3.0
        };
        var supertrend = new Supertrend_FP<double, double>(parameters);

        // Sample trending data
        var inputs = CreateTrendingData(30, startPrice: 100.0, priceIncrement: 0.5);
        var outputs = new double[inputs.Length];

        // Act
        supertrend.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(supertrend.IsReady);

        // Primary output should have a value (not NaN or 0 for ready indicator)
        var lastOutput = outputs[^1];
        Assert.False(double.IsNaN(lastOutput), "Last output should not be NaN");
        Assert.NotEqual(0, lastOutput);

        // Access additional outputs via properties
        Assert.NotEqual(0, supertrend.Value);
        Assert.True(supertrend.CurrentATR > 0, "ATR should be positive");

        // Trend direction should be set (1 for up, -1 for down)
        Assert.True(supertrend.TrendDirection == 1 || supertrend.TrendDirection == -1);
        Assert.Equal(supertrend.TrendDirection > 0, supertrend.IsUptrend);
    }

    [Fact]
    public void Supertrend_DetectsTrendChanges()
    {
        // Arrange
        var parameters = new PSupertrend<double, double>
        {
            AtrPeriod = 10,
            Multiplier = 3.0
        };
        var supertrend = new Supertrend_FP<double, double>(parameters);

        // Data with trend reversal: uptrend then downtrend
        var inputs = new HLC<double>[60];

        // Uptrend for first 30 bars
        for (int i = 0; i < 30; i++)
        {
            var price = 100.0 + i * 1.0;
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.3
            };
        }

        // Downtrend for next 30 bars
        for (int i = 30; i < 60; i++)
        {
            var price = 130.0 - (i - 30) * 1.0;
            inputs[i] = new HLC<double>
            {
                High = price + 0.3,
                Low = price - 0.5,
                Close = price - 0.3
            };
        }

        // Act - Process bar by bar to track history
        var (values, isUptrend, trendDirection) = ProcessAndRecordHistory(supertrend, inputs);

        // Assert
        Assert.True(supertrend.IsReady);

        // Check trend in uptrend phase (around bar 25)
        Assert.True(isUptrend[25], "Should detect uptrend in first half");

        // Check trend in downtrend phase (around bar 55)
        Assert.False(isUptrend[55], "Should detect downtrend in second half");
    }

    [Fact]
    public void Supertrend_StopLossLevels()
    {
        // Arrange
        var parameters = new PSupertrend<double, double>
        {
            AtrPeriod = 10,
            Multiplier = 3.0
        };
        var supertrend = new Supertrend_FP<double, double>(parameters);

        // Uptrending data
        var inputs = CreateTrendingData(30, startPrice: 100.0, priceIncrement: 0.5);

        // Act - Process bar by bar to track history
        var (values, isUptrend, _) = ProcessAndRecordHistory(supertrend, inputs);

        // Assert
        Assert.True(supertrend.IsReady);

        // In uptrend, Supertrend should be below price (acting as support)
        for (int i = 20; i < inputs.Length; i++)
        {
            if (isUptrend[i] && !double.IsNaN(values[i]) && values[i] != 0)
            {
                Assert.True(values[i] < inputs[i].Close,
                    $"In uptrend at bar {i}, Supertrend {values[i]} should be below close {inputs[i].Close}");
            }
        }
    }

    [Fact]
    public void Supertrend_DifferentMultipliers()
    {
        var multipliers = new[] { 1.0, 2.0, 3.0 };

        // Create volatile data
        var inputs = new HLC<double>[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 10;
            inputs[i] = new HLC<double>
            {
                High = price + 2,
                Low = price - 2,
                Close = price
            };
        }

        var supertrendValues = new Dictionary<double, double>();

        foreach (var multiplier in multipliers)
        {
            // Arrange
            var parameters = new PSupertrend<double, double>
            {
                AtrPeriod = 10,
                Multiplier = multiplier
            };
            var supertrend = new Supertrend_FP<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            supertrend.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(supertrend.IsReady);
            supertrendValues[multiplier] = supertrend.Value;
        }

        // All should have non-zero values
        Assert.True(supertrendValues.All(kv => kv.Value != 0));
    }

    [Fact]
    public void Supertrend_ConsolidationBehavior()
    {
        // Arrange
        var parameters = new PSupertrend<double, double>
        {
            AtrPeriod = 10,
            Multiplier = 2.0
        };
        var supertrend = new Supertrend_FP<double, double>(parameters);

        // Sideways/consolidating data
        var inputs = new HLC<double>[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.5) * 2;
            inputs[i] = new HLC<double>
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }

        // Act - Process bar by bar to track history
        var (_, isUptrend, _) = ProcessAndRecordHistory(supertrend, inputs);

        // Assert
        Assert.True(supertrend.IsReady);

        // Count trend changes in consolidation
        var trendChanges = 0;
        for (int i = 15; i < inputs.Length - 1; i++)
        {
            if (isUptrend[i] != isUptrend[i + 1])
            {
                trendChanges++;
            }
        }

        // Should have some trend changes in consolidation
        Assert.True(trendChanges > 0, "Should have trend changes in consolidating market");
    }

    [Fact]
    public void Supertrend_StrongTrendPersistence()
    {
        // Arrange
        var parameters = new PSupertrend<double, double>
        {
            AtrPeriod = 10,
            Multiplier = 3.0
        };
        var supertrend = new Supertrend_FP<double, double>(parameters);

        // Strong uptrend with minor pullbacks
        var inputs = new HLC<double>[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 2.0; // Strong trend
            // Add minor volatility
            var noise = (i % 3 == 0) ? -1.0 : 0.5;
            inputs[i] = new HLC<double>
            {
                High = price + 1.0,
                Low = price - 0.5,
                Close = price + noise
            };
        }

        // Act - Process bar by bar to track history
        var (_, isUptrend, _) = ProcessAndRecordHistory(supertrend, inputs);

        // Assert
        Assert.True(supertrend.IsReady);

        // Strong trend should maintain direction despite minor pullbacks
        var consistentTrend = true;
        for (int i = 20; i < inputs.Length; i++)
        {
            if (!isUptrend[i])
            {
                consistentTrend = false;
                break;
            }
        }

        Assert.True(consistentTrend, "Strong uptrend should maintain direction");
    }

    [Fact]
    public void Supertrend_DifferentPeriods()
    {
        var periods = new[] { 7, 10, 14 };

        // Create sample data
        var inputs = new HLC<double>[50];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5 + Math.Sin(i * 0.2) * 3;
            inputs[i] = new HLC<double>
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PSupertrend<double, double>
            {
                AtrPeriod = period,
                Multiplier = 3.0
            };
            var supertrend = new Supertrend_FP<double, double>(parameters);
            var outputs = new double[inputs.Length];

            // Act
            supertrend.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(supertrend.IsReady);
            Assert.NotEqual(0, supertrend.Value);

            // Verify ATR period is respected
            Assert.Equal(period, supertrend.AtrPeriod);
        }
    }

    [Fact]
    public void Supertrend_PropertiesMatchOutputArray()
    {
        // Arrange
        var parameters = new PSupertrend<double, double>
        {
            AtrPeriod = 10,
            Multiplier = 3.0
        };
        var supertrend = new Supertrend_FP<double, double>(parameters);
        var inputs = CreateTrendingData(30, startPrice: 100.0, priceIncrement: 0.5);
        var outputs = new double[inputs.Length];

        // Act
        supertrend.OnBarBatch(inputs, outputs);

        // Assert - The Value property should match the last output
        Assert.True(supertrend.IsReady);
        Assert.Equal(supertrend.Value, outputs[^1]);
    }

    [Fact]
    public void Supertrend_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PSupertrend<double, double>
        {
            AtrPeriod = 10,
            Multiplier = 3.0
        };
        var supertrend = new Supertrend_FP<double, double>(parameters);
        var inputs = CreateTrendingData(30, startPrice: 100.0, priceIncrement: 0.5);
        var outputs = new double[inputs.Length];

        // First run
        supertrend.OnBarBatch(inputs, outputs);
        Assert.True(supertrend.IsReady);
        var firstValue = supertrend.Value;

        // Act - Clear and verify reset
        supertrend.Clear();

        // Assert - Should no longer be ready
        Assert.False(supertrend.IsReady);

        // Process again - should get same result
        var outputs2 = new double[inputs.Length];
        supertrend.OnBarBatch(inputs, outputs2);
        Assert.True(supertrend.IsReady);
        Assert.Equal(firstValue, supertrend.Value);
    }
}
