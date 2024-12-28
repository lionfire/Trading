using CsvHelper;
using System.Reflection;

namespace LionFire.Trading.Journal;

public class ConvertToStringClass2
{
    #region Dependencies

    public PropertyInfo PropertyInfo { get; }
    public ConvertToStringClass2? ParentConverter { get; }

    #endregion

    #region Lifecycle

    public ConvertToStringClass2(PropertyInfo propertyInfo, ConvertToStringClass2? parentConverter = null)
    {
        PropertyInfo = propertyInfo;
        ParentConverter = parentConverter;
    }

    #endregion

    public object? GetValue<T>(T root)
    {
        object? instance = ParentConverter == null
            ? root
            : ParentConverter.GetValue(root);
        return instance == null
            ? null
            : PropertyInfo.GetValue(instance);
    }

    public string? ConvertToString<T>(ConvertToStringArgs<T> args)
    {
        return GetValue(args.Value)?.ToString() ?? "";
    }
}
