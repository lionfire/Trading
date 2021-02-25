using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LionFire.Collections;
using LionFire.Trading.Notifications;

namespace LionFire.Trading.Notifications
{
    public interface IPriceWatchRepository : INotifyCollectionChanged<PriceWatch>
    {
        // TODO: List

        IAsyncEnumerable<PriceWatch> List();

        Task Add(PriceWatch priceWatch);
        Task Remove(PriceWatch priceWatch);
    }
}
