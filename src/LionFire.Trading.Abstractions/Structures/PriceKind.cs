using System;

namespace LionFire.Trading
{
    [Flags]
    public enum PriceKind : int
    {
        Unspecified = 0,
        Last = 1 << 1,
        Bid = 1 << 2,
        Ask= 1 << 3,
        Mark = 1 << 4,
    }
}
