using LionFire.Attributes;
using System;
using System.Text;

namespace LionFire.Trading.Notifications
{
    [SerializationDelimiter(' ')]
    public class TPriceAlert
    {
        #region Identity

        public string Key => $"{Symbol}|{Exchange}|{Price}";
        public string SymbolKey => $"{Exchange}:{Symbol}";

        #endregion

        public string Id { get; set; }

        public string User { get; set; }

        public string Exchange { get; set; }
        public string Symbol { get; set; }

        public string Operator { get; set; }

        public double Price { get; set; }

        /// <summary>
        /// Last / bid / ask
        /// </summary>
        [IgnoreIfDefault]
        public TPriceAlertType Type { get; set; }

        [Code("r")]
        public string Profile { get; set; }

        [Code("!")]
        public int Priority { get; set; }

        #region String

        public override string ToString() => DisplayString;

        public string DisplayString => $"'{Profile}' when {Symbol} ({Exchange}) {Operator} {Price}";

        #endregion

        public string EncodeValue()
        {
            var sb = new StringBuilder($"e:{Exchange} s:{Symbol} o:{Operator} p:{Price} r:{Profile}");

            if (Priority != default(int))
            {
                sb.Append(" !:");
                sb.Append(Priority.ToString());
            }
            if (Type != default(TPriceAlertType))
            {
                sb.Append(" t:");
                sb.Append(Type.ToString());
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
                        Price = double.Parse(kvp[1]);
                        break;
                    case "r":
                        Profile = kvp[1];
                        break;
                    case "!":
                        Priority = int.Parse(kvp[1]);
                        break;
                    case "t":
                        Type = (TPriceAlertType)Enum.Parse(typeof(TPriceAlertType), kvp[1]);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    #region Abbreviation Serializer

    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    public sealed class SerializationDelimiterAttribute : Attribute
    {
        public char Value => value;

        private readonly char value;

        public SerializationDelimiterAttribute(char value)
        {
            this.value = value;
        }
    }

    // How easy / efficient would it be to genericize this?  Can 

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class IgnoreIfValueAttribute : Attribute
    {
        public object Value => value;

        private readonly object value;

        public IgnoreIfValueAttribute(object value)
        {
            this.value = value;
        }
    }

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class IgnoreIfDefaultAttribute : Attribute
    {
    }

    //public class AbbrevSerializer
    //{
    //    public string Encode(object obj)
    //    {
    //    }
    //}

    #endregion
}
