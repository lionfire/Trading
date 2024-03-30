
//namespace LionFire.Trading.Indicators.Harnesses.Tests;

namespace SMA;

public class Raw_ : BinanceDataTest
{
    [Fact]
    public void _()
    {
        var indicator = new SimpleMovingAverage(3);
        List<double> output = new();
        Assert.Empty(output);
        indicator.Subscribe(v => output.AddRange(v));

        indicator.OnNext(1);
        Assert.Equal(1, output.Count);
        Assert.Equal(double.NaN, output[0]);

        indicator.OnNext(1);
        Assert.Equal(2, output.Count);
        Assert.Equal(double.NaN, output[1]);

        indicator.OnNext(1);
        Assert.Equal(3, output.Count);
        Assert.Equal(1, output[2]);

        indicator.OnNext(2);
        Assert.Equal(4, output.Count);
        Assert.Equal((4 / 3.0), output[3]);
    }
}
