using LionFire.Ontology;
using LionFire.Trading.Journal;
//using Spectre.Console;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;

namespace LionFire.Trading;

public class HierarchicalPropertyInfo : IKeyable<string>
{
    #region Relationships

    public HierarchicalPropertyInfo? Parent { get; set; }
    public Type? RootPropertyType { get; }

    #endregion

    #region Identity

    public string Key { get; set; }

    #region Derived

    public string Path { get; }
    public string Name => LastPropertyInfo!.Name;
    public string? Alias { get; set; }
    public string EffectiveName => Alias ?? Name;

    #endregion

    #endregion

    #region Lifecycle

    // REFACTOR: separate constructor for just rootPropertyType
    public HierarchicalPropertyInfo(Type rootPropertyType) : this(null, ImmutableArray<PropertyInfo>.Empty, rootPropertyType: rootPropertyType)
    {
    }

    public HierarchicalPropertyInfo(HierarchicalPropertyInfo? parent, IReadOnlyList<PropertyInfo> propertyInfos, Type? rootPropertyType = null)
    {
        Parent = parent;
        Path = string.Join(".", propertyInfos.Select(pi => pi.Name));
        Key = Path;
        PropertyInfos = propertyInfos;
        RootPropertyType = rootPropertyType;
        Alias = LastPropertyInfo?.GetCustomAttribute<AliasAttribute>()?.Alias;
    }

    #endregion

    #region Children

    public IReadOnlyList<PropertyInfo> PropertyInfos { get; }

    #region Derived

    public Type ValueType => LastPropertyInfo.PropertyType;
    public PropertyInfo LastPropertyInfo => PropertyInfos!.LastOrDefault();
    public TradingParameterAttribute ParameterAttribute => parameterAttribute ??= LastPropertyInfo?.GetCustomAttribute<TradingParameterAttribute>()!;
    private TradingParameterAttribute? parameterAttribute;

    #endregion

    #endregion

    #region Bonus

    public ConvertToStringClass2 ConvertToString
    {
        get
        {
            if (convertToString == null)
            {
                convertToString = new ConvertToStringClass2(LastPropertyInfo, Parent?.ConvertToString);
            }
            return convertToString;
        }
    }
    private ConvertToStringClass2? convertToString;

    public bool IsOptimizable => ParameterAttribute != null;

    public int OptimizeOrderTiebreaker
    {
        get
        {
            if (ParameterAttribute.OptimizeOrderTiebreaker != null) { return (int)ParameterAttribute.OptimizeOrderTiebreaker; }

            if (ParameterAttribute.OptimizerHints.HasFlag(OptimizationDistributionKind.Period)) { return 500; }

            if (ParameterAttribute.OptimizerHints.HasFlag(OptimizationDistributionKind.Reversal)) { return 800; }

            if (ParameterAttribute.OptimizerHints.HasFlag(OptimizationDistributionKind.SpectralCategory)) { return 900; }
            else if (LastPropertyInfo!.PropertyType.IsEnum || ParameterAttribute.OptimizerHints.HasFlag(OptimizationDistributionKind.Category)) { return 1000; }

            return 0;
        }
    }


    public void SetValue(object? rootObject, object? value, bool createMissingObjects = true)
    {
        object? objectCursor = rootObject;

        foreach (var pi in PropertyInfos.SkipLast(1))
        {
            var parent = objectCursor;
            objectCursor = pi.GetValue(parent);
            if (objectCursor == null)
            {
                if (createMissingObjects)
                {
                    objectCursor = Activator.CreateInstance(pi.PropertyType);
                    pi.SetValue(parent, objectCursor);
                    //SetPropertyValue(parent, pi, objectCursor);
                }
                else
                {
                    return;
                }
            }
        }
        SetPropertyValue(objectCursor!, PropertyInfos.Last(), value);
    }

    #endregion

    #region Conversion of numeric types

    public static void SetPropertyValue(object obj, PropertyInfo propertyInfo, object? value)
    {
        try
        {
            if (value == null)
            {
                propertyInfo.SetValue(obj, null);
                return;
            }

            Type propertyType = propertyInfo.PropertyType;
            Type valueType = value.GetType();

            if (propertyType == valueType)
            {
                propertyInfo.SetValue(obj, value);
                return;
            }

            if (propertyType.IsEnum)
            {
                //if (!Enum.IsDefined(propertyType, value)) throw new ArgumentException(); // Assuming this check here passes
                propertyInfo.SetValue(obj, Enum.ToObject(propertyType, value));
            }
            else if (IsNumericType(propertyType) && IsNumericType(valueType))
            {
                try
                {
                    object convertedValue = Convert.ChangeType(value, propertyType);
                    propertyInfo.SetValue(obj, convertedValue);
                }
                catch (OverflowException)
                {
                    throw new ArgumentException($"Value {value} is too large for property {propertyInfo.Name} of type {propertyType}");
                }
                catch (InvalidCastException)
                {
                    throw new ArgumentException($"Cannot convert value of type {valueType} to property {propertyInfo.Name} of type {propertyType}");
                }
            }
            else
            {
                throw new ArgumentException($"Cannot assign value of type {valueType} to property {propertyInfo.Name} of type {propertyType}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Exception setting property {propertyInfo.Name} of type {propertyInfo.PropertyType} to value of type {value?.GetType().FullName}");
        }
    }

    private static bool IsNumericType(Type type)
    {
        if (type.IsEnum) return false;

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }
    #endregion

    public override string ToString() => Name ?? Key;

    
}
