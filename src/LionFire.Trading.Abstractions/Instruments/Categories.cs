using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Abstractions.Instruments
{
    public class MarketCategory
    {
        public string ShortCode { get; set; }
        public string Name { get; set; }
    }
    public class Categories
    {
        public static IEnumerable<MarketCategory> Items => items;
        private static List<MarketCategory> items;

        static Categories()
        {
            items = new List<MarketCategory>();

            items.Add(new MarketCategory { ShortCode = "B", Name = "Bonds" });
            items.Add(new MarketCategory { ShortCode = "BM", Name = "Base Metals" });
            items.Add(new MarketCategory { ShortCode = "CO", Name = "Commodities" });
            items.Add(new MarketCategory { ShortCode = "CR", Name = "Cryptocurrencies" });
            items.Add(new MarketCategory { ShortCode = "F", Name = "Forex" });
            items.Add(new MarketCategory { ShortCode = "I", Name = "Indicies" });
            items.Add(new MarketCategory { ShortCode = "PM", Name = "Precious Metals" });
            items.Add(new MarketCategory { ShortCode = "S", Name = "Stocks" });
        }
    }
}
