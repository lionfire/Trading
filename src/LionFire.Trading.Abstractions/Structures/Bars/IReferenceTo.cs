
namespace LionFire.Trading;

public interface IReferenceTo { }
/// <summary>
/// TODO: RENAME to IUsesPrecision
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface IReferenceTo<TValue> : IReferenceTo { }

public static class IReferenceToX
{
    /// <summary>
    /// TODO: RENAME to GetPrecision
    /// </summary>
    /// <param name="parentType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Type GetTypeOfReferenced(this Type parentType)
    {
        return TryGetTypeOfReferenced(parentType) ?? throw new ArgumentException($"Specified type does not implement {typeof(IReferenceTo<>)}");
    }
    public static Type? TryGetTypeOfReferenced(this Type parentType)
    {
        foreach (var type in parentType.GetInterfaces().Where(t => t.IsGenericType))
        {
            var genericType = type.GetGenericTypeDefinition();
            if (genericType != typeof(IReferenceTo<>)) continue;
            return type.GetGenericArguments()[0];
        }
        return null;
        //throw new ArgumentException($"Objects that implement {typeof(IReferenceTo)} are also expected to implement one {typeof(IReferenceTo<>)}");
    }
}

