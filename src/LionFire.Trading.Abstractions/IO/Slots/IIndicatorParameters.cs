using LionFire.Trading.IO;

namespace LionFire.Trading;

public interface IIndicatorParameters
{
    Type IndicatorType { get; }
    Type InputType { get; }
    Type OutputType { get; }
}


//public interface IIndicatorParameters_FUTURE_MAYBE
//{
//    static abstract List<InputSlot> InputSlots();
//    static abstract List<OutputSlot> OutputSlots();

//    IReadOnlyList<InputSlot> Inputs { get; }
//    IReadOnlyList<InputSlot> Outputs { get; }

//    Type IndicatorType { get; set; }

//}





