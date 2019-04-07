using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Instruments
{
    public class Cryptos
    {
        public static HashSet<string> Symbols = new HashSet<string>()
            {
                "BTC",
                "ETH",
                "LTC",
                "XRP",
                "OMG",
                "EOS",
                "ZIL",
            };

    }

    //public class Majors
    //{
    //    public static List<string> Currencies = new List<string>()
    //    {
    //        "USD",
    //        "EUR",
    //        "JPY",
    //        "GBP",
    //        "CHF",
    //        "CAD",
    //        "AUD",
    //        "NZD",
    //    };
    //    public static List<string> Symbols = new List<string>()
    //        {
    //            "EURUSD",
    //            "USDJPY",
    //            "GBPUSD",
    //            "USDCHF",
    //            "USDCAD",
    //            "AUDUSD",
    //            "NZDUSD",
    //        };
    //}
}
