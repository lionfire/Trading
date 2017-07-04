using System.Collections.Generic;
using LionFire.Instantiating;
using LionFire.Trading;

namespace LionFire.Notifications.Wpf.App
{
    public class TradingNotificationsService : ITemplate<STradingNotificationsService>
    {
        public List<PriceNotifier> AlertRequests { get; set; } = new List<PriceNotifier>();

        public List<string> AccountNames { get; set; }

        public void AddDefaults()
        {
            AlertRequests.Add(new PriceNotifier
            {
                Symbol = "EURUSD",
                Price = 1.12,
                Operator = ">",
            });
            AlertRequests.Add(new PriceNotifier("XAUUSD", ">", 1260));
        }
    }
}