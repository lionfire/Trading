namespace LionFire.Trading
{
    public class SymbolNotifier : TradingNotifier
    {
        public string Symbol { get; set; }


        public SymbolNotifier() { }
        public SymbolNotifier(Importance importance, Urgency urgency) : base(importance, urgency) { }

    }
}
//public class TradingNotifications
//{
//    public Dictionary<TradingNotificationType, TNotification> Notifications
//    {
//        get
//        {
//            var dict = new Dictionary<TradingNotificationType, TNotification>();

//            dict.Add(TradingNotificationType.PriceReached, new TNotification
//            {
//                Message = "Price for {Symbol} has reached {Price} {BidOrAsk}",
//                Profile = "G1",
//            });

//            return dict;
//        }
//    }

//    public TradingNotifications()
//    {
//    }

//}