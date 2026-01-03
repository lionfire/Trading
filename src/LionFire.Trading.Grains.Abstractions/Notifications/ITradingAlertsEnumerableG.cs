using LionFire.Orleans_;
using LionFire.Trading.Alerts;
using Orleans;

namespace LionFire.Trading.Notifications;

public interface ITradingAlertsEnumerableG : IGrainWithStringKey, IAsyncEnumerableGrain<TradingAlert>, System.IAsyncObserver<TradingAlert>
{
}
