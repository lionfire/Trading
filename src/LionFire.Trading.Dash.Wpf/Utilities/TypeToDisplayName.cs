using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Dash.Wpf
{
    public class TypeToDisplayName : System.Windows.Data.IValueConverter
    {
        public static string ToCamelCase(string typeName)
        {
            StringBuilder result2 = new StringBuilder();
            foreach (var c in typeName)
            {
                if (result2.Length != 0 && char.IsUpper(c))
                {
                    result2.Append(' ');
                }
                result2.Append(c);
            }
            return result2.ToString();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string typeName = null;
            if (value is Type)
            {
                typeName = ((Type)value).Name;
            }
            if (value is string)
            {
                typeName = (string)value;
            }
            if (typeName != null)
            {
                return ToCamelCase(typeName.Replace("ViewModel", ""));
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
