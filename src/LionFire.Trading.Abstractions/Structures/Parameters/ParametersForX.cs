using System.Linq;

namespace LionFire.Structures;

public static class ParametersForX
{
    public static Type? IsParametersFor(this Type type)
    {
        var parametersForType = type.GetInterfaces().Where(i => i.IsGenericType && i.Name == typeof(IParametersFor<>).Name).FirstOrDefault();

        return parametersForType?.GetGenericArguments().FirstOrDefault();
    }
}
