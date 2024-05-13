using LionFire.Instantiating;
using LionFire.Trading.Data;

namespace LionFire.Trading; // TODO: Move to .Components or .Slots namespace

public interface IInputSignal : IInstanceFor<InputSlot>
{

}


/// <typeparam name="TValue"></typeparam>
/// <remarks>
/// Could be:
/// - historical
/// - real-time
/// - both
/// </remarks>
public class InputSignal<TValue> : IInputSignal
//, IParameterizedTemplateInstance<InputSlot, IInputParameters>
{
    #region Relationships

    public required InputSlot Slot { get; init; }
    InputSlot IInstanceFor<InputSlot>.Template { get => Slot; }

    public required IHistoricalTimeSeries Source { get; set; }

    #endregion

    #region Parameters

    public required IInputParameters Parameters { get; init; }

    // REVIEW - does this belong here?
    public uint Lookback { get; set; }

    #endregion
}


