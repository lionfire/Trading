using LionFire.Trading.Backtesting;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Parsing.String
{
    public static class StringParser
    {
        private static ConcurrentDictionary<Type, Dictionary<KeyValuePair<string, bool>, PropertyInfo>> TypePropertyDictionary = new ConcurrentDictionary<Type, Dictionary<KeyValuePair<string, bool>, PropertyInfo>>();

        public static bool ParseUnitValue(this string unitValue, Type type, out string unit, out object val, out PropertyInfo propertyInfo)
        {

            var dict = TypePropertyDictionary.GetOrAdd(type, t =>
            {
                var d = new Dictionary<KeyValuePair<string, bool>, PropertyInfo>();
                foreach (var pi in t.GetProperties())
                {
                    var attr = pi.GetCustomAttribute<UnitAttribute>();
                    if (attr == null) continue;
                    d.Add(new KeyValuePair<string, bool>(attr.Unit.TrimEnd('='), attr.IsPrefix), pi);
                }
                return d;
            });

            unit = "";
            string valString = "";
            bool isPrefix = false;
            if (unitValue.Length != 0)
            {
                if (IsNumeric(unitValue[0]))
                {
                    int i;
                    for (i = 0; i < unitValue.Length && IsNumeric(unitValue[i]); i++) ;

                    unit = unitValue.Substring(i);
                    valString = unitValue.Substring(0, i);
                }
                else
                {
                    isPrefix = true;
                    int i;
                    for (i = 0; i < unitValue.Length && !IsNumeric(unitValue[i]); i++) ;

                    unit = unitValue.Substring(0, i);
                    valString = unitValue.Substring(i);
                }
            }
            dict.TryGetValue(new KeyValuePair<string, bool>(unit, isPrefix), out propertyInfo);

            if (propertyInfo == null && unitValue.Contains("="))
            {
                var split = unitValue.Split('=');
                if (split.Length == 2)
                {
                    valString = split[1];
                    dict.TryGetValue(new KeyValuePair<string, bool>(split[0], isPrefix), out propertyInfo);
                }
            }

            if (propertyInfo == null)
            {
                val = null;
                return false;
            }

            switch (propertyInfo.PropertyType.Name)
            {
                case "Single":
                    val = Convert.ToSingle(valString);
                    break;
                case "Double":
                    val = Convert.ToDouble(valString);
                    break;
                case "Int32":
                    val = Convert.ToInt32(valString);
                    break;
                case "String":
                    val = valString;
                    break;
                default:
                    throw new NotImplementedException($"Convert to {propertyInfo.PropertyType.Name}");
            }
            return true;
        }

        public static bool IsNumeric(char x)
        {
            return x == '.' || char.IsDigit(x);
        }

        public static void AssignFromString(this object obj, string str)
        {
            var type = obj.GetType();

            var split = str.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var chunk in split)
            {
                PropertyInfo pi;
                string unit;
                object val;
                if (chunk.ParseUnitValue(type, out unit, out val, out pi))
                {
                    pi.SetValue(obj, val);
                }
            }
        }
    }

    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    public sealed class UnitAttribute : Attribute
    {
        public string Unit { get; private set; }

        public bool IsPrefix { get; set; }

        public UnitAttribute(string unit)
        {
            _ctor(unit, null);
        }
        public UnitAttribute(string unit, bool isPrefix)
        {
            _ctor(unit, isPrefix);
        }

        private void _ctor(string unit, bool? isPrefix = null)
        {
            this.Unit = unit;
            if (isPrefix.HasValue)
            {
                this.IsPrefix = isPrefix.Value;
            }
            else
            {
                if (unit.EndsWith("="))
                {
                    IsPrefix = true;
                }
                else
                {
                    IsPrefix = false;
                }
            }
        }

    }

    public class BacktestResultHandle // TODO: Use IReadHandle or something
    {
        public static implicit operator BacktestResultHandle(BacktestResult r)
        {
            return new BacktestResultHandle { Object = r };
        }
        public BacktestResult Object
        {
            get
            {
                if (obj == null && Path != null)
                {
                    try
                    {
                        obj = JsonConvert.DeserializeObject<BacktestResult>(File.ReadAllText(Path));
                    }
                    catch { }
                }
                return obj;
            }
            set { obj = value; }
        }
        private BacktestResult obj;

        public BacktestResultHandle Self { get { return this; } } // REVIEW - another way to get context from datagrid: ancestor row?
        public string Path { get; set; }

        [Unit("id=")]
        public string Id { get; set; }

        [Unit("bot=")]
        public string Bot { get; set; }

        [Unit("sym=")]
        public string Symbol { get; set; }

        /// <summary>
        /// AROI vs Max Equity Drawdown
        /// </summary>
        [Unit("ad")]
        public double AD { get; set; }

        /// <summary>
        /// Trades Per month
        /// </summary>
        [Unit("tpm")]
        public double TPM { get; set; }

        [Unit("d")]
        public double Days { get; set; }
    }
}
