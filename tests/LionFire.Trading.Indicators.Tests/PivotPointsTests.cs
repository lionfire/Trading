using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class PivotPointsTests
{
    [Fact]
    public void PivotPoints_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PPivotPoints<OHLC<double>, double>
        {
            PeriodType = PivotPointsPeriod.Daily
        };
        var pivots = new PivotPoints_FP<OHLC<double>, double>(parameters);

        // Sample daily data
        var inputs = new OHLC<double>[]
        {
            new() { Open = 100, High = 105, Low = 95, Close = 100 }, // Day 1
            new() { Open = 100, High = 108, Low = 98, Close = 103 }, // Day 2
            new() { Open = 103, High = 110, Low = 100, Close = 107 }, // Day 3
            new() { Open = 107, High = 112, Low = 102, Close = 109 }, // Day 4
            new() { Open = 109, High = 115, Low = 105, Close = 112 }, // Day 5
        };

        var outputs = new double[inputs.Length * 7]; // 7 outputs per bar

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);

        // Pivot point should be calculated
        Assert.True(pivots.PivotPoint > 0);

        // R1 should be above pivot
        Assert.True(pivots.Resistance1 > pivots.PivotPoint);

        // S1 should be below pivot
        Assert.True(pivots.Support1 < pivots.PivotPoint);
    }

    [Fact]
    public void PivotPoints_SupportResistanceLevels()
    {
        // Arrange
        var parameters = new PPivotPoints<OHLC<double>, double>
        {
            PeriodType = PivotPointsPeriod.Daily
        };
        var pivots = new PivotPoints_FP<OHLC<double>, double>(parameters);

        // Sample data
        var inputs = new OHLC<double>[]
        {
            new() { Open = 100, High = 105, Low = 95, Close = 100 },
            new() { Open = 100, High = 110, Low = 98, Close = 105 },
            new() { Open = 105, High = 108, Low = 96, Close = 102 },
            new() { Open = 102, High = 112, Low = 100, Close = 108 },
            new() { Open = 108, High = 115, Low = 103, Close = 110 },
        };

        var outputs = new double[inputs.Length * 7];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);

        // Verify ordering: S3 < S2 < S1 < Pivot < R1 < R2 < R3
        Assert.True(pivots.Support3 < pivots.Support2);
        Assert.True(pivots.Support2 < pivots.Support1);
        Assert.True(pivots.Support1 < pivots.PivotPoint);
        Assert.True(pivots.PivotPoint < pivots.Resistance1);
        Assert.True(pivots.Resistance1 < pivots.Resistance2);
        Assert.True(pivots.Resistance2 < pivots.Resistance3);
    }

    [Fact]
    public void PivotPoints_IntraDay()
    {
        // Arrange
        var parameters = new PPivotPoints<OHLC<double>, double>
        {
            PeriodType = PivotPointsPeriod.Daily
        };
        var pivots = new PivotPoints_FP<OHLC<double>, double>(parameters);

        // Simulate intraday data (hourly bars)
        var inputs = new OHLC<double>[24];
        for (int i = 0; i < inputs.Length; i++)
        {
            var basePrice = 100.0 + Math.Sin(i * 0.26) * 5; // Intraday volatility
            inputs[i] = new OHLC<double>
            {
                Open = basePrice - 0.2,
                High = basePrice + 0.5,
                Low = basePrice - 0.5,
                Close = basePrice + (i % 2 == 0 ? 0.2 : -0.2)
            };
        }

        var outputs = new double[inputs.Length * 7];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);

        // Each bar should have pivot levels calculated
        Assert.True(pivots.PivotPoint > 0);
        Assert.True(pivots.Resistance1 > pivots.PivotPoint);
        Assert.True(pivots.Support1 < pivots.PivotPoint);
    }

    [Fact]
    public void PivotPoints_TrendingMarket()
    {
        // Arrange
        var parameters = new PPivotPoints<OHLC<double>, double>
        {
            PeriodType = PivotPointsPeriod.Daily
        };
        var pivots = new PivotPoints_FP<OHLC<double>, double>(parameters);

        // Strong uptrend
        var inputs = new OHLC<double>[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 2.0;
            inputs[i] = new OHLC<double>
            {
                Open = price - 0.5,
                High = price + 1,
                Low = price - 0.5,
                Close = price + 0.8
            };
        }

        var outputs = new double[inputs.Length * 7];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);

        // Price should be above pivot in uptrend
        var lastPrice = inputs[inputs.Length - 1].Close;
        Assert.True(lastPrice > pivots.PivotPoint);
    }

    [Fact]
    public void PivotPoints_RangeBoundMarket()
    {
        // Arrange
        var parameters = new PPivotPoints<OHLC<double>, double>
        {
            PeriodType = PivotPointsPeriod.Daily
        };
        var pivots = new PivotPoints_FP<OHLC<double>, double>(parameters);

        // Range-bound data
        var inputs = new OHLC<double>[15];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + Math.Sin(i * 0.5) * 3;
            inputs[i] = new OHLC<double>
            {
                Open = price - 0.3,
                High = price + 0.8,
                Low = price - 0.8,
                Close = price + 0.1
            };
        }

        var outputs = new double[inputs.Length * 7];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);

        // Pivot should be near the center of the range
        Assert.InRange(pivots.PivotPoint, 95, 105);
    }

    [Fact]
    public void PivotPoints_HighVolatility()
    {
        // Arrange
        var parameters = new PPivotPoints<OHLC<double>, double>
        {
            PeriodType = PivotPointsPeriod.Daily
        };
        var pivots = new PivotPoints_FP<OHLC<double>, double>(parameters);

        // High volatility data
        var inputs = new OHLC<double>[]
        {
            new() { Open = 100, High = 120, Low = 80, Close = 95 },
            new() { Open = 95, High = 130, Low = 70, Close = 110 },
            new() { Open = 110, High = 140, Low = 85, Close = 100 },
        };

        var outputs = new double[inputs.Length * 7];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);

        // Support and resistance levels should be wider apart for high volatility
        var range = pivots.Resistance3 - pivots.Support3;
        Assert.True(range > 30, $"Range {range} should be significant for high volatility");
    }

    [Fact]
    public void PivotPoints_MultipleDataPoints()
    {
        // Arrange
        var parameters = new PPivotPoints<OHLC<double>, double>
        {
            PeriodType = PivotPointsPeriod.Daily
        };
        var pivots = new PivotPoints_FP<OHLC<double>, double>(parameters);

        // Extended data set
        var inputs = new OHLC<double>[30];
        for (int i = 0; i < inputs.Length; i++)
        {
            var basePrice = 100.0 + i * 0.5;
            inputs[i] = new OHLC<double>
            {
                Open = basePrice,
                High = basePrice + 2 + Math.Sin(i) * 1,
                Low = basePrice - 2 - Math.Sin(i) * 1,
                Close = basePrice + 1
            };
        }

        var outputs = new double[inputs.Length * 7];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);

        // All levels should have valid values
        Assert.False(double.IsNaN(pivots.PivotPoint));
        Assert.False(double.IsNaN(pivots.Resistance1));
        Assert.False(double.IsNaN(pivots.Resistance2));
        Assert.False(double.IsNaN(pivots.Resistance3));
        Assert.False(double.IsNaN(pivots.Support1));
        Assert.False(double.IsNaN(pivots.Support2));
        Assert.False(double.IsNaN(pivots.Support3));
    }

    [Fact]
    public void PivotPoints_Clear_ResetsState()
    {
        // Arrange
        var parameters = new PPivotPoints<OHLC<double>, double>
        {
            PeriodType = PivotPointsPeriod.Daily
        };
        var pivots = new PivotPoints_FP<OHLC<double>, double>(parameters);

        var inputs = new OHLC<double>[]
        {
            new() { Open = 100, High = 105, Low = 95, Close = 100 },
            new() { Open = 100, High = 108, Low = 98, Close = 105 },
        };

        pivots.OnBarBatch(inputs, new double[inputs.Length * 7]);
        Assert.True(pivots.IsReady);

        // Act
        pivots.Clear();

        // Assert
        Assert.False(pivots.IsReady);
    }
}
