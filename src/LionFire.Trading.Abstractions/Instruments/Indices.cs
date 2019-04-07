using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Instruments
{
    public class Indices
    {
        public static Dictionary<string, string> NormalizedSymbols = new Dictionary<string, string>() {
            ["US SPX 500 (Mini)"] = "US500", // TVM
        };

        // FUTURE: Set these symbols up per broker. 
        public static Dictionary<string, string> Symbols = new Dictionary<string, string>()
        {
            ["AUS200"] = "AUD",
            ["US2000"] = "USD",
            ["US500"] = "USD",
            ["USTEC"] = "USD",
            ["US30"] = "USD",
            ["JP225"] = "JPY",
            ["UK100"] = "GBP",
            ["DE30"] = "EUR",
            ["STOXX50"] = "EUR",
            ["IT40"] = "EUR",
            ["ES35"] = "EUR",
            ["F40"] = "EUR",
            ["HK50"] = "HKD",            
        };
    }
}
