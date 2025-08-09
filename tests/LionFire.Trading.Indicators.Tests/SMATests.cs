using LionFire.Trading.Indicators.Defaults;
using LionFire.Trading.Indicators.Native;
using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using Xunit;

namespace LionFire.Trading.Indicators.Tests;

public class SMATests
{
    [Fact]
    public void FirstPartyImplementation_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PSMA<double, double> { Period = 3 };
        var sma = new SMAFP<double, double>(parameters);
        var inputs = new double[] { 1, 2, 3, 4, 5 };
        var outputs = new double[inputs.Length];

        // Act
        sma.OnBarBatch(inputs, outputs);

        // Assert
        Assert.False(sma.IsReady); // Not ready until period 3
        Assert.Equal(0, outputs[0]); // No output yet
        Assert.Equal(0, outputs[1]); // No output yet
        Assert.Equal(2, outputs[2]); // (1+2+3)/3 = 2
        Assert.Equal(3, outputs[3]); // (2+3+4)/3 = 3
        Assert.Equal(4, outputs[4]); // (3+4+5)/3 = 4
        Assert.Equal(4, sma.Value); // Current value should be 4
    }

    [Fact]
    public void QuantConnectImplementation_CalculatesCorrectly()
    {
        // Arrange
        var parameters = new PSMA<double, double> { Period = 3 };
        var sma = new SMAQC<double, double>(parameters);
        var inputs = new double[] { 1, 2, 3, 4, 5 };
        var outputs = new double[inputs.Length];

        // Act
        sma.OnBarBatch(inputs, outputs);

        // Assert
        Assert.True(sma.IsReady); // Should be ready after processing enough data
        Assert.Equal(0, outputs[0]); // No output yet
        Assert.Equal(0, outputs[1]); // No output yet
        Assert.Equal(2, outputs[2]); // (1+2+3)/3 = 2
        Assert.Equal(3, outputs[3]); // (2+3+4)/3 = 3
        Assert.Equal(4, outputs[4]); // (3+4+5)/3 = 4
        Assert.Equal(4, sma.Value); // Current value should be 4
    }

    [Fact]
    public void DefaultFactory_CreatesWorkingImplementation()
    {
        // Arrange
        var sma = SMA.CreateDouble(3);
        
        // Act - Process single values
        sma.OnNext(1.0);
        sma.OnNext(2.0);
        sma.OnNext(3.0);
        
        // Assert
        Assert.True(sma.IsReady);
        Assert.Equal(2.0, sma.Value); // (1+2+3)/3 = 2
        
        // Continue processing
        sma.OnNext(4.0);
        Assert.Equal(3.0, sma.Value); // (2+3+4)/3 = 3
        
        sma.OnNext(5.0);
        Assert.Equal(4.0, sma.Value); // (3+4+5)/3 = 4
    }

    [Fact]
    public void CircularBuffer_HandlesLargerDataset()
    {
        // Arrange
        var parameters = new PSMA<double, double> { Period = 10 };
        var sma = new SMAFP<double, double>(parameters);
        
        // Create test data: 1 to 20
        var inputs = Enumerable.Range(1, 20).Select(x => (double)x).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        sma.OnBarBatch(inputs, outputs);

        // Assert
        // First 9 should be 0 (not ready)
        for (int i = 0; i < 9; i++)
        {
            Assert.Equal(0, outputs[i]);
        }
        
        // 10th value: average of 1-10 = 5.5
        Assert.Equal(5.5, outputs[9]);
        
        // 11th value: average of 2-11 = 6.5
        Assert.Equal(6.5, outputs[10]);
        
        // Last value: average of 11-20 = 15.5
        Assert.Equal(15.5, outputs[19]);
        Assert.Equal(15.5, sma.Value);
    }

    [Fact]
    public void Clear_ResetsIndicatorState()
    {
        // Arrange
        var parameters = new PSMA<double, double> { Period = 3 };
        var sma = new SMAFP<double, double>(parameters);
        
        // Process some data
        sma.OnBarBatch([1, 2, 3], null);
        Assert.True(sma.IsReady);
        Assert.Equal(2, sma.Value);
        
        // Act
        sma.Clear();
        
        // Assert
        Assert.False(sma.IsReady);
        Assert.Equal(0, sma.Value);
        
        // Process new data after clear
        sma.OnBarBatch([4, 5, 6], null);
        Assert.True(sma.IsReady);
        Assert.Equal(5, sma.Value); // (4+5+6)/3 = 5
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(200)]
    public void DifferentPeriods_CalculateCorrectly(int period)
    {
        // Arrange
        var parameters = new PSMA<double, double> { Period = period };
        var sma = new SMAFP<double, double>(parameters);
        
        // Create constant input data
        var inputs = Enumerable.Repeat(10.0, period * 2).ToArray();
        var outputs = new double[inputs.Length];

        // Act
        sma.OnBarBatch(inputs, outputs);

        // Assert
        // After the period, all values should be 10 (average of all 10s)
        for (int i = period - 1; i < outputs.Length; i++)
        {
            Assert.Equal(10.0, outputs[i]);
        }
    }
}