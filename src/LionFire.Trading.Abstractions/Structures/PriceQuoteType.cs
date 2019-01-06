using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading
{
    [Flags]
    public enum PriceQuoteType
    {
        Unspecified,
        Bid = 1 << 0,
        Ask = 1 << 1,
        Last = 1 << 2,
    }
}
