namespace LionFire.Trading.Maths;

public sealed class RollingAverage
{
    private double sum = 0;
    private long count = 0;

    public void AddValue(double value)
    {
        sum += value;
        count++;
    }

    public double CurrentAverage => count > 0 ? (sum / count) : double.NaN;
}
