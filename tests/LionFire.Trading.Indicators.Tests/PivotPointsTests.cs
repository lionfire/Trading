using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueTypes;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class PivotPointsTests
{
    [Fact]
    public void PivotPoints_CalculatesStandardPivots()
    {
        // Arrange
        var parameters = new PPivotPoints<HLC, PivotPointsResult>
        {
            PivotType = PivotPointType.Standard
        };
        var pivots = new PivotPoints_QC<HLC, PivotPointsResult>(parameters);
        
        // Sample daily data (previous day's HLC)
        var inputs = new HLC[]
        {
            new() { High = 105, Low = 95, Close = 100 }, // Day 1
            new() { High = 108, Low = 98, Close = 103 },  // Day 2
            new() { High = 110, Low = 100, Close = 107 }, // Day 3
            new() { High = 112, Low = 102, Close = 109 }, // Day 4
            new() { High = 115, Low = 105, Close = 112 }, // Day 5
        };
        
        var outputs = new PivotPointsResult[inputs.Length];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // Standard Pivot = (H + L + C) / 3
        var expectedPivot = (115 + 105 + 112) / 3.0;
        Assert.Equal(expectedPivot, lastResult.Pivot, 2);
        
        // R1 = 2 * Pivot - Low
        var expectedR1 = 2 * expectedPivot - 105;
        Assert.Equal(expectedR1, lastResult.R1, 2);
        
        // S1 = 2 * Pivot - High
        var expectedS1 = 2 * expectedPivot - 115;
        Assert.Equal(expectedS1, lastResult.S1, 2);
        
        // R2 = Pivot + (High - Low)
        var expectedR2 = expectedPivot + (115 - 105);
        Assert.Equal(expectedR2, lastResult.R2, 2);
        
        // S2 = Pivot - (High - Low)
        var expectedS2 = expectedPivot - (115 - 105);
        Assert.Equal(expectedS2, lastResult.S2, 2);
    }

    [Fact]
    public void PivotPoints_CalculatesFibonacciPivots()
    {
        // Arrange
        var parameters = new PPivotPoints<HLC, PivotPointsResult>
        {
            PivotType = PivotPointType.Fibonacci
        };
        var pivots = new PivotPoints_QC<HLC, PivotPointsResult>(parameters);
        
        // Sample data
        var inputs = new HLC[]
        {
            new() { High = 110, Low = 90, Close = 100 },
            new() { High = 115, Low = 95, Close = 105 },
            new() { High = 120, Low = 100, Close = 110 },
        };
        
        var outputs = new PivotPointsResult[inputs.Length];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // Fibonacci Pivot = (H + L + C) / 3
        var pivot = (120 + 100 + 110) / 3.0;
        var range = 120 - 100;
        
        Assert.Equal(pivot, lastResult.Pivot, 2);
        
        // R1 = Pivot + 0.382 * Range
        Assert.Equal(pivot + 0.382 * range, lastResult.R1, 2);
        
        // S1 = Pivot - 0.382 * Range
        Assert.Equal(pivot - 0.382 * range, lastResult.S1, 2);
        
        // R2 = Pivot + 0.618 * Range
        Assert.Equal(pivot + 0.618 * range, lastResult.R2, 2);
        
        // S2 = Pivot - 0.618 * Range
        Assert.Equal(pivot - 0.618 * range, lastResult.S2, 2);
        
        // R3 = Pivot + 1.000 * Range
        Assert.Equal(pivot + range, lastResult.R3, 2);
        
        // S3 = Pivot - 1.000 * Range
        Assert.Equal(pivot - range, lastResult.S3, 2);
    }

    [Fact]
    public void PivotPoints_CalculatesCamarillaPivots()
    {
        // Arrange
        var parameters = new PPivotPoints<HLC, PivotPointsResult>
        {
            PivotType = PivotPointType.Camarilla
        };
        var pivots = new PivotPoints_QC<HLC, PivotPointsResult>(parameters);
        
        // Sample data
        var inputs = new HLC[]
        {
            new() { High = 100, Low = 90, Close = 95 },
            new() { High = 105, Low = 93, Close = 100 },
            new() { High = 110, Low = 96, Close = 105 },
        };
        
        var outputs = new PivotPointsResult[inputs.Length];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        var range = 110 - 96;
        var close = 105;
        
        // Camarilla calculations
        // R1 = Close + Range * 1.1/12
        Assert.Equal(close + range * 1.1 / 12, lastResult.R1, 2);
        
        // S1 = Close - Range * 1.1/12
        Assert.Equal(close - range * 1.1 / 12, lastResult.S1, 2);
    }

    [Fact]
    public void PivotPoints_CalculatesWoodiePivots()
    {
        // Arrange
        var parameters = new PPivotPoints<HLC, PivotPointsResult>
        {
            PivotType = PivotPointType.Woodie
        };
        var pivots = new PivotPoints_QC<HLC, PivotPointsResult>(parameters);
        
        // Sample data
        var inputs = new HLC[]
        {
            new() { High = 102, Low = 98, Close = 100 },
            new() { High = 106, Low = 99, Close = 104 },
            new() { High = 108, Low = 101, Close = 106 },
        };
        
        var outputs = new PivotPointsResult[inputs.Length];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        Assert.NotNull(lastResult);
        
        // Woodie Pivot = (H + L + 2*C) / 4
        var high = 108;
        var low = 101;
        var close = 106;
        var pivot = (high + low + 2 * close) / 4.0;
        
        Assert.Equal(pivot, lastResult.Pivot, 2);
        
        // R1 = 2 * Pivot - Low
        Assert.Equal(2 * pivot - low, lastResult.R1, 2);
        
        // S1 = 2 * Pivot - High
        Assert.Equal(2 * pivot - high, lastResult.S1, 2);
    }

    [Fact]
    public void PivotPoints_SupportResistanceLevels()
    {
        // Arrange
        var parameters = new PPivotPoints<HLC, PivotPointsResult>
        {
            PivotType = PivotPointType.Standard
        };
        var pivots = new PivotPoints_QC<HLC, PivotPointsResult>(parameters);
        
        // Sample data
        var inputs = new HLC[]
        {
            new() { High = 105, Low = 95, Close = 100 },
            new() { High = 110, Low = 98, Close = 105 },
            new() { High = 108, Low = 96, Close = 102 },
            new() { High = 112, Low = 100, Close = 108 },
            new() { High = 115, Low = 103, Close = 110 },
        };
        
        var outputs = new PivotPointsResult[inputs.Length];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);
        
        var lastResult = outputs[outputs.Length - 1];
        
        // Verify ordering: S3 < S2 < S1 < Pivot < R1 < R2 < R3
        Assert.True(lastResult.S3 < lastResult.S2);
        Assert.True(lastResult.S2 < lastResult.S1);
        Assert.True(lastResult.S1 < lastResult.Pivot);
        Assert.True(lastResult.Pivot < lastResult.R1);
        Assert.True(lastResult.R1 < lastResult.R2);
        Assert.True(lastResult.R2 < lastResult.R3);
    }

    [Fact]
    public void PivotPoints_IntraDay()
    {
        // Arrange
        var parameters = new PPivotPoints<HLC, PivotPointsResult>
        {
            PivotType = PivotPointType.Standard
        };
        var pivots = new PivotPoints_QC<HLC, PivotPointsResult>(parameters);
        
        // Simulate intraday data (hourly bars)
        var inputs = new HLC[24];
        for (int i = 0; i < inputs.Length; i++)
        {
            var basePrice = 100.0 + Math.Sin(i * 0.26) * 5; // Intraday volatility
            inputs[i] = new HLC
            {
                High = basePrice + 0.5,
                Low = basePrice - 0.5,
                Close = basePrice + (i % 2 == 0 ? 0.2 : -0.2)
            };
        }
        
        var outputs = new PivotPointsResult[inputs.Length];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);
        
        // Each bar should have pivot levels calculated
        for (int i = 1; i < outputs.Length; i++)
        {
            Assert.NotNull(outputs[i]);
            Assert.True(outputs[i].Pivot > 0);
            Assert.True(outputs[i].R1 > outputs[i].Pivot);
            Assert.True(outputs[i].S1 < outputs[i].Pivot);
        }
    }

    [Fact]
    public void PivotPoints_TrendingMarket()
    {
        // Arrange
        var parameters = new PPivotPoints<HLC, PivotPointsResult>
        {
            PivotType = PivotPointType.Standard
        };
        var pivots = new PivotPoints_QC<HLC, PivotPointsResult>(parameters);
        
        // Strong uptrend
        var inputs = new HLC[20];
        for (int i = 0; i < inputs.Length; i++)
        {
            var price = 100.0 + i * 2.0;
            inputs[i] = new HLC
            {
                High = price + 1,
                Low = price - 0.5,
                Close = price + 0.8
            };
        }
        
        var outputs = new PivotPointsResult[inputs.Length];

        // Act
        pivots.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(pivots.IsReady);
        
        // In uptrend, pivots should be rising
        Assert.True(outputs[10].Pivot < outputs[15].Pivot);
        Assert.True(outputs[15].Pivot < outputs[19].Pivot);
        
        // Price should be testing resistance levels in uptrend
        var lastPrice = inputs[inputs.Length - 1].Close;
        var lastPivot = outputs[outputs.Length - 1];
        Assert.True(lastPrice > lastPivot.Pivot);
    }
}

public class PivotPointsResult
{
    public double Pivot { get; set; }
    public double R1 { get; set; }
    public double R2 { get; set; }
    public double R3 { get; set; }
    public double S1 { get; set; }
    public double S2 { get; set; }
    public double S3 { get; set; }
}

public enum PivotPointType
{
    Standard,
    Fibonacci,
    Camarilla,
    Woodie,
    Demark
}