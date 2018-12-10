using System;
using LionFire.Trading.Notifications;

namespace LionFire.Trading.Notifications
{
    public interface IPriceAlerts
    {
        void Add(TPriceAlert priceAlert);
        event Action<ExchangeSymbolTick> PriceChanged;
    }
}
