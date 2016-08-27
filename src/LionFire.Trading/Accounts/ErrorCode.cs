using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public enum ErrorCode
    {
        TechnicalError = 0,
        BadVolume = 1,
        NoMoney = 2,
        MarketClosed = 3,
        Disconnected = 4,
        EntityNotFound = 5,
        Timeout = 6
    }

}
