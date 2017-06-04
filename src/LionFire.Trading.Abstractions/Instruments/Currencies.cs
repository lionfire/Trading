using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Instruments
{
    public class Currencies
    {
        public static HashSet<string> Symbols = new HashSet<string>()
            {
                "AUD",
                "CAD",
                "CHF",
                "CHN",
                "CZK",
                "DKK",
                "EUR",
                "GBP",
                "HKD",
                "HUF",
                "JPY",
                "MXN",
                "NZD",
                "NOK",
                "PLN",
                "RUB",
                "SEK",
                "SGD",
                "THB",
                "TRY",
                "USD",
                "XAG",
                "XAU",
                "ZAR",
            };

    }

    public class Majors
    {
        public static List<string> Currencies = new List<string>()
        {
            "USD",
            "EUR",
            "JPY",
            "GBP",
            "CHF",
            "CAD",
            "AUD",
            "NZD",
        };
        public static List<string> Symbols = new List<string>()
            {
                "EURUSD",
                "USDJPY",
                "GBPUSD",
                "USDCHF",
                "USDCAD",
                "AUDUSD",
                "NZDUSD",
            };
    }
}
