using LionFire.Trading.DataFlow;
using LionFire.Trading.IO;

namespace LionFire.Trading;



// TODO: RENAME to something more generic than Indicator.  For example, it could be a processor.

public interface IIndicatorParameters : IPInput
{
    SlotsInfo Slots { get; }

    //static abstract IReadOnlyList<OutputSlot> Outputs(); // Should this be Signal instead of Slot?

    Type InstanceType { get; }

    /// <summary>
    /// Aggregate of all Inputs
    /// </summary>
    Type InputType { get; }
    IReadOnlyList<Type> SlotTypes { get; }
    Type OutputType { get; }
    int Memory { get; }
    int Lookback => Memory - 1;

    /// <summary>
    /// Number of (resolvable) components of InputType
    /// </summary>
    int InputCount { get; }
}


//public interface IIndicatorParameters_FUTURE_MAYBE
//{
//    static abstract List<InputSlot> InputSlots();
//    static abstract List<OutputSlot> OutputSlots();

//    IReadOnlyList<InputSlot> Inputs { get; }
//    IReadOnlyList<InputSlot> Outputs { get; }

//    Type IndicatorType { get; set; }

//}


