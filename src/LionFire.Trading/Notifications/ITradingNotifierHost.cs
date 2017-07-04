using LionFire.Notifications;

namespace LionFire.Trading
{
    public interface ITradingNotifierHost : INotifierHost
    {
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