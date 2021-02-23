using LionFire.Instantiating;
using LionFire.Notifications;
using System;
using System.Text;

namespace LionFire.Trading.Notifications
{
    public class PriceWatch
    {
        #region Identity

        public string Symbol { get; set; }

        public string Key
        {
            get => key ?? DisplayString;
            set => key = value;
        }
        private string key;

        public string Name { get; set; }

        //public override string CalculateKey()
        //{
        //    if (Symbol == null) return null;
        //    if (Operator == null) return null;
        //    return DisplayString;
        //    //return $"Market.{Symbol}.{Operator}";
        //}
        #region Display Strings

        #region Derived

        public override string ToString() => Key;

        public string DisplayString => $"{Exchange} {Symbol} {Operator} {Price} {PriceCode}{(Name != null ? $" #{Name}" : "")}".Trim();
        public string PriceCode
        {
            get
            {
                switch (PriceKind)
                {
                    case PriceKind.Unspecified:
                        return "";
                    case PriceKind.Last:
                        return "L";
                    case PriceKind.Bid:
                        return "B";
                    case PriceKind.Ask:
                        return "A";
                    case PriceKind.Mark:
                        return "M";
                    default:
                        var sb = new StringBuilder();
                        if (PriceKind.HasFlag(PriceKind.Last)) sb.Append("L");
                        if (PriceKind.HasFlag(PriceKind.Bid)) sb.Append("B");
                        if (PriceKind.HasFlag(PriceKind.Ask)) sb.Append("A");
                        if (PriceKind.HasFlag(PriceKind.Mark)) sb.Append("M");
                        return sb.ToString();
                }
            }
        }

        #endregion

        #endregion

        #endregion

        #region Parameters

        public Notifier Notifier { get; set; }
        public string Exchange { get; set; }

        public PriceKind PriceKind { get; set; }

        public string Operator { get; set; } // TODO: Use an enum
        public void InvertOperator() // TODO: Don't have mutable PriceWatches!  Create (or retrieve) a clone
        {
            switch (Operator)
            {
                case ">":
                    Operator = "<";
                    break;
                case ">=":
                    Operator = "<=";
                    break;
                case "<":
                    Operator = ">";
                    break;
                case "<=":
                    Operator = ">=";
                    break;
                default:
                    break;
            }
        }

        public decimal Price { get; set; }

        #region Derived

        public bool? OperatorUp
        {
            get
            {
                if (Operator.Contains(">")) return true;
                else if (Operator.Contains("<")) return false;
                return null;
            }
        }

        #endregion

        #endregion

        #region Construction

        public PriceWatch()
        {
            Notifier = new Notifier();
        }

        public PriceWatch(string symbol, string op, decimal price) : this()
        {
            this.Symbol = symbol;
            this.Operator = op;
            this.Price = price;
        }

        public PriceWatch(Importance importance, Urgency urgency, string symbol, string op, decimal price)
            : this(symbol, op, price)
        {
            Notifier.Importance = importance;
            Notifier.Urgency = urgency;

        }

        #endregion

        #region State

        #region History

        // REFACTOR - Decouple this?
        public PriceWatchHistory PriceWatchHistory { get; set; }

        #endregion

        #endregion


        #region Serialization

        // REVIEW

        public string EncodeValue()
        {
            var sb = new StringBuilder($"e:{Exchange} s:{Symbol} o:{Operator} p:{Price} r:{Notifier?.Profile}");

            if (Notifier.Importance != default(int))
            {
                sb.Append(" !:");
                sb.Append(Notifier.Importance.ToString());
            }

            if (Notifier.Urgency != default(int))
            {
                sb.Append(" ^:");
                sb.Append(Notifier.Urgency.ToString());
            }

            if (PriceKind != default(PriceKind))
            {
                sb.Append(" t:");
                sb.Append(PriceKind.ToString());
            }

            return sb.ToString();
        }

        public void DecodeValue(string val)
        {
            var chunks = val.Split(' ');
            foreach (var chunk in chunks)
            {
                var kvp = chunk.Split(new char[] { ':' }, 2);
                switch (kvp[0])
                {
                    case "e":
                        Exchange = kvp[1];
                        break;
                    case "s":
                        Symbol = kvp[1];
                        break;
                    case "o":
                        Operator = kvp[1];
                        break;
                    case "p":
                        Price = decimal.Parse(kvp[1]);
                        break;
                    case "r":
                        Notifier.Profile = kvp[1];
                        break;
                    case "!":
                        Notifier.Importance = (Importance)Enum.Parse(typeof(Importance), kvp[1]);
                        break;
                    case "^":
                        Notifier.Urgency = (Urgency)Enum.Parse(typeof(Urgency), kvp[1]);
                        break;
                    case "t":
                        PriceKind = (PriceKind)Enum.Parse(typeof(PriceKind), kvp[1]);
                        break;
                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
