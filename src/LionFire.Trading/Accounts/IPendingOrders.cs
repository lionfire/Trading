using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IPendingOrders
    {
        int Count { get; }
        PendingOrder this[int index] { get; }
    }
}
