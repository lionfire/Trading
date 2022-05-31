using LionFire.Instantiating;
using LionFire.Notifications;
using System;
using System.Text;

namespace LionFire.Trading.Notifications
{
    public class PriceWatch
    {
        #region Identity

        public SymbolId SymbolId { get; set; }

        public string ExchangeCode => SymbolId.ExchangeAndAreaCode;

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

        public string DisplayString => $"{ExchangeCode} {Symbol} {Operator} {Price} {PriceCode}{(Name != null ? $" #{Name}" : "")}".Trim();
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

        /// <summary>
        /// Named profile owned by the User
        /// </summary>

        #region Profile

        const string codePrefix = "code:";
        public string Profile
        {
            get
            {
                if (profile != null) return profile;
                if (ProfileCode != null) return codePrefix + ProfileCode;
                return null;
            }
            set
            {
                if (value?.StartsWith(codePrefix) == true)
                {
                    ProfileCode = value.Substring(codePrefix.Length);
                    profile = null;
                }
                else
                {
                    profile = value;
                    profileCode = null;
                }
            }
        }
        private string profile;

        #endregion

        #region ProfileCode

        public string ProfileCode
        {
            get => profileCode;
            set { profileCode = value; profile = null; }
        }
        private string profileCode;

        #endregion

        
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
        }
        public PriceWatch(string key, string value, decimal price) : this()
        {
            this.DecodeValue(key, value, price);
        }

        //public static PriceWatch Deserialize(string key, string value, decimal price) : this()
        //{
        //    this.DecodeValue(key, value, price);
        //}

        //public PriceWatch(string symbol, string op, decimal price) : this()
        //{
        //    this.Symbol = symbol;
        //    this.Operator = op;
        //    this.Price = price;
        //}

        //public PriceWatch(Importance importance, Urgency urgency, string symbol, string op, decimal price)
        //    : this(symbol, op, price)
        //{
        //    Notifier.Importance = importance;
        //    Notifier.Urgency = urgency;
        //}

        #endregion

        #region State

        #region History

        // REFACTOR - Decouple this?
        public PriceWatchHistory PriceWatchHistory { get; set; }

        #endregion

        #endregion


        #region Serialization

        // REVIEW

        public uint UserId { get; set; }

        public string EncodeValue()
        {
            var sb = new StringBuilder();
            sb.Append("u:");
            sb.Append(UserId);

            sb.Append(" p:");
            sb.Append(Price);

            //var sb = new StringBuilder($"e:{Exchange} s:{Symbol} o:{Operator} p:{Price} r:{Notifier?.Profile}");

            if (ProfileCode != null)
            {
                sb.Append(" c:");
                sb.Append(ProfileCode);
            }
            else if (Profile != default)
            {
                sb.Append(" r:");
                sb.Append(Profile);
            }

            //if (Notifier.Importance != default(int))
            //{
            //    sb.Append(" !:");
            //    sb.Append(Notifier.Importance.ToString());
            //}

            //if (Notifier.Urgency != default(int))
            //{
            //    sb.Append(" ^:");
            //    sb.Append(Notifier.Urgency.ToString());
            //}

            //if (PriceKind != default(PriceKind))
            //{
            //    sb.Append(" t:");
            //    sb.Append(PriceKind.ToString());
            //}

            return sb.ToString();
        }

        public void DecodeParentKey(string parentKey)
        {
            var chunks = parentKey.Split(':');

            if (chunks.Length != 5) throw new ArgumentException("Format: watch:{ExchangeCode}:{Symbol}:{PriceKind}:{up | down}");

            SymbolId = SymbolId.FromExchangeAndAreaCode(chunks[1], chunks[2]);
            
            PriceKind = (PriceKind)Enum.Parse(typeof(PriceKind), chunks[3]);
            Operator = chunks[4] == "Up" ? ">" : (chunks[4] == "Down" ? "<" : throw new ArgumentException("up/down missing"));

            //foreach (var chunk in chunks)
            //{

            //    var kvp = chunk.Split(new char[] { ':' }, 2);
            //    switch (kvp[0])
            //    {
            //        case "u":
            //            UserId = UInt32.Parse(kvp[1]);
            //            break;
            //        case "e":
            //            Exchange = kvp[1];
            //            break;
            //        case "s":
            //            Symbol = kvp[1];
            //            break;
            //        case "o":
            //            Operator = kvp[1];
            //            break;
            //        case "p":
            //            Price = decimal.Parse(kvp[1]);
            //            break;
            //        case "r":
            //            Profile = kvp[1];
            //            break;
            //        //case "!":
            //        //    Notifier.Importance = (Importance)Enum.Parse(typeof(Importance), kvp[1]);
            //        //    break;
            //        //case "^":
            //        //    Notifier.Urgency = (Urgency)Enum.Parse(typeof(Urgency), kvp[1]);
            //        //    break;
            //        case "t":
            //            PriceKind = (PriceKind)Enum.Parse(typeof(PriceKind), kvp[1]);
            //            break;
            //        default:
            //            break;
            //    }
            //}
        }
        public void DecodeValue(string parentKey, string val, decimal price)
        {
            Price = price;
            DecodeParentKey(parentKey);

            var chunks = val.Split(' ');
            foreach (var chunk in chunks)
            {
                var kvp = chunk.Split(new char[] { ':' }, 2);
                switch (kvp[0])
                {
                    case "u":
                        UserId = UInt32.Parse(kvp[1]);
                        break;
                    //case "e":
                    //    Exchange = kvp[1];
                    //    break;
                    //case "s":
                    //    Symbol = kvp[1];
                    //    break;
                    //case "o":
                    //    Operator = kvp[1];
                    //    break;
                    //case "p":
                    //    Price = decimal.Parse(kvp[1]);
                    //    break;
                    case "c":
                        ProfileCode = kvp[1];
                        break;
                    case "r":
                        Profile = kvp[1];
                        break;
                    //case "!":
                    //    Notifier.Importance = (Importance)Enum.Parse(typeof(Importance), kvp[1]);
                    //    break;
                    //case "^":
                    //    Notifier.Urgency = (Urgency)Enum.Parse(typeof(Urgency), kvp[1]);
                    //    break;
                    //case "t":
                    //    PriceKind = (PriceKind)Enum.Parse(typeof(PriceKind), kvp[1]);
                    //    break;
                    default:
                        break;
                }
            }
        }

        #endregion
    }
}
