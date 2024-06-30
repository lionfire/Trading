#nullable enable
using LionFire.Persistence;
using System.Linq;

namespace LionFire.Trading;

public interface IReferenceTo { }
public interface IReferenceTo<TValue> : IReferenceTo
{

}
public static class IReferenceToX
{
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

public record HLCReference<TValue> : ExchangeSymbolTimeFrame, IPKlineInput, IReferenceTo<HLC<TValue>>
{
    public HLCReference(string Exchange, string ExchangeArea, string Symbol, TimeFrame TimeFrame) : base(Exchange, ExchangeArea, Symbol, TimeFrame) { }

    public override Type ValueType => typeof(HLC<TValue>);
    public override string Key => base.Key + SymbolValueAspect.AspectSeparator + "HLC";
}
