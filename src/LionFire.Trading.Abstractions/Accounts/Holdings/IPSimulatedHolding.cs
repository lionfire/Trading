using LionFire.Trading.Automation;

namespace LionFire.Trading.Automation;

// Marker interface: must implement IPSimulatedHolding<TPrecision> for generic precision
public interface IPSimulatedHolding : IPHolding { }

public interface IPSimulatedHolding<TPrecision> : IPSimulatedHolding
    where TPrecision : struct, INumber<TPrecision>
{
    TPrecision StartingBalance { get; set; }

    //DateTimeOffset StartTime { get; set; }

    PAssetProtection<TPrecision>? AssetProtection { get; set; }

    
}
