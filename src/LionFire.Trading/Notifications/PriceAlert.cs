using LionFire.Instantiating;
using LionFire.Notifications;
using LionFire.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class TPriceAlert : TNotification, ITemplate<PriceAlert>, IKeyed<string>
    {

            
        public override string Key
        {
            get
            {
                return base.Key ?? $"{Symbol} {Operator} {Price}";
            }
            set { base.Key = value; }
        }

        #region Construction

        public TPriceAlert() { }
        public TPriceAlert(string symbol, string op, double price)
        {
            this.Symbol = symbol;
            this.Operator = op;
            this.Price = price;
        }
        public TPriceAlert(Importance importance, Urgency urgency, string symbol, string op, double price) : base(importance, urgency)
        {
            this.Symbol = symbol;
            this.Operator = op;
            this.Price = price;
        }

        #endregion
        
        public TimeFrame TimeFrame { get; set; } = TimeFrame.t1;

        public override string CalculateKey()
        {
            if (Symbol == null) return null;
            if (Operator == null) return null;
            return $"Market.{Symbol}.{Operator}";
        }

        public string Symbol { get; set; }
        public string Operator { get; set; }
        public double Price { get; set; }
        
        public bool? OperatorUp
        {
            get
            {
                if (Operator.Contains(">")) return true;
                else if (Operator.Contains("<")) return false;
                return null;
            }
        }
        /// <summary>
        /// If Unspecified, watch for either
        /// </summary>
        public PriceQuoteType BidOrAsk { get; set; }

        /// <summary>
        /// If null, watch all accounts
        /// </summary>
        public string Account { get; set; }


        public void Attach(IFeed feed)
        {
            if (TimeFrame == TimeFrame.t1)
            {
                feed.GetSymbol(Symbol).Ticked += OnTick;
            }
            else
            {
                throw new NotImplementedException("timeframes other than t1");
            }
            feeds.Add(feed);
        }

        void RaiseAlert()
        {
            Debug.WriteLine("ALERT REACHED: " + this.ToString());
        }
        private void OnTick(SymbolTick obj)
        {
            switch (Operator)
            {
                case ">":
                    if (obj.Ask > Price) RaiseAlert();
                    break;
                case "<":
                    if (obj.Ask < Price) RaiseAlert();
                    break;
                default:
                    break;
            }
        }

        List<IFeed> feeds = new List<IFeed>();

        public void Detach(IFeed feed=null)
        {
            if (feed == null)
            {
                foreach (var f in feeds)
                {
                    Detach(f);
                }
            }
            else
            {
                feed.GetSymbol(Symbol).Ticked -= OnTick;
            }
        }
    }

    public class PriceAlert : ITemplateInstance<TPriceAlert>
    {
        public TPriceAlert Template { get; set; }
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
                var verb = Template.OperatorUp.HasValue ? (Template.OperatorUp.Value ? "up to": "down to"):"at";
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