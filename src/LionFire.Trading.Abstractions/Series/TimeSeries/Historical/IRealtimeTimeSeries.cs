namespace LionFire.Trading.Data;

public interface IRealtimeTimeSeries<TValue> : IHistoricalTimeSeries
{
    IObservable<TValue> Values { get; }
}

