#nullable enable
namespace LionFire.Trading;

public interface IHasPrecision
{
    Type PrecisionType { get; }
}

public interface IValueType
{
    Type ValueType { get; }
}


