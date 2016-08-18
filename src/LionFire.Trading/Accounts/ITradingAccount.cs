using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class ApiResult
    {
    }

    public enum TradeType
    {
        Buy = 0,
        Sell = 1
    }

    public interface ITradingAccount
    {
        bool IsDemo { get; set; }
        ApiResult ExecuteOrder(TradeType tradeType, string symbol, long volume, string label = null, double? stopLossPips = null, double? takeProfitPips = null, double? marketRangePips = null, string comment = null);
    }

    public class TradingAccount
    {
        public bool IsDemo { get; protected set; }


    }
}
