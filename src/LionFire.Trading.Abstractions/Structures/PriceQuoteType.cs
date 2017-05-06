using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading
{
    public enum PriceQuoteType
    {
        Unspecified,
        Bid = 1 << 0,
        Ask = 1 << 1,
    }
}
