using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Reflection;

// Based on: Claude Sonnet 3.5

public static class PropertyFlattener
{
    public static Dictionary<string, PropertyInfo> GetFlattenedProperties(Type type, List<Type>? excludeAttributes = null)
    {
        var result = new Dictionary<string, PropertyInfo>();
        GetFlattenedPropertiesRecursive(type, string.Empty, result, excludeAttributes);
        return result;
    }

    private static void GetFlattenedPropertiesRecursive(Type type, string prefix, Dictionary<string, PropertyInfo> result, List<Type>? excludeAttributes = null)
    {
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip read-only properties
            if (!prop.CanWrite)
                continue;

            // Skip properties with excluded attributes
            if (excludeAttributes != null && excludeAttributes.Any(attr => prop.GetCustomAttributes(attr, true).Any()))
                continue;

            var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";

            if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string) || prop.PropertyType == typeof(DateTime))
            {
                result[key] = prop;
            }
            else
            {
                GetFlattenedPropertiesRecursive(prop.PropertyType, key, result, excludeAttributes);
            }
        }
    }

    public static Dictionary<string, object?> GetFlattenedValues(object obj)
    {
        var properties = GetFlattenedProperties(obj.GetType());
        return properties.ToDictionary(
            kvp => kvp.Key,
            kvp => GetValueFromPath(obj, kvp.Key)
        );
    }

    public static object? GetValueFromPath(object? obj, string path)
    {
        var parts = path.Split('.');
        object? current = obj;
        if (current == null) return null;

        foreach (var part in parts)
        {
            var property = current.GetType().GetProperty(part);
            if (property == null)
                return null;

            current = property.GetValue(current);
            if (current == null)
                return null;
        }

        return current;
    }
}
