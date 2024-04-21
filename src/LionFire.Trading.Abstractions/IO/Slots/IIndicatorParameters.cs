using LionFire.Trading.IO;

namespace LionFire.Trading;

public interface IIndicatorParameters
{
    static abstract List<InputSlot> InputSlots();
    static abstract List<OutputSlot> OutputSlots();

    IReadOnlyList<InputSlot> Inputs { get; }
    IReadOnlyList<InputSlot> Outputs { get; }

    Type IndicatorType { get; set; }

}



