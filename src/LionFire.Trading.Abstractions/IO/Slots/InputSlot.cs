using LionFire.Instantiating;
using LionFire.Trading.Data;
using System.Threading.Channels;

namespace LionFire.Trading; // TODO: Move to .Components or .Slots namespace

public class InputSlot : Slot
{
}

public interface IInputParameters
{
    int Phase { get; }
}

public class InputParameters<T> : IInputParameters
{
    public required T Parameters { get; init; }
    public int Phase { get; init; }
}

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

public interface IParametersFor<T> { }

public class IndicatorParameters<TIndicator> : IParametersFor<TIndicator>
{
}


