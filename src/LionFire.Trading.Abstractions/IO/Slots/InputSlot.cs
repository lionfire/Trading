using LionFire.Trading.Data;
using LionFire.Trading.IO;
using System.Threading.Channels;

namespace LionFire.Trading;

public class InputSlot : Slot
{
}
public class InstantiatedInputSlot : InputSlot
{
    public uint Lookback { get; set; }
    public int Phase { get; set; }
    public IHistoricalTimeSeries Source { get; set; }
}


public interface IIndicatorParameters
{
    static abstract List<InputSlot> InputSlots();
    static abstract List<OutputSlot> OutputSlots();

    IReadOnlyList<InputSlot> Inputs { get; }
    IReadOnlyList<InputSlot> Outputs { get; }

    Type IndicatorType { get; set; }

}

public class IndicatorParameters
{
}



