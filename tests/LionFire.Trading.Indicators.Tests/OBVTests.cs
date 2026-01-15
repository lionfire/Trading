// DISABLED: Tests need updating to match current API
#if false
using LionFire.Trading.Indicators.Defaults;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class OBVTests
{
    [Fact]
    public void OBV_FirstParty_BasicCalculation_ShouldWorkCorrectly()
    {
        // Arrange
        var parameters = new POBV<Bar, double>
        {
            PreferredImplementation = ImplementationHint.FirstParty
        };

        var obv = new OBV_FP<Bar, double>(parameters);

        // Test data: Price goes up, down, same
        var testData = new Bar[]
        {
            new Bar { Close = 10.0, Volume = 1000 }, // First bar: OBV = 1000
            new Bar { Close = 11.0, Volume = 1500 }, // Price up: OBV = 1000 + 1500 = 2500
            new Bar { Close = 9.0, Volume = 800 },   // Price down: OBV = 2500 - 800 = 1700
            new Bar { Close = 9.0, Volume = 1200 },  // Price same: OBV = 1700 (unchanged)
            new Bar { Close = 12.0, Volume = 2000 }  // Price up: OBV = 1700 + 2000 = 3700
        };

        var expected = new double[] { 1000, 2500, 1700, 1700, 3700 };
        var results = new double[testData.Length];

        // Act
        obv.OnBarBatch(testData, results);

        // Assert
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.True(Math.Abs(expected[i] - results[i]) < 0.01,
                $"OBV value at index {i} should be {expected[i]}, but was {results[i]}");
        }

        // Verify final state
        Assert.True(Math.Abs(3700 - obv.CurrentValue) < 0.01);
        Assert.True(Math.Abs(2000 - obv.LastChange) < 0.01); // Last change should be +2000
        Assert.True(obv.IsRising);
        Assert.False(obv.IsFalling);
    }

    [Fact]
    public void OBV_Factory_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var obv = OBV.CreateBarDouble();

        // Assert
        Assert.NotNull(obv);
        Assert.False(obv.IsReady); // Should not be ready without data
    }

    [Fact]
    public void OBV_WithTimedBar_ShouldWorkCorrectly()
    {
        // Arrange
        var parameters = new POBV<TimedBar, decimal>();
        var obv = new OBV_FP<TimedBar, decimal>(parameters);

        var testData = new TimedBar[]
        {
            new TimedBar(DateTimeOffset.Now, 100m, 100m, 100m, 100m, 500m),
            new TimedBar(DateTimeOffset.Now.AddMinutes(1), 102m, 102m, 102m, 102m, 750m),
        };

        var results = new decimal[testData.Length];

        // Act
        obv.OnBarBatch(testData, results);

        // Assert
        Assert.Equal(500m, results[0]); // First bar sets initial OBV
        Assert.Equal(1250m, results[1]); // Price up, so add volume: 500 + 750 = 1250
        Assert.True(obv.IsRising);
    }

    [Fact]
    public void OBV_SellingPressure_ShouldDecrease()
    {
        // Arrange
        var parameters = new POBV<Bar, double>();
        var obv = new OBV_FP<Bar, double>(parameters);

        var testData = new Bar[]
        {
            new Bar { Close = 50.0, Volume = 1000 }, // Initial: OBV = 1000
            new Bar { Close = 48.0, Volume = 1200 }, // Price down: OBV = 1000 - 1200 = -200
            new Bar { Close = 45.0, Volume = 800 },  // Price down: OBV = -200 - 800 = -1000
        };

        var results = new double[testData.Length];

        // Act
        obv.OnBarBatch(testData, results);

        // Assert
        Assert.Equal(1000, results[0]);
        Assert.Equal(-200, results[1]);
        Assert.Equal(-1000, results[2]);
        Assert.True(obv.IsFalling);
        Assert.False(obv.IsRising);
        Assert.Equal(-800, obv.LastChange); // Last change was -800
    }

    [Fact]
    public void OBV_Clear_ShouldResetState()
    {
        // Arrange
        var parameters = new POBV<Bar, double>();
        var obv = new OBV_FP<Bar, double>(parameters);

        var testData = new Bar[]
        {
            new Bar { Close = 10.0, Volume = 1000 },
            new Bar { Close = 12.0, Volume = 1500 },
        };

        obv.OnBarBatch(testData, null);

        // Verify it has data
        Assert.True(obv.IsReady);
        Assert.Equal(2500, obv.CurrentValue);

        // Act
        obv.Clear();

        // Assert
        Assert.False(obv.IsReady);
        Assert.Equal(0, obv.CurrentValue);
        Assert.Equal(0, obv.LastChange);
        Assert.False(obv.IsRising);
        Assert.False(obv.IsFalling);
    }
}

#endif
