using CsvHelper;
using CsvHelper.Configuration;
using LionFire.Inspection.Nodes;
using LionFire.Trading.Journal;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LionFire.Serialization.Csv;

/// <summary>
/// Special case hierarchy:
/// - root object has a property 'rootPropertyName' which may be of type object
/// - but the type we want a hierarchy for is of type 'parametersType'
/// </summary>
/// <typeparam name="T"></typeparam>
public partial class DynamicSplitMap<T> : ClassMap<T>
{

    public readonly PropertyInfo RootPropertyInfo;
    public readonly Type RootPropertyInstanceType;

    private class ConvertToStringClass
    {
        public ConvertToStringClass(PropertyInfo propertyInfo, string? parametersName = null, ConvertToStringClass? parentConverter = null)
        {
            PropertyInfo = propertyInfo;
            ParentConverter = parentConverter;
            //try
            //{
            //    parametersPropertyInfo = typeof(T).GetProperty(parametersName);// ?? throw new ArgumentNullException($"No {parametersName} on {typeof(T).GetType().FullName}");
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine($"Error: {ex.Message}");
            //}
        }

        public PropertyInfo PropertyInfo { get; }
        public ConvertToStringClass? ParentConverter { get; }

        //public PropertyInfo? parametersPropertyInfo;

        public object? GetValue(T root)
        {
            object? instance = ParentConverter == null
                ? root
                : ParentConverter.GetValue(root);
            return instance == null
                ? null
                : PropertyInfo.GetValue(instance);
        }

        public string? ConvertToString(ConvertToStringArgs<T> args)
        {
            return GetValue(args.Value)?.ToString() ?? "";
        }
    }

    public DynamicSplitMap(PropertyInfo rootPropertyInfo, Type rootPropertyInstanceType,  bool manualInitBase = false)
    {
        if(!rootPropertyInfo.DeclaringType!.IsAssignableTo(typeof(T)))
        {
            throw new ArgumentException($"Property {rootPropertyInfo.Name} must be a property of {typeof(T).Name}");
        }

        this.RootPropertyInfo = rootPropertyInfo;
        this.RootPropertyInstanceType = rootPropertyInstanceType;
        if (!manualInitBase) InitBase();
    }

    protected void InitBase()
    {
        var infos = BotParameterPropertiesInfo.Get(RootPropertyInstanceType);

        foreach (var kvp in infos.Dictionary)
        {
            var memberMap = Map().Name(kvp.Key);

            var fieldParameter = Expression.Parameter(typeof(ConvertToStringArgs<T>), "args");

            var converter = kvp.Value.ConvertToString;
            var instance = Expression.Constant(converter);
            var methodExpression = Expression.Call
            (
                instance,
                typeof(ConvertToStringClass2)
                    .GetMethod(nameof(ConvertToStringClass2.ConvertToString))!
                    .MakeGenericMethod(typeof(T)),
                fieldParameter
            );
            var lambdaExpression = Expression.Lambda<ConvertToString<T>>(methodExpression, fieldParameter);
            memberMap.Data.WritingConvertExpression = lambdaExpression;
        }
    }

#if OLD
    //private void MapProperties(Type type, ConvertToStringClass? parentConverter = null)
    //{

//    foreach(var kvp in BotParameterPropertiesInfo.Get(type).Parameters)
//    {
//        var name = prefix + (prefix.Length == 0 ? propertyName : "") + "." + kvp.Key;
//        var prop = kvp.Value.PropertyInfo;
//        MapProperty(name, prop);

//        var converter = new ConvertToStringClass(subProp, propertyName, parentConverter);
//        var memberMap = Map().Name(name);
//        var fieldParameter = Expression.Parameter(typeof(ConvertToStringArgs<T>), "args");

//        var instance = Expression.Constant(converter);
//        var methodExpression = Expression.Call
//        (
//            instance,
//            typeof(ConvertToStringClass).GetMethod(nameof(ConvertToStringClass.ConvertToString))!,
//            fieldParameter
//        );
//        var lambdaExpression = Expression.Lambda<ConvertToString<T>>(methodExpression, fieldParameter);
//        memberMap.Data.WritingConvertExpression = lambdaExpression;

//    }
//}

    private void AddSubProperties(Type type, string propertyName, string prefix = "", ConvertToStringClass? parentConverter = null)
    {
        var subProperties = type.GetProperties();

        foreach (var subProp in subProperties)
        {
            if (!subProp.CanWrite) continue;
            if (subProp.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

            var displayPropertyName = propertyName;
            if (displayPropertyName == "Parameters") displayPropertyName = "P"; // Alias (HARDCODE) ENH: Use alias attribute


            var name = prefix + (prefix.Length == 0 ? displayPropertyName : "") + "." + subProp.Name;

            try
            {
                var converter = new ConvertToStringClass(subProp, propertyName, parentConverter);
                if (subProp.PropertyType.IsPrimitive)
                {
                    var memberMap = Map().Name(name);
                    var fieldParameter = Expression.Parameter(typeof(ConvertToStringArgs<T>), "args");

                    var instance = Expression.Constant(converter);
                    var methodExpression = Expression.Call
                    (
                        instance,
                        typeof(ConvertToStringClass).GetMethod(nameof(ConvertToStringClass.ConvertToString))!,
                        fieldParameter
                    );
                    var lambdaExpression = Expression.Lambda<ConvertToString<T>>(methodExpression, fieldParameter);
                    memberMap.Data.WritingConvertExpression = lambdaExpression;
                }
                else
                {
                    if (prefix.Select(c => c == '_').Count() <= 3) // TEMP
                    {
                        AddSubProperties(subProp.PropertyType, subProp.Name, name, converter);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Warning: Unable to map property {subProp.Name}. Error: {ex.Message}");
            }
        }
    }

    public static Expression<Func<TModel, object>> BuildDynamicExpression<TModel>(params (string, Type)[] propertyNames)
    {
        Type currentType = typeof(TModel);
        ParameterExpression parameter = Expression.Parameter(currentType, "m");
        Expression propertyAccess = parameter;

        foreach (var kvp in propertyNames)
        {
            var propName = kvp.Item1;
            currentType = kvp.Item2;

            // Get the property info for the current property name on the current type
            var propertyInfo = currentType.GetProperty(propName)
                ?? throw new InvalidOperationException($"Property '{propName}' not found on type '{currentType.Name}'");

            // Check if the property type is a reference type or nullable type
            if (propertyInfo.PropertyType.IsClass || propertyInfo.PropertyType.IsValueType && Nullable.GetUnderlyingType(propertyInfo.PropertyType) != null)
            {
                // If it's a reference type or nullable, we can safely navigate further
                propertyAccess = Expression.Convert(propertyAccess, propertyInfo.DeclaringType!);
                propertyAccess = Expression.Property(propertyAccess, propertyInfo);
                currentType = propertyInfo.PropertyType;
            }
            else
            {
                throw new InvalidOperationException($"Property '{propName}' of type '{propertyInfo.PropertyType.Name}' is not a reference or nullable type and cannot be dynamically accessed.");
            }
        }

        // Convert the final property access to object type for a generic return
        Expression converted = Expression.Convert(propertyAccess, typeof(object));

        // Create the lambda expression
        return Expression.Lambda<Func<TModel, object>>(converted, parameter);
    }
    //public static Expression<Func<TModel, object>> BuildDynamicExpression_OLD<TModel>(params string[] propertyNames)
    //{
    //    // Start with the parameter expression (like 'x' in x.B.C)
    //    ParameterExpression parameter = Expression.Parameter(typeof(TModel), "x");

//    Expression propertyAccess = parameter;

//    foreach (var propName in propertyNames)
//    {
//        MemberExpression member = Expression.PropertyOrField(propertyAccess, propName);
//        propertyAccess = member;
//    }
//    Expression converted = Expression.Convert(propertyAccess, typeof(ExpandoObject));
//    return Expression.Lambda<Func<TModel, object>>(converted, parameter);
//}

    private void MapProperty(string parentPropertyName, PropertyInfo subProp)
    {
        var columnName = $"{parentPropertyName}_{subProp.Name}";
        MapPropertyInternal(subProp, columnName);
    }

    private void MapProperty(PropertyInfo prop)
    {
        MapPropertyInternal(prop, prop.Name);
    }

    private void MapPropertyInternal(PropertyInfo prop, string columnName)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression propertyExpression;

        if (prop.DeclaringType == typeof(T))
        {
            propertyExpression = Expression.Property(parameter, prop);
        }
        else
        {
            var parentProperty = typeof(T).GetProperties()
                .FirstOrDefault(p => p.PropertyType == prop.DeclaringType || p.Name == this.rootPropertyName);

            if (parentProperty == null)
                throw new InvalidOperationException($"Unable to find parent property for {prop.Name} on type {typeof(T).Name}");

            var parentExpression = Expression.Property(parameter, parentProperty);

            if (parentProperty.PropertyType == typeof(object))
            {
                var getMethodInfo = typeof(DynamicSplitMap<T>).GetMethod(nameof(GetPropertyValueDynamic), BindingFlags.NonPublic | BindingFlags.Static);
                propertyExpression = Expression.Call(null, getMethodInfo, parentExpression, Expression.Constant(prop.Name));
            }
            else
            {
                propertyExpression = Expression.Property(parentExpression, prop);
            }
        }

        // Create a lambda expression with the correct return type
        var lambdaType = typeof(Func<,>).MakeGenericType(typeof(T), prop.PropertyType);
        var lambda = Expression.Lambda(lambdaType, propertyExpression, parameter);

        // Find the generic Map method
        var mapMethod = typeof(ClassMap<T>).GetMethods()
            .First(m => m.Name == "Map" && m.IsGenericMethod && m.GetParameters().Length == 2);

        // Make a generic method with the correct type
        var genericMapMethod = mapMethod.MakeGenericMethod(prop.PropertyType);

        // Invoke the generic method
        var mapping = genericMapMethod.Invoke(this, new object[] { lambda, true });

        var nameMethod = mapping.GetType().GetMethod("Name");
        nameMethod.Invoke(mapping, new object[] { columnName });

        // Special handling for System.Type properties
        if (prop.PropertyType == typeof(System.Type))
        {
            var convertUsingMethod = mapping.GetType().GetMethod("ConvertUsing", new Type[] { typeof(Func<string, System.Type>) });
            convertUsingMethod.Invoke(mapping, new object[] {
                new Func<string, System.Type>(typeName => Type.GetType(typeName) ?? typeof(object))
            });
        }
    }
#endif
    private static object? GetPropertyValueDynamic(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)!.GetValue(obj, null);
    }
}
