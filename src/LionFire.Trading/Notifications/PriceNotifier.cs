using LionFire.Instantiating;
using LionFire.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LionFire.Trading
{
    public class PriceNotifier : SymbolNotifier, ITemplate<PriceAlert>
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

        public PriceNotifier() { }
        public PriceNotifier(string symbol, string op, double price)
        {
            this.Symbol = symbol;
            this.Operator = op;
            this.Price = price;
        }
        public PriceNotifier(Importance importance, Urgency urgency, string symbol, string op, double price) : base(importance, urgency)
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
                case ">=":
                    State.LastPrice = obj.Ask;
                    if (obj.Ask > Price) RaiseAlert();
                    break;
                case "<":
                case "<=":
                    State.LastPrice = obj.Bid;
                    if (obj.Bid < Price) RaiseAlert();
                    break;
                default:
                    break;
            }
        }

        List<IFeed> feeds = new List<IFeed>();

        public void Detach(IFeed feed = null)
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


        #region State

        [SerializeIgnore]
        public PriceNotifierState State
        {
            get { if (state == null) { state = new PriceNotifierState(); } return state; }
            set { state = value; }
        }
        private PriceNotifierState state;



        public double DistanceToPrice
        {
            get
            {
                if (double.IsNaN(State.LastPrice)) return double.NaN;
                switch (Operator)
                {
                    case ">":
                    case ">=":
                        return Price - State.LastPrice;
                    case "<":
                    case "<=":
                        return State.LastPrice - Price;
                    default:
                        return Math.Abs(State.LastPrice - Price); // REVIEW - figure out direction before hand
                }
            }
        }

        #endregion
    }

    public class PriceNotifierState
    {
        public double LastPrice { get; set; } = double.NaN;
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