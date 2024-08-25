#nullable enable
namespace LionFire.Trading;

public interface IPrecision<T>
{
    static Type StaticPrecisionType => typeof(T);
}
//public interface IHasPrecisionFor<T>
//{
//    static Type StaticPrecisionType
//    {
//        get
//        {
//            if (typeof(T).IsAssignableTo(typeof(IHasPrecision)){
//                var instance = (IHasPrecision)Activator.CreateInstance(typeof(T))!;
//                return instance.PrecisionType;
//            }
//            throw new NotImplementedException();
//        }
//    }
//}

public interface IHasPrecision
{
    Type PrecisionType { get; }
}

public interface IValueType
{
    Type ValueType { get; }
}


