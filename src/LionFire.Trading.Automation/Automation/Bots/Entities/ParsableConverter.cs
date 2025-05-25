using LionFire.Serialization;
using System.ComponentModel;
using System.Globalization;

namespace LionFire.Trading.Automation;

public class ParsableConverter<T> : TypeConverter
    where T : IParsableSlim<T>
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
    {
        if (value is string str)
        {
            try
            {
                // Delegate to the Parse method of the type T
                return T.Parse(str);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Failed to convert '{str}' to {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object value, Type? destinationType)
    {
        if (destinationType == typeof(string) && value is OptimizationRunReference reference)
        {
            return reference.ToString();
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}

#if UNUSED

public class OptimizationRunReferenceConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
    {
        if (value is string str) { return OptimizationRunReference.Parse(str); }
        return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
    {
        if (destinationType == typeof(string) && value is OptimizationRunReference reference)
        {
            return reference.ToString();
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}

#endif
