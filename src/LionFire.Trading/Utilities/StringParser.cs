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

    


}
