using System.Collections.Generic;
using LionFire.Instantiating;
using LionFire.Trading;

namespace LionFire.Notifications.Wpf.App
{
    public class TradingNotificationsService : ITemplate<STradingNotificationsService>
    {
        public List<TPriceAlert> AlertRequests { get; set; } = new List<TPriceAlert>();

        public List<string> AccountNames { get; set; }

        public void AddDefaults()
        {
            AlertRequests.Add(new TPriceAlert
            {
                Symbol = "EURUSD",
                Price = 1.12,
                Operator = ">",
            });
            AlertRequests.Add(new TPriceAlert("XAUUSD", ">", 1260));
        }
    }
}