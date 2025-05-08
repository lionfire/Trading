using System.Reflection;

namespace LionFire.Trading.Automation.Portfolios;

public static class BotParameterEquality
{

    /// <summary>
    /// TODO
    /// Return true if parameters are the same (assuming default values for missing parameters), false if not.
    /// Return null if one has extra parameters with non-default values not present in the first one.
    /// </summary>
    /// <param name="entry1"></param>
    /// <param name="entry2"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static bool? BotParametersEqual(object? entry1, object? entry2)
    {
        bool gotNullResult =false;
        if (entry1 == null || entry2 == null) return null;

        var type1 = entry1.GetType();
        var type2 = entry2.GetType();

        if (type1 != type2) return false;

        foreach (var property in type1.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(pi => pi.GetCustomAttribute<NotAParameterAttribute>() == null))
        {
            var parameterAttr = property.GetCustomAttribute<ParameterAttribute>();
            if (parameterAttr != null)
            {
                var value1 = property.GetValue(entry1);
                var value2 = property.GetValue(entry2);
                    
                // TODO: coerce value1 and value2 from a default marker to the actual default value

                if (!Equals(value1, value2))
                {
                    if (value1 == null || value2 == null) return null;
                    return false;
                }
            }

            var containsParametersAttr = property.PropertyType.GetCustomAttribute<ContainsParametersAttribute>();
            if (containsParametersAttr != null)
            {
                var nestedValue1 = property.GetValue(entry1);
                var nestedValue2 = property.GetValue(entry2);

                var nestedResult = BotParametersEqual(nestedValue1, nestedValue2);
                if (nestedResult == false) return false;
                gotNullResult |= nestedResult == null;
            }
        }

        return true;
    }
}
