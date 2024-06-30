using LionFire.Trading.ValueWindows;
using System.Reflection;

namespace LionFire.Trading.DataFlow;

public static class ProcessorValuesWindow
{
    public static PropertyInfo? TryGetInstanceProperty(Type type, string name)
    {
        var instanceProperty = type.GetProperty(name);

        if (instanceProperty != null)
        {
            // TODO: REFACTOR: this is duplicated somewhere -- consolidate that code here.
            if (!instanceProperty.PropertyType.IsAssignableTo(typeof(IReadOnlyValuesWindow)))
            {
                return null;
            }
        }
        return instanceProperty;
    }
}
