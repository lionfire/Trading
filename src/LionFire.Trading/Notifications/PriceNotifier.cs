using LionFire.Trading;
using System;

namespace LionFire.Notifications.Wpf.App
{
    // Where did this class go?  FireLynx now uses PriceWatch/PriceWatcher
    public class PriceNotifier
    {
        public PriceNotifier() { }
        public PriceNotifier(string symbol, string op, decimal price)
        {
            Symbol = symbol;
            Operator = op;
            Price = price;
        }

        public string Symbol { get; set; }
        public string Operator { get; set; }
        public decimal Price { get; set; }



        public void Attach(IFeed_Old feed) { throw new NotImplementedException(); }
        public void Detach(IFeed_Old feed) { throw new NotImplementedException(); }
    }
}
