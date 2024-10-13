using CsvHelper;
using CsvHelper.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

// Based on Claude 3.5 Sonnet

public class DynamicSplitMap<T> : ClassMap<T>
{
    private readonly string propertyToSplit;

    private class ConvertToStringClass
    {
        //public delegate string? ConvertToString<T>(ConvertToStringArgs<T> args);

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
            //if (parametersPropertyInfo == null)
            //{
            //    return "(todo)";
            //}

            return GetValue(args.Value)?.ToString() ?? "";
            //var parameters = parametersPropertyInfo.GetValue(args.Value);
            //var val = PropertyInfo.GetValue(parameters);
            //return val?.ToString();
        }
    }

    public DynamicSplitMap(Type type, string propertyToSplit)
    {
        this.propertyToSplit = propertyToSplit;
        var propertyInfo = typeof(T).GetProperty(propertyToSplit);
        if (propertyInfo == null)
            throw new ArgumentException($"Property {propertyToSplit} not found in type {typeof(T).Name}");

        //var type = ((obj.GetType().GetProperty(propertyName)
        //        ?? throw new ArgumentException($"Property {propertyName} not found in type {typeof(T).Name}")
        //    )
        //    .GetValue(obj) ?? typeof(object)).GetType();

        //var subProperties = propertyInfo.PropertyType.GetProperties();

        ConvertToStringClass? parentConverter = new ConvertToStringClass(propertyInfo);
        AddSubProperties(type, propertyToSplit, parentConverter: parentConverter);

        // Map other properties
        //foreach (var prop in typeof(T).GetProperties().Where(p => p.Name != propertyName))
        //{
        //    var parameter = Expression.Parameter(typeof(T), "x");
        //    var propExpression = Expression.Property(parameter, prop);
        //    var lambda = Expression.Lambda(propExpression, parameter);

        //    var mapMethod = typeof(ClassMap<T>).GetMethods()
        //        .First(m => m.Name == "Map" && m.IsGenericMethod);
        //    var genericMapMethod = mapMethod.MakeGenericMethod(prop.PropertyType);
        //    genericMapMethod.Invoke(this, new object[] { lambda });
        //}
    }

    private void AddSubProperties(Type type, string propertyName, string prefix = "", ConvertToStringClass? parentConverter = null)
    {
        var subProperties = type.GetProperties();

        foreach (var subProp in subProperties)
        {
            if (!subProp.CanWrite) continue;
            if (subProp.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

            var name = prefix + (prefix.Length == 0 ? propertyName : "") + "_" + subProp.Name;

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
                .FirstOrDefault(p => p.PropertyType == prop.DeclaringType || p.Name == this.propertyToSplit);

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
    private static object? GetPropertyValueDynamic(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)!.GetValue(obj, null);
    }
}

#if false
if (false) // OLD, WIP
        {
#if false
            //var columnName = $"{propertyToSplit}_{subProp.Name}";

            // Create a lambda expression for accessing the sub-property
            var parameter = Expression.Parameter(typeof(T), "x");
            var propertyExpression = Expression.Property(parameter, propertyToSplit);
            var castExpression = Expression.Convert(propertyExpression, propertiesType);
            var subPropertyExpression = Expression.Property(castExpression, propertiesType, subProp.Name);
            var lambda = Expression.Lambda(subPropertyExpression, parameter);

            // Use reflection to call the Map method with the correct generic type
            var mapMethod = typeof(ClassMap<T>).GetMethods()
                .First(m => m.Name == "Map" && m.IsGenericMethod);
            var genericMapMethod = mapMethod.MakeGenericMethod(subProp.PropertyType);
            var mapping = genericMapMethod.Invoke(this, new object[] { lambda, false });

            // Set the column name
            var nameMethod = mapping.GetType().GetMethod("Name");
            nameMethod.Invoke(mapping, new object[] { new string[] { columnName } });
#endif
        }

#endif
