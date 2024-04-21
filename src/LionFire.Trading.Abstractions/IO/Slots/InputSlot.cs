using LionFire.Instantiating;
using LionFire.Trading.Data;
using System.Threading.Channels;

namespace LionFire.Trading; // TODO: Move to .Components or .Slots namespace

public class InputSlot : Slot
{
}

public interface IInputParameters
{

}

public class Input<TParameters> : IParameterizedTemplateInstance<InputSlot, IInputParameters>
{

    public InputSlot? Slot { get; set; }
    public uint Lookback { get; set; }
    public int Phase { get; set; }
    public IHistoricalTimeSeries Source { get; set; }
}

public class IndicatorParameters
{
}



