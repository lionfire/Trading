using System;
using System.Threading.Tasks;
using LionFire.Trading.Notifications;

namespace LionFire.Trading.Notifications
{
    public interface IPriceAlerts
    {
        //[Obsolete]
        //void Add(TPriceAlert priceAlert);

        event Action<ExchangeSymbolTick> PriceChanged;
        
        Task Add(PriceWatch priceWatch);
        Task Remove(PriceWatch priceWatch);
    }
}
