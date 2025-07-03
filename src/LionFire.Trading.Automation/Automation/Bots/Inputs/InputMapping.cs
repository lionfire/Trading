using LionFire.Trading.Automation.Bots;

namespace LionFire.Trading.Automation;

public readonly record struct InputMapping(IPInput PInput, InputParameterToValueMapping Mapping);
