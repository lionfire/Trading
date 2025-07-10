using System;
using System.Collections.Concurrent;

namespace LionFire.Trading.Feeds.Tracking;

public interface ICvdTracker
{
    decimal GetCvd(string symbol);
    void UpdateCvd(string symbol, decimal tradeVolume, bool isBuy);
    void Reset(string symbol);
    void ResetAll();
}

public class CvdTracker : ICvdTracker
{
    private readonly ConcurrentDictionary<string, decimal> _cvdValues = new();

    public decimal GetCvd(string symbol)
    {
        return _cvdValues.GetOrAdd(symbol, 0m);
    }

    public void UpdateCvd(string symbol, decimal tradeVolume, bool isBuy)
    {
        var volumeDelta = isBuy ? tradeVolume : -tradeVolume;
        _cvdValues.AddOrUpdate(symbol, 
            volumeDelta, 
            (_, currentCvd) => currentCvd + volumeDelta);
    }

    public void Reset(string symbol)
    {
        _cvdValues.TryRemove(symbol, out _);
    }

    public void ResetAll()
    {
        _cvdValues.Clear();
    }
}