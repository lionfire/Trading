using LionFire.Instantiating;
using System.Threading.Channels;

namespace LionFire.Trading; // TODO: Move to .Components or .Slots namespace

/// <summary>
/// Slots appear on Parameters objects, and allow the Parameter object to be instantiated using the Parameter object plus some context that provides ordered Input Sources.
/// Slots, if left null at instantiation time, are to be populated from the ordered list of Sources, in order.
/// </summary>
public class InputSlot : Slot
{
    public int DefaultSource { get; set; }

    #region Construction

    public static InputSlot SymbolAspect<T>()
    {
        return new InputSlot
        {
            Name = "Symbol",
            ValueType = typeof(T),
            Aspects = DataPointAspect.Unspecified,
        };
    }

    public static InputSlot BarMultiAspect<T>(DataPointAspect aspects)
    {
        return new InputSlot
        {
            Name = "Symbol",
            ValueType = typeof(T),
            Aspects = aspects,
        };
    }

    #endregion
}

public interface IInputParameters
{
    int Phase { get; }
}

#if UNUSED
public class InputParameters<T> : IInputParameters
{
    public required T Parameters { get; init; }
    public int Phase { get; init; }
}
#endif



