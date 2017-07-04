using LionFire.Instantiating;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class PriceAlert : ITemplateInstance<PriceNotifier>
    {
        public PriceNotifier Template { get; set; }
        public DateTime UtcTime { get; set; }
        public PriceQuoteType BidOrAsk { get; set; }
        public string Account { get; set; }
        public double CurrentBid { get; set; }
        public double CurrentAsk { get; set; }
        public double RelevantPrice { get; set; }

        public double Spread { get => CurrentAsk - CurrentBid; }

        public void UpdateRelevantPriceIfNeeded()
        {
            if (RelevantPrice != default(double)) return;
            if (Template.Operator.Contains(">"))
            {
                BidOrAsk = CurrentAsk >= CurrentBid ? PriceQuoteType.Ask : PriceQuoteType.Bid;
            }
            else
            {
                BidOrAsk = CurrentBid <= CurrentAsk ? PriceQuoteType.Bid : PriceQuoteType.Ask;
            }
            RelevantPrice = BidOrAsk == PriceQuoteType.Ask ? CurrentAsk : CurrentBid;
        }
        public string Message
        {
            get
            {
                UpdateRelevantPriceIfNeeded();
                var verb = Template.OperatorUp.HasValue ? (Template.OperatorUp.Value ? "up to" : "down to") : "at";
                var msg = $"{Template.Symbol} {verb} {RelevantPrice}";
                return msg;
            }
        }
        //public string VerboseMessage
        //{
        //    get
        //    {
        //        UpdateRelevantPriceIfNeeded();
        //        var msg = $"Price for {Template.Symbol} has reached {RelevantPrice} ({BidOrAsk})";
        //        return msg;
        //    }
        //}

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