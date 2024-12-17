using System.Reactive.Subjects;

namespace LionFire.Trading.Automation;

public class BacktestBatchProgress
{
    public int BatchId { get; internal set; }

    public int Total
    {
        get => total;
        internal set
        {
            total = value;
            progresses.OnNext(PerUn);
        }
    }
    private int total;

    public double EffectiveCompleted => perUn * Total;

    public double PerUn
    {
        get => perUn;
        set
        {
            perUn = value;
            progresses.OnNext(value);
        }
    }
    private double perUn;

    public IObservable<double> Progresses => progresses;
    Subject<double> progresses = new();
}
