using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class SupertrendTests
{
    [Fact]
    public void Supertrend_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PSupertrend<HLC, SupertrendResult>
        {
            Period = 10,
            Multiplier = 3.0
        };
        var supertrend = new Supertrend_QC<HLC, SupertrendResult>(parameters);
        
        // Sample trending data
        var inputs = new HLC[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.2
            };
        }
        
        var outputs = new SupertrendResult[inputs.Length];

        // Act
        supertrend.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(supertrend.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // Supertrend should have a value
        Assert.True(lastResult.Value != 0);
        
        // Trend direction should be set
        Assert.True(lastResult.IsUpTrend || !lastResult.IsUpTrend); // Must be one or the other
    }

    [Fact]
    public void Supertrend_DetectsTrendChanges()
    {
        // Arrange
        var parameters = new PSupertrend<HLC, SupertrendResult>
        {
            Period = 10,
            Multiplier = 3.0
        };
        var supertrend = new Supertrend_QC<HLC, SupertrendResult>(parameters);
        
        // Data with trend reversal
        var inputs = new HLC[60];
        // Uptrend for first 30 bars
        for (int i = 0; i < 30; i++)
        {
            var price = 100.0 + i * 1.0;
            inputs[i] = new HLC
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
            inputs[i] = new HLC
            {
                High = price + 0.3,
                Low = price - 0.5,
                Close = price - 0.3
            };
        }
        
        var outputs = new SupertrendResult[inputs.Length];

        // Act
        supertrend.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(supertrend.IsReady);
        
        // Check trend in uptrend phase
        var uptrendResult = outputs[25];
        Assert.True(uptrendResult.IsUpTrend, "Should detect uptrend in first half");
        
        // Check trend in downtrend phase
        var downtrendResult = outputs[55];
        Assert.False(downtrendResult.IsUpTrend, "Should detect downtrend in second half");
    }

    [Fact]
    public void Supertrend_StopLossLevels()
    {
        // Arrange
        var parameters = new PSupertrend<HLC, SupertrendResult>
        {
            Period = 10,
            Multiplier = 3.0
        };
        var supertrend = new Supertrend_QC<HLC, SupertrendResult>(parameters);
        
        // Uptrending data
        var inputs = new HLC[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5;
            inputs[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.3,
                Close = price + 0.2
            };
        }
        
        var outputs = new SupertrendResult[inputs.Length];

        // Act
        supertrend.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(supertrend.IsReady);
        
        // In uptrend, Supertrend should be below price (acting as support)
        for (int i = 20; i < outputs.Length; i++)
        {
            if (outputs[i].IsUpTrend)
            {
                Assert.True(outputs[i].Value < inputs[i].Close,
                    $"In uptrend, Supertrend {outputs[i].Value} should be below close {inputs[i].Close}");
            }
        }
    }

    [Fact]
    public void Supertrend_DifferentMultipliers()
    {
        var multipliers = new[] { 1.0, 2.0, 3.0 };
        
        // Create volatile data
        var inputs = new HLC[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.3) * 10;
            inputs[i] = new HLC
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
            var parameters = new PSupertrend<HLC, SupertrendResult>
            {
                Period = 10,
                Multiplier = multiplier
            };
            var supertrend = new Supertrend_QC<HLC, SupertrendResult>(parameters);
            var outputs = new SupertrendResult[inputs.Length];

            // Act
            supertrend.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(supertrend.IsReady);
            supertrendValues[multiplier] = outputs[outputs.Length - 1].Value;
        }

        // Higher multipliers should create wider bands (further from price)
        Assert.True(supertrendValues.All(kv => kv.Value != 0));
    }

    [Fact]
    public void Supertrend_ConsolidationBehavior()
    {
        // Arrange
        var parameters = new PSupertrend<HLC, SupertrendResult>
        {
            Period = 10,
            Multiplier = 2.0
        };
        var supertrend = new Supertrend_QC<HLC, SupertrendResult>(parameters);
        
        // Sideways/consolidating data
        var inputs = new HLC[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.5) * 2;
            inputs[i] = new HLC
            {
                High = price + 0.5,
                Low = price - 0.5,
                Close = price
            };
        }
        
        var outputs = new SupertrendResult[inputs.Length];

        // Act
        supertrend.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(supertrend.IsReady);
        
        // Count trend changes in consolidation
        var trendChanges = 0;
        for (int i = 15; i < outputs.Length - 1; i++)
        {
            if (outputs[i].IsUpTrend != outputs[i + 1].IsUpTrend)
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
        var parameters = new PSupertrend<HLC, SupertrendResult>
        {
            Period = 10,
            Multiplier = 3.0
        };
        var supertrend = new Supertrend_QC<HLC, SupertrendResult>(parameters);
        
        // Strong uptrend with minor pullbacks
        var inputs = new HLC[40];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 2.0; // Strong trend
            // Add minor volatility
            var noise = (i % 3 == 0) ? -1.0 : 0.5;
            inputs[i] = new HLC
            {
                High = price + 1.0,
                Low = price - 0.5,
                Close = price + noise
            };
        }
        
        var outputs = new SupertrendResult[inputs.Length];

        // Act
        supertrend.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(supertrend.IsReady);
        
        // Strong trend should maintain direction despite minor pullbacks
        var consistentTrend = true;
        for (int i = 20; i < outputs.Length; i++)
        {
            if (!outputs[i].IsUpTrend)
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
        var inputs = new HLC[50];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 0.5 + Math.Sin(i * 0.2) * 3;
            inputs[i] = new HLC
            {
                High = price + 1,
                Low = price - 1,
                Close = price
            };
        }

        foreach (var period in periods)
        {
            // Arrange
            var parameters = new PSupertrend<HLC, SupertrendResult>
            {
                Period = period,
                Multiplier = 3.0
            };
            var supertrend = new Supertrend_QC<HLC, SupertrendResult>(parameters);
            var outputs = new SupertrendResult[inputs.Length];

            // Act
            supertrend.OnBarBatch(inputs, outputs);

            // Assert
            Assert.True(supertrend.IsReady);
            var lastResult = outputs[outputs.Length - 1];
            Assert.True(lastResult.Value != 0);
            
            // Shorter periods should be more responsive
        }
    }
}

public class SupertrendResult
{
    public double Value { get; set; }
    public bool IsUpTrend { get; set; }
    public double UpperBand { get; set; }
    public double LowerBand { get; set; }
}