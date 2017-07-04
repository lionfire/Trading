using LionFire.Notifications;
using LionFire.Structures;

namespace LionFire.Trading
{
    [NotifierHost(typeof(ITradingNotifierHost))]
    public class TradingNotifier : Notifier, IKeyed<string>
    {
        public TradingNotifier() { }
        public TradingNotifier(Importance importance, Urgency urgency) : base(importance, urgency) { }


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