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

public class InputSignal<TParameters> 
    : IInstanceFor<InputSlot>
    //: IParameterizedTemplateInstance<InputSlot, IInputParameters>
{
    #region Relationships

    public required InputSlot Slot { get; init; }
    InputSlot IInstanceFor<InputSlot>.Template { get => Slot; }

    public required IHistoricalTimeSeries Source { get; set; }

    #endregion

    #region Parameters

    public uint Lookback { get; set; }
    public int Phase { get; set; }

    #endregion

}

public class IndicatorParameters
{
}


